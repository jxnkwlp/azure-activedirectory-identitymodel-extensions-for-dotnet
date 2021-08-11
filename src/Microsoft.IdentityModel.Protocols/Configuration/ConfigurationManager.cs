//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.IdentityModel.Protocols
{
    /// <summary>
    /// Manages the retrieval of Configuration data.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IDocumentRetriever"/>.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ConfigurationManager<T> : StandardConfigurationManager, IConfigurationManager<T> where T : class
    {
        private static TimeSpan _jitter = new TimeSpan(0,1,0,0);
        private TimeSpan _automaticRefreshInterval = DefaultAutomaticRefreshInterval.Add(TimeSpan.FromMinutes(new Random().Next((int)_jitter.TotalMinutes)));
        private TimeSpan _refreshInterval = DefaultRefreshInterval;
        private DateTimeOffset _syncAfter = DateTimeOffset.MinValue;
        private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;
        private bool _isFirstRefreshRequest = true;

        private readonly SemaphoreSlim _refreshLock;
        private readonly string _metadataAddress;
        private readonly IDocumentRetriever _docRetriever;
        private readonly IConfigurationRetriever<T> _configRetriever;

        /// <summary>
        /// Static initializer for a new object. Static initializers run before the first instance of the type is created.
        /// </summary>
        static ConfigurationManager()
        {
            LogHelper.LogVerbose("Assembly version info: " + typeof(ConfigurationManager<T>).AssemblyQualifiedName);
        }

        /// <summary>
        /// Instantiates a new <see cref="ConfigurationManager{T}"/> that manages automatic and controls refreshing on configuration data.
        /// </summary>
        /// <param name="metadataAddress">The address to obtain configuration.</param>
        /// <param name="configRetriever">The <see cref="IConfigurationRetriever{T}"/></param>
        public ConfigurationManager(string metadataAddress, IConfigurationRetriever<T> configRetriever)
            : this(metadataAddress, configRetriever, new HttpDocumentRetriever())
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="ConfigurationManager{T}"/> that manages automatic and controls refreshing on configuration data.
        /// </summary>
        /// <param name="metadataAddress">The address to obtain configuration.</param>
        /// <param name="configRetriever">The <see cref="IConfigurationRetriever{T}"/></param>
        /// <param name="httpClient">The client to use when obtaining configuration.</param>
        public ConfigurationManager(string metadataAddress, IConfigurationRetriever<T> configRetriever, HttpClient httpClient)
            : this(metadataAddress, configRetriever, new HttpDocumentRetriever(httpClient))
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="ConfigurationManager{T}"/> that manages automatic and controls refreshing on configuration data.
        /// </summary>
        /// <param name="metadataAddress">The address to obtain configuration.</param>
        /// <param name="configRetriever">The <see cref="IConfigurationRetriever{T}"/></param>
        /// <param name="docRetriever">The <see cref="IDocumentRetriever"/> that reaches out to obtain the configuration.</param>
        /// <exception cref="ArgumentNullException">If 'metadataAddress' is null or empty.</exception>
        /// <exception cref="ArgumentNullException">If 'configRetriever' is null.</exception>
        /// <exception cref="ArgumentNullException">If 'docRetriever' is null.</exception>
        public ConfigurationManager(string metadataAddress, IConfigurationRetriever<T> configRetriever, IDocumentRetriever docRetriever)
        {
            if (string.IsNullOrWhiteSpace(metadataAddress))
                throw LogHelper.LogArgumentNullException(nameof(metadataAddress));

            if (configRetriever == null)
                throw LogHelper.LogArgumentNullException(nameof(configRetriever));

            if (docRetriever == null)
                throw LogHelper.LogArgumentNullException(nameof(docRetriever));

            _metadataAddress = metadataAddress;
            _docRetriever = docRetriever;
            _configRetriever = configRetriever;
            _refreshLock = new SemaphoreSlim(1);
        }

        /// <summary>
        /// Obtains an updated version of Configuration.
        /// </summary>
        /// <returns>Configuration of type T.</returns>
        /// <remarks>If the time since the last call is less than <see cref="StandardConfigurationManager.AutomaticRefreshInterval"/> then <see cref="IConfigurationRetriever{T}.GetConfigurationAsync"/> is not called and the current Configuration is returned.</remarks>
        public async Task<T> GetConfigurationAsync()
        {
            return await GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Obtains an updated version of Configuration.
        /// </summary>
        /// <param name="cancel">CancellationToken</param>
        /// <returns>Configuration of type T.</returns>
        /// <remarks>If the time since the last call is less than <see cref="StandardConfigurationManager.AutomaticRefreshInterval"/> then <see cref="IConfigurationRetriever{T}.GetConfigurationAsync"/> is not called and the current Configuration is returned.</remarks>
        public async Task<T> GetConfigurationAsync(CancellationToken cancel)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (CurrentConfiguration != null && _syncAfter > now)
            {
                return CurrentConfiguration as T;
            }

            await _refreshLock.WaitAsync(cancel).ConfigureAwait(false);
            try
            {
                if (_syncAfter <= now)
                {
                    try
                    {
                        // Don't use the individual CT here, this is a shared operation that shouldn't be affected by an individual's cancellation.
                        // The transport should have it's own timeouts, etc..
                        CurrentConfiguration = await _configRetriever.GetConfigurationAsync(_metadataAddress, _docRetriever, CancellationToken.None).ConfigureAwait(false) as StandardConfiguration;
                        Contract.Assert(CurrentConfiguration != null);
                        _lastRefresh = now;
                        _syncAfter = DateTimeUtil.Add(now.UtcDateTime, _automaticRefreshInterval);
                    }
                    catch (Exception ex)
                    {
                        _syncAfter = DateTimeUtil.Add(now.UtcDateTime, _automaticRefreshInterval < _refreshInterval ? _automaticRefreshInterval : _refreshInterval);
                        if (CurrentConfiguration == null) // Throw an exception if there's no configuration to return.
                            throw LogHelper.LogExceptionMessage(new InvalidOperationException(LogHelper.FormatInvariant(LogMessages.IDX20803, (_metadataAddress ?? "null")), ex));
                        else
                            LogHelper.LogExceptionMessage(new InvalidOperationException(LogHelper.FormatInvariant(LogMessages.IDX20806, (_metadataAddress ?? "null")), ex));
                    }
                }

                // Stale metadata is better than no metadata
                if (CurrentConfiguration != null)
                    return CurrentConfiguration as T;
                else
                {
                    throw LogHelper.LogExceptionMessage(new InvalidOperationException(LogHelper.FormatInvariant(LogMessages.IDX20803, (_metadataAddress ?? "null"))));
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        /// <summary>
        /// Obtains an updated version of Configuration.
        /// </summary>
        /// <param name="cancel">CancellationToken</param>
        /// <returns>Configuration of type StandardConfiguration.</returns>
        /// <remarks>If the time since the last call is less than <see cref="StandardConfigurationManager.AutomaticRefreshInterval"/> then <see cref="IConfigurationRetriever{T}.GetConfigurationAsync"/> is not called and the current Configuration is returned.</remarks>
        public override async Task<StandardConfiguration> GetGeneralConfigurationAsync(CancellationToken cancel)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (UseCurrentConfiguration && CurrentConfiguration != null && _syncAfter > now)
            {
                return CurrentConfiguration;
            }

            await _refreshLock.WaitAsync(cancel).ConfigureAwait(false);
            try
            {
                if (_syncAfter <= now)
                {
                    try
                    {
                        // Don't use the individual CT here, this is a shared operation that shouldn't be affected by an individual's cancellation.
                        // The transport should have it's own timeouts, etc..
                        CurrentConfiguration = await _configRetriever.GetConfigurationAsync(_metadataAddress, _docRetriever, CancellationToken.None).ConfigureAwait(false) as StandardConfiguration;
                        Contract.Assert(CurrentConfiguration != null);
                        _lastRefresh = now;
                        _syncAfter = DateTimeUtil.Add(now.UtcDateTime, _automaticRefreshInterval);
                    }
                    catch (Exception ex)
                    {
                        _syncAfter = DateTimeUtil.Add(now.UtcDateTime, _automaticRefreshInterval < _refreshInterval ? _automaticRefreshInterval : _refreshInterval);
                        if (CurrentConfiguration == null) // Throw an exception if there's no configuration to return.
                            throw LogHelper.LogExceptionMessage(new InvalidOperationException(LogHelper.FormatInvariant(LogMessages.IDX20803, (_metadataAddress ?? "null")), ex));
                        else
                            LogHelper.LogExceptionMessage(new InvalidOperationException(LogHelper.FormatInvariant(LogMessages.IDX20806, (_metadataAddress ?? "null")), ex));
                    }
                }

                // Stale metadata is better than no metadata
                if (UseCurrentConfiguration && CurrentConfiguration != null)
                    return CurrentConfiguration;
                else
                {
                    throw LogHelper.LogExceptionMessage(new InvalidOperationException(LogHelper.FormatInvariant(LogMessages.IDX20803, (_metadataAddress ?? "null"))));
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        /// <summary>
        /// Requests that then next call to <see cref="GetConfigurationAsync()"/> obtain new configuration.
        /// <para>If it is a first force refresh or the last refresh was greater than <see cref="StandardConfigurationManager.RefreshInterval"/> then the next call to <see cref="GetConfigurationAsync()"/> will retrieve new configuration.</para>
        /// <para>If <see cref="StandardConfigurationManager.RefreshInterval"/> == <see cref="TimeSpan.MaxValue"/> then this method does nothing.</para>
        /// </summary>
        public override void RequestRefresh()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (_isFirstRefreshRequest)
            {
                _syncAfter = now;
                _isFirstRefreshRequest = false;
            }
            else if (now >= DateTimeUtil.Add(_lastRefresh.UtcDateTime, RefreshInterval))
            {
                _syncAfter = now;
            }
        }
    }
}
