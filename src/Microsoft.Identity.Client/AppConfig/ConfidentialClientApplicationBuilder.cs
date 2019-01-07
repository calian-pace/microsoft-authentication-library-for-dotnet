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

using System;

namespace Microsoft.Identity.Client.AppConfig
{
#if !ANDROID_BUILDTIME && !iOS_BUILDTIME && !WINDOWS_APP_BUILDTIME // Hide confidential client on mobile platforms

    /// <summary>
    /// </summary>
    public class ConfidentialClientApplicationBuilder : AbstractApplicationBuilder<ConfidentialClientApplicationBuilder>
    {
        /// <inheritdoc />
        internal ConfidentialClientApplicationBuilder(ApplicationConfiguration configuration)
            : base(configuration)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static ConfidentialClientApplicationBuilder CreateWithApplicationOptions(
            ConfidentialClientApplicationOptions options)
        {
            var config = new ApplicationConfiguration();
            return new ConfidentialClientApplicationBuilder(config)
                   .WithOptions(options).WithClientCredential(new ClientCredential(options.ClientCredentialSecret));
        }

        /// <summary>
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static ConfidentialClientApplicationBuilder Create(string clientId)
        {
            var config = new ApplicationConfiguration();
            return new ConfidentialClientApplicationBuilder(config).WithClientId(clientId);
        }

        /// <summary>
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="authority"></param>
        /// <param name="validateAuthority"></param>
        /// <returns></returns>
        public static ConfidentialClientApplicationBuilder Create(
            string clientId,
            string authority,
            bool validateAuthority = true)
        {
            return Create(clientId).WithAuthority(authority, validateAuthority, true);
        }

        /// <summary>
        /// </summary>
        /// <param name="tokenCache"></param>
        /// <returns></returns>
        public ConfidentialClientApplicationBuilder WithAppTokenCache(ITokenCache tokenCache)
        {
            Config.AppTokenCache = tokenCache;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="tokenCache"></param>
        /// <returns></returns>
        public ConfidentialClientApplicationBuilder WithUserTokenCache(ITokenCache tokenCache)
        {
            Config.UserTokenCache = tokenCache;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="clientCredential"></param>
        /// <returns></returns>
        public ConfidentialClientApplicationBuilder WithClientCredential(ClientCredential clientCredential)
        {
            Config.ClientCredential = clientCredential;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public IConfidentialClientApplication Build()
        {
            return BuildConcrete();
        }

        /// <summary>
        ///     TODO: this should be internal, but tests are still using this publicly, some of which are not internalsvisible.
        /// </summary>
        /// <returns></returns>
        public ConfidentialClientApplication BuildConcrete()
        {
            throw new NotImplementedException();
            // return new ConfidentialClientApplication(BuildConfiguration());
        }
    }
#endif
}