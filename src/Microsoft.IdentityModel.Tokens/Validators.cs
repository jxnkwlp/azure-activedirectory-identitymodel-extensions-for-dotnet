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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Logging;

namespace Microsoft.IdentityModel.Tokens
{
    /// <summary>
    /// AudienceValidator
    /// </summary>
    public static class Validators
    {
        /// <summary>
        /// Validates if a given algorithm for a <see cref="SecurityKey"/> is valid.
        /// </summary>
        /// <param name="algorithm">The algorithm to be validated.</param>
        /// <param name="securityKey">The <see cref="SecurityKey"/> that signed the <see cref="SecurityToken"/>.</param>
        /// <param name="securityToken">The <see cref="SecurityToken"/> being validated.</param>
        /// <param name="validationParameters"><see cref="TokenValidationParameters"/> required for validation.</param>
        public static void ValidateAlgorithm(string algorithm, SecurityKey securityKey, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (validationParameters == null)
                throw LogHelper.LogArgumentNullException(nameof(validationParameters));

            if (validationParameters.AlgorithmValidator != null)
            {
                if (!validationParameters.AlgorithmValidator(algorithm, securityKey, securityToken, validationParameters))
                {
                    throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidAlgorithmException(LogHelper.FormatInvariant(LogMessages.IDX10697, LogHelper.MarkAsNonPII(algorithm), securityKey))
                    {
                        InvalidAlgorithm = algorithm,
                    });
                }

                return;
            }

