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
#if !CrossVersionTokenValidation
using System.IdentityModel.Tokens.Jwt;
#endif
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Json.Linq;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml;
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.IdentityModel.Xml;

namespace Microsoft.IdentityModel.TestUtils
{
    /// <summary>
    /// Returns default token creation / validation artifacts:
    /// Claim
    /// ClaimIdentity
    /// ClaimPrincipal
    /// SecurityTokenDescriptor
    /// TokenValidationParameters
    /// </summary>
    public static class Default
    {
#if !CrossVersionTokenValidation
        private static string _referenceDigestValue;

        static Default()
        {
            _referenceDigestValue = Convert.ToBase64String(XmlUtilities.CreateDigestBytes("<OuterXml></OuterXml>", false));
        }
#endif

        public static string ActorIssuer
        {
            get => "http://Default.ActorIssuer.com/Actor";
        }

        public static string Acr
        {
            get => "Default.Acr";
        }

        public static string Amr
        {
            get => "Default.Amr";
        }

        public static List<string> Amrs
        {
            get => new List<string> { "Default.Amr1", "Default.Amr2", "Default.Amr3", "Default.Amr4" };
        }

#if !CrossVersionTokenValidation
        public static string AsymmetricJwt
        {
            get => Jwt(SecurityTokenDescriptor(AsymmetricSigningCredentials));
        }
#endif
        public static SecurityTokenDescriptor AsymmetricSignSecurityTokenDescriptor(List<Claim> claims)
        {
            return SecurityTokenDescriptor(null, AsymmetricSigningCredentials, claims);
        }

        public static SigningCredentials AsymmetricSigningCredentials
        {
            get => new SigningCredentials(KeyingMaterial.DefaultX509SigningCreds_2048_RsaSha2_Sha2.Key, KeyingMaterial.DefaultX509SigningCreds_2048_RsaSha2_Sha2.Algorithm, KeyingMaterial.DefaultX509SigningCreds_2048_RsaSha2_Sha2.Digest);
        }

        public static SigningCredentials AsymmetricSigningCredentialsWithoutSpecifyingDigest
        {
            get => new SigningCredentials(KeyingMaterial.DefaultX509SigningCreds_2048_RsaSha2_Sha2.Key, KeyingMaterial.DefaultX509SigningCreds_2048_RsaSha2_Sha2.Algorithm);
        }

        public static X509SigningCredentials X509AsymmetricSigningCredentials
        {
            get => new X509SigningCredentials(KeyingMaterial.DefaultCert_2048, SecurityAlgorithms.RsaSha256Signature);
        }

        public static SignatureProvider AsymmetricSignatureProvider
        {
            get => CryptoProviderFactory.Default.CreateForSigning(KeyingMaterial.DefaultX509Key_2048, SecurityAlgorithms.RsaSha256);
        }

        public static string AsymmetricSigningAlgorithm
        {
            get => SecurityAlgorithms.RsaSha256;
        }

        public static SecurityKey AsymmetricSigningKey
        {
            get => new X509SecurityKey(KeyingMaterial.DefaultCert_2048);
        }

        public static SecurityKey AsymmetricSigningKeyPublic
        {
            get => new X509SecurityKey(KeyingMaterial.DefaultCert_2048_Public);
        }

        public static SecurityKey AsymmetricEncryptionKeyPublic
        {
            get => new X509SecurityKey(KeyingMaterial.DefaultCert_2048_Public);
        }

#if !CrossVersionTokenValidation
        public static TokenValidationParameters AsymmetricEncryptSignTokenValidationParameters
        {
            get => TokenValidationParameters(SymmetricEncryptionKey256, AsymmetricSigningKey);
        }

        public static TokenValidationParameters AsymmetricSignTokenValidationParameters
        {
            get => TokenValidationParameters(null, AsymmetricSigningKey);
        }
#endif

        public static string AttributeName
        {
            get => "Country";
        }

        public static string AttributeNamespace
        {
            get => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims";
        }

        public static string Audience
        {
            get => "http://Default.Audience.com";
        }

        public static List<string> Audiences
        {
            get
            {
                return new List<string>
                {
                  "http://Default.Audience.com",
                  "http://Default.Audience1.com",
                  "http://Default.Audience2.com",
                  "http://Default.Audience3.com",
                  "http://Default.Audience4.com"
                };
            }
        }

        public static string AuthenticationInstant
        {
            get => "2017-03-18T18:33:37.080Z";
        }

        public static DateTime AuthenticationInstantDateTime
        {
            get => new DateTime(2017, 03, 18, 18, 33, 37, 80, DateTimeKind.Utc);
        }

        public static string AuthenticationMethod
        {
            get => "urn:oasis:names:tc:SAML:1.0:am:password";
        }

        public static Uri AuthenticationMethodUri
        {
            get => new Uri("urn:oasis:names:tc:SAML:1.0:am:password");
        }

        public static string AuthenticationType
        {
            get => "Default.Federation";
        }

        public static string AuthorityKind
        {
            get => "samlp:AttributeQuery";
        }

        public static string AuthorizedParty
        {
            get => "http://relyingparty.azp.com";
        }

        public static string Azp
        {
            get => "http://Default.Azp.com";
        }

        public static string Binding
        {
            get => "http://www.w3.org/";
        }

        public static X509Certificate2 Certificate => new X509Certificate2(Convert.FromBase64String(CertificateData));

