﻿// ------------------------------------------------------------------------------
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
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client.ApiConfig;
using Microsoft.Identity.Client.AppConfig;
using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Exceptions;
using Microsoft.Identity.Client.Internal.Requests;
using Microsoft.Identity.Client.TelemetryCore;
using Microsoft.Identity.Client.Utils;

namespace Microsoft.Identity.Client
{
    public abstract partial class ClientApplicationBase : IClientApplicationBaseExecutor
    {
        internal ClientApplicationBase(ApplicationConfiguration config)
        {
            ServiceBundle = Core.ServiceBundle.Create(config);

            if (config.UserTokenLegacyCachePersistenceForTest != null)
            {
                UserTokenCacheInternal = new TokenCache(ServiceBundle, config.UserTokenLegacyCachePersistenceForTest);
            }
            else
            {
                UserTokenCacheInternal = new TokenCache(ServiceBundle);
            }

            CreateRequestContext().Logger.Info(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "MSAL {0} with assembly version '{1}', file version '{2}' and informational version '{3}' is running...",
                    ServiceBundle.PlatformProxy.GetProductName(),
                    MsalIdHelper.GetMsalVersion(),
                    AssemblyUtils.GetAssemblyFileVersionAttribute(),
                    AssemblyUtils.GetAssemblyInformationalVersion()));
        }

        async Task<AuthenticationResult> IClientApplicationBaseExecutor.ExecuteAsync(
            IAcquireTokenSilentParameters silentParameters,
            CancellationToken cancellationToken)
        {
            var authorityInstance = string.IsNullOrWhiteSpace(silentParameters.AuthorityOverride)
                                        ? GetAuthority(silentParameters.Account)
                                        : Instance.Authority.CreateAuthority(ServiceBundle, silentParameters.AuthorityOverride);

            ApiEvent.ApiIds apiId = string.IsNullOrWhiteSpace(silentParameters.AuthorityOverride)
                                        ? ApiEvent.ApiIds.AcquireTokenSilentWithoutAuthority
                                        : ApiEvent.ApiIds.AcquireTokenSilentWithAuthority;

            var handler = new SilentRequest(
                ServiceBundle,
                CreateRequestParameters(silentParameters, UserTokenCacheInternal, authorityInstance),
                apiId,
                silentParameters.ForceRefresh);

            return await handler.RunAsync(cancellationToken).ConfigureAwait(false);
        }

        async Task<AuthenticationResult> IClientApplicationBaseExecutor.ExecuteAsync(
            IAcquireTokenByRefreshTokenParameters byRefreshTokenParameters,
            CancellationToken cancellationToken)
        {
            var context = CreateRequestContext();
            SortedSet<string> scopes;

            if (byRefreshTokenParameters.Scopes == null || !byRefreshTokenParameters.Scopes.Any())
            {
                scopes = new SortedSet<string>
                {
                    ClientId + "/.default"
                };
                context.Logger.Info(LogMessages.NoScopesProvidedForRefreshTokenRequest);
            }
            else
            {
                scopes = ScopeHelper.CreateSortedSetFromEnumerable(byRefreshTokenParameters.Scopes);
                context.Logger.Info(LogMessages.UsingXScopesForRefreshTokenRequest(scopes.Count()));
            }

            var reqParams = CreateRequestParameters(byRefreshTokenParameters, UserTokenCacheInternal);
            reqParams.IsRefreshTokenRequest = true;
            reqParams.Scope = scopes;

            var handler = new ByRefreshTokenRequest(
                ServiceBundle,
                reqParams,
                ApiEvent.ApiIds.AcquireTokenByRefreshToken,
                byRefreshTokenParameters.RefreshToken);

            return await handler.RunAsync(CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to acquire an access token for the <paramref name="account"/> from the user token cache, 
        /// with advanced parameters controlling the network call. See https://aka.ms/msal-net-acquiretokensilent for more details
        /// </summary>
        /// <param name="scopes">Scopes requested to access a protected API</param>
        /// <param name="account">Account for which the token is requested. <see cref="IAccount"/></param>
        /// <returns>An <see cref="AcquireTokenSilentParameterBuilder"/> used to build the token request, adding optional
        /// parameters</returns>
        /// <exception cref="MsalUiRequiredException">will be thrown in the case where an interaction is required with the end user of the application,
        /// for instance, if no refresh token was in the cache,a or the user needs to consent, or re-sign-in (for instance if the password expired),
        /// or the user needs to perform two factor authentication</exception>
        /// <remarks>
        /// The access token is considered a match if it contains <b>at least</b> all the requested scopes. This means that an access token with more scopes than
        /// requested could be returned as well. If the access token is expired or close to expiration (within a 5 minute window),
        /// then the cached refresh token (if available) is used to acquire a new access token by making a silent network call.
        ///
        /// See also the additional parameters that you can set chain:
        /// <see cref="AbstractAcquireTokenParameterBuilder{T}.WithAuthorityOverride(string)"/> or one of its
        /// overrides to request a token for a different authority than the one set at the application construction
        /// <see cref="AcquireTokenSilentParameterBuilder.WithForceRefresh(bool)"/> to bypass the user token cache and
        /// force refreshing the token, as well as
        /// <see cref="AbstractAcquireTokenParameterBuilder{T}.WithExtraQueryParameters(Dictionary{string, string})"/> to
        /// specify extra query parameters
        /// </remarks>
        public AcquireTokenSilentParameterBuilder AcquireTokenSilent(IEnumerable<string> scopes, IAccount account)
        {
            return AcquireTokenSilentParameterBuilder.Create(this, scopes, account);
        }
    }
}