            if (validationParameters.ValidAlgorithms != null && validationParameters.ValidAlgorithms.Any() && !validationParameters.ValidAlgorithms.Contains(algorithm, StringComparer.Ordinal))
            {
                throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidAlgorithmException(LogHelper.FormatInvariant(LogMessages.IDX10696, LogHelper.MarkAsNonPII(algorithm)))
                {
                    InvalidAlgorithm = algorithm,
                });
            }
        }

        /// <summary>
        /// Determines if the audiences found in a <see cref="SecurityToken"/> are valid.
        /// </summary>
        /// <param name="audiences">The audiences found in the <see cref="SecurityToken"/>.</param>
        /// <param name="securityToken">The <see cref="SecurityToken"/> being validated.</param>
        /// <param name="validationParameters"><see cref="TokenValidationParameters"/> required for validation.</param>
        /// <exception cref="ArgumentNullException">If 'vaidationParameters' is null.</exception>
        /// <exception cref="ArgumentNullException">If 'audiences' is null and <see cref="TokenValidationParameters.ValidateAudience"/> is true.</exception>
        /// <exception cref="SecurityTokenInvalidAudienceException">If <see cref="TokenValidationParameters.ValidAudience"/> is null or whitespace and <see cref="TokenValidationParameters.ValidAudiences"/> is null.</exception>
        /// <exception cref="SecurityTokenInvalidAudienceException">If none of the 'audiences' matched either <see cref="TokenValidationParameters.ValidAudience"/> or one of <see cref="TokenValidationParameters.ValidAudiences"/>.</exception>
        /// <remarks>An EXACT match is required.</remarks>
        public static void ValidateAudience(IEnumerable<string> audiences, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (validationParameters == null)
                throw LogHelper.LogArgumentNullException(nameof(validationParameters));

            if (validationParameters.AudienceValidator != null)
            {
                if (!validationParameters.AudienceValidator(audiences, securityToken, validationParameters))
                    throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidAudienceException(LogHelper.FormatInvariant(LogMessages.IDX10231, securityToken))
                    {
                        InvalidAudience = Utility.SerializeAsSingleCommaDelimitedString(audiences)
                    });

                return;
            }

            if (!validationParameters.ValidateAudience)
            {
                LogHelper.LogWarning(LogMessages.IDX10233);
                return;
            }

            if (audiences == null)
                throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidAudienceException(LogMessages.IDX10207) { InvalidAudience = null });

            if (string.IsNullOrWhiteSpace(validationParameters.ValidAudience) && (validationParameters.ValidAudiences == null))
                throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidAudienceException(LogMessages.IDX10208) { InvalidAudience = Utility.SerializeAsSingleCommaDelimitedString(audiences) });

            if (!audiences.Any())
                throw LogHelper.LogExceptionMessage(
                    new SecurityTokenInvalidAudienceException(LogHelper.FormatInvariant(LogMessages.IDX10206))
                    { InvalidAudience = Utility.SerializeAsSingleCommaDelimitedString(audiences) });

            // create enumeration of all valid audiences from validationParameters
            IEnumerable<string> validationParametersAudiences;

            if (validationParameters.ValidAudiences == null)
                validationParametersAudiences = new[] { validationParameters.ValidAudience };
            else if (string.IsNullOrWhiteSpace(validationParameters.ValidAudience))
                validationParametersAudiences = validationParameters.ValidAudiences;
            else
                validationParametersAudiences = validationParameters.ValidAudiences.Concat(new[] { validationParameters.ValidAudience });

            if (AudienceIsValid(audiences, validationParameters, validationParametersAudiences))
                return;

            throw LogHelper.LogExceptionMessage(
                new SecurityTokenInvalidAudienceException(LogHelper.FormatInvariant(LogMessages.IDX10214, Utility.SerializeAsSingleCommaDelimitedString(audiences), (validationParameters.ValidAudience ?? "null"), Utility.SerializeAsSingleCommaDelimitedString(validationParameters.ValidAudiences)))
                { InvalidAudience = Utility.SerializeAsSingleCommaDelimitedString(audiences) });
        }

        private static bool AudienceIsValid(IEnumerable<string> audiences, TokenValidationParameters validationParameters, IEnumerable<string> validationParametersAudiences)
        {
            foreach (string tokenAudience in audiences)
            {
                if (string.IsNullOrWhiteSpace(tokenAudience))
                    continue;

                foreach (string validAudience in validationParametersAudiences)
                {
                    if (string.IsNullOrWhiteSpace(validAudience))
                        continue;

                    if (AudiencesMatch(validationParameters, tokenAudience, validAudience))
                    {
                        LogHelper.LogInformation(LogMessages.IDX10234, tokenAudience);
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool AudiencesMatch(TokenValidationParameters validationParameters, string tokenAudience, string validAudience)
        {
            if (validAudience.Length == tokenAudience.Length)
            {
                if (string.Equals(validAudience, tokenAudience))
                    return true;
            }
            else if (validationParameters.IgnoreTrailingSlashWhenValidatingAudience && AudiencesMatchIgnoringTrailingSlash(tokenAudience, validAudience))
                return true;

            return false;
        }

        private static bool AudiencesMatchIgnoringTrailingSlash(string tokenAudience, string validAudience)
        {
            int length = -1;

            if (validAudience.Length == tokenAudience.Length + 1 && validAudience.EndsWith("/", StringComparison.InvariantCulture))
                length = validAudience.Length - 1;
            else if (tokenAudience.Length == validAudience.Length + 1 && tokenAudience.EndsWith("/", StringComparison.InvariantCulture))
                length = tokenAudience.Length - 1;

            // the length of the audiences is different by more than 1 and neither ends in a "/"
            if (length == -1)
                return false;

            if (string.CompareOrdinal(validAudience, 0, tokenAudience, 0, length) == 0)
            {
                LogHelper.LogInformation(LogMessages.IDX10234, tokenAudience);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if an issuer found in a <see cref="SecurityToken"/> is valid.
        /// </summary>
        /// <param name="issuer">The issuer to validate</param>
        /// <param name="securityToken">The <see cref="SecurityToken"/> that is being validated.</param>
        /// <param name="validationParameters"><see cref="TokenValidationParameters"/> required for validation.</param>
        /// <returns>The issuer to use when creating the "Claim"(s) in a "ClaimsIdentity".</returns>
        /// <exception cref="ArgumentNullException">If 'vaidationParameters' is null.</exception>
        /// <exception cref="ArgumentNullException">If 'issuer' is null or whitespace and <see cref="TokenValidationParameters.ValidateIssuer"/> is true.</exception>
        /// <exception cref="SecurityTokenInvalidIssuerException">If <see cref="TokenValidationParameters.ValidIssuer"/> is null or whitespace and <see cref="TokenValidationParameters.ValidIssuers"/> is null.</exception>
        /// <exception cref="SecurityTokenInvalidIssuerException">If 'issuer' failed to matched either <see cref="TokenValidationParameters.ValidIssuer"/> or one of <see cref="TokenValidationParameters.ValidIssuers"/>.</exception>
        /// <remarks>An EXACT match is required.</remarks>
        public static string ValidateIssuer(string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            return ValidateIssuer(issuer, securityToken, validationParameters, null);
        }

        /// <summary>
        /// Determines if an issuer found in a <see cref="SecurityToken"/> is valid.
        /// </summary>
        /// <param name="issuer">The issuer to validate</param>
        /// <param name="securityToken">The <see cref="SecurityToken"/> that is being validated.</param>
        /// <param name="validationParameters"><see cref="TokenValidationParameters"/> required for validation.</param>
        /// <param name="configuration">The <see cref="BaseConfiguration"/> required for issuer and signing key validation.</param>
        /// <returns>The issuer to use when creating the "Claim"(s) in a "ClaimsIdentity".</returns>
        /// <exception cref="ArgumentNullException">If 'vaidationParameters' is null.</exception>
        /// <exception cref="ArgumentNullException">If 'issuer' is null or whitespace and <see cref="TokenValidationParameters.ValidateIssuer"/> is true.</exception>
        /// <exception cref="ArgumentNullException">If ' configuration' is null.</exception>
        /// <exception cref="SecurityTokenInvalidIssuerException">If <see cref="TokenValidationParameters.ValidIssuer"/> is null or whitespace and <see cref="TokenValidationParameters.ValidIssuers"/> is null and <see cref="BaseConfiguration.Issuer"/> is null.</exception>
        /// <exception cref="SecurityTokenInvalidIssuerException">If 'issuer' failed to matched either <see cref="TokenValidationParameters.ValidIssuer"/> or one of <see cref="TokenValidationParameters.ValidIssuers"/> or <see cref="BaseConfiguration.Issuer"/>.</exception>
        /// <remarks>An EXACT match is required.</remarks>
        internal static string ValidateIssuer(string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters, BaseConfiguration configuration)
        {
            if (validationParameters == null)
                throw LogHelper.LogArgumentNullException(nameof(validationParameters));

            if (validationParameters.IssuerValidatorUsingConfiguration != null)
                return validationParameters.IssuerValidatorUsingConfiguration(issuer, securityToken, validationParameters, configuration);

            if (validationParameters.IssuerValidator != null)
                return validationParameters.IssuerValidator(issuer, securityToken, validationParameters);

            if (!validationParameters.ValidateIssuer)
            {
                LogHelper.LogInformation(LogMessages.IDX10235);
                return issuer;
            }

            if (string.IsNullOrWhiteSpace(issuer))
                throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidIssuerException(LogMessages.IDX10211)
                { InvalidIssuer = issuer });

            // Throw if all possible places to validate against are null or empty
            if (string.IsNullOrWhiteSpace(validationParameters.ValidIssuer) && validationParameters.ValidIssuers.IsNullOrEmpty() && string.IsNullOrWhiteSpace(configuration?.Issuer))
                throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidIssuerException(LogMessages.IDX10204)
                { InvalidIssuer = issuer });

            if (configuration != null)
            {
                if (string.Equals(configuration.Issuer, issuer))
                {
                    LogHelper.LogInformation(LogMessages.IDX10236, issuer);
                    return issuer;
                }

                throw LogHelper.LogExceptionMessage(
                    new SecurityTokenInvalidIssuerException(LogHelper.FormatInvariant(LogMessages.IDX10260, issuer, (validationParameters.ValidIssuer ?? "null"), Utility.SerializeAsSingleCommaDelimitedString(validationParameters.ValidIssuers), configuration.Issuer))
                    { InvalidIssuer = issuer });
            }

            if (string.Equals(validationParameters.ValidIssuer, issuer))
            {
                LogHelper.LogInformation(LogMessages.IDX10236, issuer);
                return issuer;
            }

            if (null != validationParameters.ValidIssuers)
            {
                foreach (string str in validationParameters.ValidIssuers)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        LogHelper.LogInformation(LogMessages.IDX10262);
                        continue;
                    }

                    if (string.Equals(str, issuer))
                    {
                        LogHelper.LogInformation(LogMessages.IDX10236, issuer);
                        return issuer;
                    }
                }
            }

            throw LogHelper.LogExceptionMessage(
                new SecurityTokenInvalidIssuerException(LogHelper.FormatInvariant(LogMessages.IDX10205, issuer, (validationParameters.ValidIssuer ?? "null"), Utility.SerializeAsSingleCommaDelimitedString(validationParameters.ValidIssuers)))
                { InvalidIssuer = issuer });
        }

        /// <summary>
        /// Validates the <see cref="SecurityKey"/> that signed a <see cref="SecurityToken"/>.
        /// </summary>
        /// <param name="securityKey">The <see cref="SecurityKey"/> that signed the <see cref="SecurityToken"/>.</param>
        /// <param name="securityToken">The <see cref="SecurityToken"/> being validated.</param>
        /// <param name="validationParameters"><see cref="TokenValidationParameters"/> required for validation.</param>
        /// <exception cref="ArgumentNullException"> if 'securityKey' is null and ValidateIssuerSigningKey is true.</exception>
        /// <exception cref="ArgumentNullException"> if 'securityToken' is null and ValidateIssuerSigningKey is true.</exception>
        /// <exception cref="ArgumentNullException"> if 'vaidationParameters' is null.</exception>
        public static void ValidateIssuerSecurityKey(SecurityKey securityKey, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            ValidateIssuerSecurityKey(securityKey, securityToken, validationParameters, null);
        }

        /// <summary>
        /// Validates the <see cref="SecurityKey"/> that signed a <see cref="SecurityToken"/>.
        /// </summary>
        /// <param name="securityKey">The <see cref="SecurityKey"/> that signed the <see cref="SecurityToken"/>.</param>
        /// <param name="securityToken">The <see cref="SecurityToken"/> being validated.</param>
        /// <param name="validationParameters"><see cref="TokenValidationParameters"/> required for validation.</param>
        /// <param name="configuration">The <see cref="BaseConfiguration"/> required for issuer and signing key validation.</param>
        /// <exception cref="ArgumentNullException"> if 'securityKey' is null and ValidateIssuerSigningKey is true.</exception>
        /// <exception cref="ArgumentNullException"> if 'securityToken' is null and ValidateIssuerSigningKey is true.</exception>
        /// <exception cref="ArgumentNullException"> if 'validationParameters' is null.</exception>
        internal static void ValidateIssuerSecurityKey(SecurityKey securityKey, SecurityToken securityToken, TokenValidationParameters validationParameters, BaseConfiguration configuration)
        {
            if (validationParameters == null)
                throw LogHelper.LogArgumentNullException(nameof(validationParameters));

            if (validationParameters.IssuerSigningKeyValidatorUsingConfiguration != null)
            {
                if (!validationParameters.IssuerSigningKeyValidatorUsingConfiguration(securityKey, securityToken, validationParameters, configuration))
                    throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidSigningKeyException(LogHelper.FormatInvariant(LogMessages.IDX10232, securityKey)) { SigningKey = securityKey });

                return;
            }

            if (validationParameters.IssuerSigningKeyValidator != null)
            {
                if (!validationParameters.IssuerSigningKeyValidator(securityKey, securityToken, validationParameters))
                    throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidSigningKeyException(LogHelper.FormatInvariant(LogMessages.IDX10232, securityKey)) { SigningKey = securityKey });

                return;
            }

            if (!validationParameters.ValidateIssuerSigningKey)
            {
                LogHelper.LogInformation(LogMessages.IDX10237);
                return;
            }

            if (!validationParameters.RequireSignedTokens && securityKey == null)
            {
                LogHelper.LogInformation(LogMessages.IDX10252);
                return;
            }
            else if (securityKey == null)
            {
                throw LogHelper.LogExceptionMessage(new ArgumentNullException(nameof(securityKey), LogMessages.IDX10253));
            }

            if (securityToken == null)
                throw LogHelper.LogArgumentNullException(nameof(securityToken));

            X509SecurityKey x509SecurityKey = securityKey as X509SecurityKey;
            if (x509SecurityKey?.Certificate is X509Certificate2 cert)
            {
                DateTime utcNow = DateTime.UtcNow;
                var notBeforeUtc = cert.NotBefore.ToUniversalTime();
                var notAfterUtc = cert.NotAfter.ToUniversalTime();

                if (notBeforeUtc > DateTimeUtil.Add(utcNow, validationParameters.ClockSkew))
                    throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidSigningKeyException(LogHelper.FormatInvariant(LogMessages.IDX10248, LogHelper.MarkAsNonPII(notBeforeUtc), LogHelper.MarkAsNonPII(utcNow))));

                LogHelper.LogInformation(LogMessages.IDX10250, LogHelper.MarkAsNonPII(notBeforeUtc), LogHelper.MarkAsNonPII(utcNow));

                if (notAfterUtc < DateTimeUtil.Add(utcNow, validationParameters.ClockSkew.Negate()))
                    throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidSigningKeyException(LogHelper.FormatInvariant(LogMessages.IDX10249, LogHelper.MarkAsNonPII(notAfterUtc), LogHelper.MarkAsNonPII(utcNow))));

                LogHelper.LogInformation(LogMessages.IDX10251, LogHelper.MarkAsNonPII(notAfterUtc), LogHelper.MarkAsNonPII(utcNow));
            }
        }

        /// <summary>
        /// Validates the lifetime of a <see cref="SecurityToken"/>.
        /// </summary>
        /// <param name="notBefore">The 'notBefore' time found in the <see cref="SecurityToken"/>.</param>
        /// <param name="expires">The 'expiration' time found in the <see cref="SecurityToken"/>.</param>
        /// <param name="securityToken">The <see cref="SecurityToken"/> being validated.</param>
        /// <param name="validationParameters"><see cref="TokenValidationParameters"/> required for validation.</param>
        /// <exception cref="ArgumentNullException">If 'vaidationParameters' is null.</exception>
        /// <exception cref="SecurityTokenNoExpirationException">If 'expires.HasValue' is false and <see cref="TokenValidationParameters.RequireExpirationTime"/> is true.</exception>
        /// <exception cref="SecurityTokenInvalidLifetimeException">If 'notBefore' is &gt; 'expires'.</exception>
        /// <exception cref="SecurityTokenNotYetValidException">If 'notBefore' is &gt; DateTime.UtcNow.</exception>
        /// <exception cref="SecurityTokenExpiredException">If 'expires' is &lt; DateTime.UtcNow.</exception>
        /// <remarks>All time comparisons apply <see cref="TokenValidationParameters.ClockSkew"/>.</remarks>
        public static void ValidateLifetime(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (validationParameters == null)
                throw LogHelper.LogArgumentNullException(nameof(validationParameters));

            if (validationParameters.LifetimeValidator != null)
            {
                if (!validationParameters.LifetimeValidator(notBefore, expires, securityToken, validationParameters))
                    throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidLifetimeException(LogHelper.FormatInvariant(LogMessages.IDX10230, securityToken))
                        { NotBefore = notBefore, Expires = expires });

                return;
            }

            if (!validationParameters.ValidateLifetime)
            {
                LogHelper.LogInformation(LogMessages.IDX10238);
                return;
            }

            if (!expires.HasValue && validationParameters.RequireExpirationTime)
                throw LogHelper.LogExceptionMessage(new SecurityTokenNoExpirationException(LogHelper.FormatInvariant(LogMessages.IDX10225, LogHelper.MarkAsNonPII(securityToken == null ? "null" : securityToken.GetType().ToString()))));

            if (notBefore.HasValue && expires.HasValue && (notBefore.Value > expires.Value))
                throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidLifetimeException(LogHelper.FormatInvariant(LogMessages.IDX10224, LogHelper.MarkAsNonPII(notBefore.Value), LogHelper.MarkAsNonPII(expires.Value)))
                { NotBefore = notBefore, Expires = expires });

            DateTime utcNow = DateTime.UtcNow;
            if (notBefore.HasValue && (notBefore.Value > DateTimeUtil.Add(utcNow, validationParameters.ClockSkew)))
                throw LogHelper.LogExceptionMessage(new SecurityTokenNotYetValidException(LogHelper.FormatInvariant(LogMessages.IDX10222, LogHelper.MarkAsNonPII(notBefore.Value), LogHelper.MarkAsNonPII(utcNow)))
                    { NotBefore = notBefore.Value });
 
            if (expires.HasValue && (expires.Value < DateTimeUtil.Add(utcNow, validationParameters.ClockSkew.Negate())))
                throw LogHelper.LogExceptionMessage(new SecurityTokenExpiredException(LogHelper.FormatInvariant(LogMessages.IDX10223, LogHelper.MarkAsNonPII(expires.Value), LogHelper.MarkAsNonPII(utcNow)))
                    { Expires = expires.Value });

            // if it reaches here, that means lifetime of the token is valid
            LogHelper.LogInformation(LogMessages.IDX10239);
        }

        /// <summary>
        /// Validates if a token has been replayed.
        /// </summary>
        /// <param name="expirationTime">When does the security token expire.</param>
        /// <param name="securityToken">The <see cref="SecurityToken"/> being validated.</param>
        /// <param name="validationParameters"><see cref="TokenValidationParameters"/> required for validation.</param>
        /// <exception cref="ArgumentNullException">If 'securityToken' is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">If 'validationParameters' is null or whitespace.</exception>
        /// <exception cref="SecurityTokenNoExpirationException">If <see cref="TokenValidationParameters.TokenReplayCache"/> is not null and expirationTime.HasValue is false. When a TokenReplayCache is set, tokens require an expiration time.</exception>
        /// <exception cref="SecurityTokenReplayDetectedException">If the 'securityToken' is found in the cache.</exception>
        /// <exception cref="SecurityTokenReplayAddFailedException">If the 'securityToken' could not be added to the <see cref="TokenValidationParameters.TokenReplayCache"/>.</exception>
        public static void ValidateTokenReplay(DateTime? expirationTime, string securityToken, TokenValidationParameters validationParameters)
        {
            if (string.IsNullOrWhiteSpace(securityToken))
                throw LogHelper.LogArgumentNullException(nameof(securityToken));

            if (validationParameters == null)
                throw LogHelper.LogArgumentNullException(nameof(validationParameters));

            if (validationParameters.TokenReplayValidator != null)
            {
                if (!validationParameters.TokenReplayValidator(expirationTime, securityToken, validationParameters))
                    throw LogHelper.LogExceptionMessage(new SecurityTokenReplayDetectedException(LogHelper.FormatInvariant(LogMessages.IDX10228, securityToken)));
                return;
            }

            if (!validationParameters.ValidateTokenReplay)
            {
                LogHelper.LogInformation(LogMessages.IDX10246);
                return;
            }

            // check if token if replay cache is set, then there must be an expiration time.
            if (validationParameters.TokenReplayCache != null)
            {
                if (!expirationTime.HasValue)
                    throw LogHelper.LogExceptionMessage(new SecurityTokenNoExpirationException(LogHelper.FormatInvariant(LogMessages.IDX10227, securityToken)));

                if (validationParameters.TokenReplayCache.TryFind(securityToken))
                    throw LogHelper.LogExceptionMessage(new SecurityTokenReplayDetectedException(LogHelper.FormatInvariant(LogMessages.IDX10228, securityToken)));

                if (!validationParameters.TokenReplayCache.TryAdd(securityToken, expirationTime.Value))
                    throw LogHelper.LogExceptionMessage(new SecurityTokenReplayAddFailedException(LogHelper.FormatInvariant(LogMessages.IDX10229, securityToken)));
            }

            // if it reaches here, that means no token replay is detected.
            LogHelper.LogInformation(LogMessages.IDX10240);
        }

        /// <summary>
        /// Validates if a token has been replayed.
        /// </summary>
        /// <param name="securityToken">The <see cref="SecurityToken"/> being validated.</param>
        /// <param name="expirationTime">When does the security token expire.</param>
        /// <param name="validationParameters"><see cref="TokenValidationParameters"/> required for validation.</param>
        /// <exception cref="ArgumentNullException">If 'securityToken' is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">If 'validationParameters' is null or whitespace.</exception>
        /// <exception cref="SecurityTokenNoExpirationException">If <see cref="TokenValidationParameters.TokenReplayCache"/> is not null and expirationTime.HasValue is false. When a TokenReplayCache is set, tokens require an expiration time.</exception>
        /// <exception cref="SecurityTokenReplayDetectedException">If the 'securityToken' is found in the cache.</exception>
        /// <exception cref="SecurityTokenReplayAddFailedException">If the 'securityToken' could not be added to the <see cref="TokenValidationParameters.TokenReplayCache"/>.</exception>
        public static void ValidateTokenReplay(string securityToken, DateTime? expirationTime, TokenValidationParameters validationParameters)
        {
            ValidateTokenReplay(expirationTime, securityToken, validationParameters);
        }

        /// <summary>
        /// Validates the type of the token.
        /// </summary>
        /// <param name="type">The token type or <c>null</c> if it couldn't be resolved (e.g from the 'typ' header for a JWT).</param>
        /// <param name="securityToken">The <see cref="SecurityToken"/> that is being validated.</param>
        /// <param name="validationParameters"><see cref="TokenValidationParameters"/> required for validation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="validationParameters"/> is null.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="securityToken"/> is null.</exception>
        /// <exception cref="SecurityTokenInvalidTypeException">If <paramref name="type"/> is null or whitespace and <see cref="TokenValidationParameters.ValidTypes"/> is not null.</exception>
        /// <exception cref="SecurityTokenInvalidTypeException">If <paramref name="type"/> failed to match <see cref="TokenValidationParameters.ValidTypes"/>.</exception>
        /// <remarks>An EXACT match is required. <see cref="StringComparison.Ordinal"/> (case sensitive) is used for comparing <paramref name="type"/> against <see cref="TokenValidationParameters.ValidTypes"/>.</remarks>
        /// <returns>The actual token type, that may be the same as <paramref name="type"/> or a different value if the token type was resolved from a different location.</returns>
        public static string ValidateTokenType(string type, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (securityToken == null)
                throw new ArgumentNullException(nameof(securityToken));

            if (validationParameters == null)
                throw LogHelper.LogArgumentNullException(nameof(validationParameters));

            if (validationParameters.TypeValidator == null && (validationParameters.ValidTypes == null || !validationParameters.ValidTypes.Any()))
            {
                LogHelper.LogInformation(LogMessages.IDX10255);
                return type;
            }

            if (validationParameters.TypeValidator != null)
                return validationParameters.TypeValidator(type, securityToken, validationParameters);

            // Note: don't throw an exception for a null or empty token type when a user-defined delegate is set
            // to allow it to extract the actual token type from a different location (e.g from the claims).
            if (string.IsNullOrEmpty(type))
                throw LogHelper.LogExceptionMessage(new SecurityTokenInvalidTypeException(LogMessages.IDX10256) { InvalidType = null });

            if (!validationParameters.ValidTypes.Contains(type, StringComparer.Ordinal))
            {
                throw LogHelper.LogExceptionMessage(
                    new SecurityTokenInvalidTypeException(LogHelper.FormatInvariant(LogMessages.IDX10257, LogHelper.MarkAsNonPII(type), Utility.SerializeAsSingleCommaDelimitedString(validationParameters.ValidTypes)))
                    { InvalidType = type });
            }

            // if it reaches here, token type was succcessfully validated.
            LogHelper.LogInformation(LogMessages.IDX10258, LogHelper.MarkAsNonPII(type));
            return type;
        }
    }
}