        public static string CertificateData
        {
            get => "MIIDBTCCAe2gAwIBAgIQY4RNIR0dX6dBZggnkhCRoDANBgkqhkiG9w0BAQsFADAtMSswKQYDVQQDEyJhY2NvdW50cy5hY2Nlc3Njb250cm9sLndpbmRvd3MubmV0MB4XDTE3MDIxMzAwMDAwMFoXDTE5MDIxNDAwMDAwMFowLTErMCkGA1UEAxMiYWNjb3VudHMuYWNjZXNzY29udHJvbC53aW5kb3dzLm5ldDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAMBEizU1OJms31S/ry7iav/IICYVtQ2MRPhHhYknHImtU03sgVk1Xxub4GD7R15i9UWIGbzYSGKaUtGU9lP55wrfLpDjQjEgaXi4fE6mcZBwa9qc22is23B6R67KMcVyxyDWei+IP3sKmCcMX7Ibsg+ubZUpvKGxXZ27YgqFTPqCT2znD7K81YKfy+SVg3uW6epW114yZzClTQlarptYuE2mujxjZtx7ZUlwc9AhVi8CeiLwGO1wzTmpd/uctpner6oc335rvdJikNmc1cFKCK+2irew1bgUJHuN+LJA0y5iVXKvojiKZ2Ii7QKXn19Ssg1FoJ3x2NWA06wc0CnruLsCAwEAAaMhMB8wHQYDVR0OBBYEFDAr/HCMaGqmcDJa5oualVdWAEBEMA0GCSqGSIb3DQEBCwUAA4IBAQAiUke5mA86R/X4visjceUlv5jVzCn/SIq6Gm9/wCqtSxYvifRXxwNpQTOyvHhrY/IJLRUp2g9/fDELYd65t9Dp+N8SznhfB6/Cl7P7FRo99rIlj/q7JXa8UB/vLJPDlr+NREvAkMwUs1sDhL3kSuNBoxrbLC5Jo4es+juQLXd9HcRraE4U3UZVhUS2xqjFOfaGsCbJEqqkjihssruofaxdKT1CPzPMANfREFJznNzkpJt4H0aMDgVzq69NxZ7t1JiIuc43xRjeiixQMRGMi1mAB75fTyfFJ/rWQ5J/9kh0HMZVtHsqICBF1tHMTMIK5rwoweY0cuCIpN7A/zMOQtoD";
        }

        public static List<Claim> Claims
        {
            get => ClaimSets.DefaultClaims;
        }

        public static ClaimsIdentity ClaimsIdentity
        {
            get => new ClaimsIdentity(Claims, AuthenticationType);
        }

        public static string ClaimsIdentityLabel
        {
            get => "Default.ClaimsIdentityLabel";
        }

        public static string ClaimsIdentityLabelDup
        {
            get => "Default.ClaimsIdentityLabelDup";
        }

        public static ClaimsPrincipal ClaimsPrincipal
        {
            get => new ClaimsPrincipal(ClaimsIdentity);
        }

        public static string ClientId
        {
            get => "http://Default.ClientId";
        }

        public static string Country
        {
            get => "USA";
        }

        public static string DNSAddress
        {
            get => "corp.microsoft.com";
        }

        public static string DNSName
        {
            get => "default.dns.name";
        }

        public static DateTime Expires
        {
            get => DateTime.Parse(ExpiresString);
        }

        public static string ExpiresString
        {
            get => DateTime.MaxValue.ToString("s") + "Z";
        }

        public static HashAlgorithm HashAlgorithm
        {
            get => SHA256.Create();
        }

        public static KeyInfo KeyInfo
        {
            get
            {
                var keyInfo = new KeyInfo();
                keyInfo.X509Data.Add(new X509Data(new X509Certificate2(Convert.FromBase64String(CertificateData))));
                return keyInfo;
            }
        }

        public static string IPAddress
        {
            get => "127.0.0.1";
        }

        public static DateTime IssueInstant
        {
            get => DateTime.Parse(IssueInstantString);
        }

        public static string IssueInstantString
        {
            get => "2017-03-17T18:33:37.095Z";
        }

        public static string Issuer
        {
            get => "http://Default.Issuer.com";
        }

        public static IEnumerable<string> Issuers
        {
            get => new List<string> { Guid.NewGuid().ToString(), "http://Default.Issuer.com", "http://Default.Issuer2.com", "http://Default.Issuer3.com" };
        }

#if !CrossVersionTokenValidation
        public static string Jwt(SecurityTokenDescriptor tokenDescriptor)
        {
            return (new JwtSecurityTokenHandler()).CreateEncodedJwt(tokenDescriptor);
        }
#endif

        public static string Location
        {
            get => "http://www.w3.org/";
        }

        public static string NameClaimType
        {
            get => "Default.NameClaimType";
        }

        public static string NameQualifier
        {
            get => "NameIdentifier";
        }

        public static string NameIdentifierFormat
        {
            get => "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";
        }

        public static string Nonce
        {
            get => "Default.Nonce";
        }

        public static DateTime NotBefore
        {
            get => DateTime.Parse("2017-03-17T18:33:37.080Z");
        }

        public static string NotBeforeString
        {
            get => "2017-03-17T18:33:37.080Z";
        }

        public static DateTime NotOnOrAfter
        {
            get => DateTime.Parse("2017-03-18T18:33:37.080Z");
        }

        public static string NotOnOrAfterString
        {
            get => "2017-03-18T18:33:37.080Z";
        }

        public static string OriginalIssuer
        {
            get => "http://Default.OriginalIssuer.com";
        }

