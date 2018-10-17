﻿//----------------------------------------------------------------------
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
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Http;
using Microsoft.Identity.Core.OAuth2;

namespace Microsoft.Identity.Client.Internal.Requests
{
    internal class AuthorizationCodeRequest : RequestBase
    {
        public AuthorizationCodeRequest(
            IHttpManager httpManager, 
            ICryptographyManager cryptographyManager,
            AuthenticationRequestParameters authenticationRequestParameters)
            : base(httpManager, cryptographyManager, authenticationRequestParameters)
        {
            if (string.IsNullOrWhiteSpace(authenticationRequestParameters.AuthorizationCode))
            {
                throw new ArgumentNullException(nameof(authenticationRequestParameters.AuthorizationCode));
            }

            PlatformProxyFactory.GetPlatformProxy().ValidateRedirectUri(authenticationRequestParameters.RedirectUri,
                AuthenticationRequestParameters.RequestContext);
            if (!string.IsNullOrWhiteSpace(authenticationRequestParameters.RedirectUri.Fragment))
            {
                throw new ArgumentException(MsalErrorMessage.RedirectUriContainsFragment, nameof(authenticationRequestParameters.RedirectUri));
            }

            LoadFromCache = false;
        }

        protected override void SetAdditionalRequestParameters(OAuth2Client client)
        {
            client.AddBodyParameter(OAuth2Parameter.GrantType, OAuth2GrantType.AuthorizationCode);
            client.AddBodyParameter(OAuth2Parameter.Code, AuthenticationRequestParameters.AuthorizationCode);
            client.AddBodyParameter(OAuth2Parameter.RedirectUri,
                AuthenticationRequestParameters.RedirectUri.OriginalString);
        }
    }
}