        public static string OuterXml
        {
            get => "<OuterXml></OuterXml>";
        }
#if !CrossVersionTokenValidation
        public static string PayloadString
        {
            get => new JObject()
            {
                { JwtRegisteredClaimNames.Email, "Bob@contoso.com" },
                { JwtRegisteredClaimNames.GivenName, "Bob" },
                { JwtRegisteredClaimNames.Iss, Default.Issuer },
                { JwtRegisteredClaimNames.Aud, Default.Audience },
                { JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(Default.IssueInstant).ToString() },
                { JwtRegisteredClaimNames.Nbf, EpochTime.GetIntDate(Default.NotBefore).ToString()},
                { JwtRegisteredClaimNames.Exp, EpochTime.GetIntDate(Default.Expires).ToString() },
            }.ToString();
        }

        public static List<Claim> PayloadClaims
        {
            get => new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Email, "Bob@contoso.com", ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.GivenName, "Bob", ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.Iss, Default.Issuer, ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, Default.Audience, ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(Default.IssueInstant).ToString(), ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.Nbf, EpochTime.GetIntDate(Default.NotBefore).ToString(), ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.Exp, EpochTime.GetIntDate(Default.Expires).ToString(), ClaimValueTypes.String, Issuer, Issuer),
            };
        }

        public static List<Claim> PayloadClaimsExpired
        {
            get => new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Email, "Bob@contoso.com", ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.GivenName, "Bob", ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.Iss, Default.Issuer, ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, Default.Audience, ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(Default.IssueInstant).ToString(), ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.Nbf, EpochTime.GetIntDate(Default.NotBefore).ToString(), ClaimValueTypes.String, Issuer, Issuer),
                new Claim(JwtRegisteredClaimNames.Exp, EpochTime.GetIntDate(DateTime.UtcNow.Subtract(new TimeSpan(0, 10, 0))).ToString(), ClaimValueTypes.String, Issuer, Issuer),
            };
        }

        public static List<Claim> PayloadJsonClaims
        {
            get => new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Aud, Audience, ClaimValueTypes.String),
                        new Claim(JwtRegisteredClaimNames.Iss, Issuer, ClaimValueTypes.String),
                        new Claim("ClaimValueTypes.String", "ClaimValueTypes.String.Value", ClaimValueTypes.String),
                        new Claim("ClaimValueTypes.Boolean.true", "true", ClaimValueTypes.Boolean),
                        new Claim("ClaimValueTypes.Boolean.false", "false", ClaimValueTypes.Boolean),
                        new Claim("ClaimValueTypes.Double", "123.4", ClaimValueTypes.Double),
                        new Claim("ClaimValueTypes.DateTime.IS8061", "2019-11-15T14:31:21.6101326Z", ClaimValueTypes.DateTime),
                        new Claim("ClaimValueTypes.DateTime", "2019-11-15", ClaimValueTypes.DateTime),
                        new Claim("ClaimValueTypes.JsonClaimValueTypes.Json1", @"{""jsonProperty1"":""jsonvalue1""}", System.IdentityModel.Tokens.Jwt.JsonClaimValueTypes.Json),
                        new Claim("ClaimValueTypes.JsonClaimValueTypes.Json2", @"{""jsonProperty2"":""jsonvalue2""}", System.IdentityModel.Tokens.Jwt.JsonClaimValueTypes.Json),
                        new Claim("ClaimValueTypes.JsonClaimValueTypes.JsonNull", "", System.IdentityModel.Tokens.Jwt.JsonClaimValueTypes.JsonNull),
                        new Claim("ClaimValueTypes.JsonClaimValueTypes.JsonArray1", @"[1,2,3]", System.IdentityModel.Tokens.Jwt.JsonClaimValueTypes.JsonArray),
                        new Claim("ClaimValueTypes.JsonClaimValueTypes.JsonArray2", @"[1,""2"",3]", System.IdentityModel.Tokens.Jwt.JsonClaimValueTypes.JsonArray),
                        new Claim("ClaimValueTypes.JsonClaimValueTypes.Integer1", "1", ClaimValueTypes.Integer),
                        new Claim(JwtRegisteredClaimNames.Exp, EpochTime.GetIntDate(Default.Expires).ToString(), ClaimValueTypes.String, Issuer, Issuer)
                    };
        }

        public static Dictionary<string, object> PayloadJsonDictionary
        {
            get => new Dictionary<string, object>()
            {
                { JwtRegisteredClaimNames.Aud, Audience },
                { JwtRegisteredClaimNames.Iss, Issuer },
                { "ClaimValueTypes.String", "ClaimValueTypes.String.Value" },
                { "ClaimValueTypes.Boolean.true", true },
                { "ClaimValueTypes.Boolean.false", false },
                { "ClaimValueTypes.Double", 123.4 },
                { "ClaimValueTypes.DateTime.IS8061", DateTime.TryParse("2019-11-15T14:31:21.6101326Z", out DateTime dateTimeValue1) ? dateTimeValue1 : new DateTime()},
                { "ClaimValueTypes.DateTime", DateTime.TryParse("2019-11-15", out DateTime dateTimeValue2) ? dateTimeValue2 : new DateTime()},
                { "ClaimValueTypes.JsonClaimValueTypes.Json1", JObject.Parse(@"{""jsonProperty1"":""jsonvalue1""}") },
                { "ClaimValueTypes.JsonClaimValueTypes.Json2", JObject.Parse(@"{""jsonProperty2"":""jsonvalue2""}") },
                { "ClaimValueTypes.JsonClaimValueTypes.JsonNull", "" },
                { "ClaimValueTypes.JsonClaimValueTypes.JsonArray1", JArray.Parse(@"[1,2,3]") },
                { "ClaimValueTypes.JsonClaimValueTypes.JsonArray2", JArray.Parse(@"[1,""2"",3]") },
                { "ClaimValueTypes.JsonClaimValueTypes.Integer1", 1 },
                { JwtRegisteredClaimNames.Exp, EpochTime.GetIntDate(Default.Expires).ToString() }
            };
        }

        public static ClaimsIdentity PayloadClaimsIdentity
        {
            get => new ClaimsIdentity(PayloadClaims, "AuthenticationTypes.Federation");
        }

        public static Dictionary<string, object> PayloadDictionary
        {
            get => new Dictionary<string, object>()
            {
                { JwtRegisteredClaimNames.Email, "Bob@contoso.com" },
                { JwtRegisteredClaimNames.GivenName, "Bob" },
                { JwtRegisteredClaimNames.Iss, Issuer },
                { JwtRegisteredClaimNames.Aud, Audience },
                { JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(Default.IssueInstant).ToString() },
                { JwtRegisteredClaimNames.Nbf, EpochTime.GetIntDate(Default.NotBefore).ToString()},
                { JwtRegisteredClaimNames.Exp, EpochTime.GetIntDate(Default.Expires).ToString() }
            };
        }

        public static Dictionary<string, object> RemoveClaim(this Dictionary<string, object> payloadClaims, string claimName)
        {
            payloadClaims.Remove(claimName);
            return payloadClaims;
        }
#endif

#if !CrossVersionTokenValidation
        public static Reference Reference
        {
            get => new Reference(new EnvelopedSignatureTransform(), new ExclusiveCanonicalizationTransform())
            {
                DigestMethod = ReferenceDigestMethod,
                DigestValue = _referenceDigestValue,
                TokenStream = XmlUtilities.CreateXmlTokenStream(OuterXml),
                Type = ReferenceType,
                Uri = ReferenceUriWithPrefix
            };
        }

        public static Reference ReferenceWithId
        {
            get => new Reference(new EnvelopedSignatureTransform(), new ExclusiveCanonicalizationTransform())
            {
                DigestMethod = ReferenceDigestMethod,
                DigestValue = _referenceDigestValue,
                Id = ReferenceId,
                TokenStream = XmlUtilities.CreateXmlTokenStream(OuterXml),
                Type = ReferenceType,
                Uri = ReferenceUriWithPrefix
            };
        }

        public static Reference ReferenceWithoutPrefix
        {
            get => new Reference(new EnvelopedSignatureTransform(), new ExclusiveCanonicalizationTransform())
            {
                DigestMethod = ReferenceDigestMethod,
                DigestValue = _referenceDigestValue,
                TokenStream = XmlUtilities.CreateXmlTokenStream(OuterXml),
                Type = ReferenceType,
                Uri = ReferenceUriWithOutPrefix
            };
        }

        public static Reference ReferenceNS
        {
            get => new Reference(new EnvelopedSignatureTransform(), new ExclusiveCanonicalizationTransform())
            {
                DigestMethod = ReferenceDigestMethod,
                DigestValue = _referenceDigestValue,
                Prefix = "ds",
                TokenStream = XmlUtilities.CreateXmlTokenStream(OuterXml),
                Type = ReferenceType,
                Uri = ReferenceUriWithPrefix
            };
        }

        public static Reference ReferenceWithNullTokenStream
        {
            get => new Reference(new EnvelopedSignatureTransform(), new ExclusiveCanonicalizationTransform())
            {
                DigestMethod = ReferenceDigestMethod,
                DigestValue = _referenceDigestValue,
                Type = ReferenceType,
                Uri = ReferenceUriWithPrefix
            };
        }
        public static Reference ReferenceWithNullTokenStreamAndId
        {
            get => new Reference(new EnvelopedSignatureTransform(), new ExclusiveCanonicalizationTransform())
            {
                DigestMethod = ReferenceDigestMethod,
                DigestValue = _referenceDigestValue,
                Id = ReferenceId,
                Type = ReferenceType,
                Uri = ReferenceUriWithPrefix
            };
        }

        public static Reference ReferenceWithNullTokenStreamNS
        {
            get => new Reference(new EnvelopedSignatureTransform(), new ExclusiveCanonicalizationTransform())
            {
                Id = ReferenceId,
                DigestMethod = ReferenceDigestMethod,
                DigestValue = _referenceDigestValue,
                Prefix = "ds",
                Type = ReferenceType,
                Uri = ReferenceUriWithPrefix
            };
        }

        public static string ReferenceDigestMethod
        {
            get => SecurityAlgorithms.Sha256Digest;
        }

        public static string ReferenceDigestValue
        {
            get => _referenceDigestValue;
        }
#endif

        public static string ReferenceId
        {
            get => "#abcdef";
        }

        public static string ReferencePrefix
        {
            get => "ds";
        }

        public static string ReferenceType
        {
            get => "http://referenceType";
        }

        public static string ReferenceUriWithOutPrefix
        {
            get => "004C0989-1E55-4DA8-A5E6-794F7ECF0131";
        }

        public static string ReferenceUriWithPrefix
        {
            get => "#004C0989-1E55-4DA8-A5E6-794F7ECF0131";
        }

        public static string RoleClaimType
        {
            get => "Default.RoleClaimType";
        }

        public static Saml2Attribute Saml2AttributeMultiValue
        {
            get => new Saml2Attribute(AttributeName, new List<string> { Country, Country });
        }

        public static Saml2Attribute Saml2AttributeSingleValue
        {
            get => new Saml2Attribute(AttributeName, Country);
        }

        public static string SamlAccessDecision
        {
            get => "Permit";
        }

#if !CrossVersionTokenValidation
        public static SamlAction SamlAction
        {
            get => new SamlAction("Action", new Uri(SamlConstants.DefaultActionNamespace));
        }
#endif

        public static string SamlAssertionID
        {
            get => "_b95759d0-73ae-4072-a140-567ade10a7ad";
        }

        public static SamlAudienceRestrictionCondition SamlAudienceRestrictionConditionSingleAudience
        {
            get => new SamlAudienceRestrictionCondition(new Uri(Audience));
        }

        public static SamlAudienceRestrictionCondition SamlAudienceRestrictionConditionMultiAudience
        {
            get => new SamlAudienceRestrictionCondition(Audiences.ToDictionary(x => new Uri(x)).Keys);
        }

        public static SamlAttribute SamlAttributeNoValue
        {
            get => new SamlAttribute(AttributeNamespace, AttributeName, new List<string> { });
        }

        public static SamlAttribute SamlAttributeSingleValue
        {
            get => new SamlAttribute(AttributeNamespace, AttributeName, Country);
        }

        public static SamlAttribute SamlAttributeMultiValue
        {
            get => new SamlAttribute(AttributeNamespace, AttributeName, new string[] { Country, Country });
        }

        /// <summary>
        /// SamlClaims require the ability to split into name / namespace
        /// </summary>
        public static List<Claim> SamlClaims
        {
            get => new List<Claim>
            {
                new Claim(ClaimTypes.Country, "USA", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.NameIdentifier, "Bob", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.Email, "Bob@contoso.com", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.GivenName, "Bob", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.HomePhone, "555.1212", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.Role, "Developer", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.Role, "Sales", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.StreetAddress, "123AnyWhereStreet\r\nSomeTown/r/nUSA", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimsIdentity.DefaultNameClaimType, "Jean-S�bastien", ClaimValueTypes.String, Issuer, OriginalIssuer),
            };
        }

        /// <summary>
        /// SamlClaims require the ability to split into name / namespace
        /// </summary>
        public static Dictionary<string, object> SamlClaimsDictionary
        {
            get => new Dictionary<string, object>()
            {
                { ClaimTypes.Country, "USA"},
                { ClaimTypes.NameIdentifier, "Bob" },
                { ClaimTypes.Email, "Bob@contoso.com" },
                { ClaimTypes.GivenName, "Bob" },
                { ClaimTypes.HomePhone, "555.1212" },
                { ClaimTypes.Role, new List<string>{"Developer", "Sales" } },
                { ClaimTypes.StreetAddress, "123AnyWhereStreet\r\nSomeTown/r/nUSA" },
                { ClaimsIdentity.DefaultNameClaimType, "Jean-S�bastien" }
            };
        }

        /// <summary>
        /// SamlClaims require the ability to split into name / namespace
        /// </summary>
        public static List<Claim> SamlClaimsWithoutCRLF
        {
            get => new List<Claim>
            {
                new Claim(ClaimTypes.Country, "USA", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.NameIdentifier, "Bob", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.Email, "Bob@contoso.com", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.GivenName, "Bob", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.HomePhone, "555.1212", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.Role, "Developer", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.Role, "Sales", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimTypes.StreetAddress, "123AnyWhereStreet/r/nSomeTown/r/nUSA", ClaimValueTypes.String, Issuer, OriginalIssuer),
                new Claim(ClaimsIdentity.DefaultNameClaimType, "Jean-S�bastien", ClaimValueTypes.String, Issuer, OriginalIssuer),
            };
        }

        /// <summary>
        /// SamlClaims require the ability to split into name / namespace
        /// </summary>
        public static List<Claim> SamlClaimsIssuerEqOriginalIssuer
        {
            get => new List<Claim>
            {
                new Claim(ClaimTypes.Country, "USA", ClaimValueTypes.String, Issuer),
                new Claim(ClaimTypes.NameIdentifier, "Bob", ClaimValueTypes.String, Issuer),
                new Claim(ClaimTypes.Email, "Bob@contoso.com", ClaimValueTypes.String, Issuer),
                new Claim(ClaimTypes.GivenName, "Bob", ClaimValueTypes.String, Issuer),
                new Claim(ClaimTypes.HomePhone, "555.1212", ClaimValueTypes.String, Issuer),
                new Claim(ClaimTypes.Role, "Developer", ClaimValueTypes.String, Issuer),
                new Claim(ClaimTypes.Role, "Sales", ClaimValueTypes.String, Issuer),
                new Claim(ClaimTypes.StreetAddress, "123AnyWhereStreet/r/nSomeTown/r/nUSA", ClaimValueTypes.String, Issuer),
                new Claim(ClaimsIdentity.DefaultNameClaimType, "Jean-S�bastien", ClaimValueTypes.String, Issuer),
            };
        }

        public static ClaimsIdentity SamlClaimsIdentity
        {
            get => new ClaimsIdentity(SamlClaims, AuthenticationType);
        }

        public static SamlConditions SamlConditionsSingleCondition
        {
            get => new SamlConditions(NotBefore, NotOnOrAfter, new List<SamlCondition> { SamlAudienceRestrictionConditionSingleAudience });
        }

        public static SamlConditions SamlConditionsMultiCondition
        {
            get => new SamlConditions(NotBefore, NotOnOrAfter, new List<SamlCondition> { SamlAudienceRestrictionConditionMultiAudience });
        }

        public static string SamlConfirmationData
        {
            get => "ConfirmationData";
        }

        public static string SamlConfirmationMethod
        {
            get => "urn:oasis:names:tc:SAML:1.0:cm:bearer";
        }

        public static string SamlResource
        {
            get => "http://www.w3.org/";
        }

        public static SecurityTokenDescriptor SecurityTokenDescriptor()
        {
            return SecurityTokenDescriptor(SymmetricEncryptingCredentials, SymmetricSigningCredentials, ClaimSets.DefaultClaims);
        }

        public static SecurityTokenDescriptor SecurityTokenDescriptor(EncryptingCredentials encryptingCredentials)
        {
            return SecurityTokenDescriptor(encryptingCredentials, null, null);
        }

        public static SecurityTokenDescriptor SecurityTokenDescriptor(EncryptingCredentials encryptingCredentials, SigningCredentials signingCredentials, List<Claim> claims)
        {
            return new SecurityTokenDescriptor
            {
                Audience = Audience,
                EncryptingCredentials = encryptingCredentials,
                Expires = DateTime.UtcNow + TimeSpan.FromDays(1),
                Issuer = Issuer,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                SigningCredentials = signingCredentials,
                Subject = claims == null ? ClaimsIdentity : new ClaimsIdentity(claims)
            };
        }

        public static SecurityTokenDescriptor SecurityTokenDescriptor(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = Audience,
                EncryptingCredentials = null,
                Expires = DateTime.UtcNow + TimeSpan.FromDays(1),
                Issuer = claims?.FirstOrDefault(c => c.Type == "iss")?.Value ?? Issuer,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                SigningCredentials = signingCredentials,
                Subject = claims == null ? ClaimsIdentity : new ClaimsIdentity(claims),
            };

            if (securityTokenDescriptor.Claims == null)
                securityTokenDescriptor.Claims = new Dictionary<string, object>();

            foreach (Claim c in claims)
                securityTokenDescriptor.Claims.Add(c.Type, c.Value);
            return securityTokenDescriptor;
        }

        public static SecurityTokenDescriptor X509SecurityTokenDescriptor(EncryptingCredentials encryptingCredentials, X509SigningCredentials signingCredentials, List<Claim> claims)
        {
            return new SecurityTokenDescriptor
            {
                Audience = Audience,
                EncryptingCredentials = encryptingCredentials,
                Expires = DateTime.UtcNow + TimeSpan.FromDays(1),
                Issuer = Issuer,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                SigningCredentials = signingCredentials,
                Subject = claims == null ? ClaimsIdentity : new ClaimsIdentity(claims)
            };
        }

        public static SecurityTokenDescriptor SecurityTokenDescriptor(SigningCredentials signingCredentials)
        {
            return SecurityTokenDescriptor(null, signingCredentials, null);
        }

        public static SecurityTokenDescriptor X509SecurityTokenDescriptor(X509SigningCredentials signingCredentials)
        {
            return X509SecurityTokenDescriptor(null, signingCredentials, null);
        }

        public static string Session
        {
            get => "session";
        }

#if !CrossVersionTokenValidation

        public static Signature Signature
        {
            get
            {
                var signature = new Signature
                {
                    KeyInfo = KeyInfo,
                    SignedInfo = SignedInfo,
                    SignatureValue = "OaTq3jGqbPLUVROvhiqV+PneMwdu6iZgVv7vbW++wEk4tSXoqEUkY+b/M2ZzHFy0M/k33migp3s0w+Ff1vNHRI0uT8Zs1D+EdI/Oz4Pu3FwPA/UK+8qe+JTRAOhdN5H7Wv4c0p1nrWJlVlT5WWCUe2uRSpojS2+D+KC1gG/DiDqK5gWgQt/7Z0HV8ml6C0PTqXWvZcYc1u49Y3tNEPOUuSXGzSZOAfhEAMdQ6+qC+126wcbSFK5ww1aOI2K6Nk3u8sxJUXHdUXs92DKvLemcaHXw0yDNUNi/izVldy3yu6VEDEflCJkj1+yvB52U+EpvG/7IGwY66QceVbu/1FFLFA=="
                };
                return signature;
            }
        }

        public static Signature SignatureReferenceWithoutPrefix
        {
            get
            {
                var signature = new Signature
                {
                    KeyInfo = KeyInfo,
                    SignedInfo = SignedInfoReferenceWithoutPrefix,
                    SignatureValue = "OaTq3jGqbPLUVROvhiqV+PneMwdu6iZgVv7vbW++wEk4tSXoqEUkY+b/M2ZzHFy0M/k33migp3s0w+Ff1vNHRI0uT8Zs1D+EdI/Oz4Pu3FwPA/UK+8qe+JTRAOhdN5H7Wv4c0p1nrWJlVlT5WWCUe2uRSpojS2+D+KC1gG/DiDqK5gWgQt/7Z0HV8ml6C0PTqXWvZcYc1u49Y3tNEPOUuSXGzSZOAfhEAMdQ6+qC+126wcbSFK5ww1aOI2K6Nk3u8sxJUXHdUXs92DKvLemcaHXw0yDNUNi/izVldy3yu6VEDEflCJkj1+yvB52U+EpvG/7IGwY66QceVbu/1FFLFA=="
                };
                return signature;
            }
        }

        public static Signature SignatureReferenceWithId
        {
            get
            {
                var signature = new Signature
                {
                    KeyInfo = KeyInfo,
                    SignedInfo = SignedInfoReferenceWithId,
                    SignatureValue = "fqbb3WVUTLu/ihWXHUYgPWO5rgnm9AuwAT8YeiWiood/z+ObWpTwxs42be4HIDac9U94hR05rfLOR+0WxmlzhJp7/fye50VHMKex5kAAp9aCMAzCvDkfNzhMUN3WOHGEFOs4tmxrR0TBV6j+KNnjyDs3AUtdzZnZB+QmOJAlZubdOzWk/D0CGSXSgMmqYgmvH/GZGQWxQtbGMFuB29VCR7moegGN/9VAo/K7Z22xmfUWNKWVHB0OUC8FI36sadVnnUvcKnUo3M3pnQwbEWYz/+rMSYYrboM4dOKEqxZCgFXKou08Pz0MtNe2VwketLbJrKSmuEJOgVnXrzPTwlVSpw=="
                };
                return signature;
            }
        }

        public static Signature SignatureNS
        {
            get
            {
                var signature = new Signature
                {
                    KeyInfo = KeyInfo,
                    Prefix = "ds",
                    SignedInfo = SignedInfoNS,
                    SignatureValue = "biUXAYkV/sx8E7B/0POdk4J5LDkgsRLqHwZDvlJOHSDrsKuGlAlg6+oCfuV14j7uNGu/NSoOFavDSXuS9tJNAxGfeWuy3AOOeXqG+VtJY+cEJtw2WpjSs9xVc3aP58OM/x2phYOZ60Gp4h+mjjG76q7NSAoPrqaVTpw67efbB30pvPSLqTTYdXSOodcKBS25fmEFLraHvWnxAyvFCqbteIOcuOeCDL68dTcqTwVXSZIfeU3Xz8dztA7S4+DuIVuPyEFz9oV3ku8LaNfBO1Zu+v76bZMvLy2iBWhH756UILSLgEndFEOVeAb/PDzXqhwAU4NCUOeNe2WBE6nttNKmXQ==",
                };

                return signature;
            }
        }

        public static string SignatureMethod
        {
            get => SecurityAlgorithms.RsaSha256Signature;
        }

        public static SignedInfo SignedInfo
        {
            get => new SignedInfo(Reference)
            {
                CanonicalizationMethod = SecurityAlgorithms.ExclusiveC14n,
                SignatureMethod = SecurityAlgorithms.RsaSha256Signature
            };
        }

        public static SignedInfo SignedInfoReferenceWithoutPrefix
        {
            get => new SignedInfo(ReferenceWithoutPrefix)
            {
                CanonicalizationMethod = SecurityAlgorithms.ExclusiveC14n,
                SignatureMethod = SecurityAlgorithms.RsaSha256Signature
            };
        }

        public static SignedInfo SignedInfoReferenceWithId
        {
            get => new SignedInfo(ReferenceWithId)
            {
                CanonicalizationMethod = SecurityAlgorithms.ExclusiveC14n,
                SignatureMethod = SecurityAlgorithms.RsaSha256Signature
            };
        }

        public static SignedInfo SignedInfoNS
        {
            get => new SignedInfo(ReferenceWithNullTokenStreamNS)
            {
                CanonicalizationMethod = SecurityAlgorithms.ExclusiveC14n,
                Prefix = "ds",
                SignatureMethod = SecurityAlgorithms.RsaSha256Signature
            };
        }

        public static string SignatureValue
        {
            get => SignatureNS.SignatureValue;
        }
#endif

        public static string Subject
        {
            get => "urn:oasis:nams:tc:SAML:1.1:nameid-format:X509SubjectName";
        }

        public static EncryptingCredentials SymmetricEncryptingCredentials
        {
            get
            {
                return new EncryptingCredentials(
                    KeyingMaterial.DefaultSymmetricEncryptingCreds_Aes128_Sha2.Key,
                    KeyingMaterial.DefaultSymmetricEncryptingCreds_Aes128_Sha2.Alg,
                    KeyingMaterial.DefaultSymmetricEncryptingCreds_Aes128_Sha2.Enc);
            }
        }

        public static SymmetricSecurityKey SymmetricEncryptionKey128
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_128.Key)
                {
                    KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_128.KeyId
                };
            }
        }

        public static SymmetricSecurityKey SymmetricEncryptionKey128_2
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.SymmetricSecurityKey2_128.Key)
                {
                    KeyId = KeyingMaterial.SymmetricSecurityKey2_128.KeyId
                };
            }
        }

        public static SymmetricSecurityKey SymmetricEncryptionKey256
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_256.Key)
                {
                    KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_256.KeyId
                };
            }
        }

        public static SymmetricSecurityKey SymmetricEncryptionKey256_2
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.SymmetricSecurityKey2_256.Key)
                {
                    KeyId = KeyingMaterial.SymmetricSecurityKey2_256.KeyId
                };
            }
        }

        public static SymmetricSecurityKey SymmetricEncryptionKey384
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_384.Key)
                {
                    KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_384.KeyId
                };
            }
        }

        public static SymmetricSecurityKey SymmetricEncryptionKey512
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_512.Key)
                {
                    KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_512.KeyId
                };
            }
        }
        public static SymmetricSecurityKey SymmetricEncryptionKey768
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_768.Key)
                {
                    KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_768.KeyId
                };
            }
        }

        public static SymmetricSecurityKey SymmetricEncryptionKey1024
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_1024.Key)
                {
                    KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_1024.KeyId
                };
            }
        }

#if !CrossVersionTokenValidation
        public static string SymmetricJwe
        {
            get => Jwt(SecurityTokenDescriptor(KeyingMaterial.DefaultSymmetricEncryptingCreds_Aes128_Sha2));
        }

        public static string SymmetricJws
        {
            get => Jwt(SecurityTokenDescriptor(KeyingMaterial.DefaultSymmetricSigningCreds_256_Sha2));
        }

        public static string SymmetricJwsWithNoKid
        {
            get => Jwt(SecurityTokenDescriptor(KeyingMaterial.DefaultSymmetricSigningCreds_256_Sha2_NoKeyId));
        }

        public static string AsymmetricJws
        {
            get => Jwt(X509SecurityTokenDescriptor(X509AsymmetricSigningCredentials));
        }
#endif

        public static SecurityTokenDescriptor SymmetricEncryptSignSecurityTokenDescriptor()
        {
            return SecurityTokenDescriptor(SymmetricEncryptingCredentials, SymmetricSigningCredentials, ClaimSets.DefaultClaims);
        }

        public static SecurityTokenDescriptor SymmetricSignSecurityTokenDescriptor(List<Claim> claims)
        {
            return SecurityTokenDescriptor(null, SymmetricSigningCredentials, claims);
        }

        public static TokenValidationParameters SymmetricSignTokenValidationParameters
        {
            get => new TokenValidationParameters
            {
                ValidAudience = Audience,
                ValidIssuer = Issuer,
                IssuerSigningKey = SymmetricSigningKey
            };
        }

        public static SigningCredentials SymmetricSigningCredentials
        {
            get
            {
                return new SigningCredentials(
                    KeyingMaterial.DefaultSymmetricSigningCreds_256_Sha2.Key,
                    KeyingMaterial.DefaultSymmetricSigningCreds_256_Sha2.Algorithm,
                    KeyingMaterial.DefaultSymmetricSigningCreds_256_Sha2.Digest
                    );
            }
        }

        public static SecurityKey SymmetricSigningKey
        {
            get => KeyingMaterial.DefaultSymmetricSigningCreds_256_Sha2.Key;
        }

        public static SymmetricSecurityKey SymmetricSigningKey56
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_56.Key) { KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_56.KeyId };
            }
        }

        public static SymmetricSecurityKey SymmetricSigningKey64
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_64.Key) { KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_64.KeyId };
            }
        }

        public static SymmetricSecurityKey SymmetricSigningKey128
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_128.Key) { KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_128.KeyId };
            }
        }

        public static SymmetricSecurityKey SymmetricSigningKey256
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_256.Key) { KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_256.KeyId };
            }
        }

        public static SymmetricSecurityKey SymmetricSigningKey384
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_384.Key) { KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_384.KeyId };
            }
        }

        public static SymmetricSecurityKey SymmetricSigningKey512
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_512.Key) { KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_512.KeyId };
            }
        }

        public static SymmetricSecurityKey SymmetricSigningKey768
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_768.Key) { KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_768.KeyId };
            }
        }

        public static SymmetricSecurityKey SymmetricSigningKey1024
        {
            get
            {
                return new SymmetricSecurityKey(KeyingMaterial.DefaultSymmetricSecurityKey_1024.Key) { KeyId = KeyingMaterial.DefaultSymmetricSecurityKey_1024.KeyId };
            }
        }

#if !CrossVersionTokenValidation
        public static TokenValidationParameters SymmetricEncryptSignTokenValidationParameters
        {
            get => TokenValidationParameters(SymmetricEncryptionKey256, SymmetricSigningKey256);
        }

        public static TokenValidationParameters SymmetricEncryptSignInfiniteLifetimeTokenValidationParameters
        {
            get
            {
                TokenValidationParameters parameters = TokenValidationParameters(SymmetricEncryptionKey256, SymmetricSigningKey256);
                parameters.ValidateLifetime = false;
                return parameters;
            }
        }

        public static XmlTokenStream TokenStream
        {
            get => XmlUtilities.CreateXmlTokenStream(OuterXml);
        }

        public static TokenValidationParameters TokenValidationParameters(SecurityKey encryptionKey, SecurityKey signingKey)
        {
            return new TokenValidationParameters
            {
                AuthenticationType = AuthenticationType,
                TokenDecryptionKey = encryptionKey,
                IssuerSigningKey = signingKey,
                ValidAudience = Audience,
                ValidIssuer = Issuer,
                IssuerSigningKeys = new SecurityKey[] { signingKey }
            };
        }

        public static string UnsignedJwt
        {
            get => (new JwtSecurityTokenHandler()).CreateEncodedJwt(Issuer, Audience, ClaimsIdentity, null, null, null, null);
        }

        public static TokenValidationParameters JWECompressionTokenValidationParameters
        {
            get
            {
                var validationParameters = TokenValidationParameters(KeyingMaterial.DefaultX509Key_2048, KeyingMaterial.DefaultSymmetricSigningCreds_256_Sha2.Key);
                validationParameters.ValidateLifetime = false;
                return validationParameters;
            }
        }
#endif
        public static string Uri
        {
            get => "http://referenceUri";
        }

    }
}
