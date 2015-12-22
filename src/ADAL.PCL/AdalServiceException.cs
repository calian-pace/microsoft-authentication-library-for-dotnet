﻿//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// The exception type thrown when user returned by service does not match user in the request.
    /// </summary>
    public class AdalServiceException : AdalException
    {
        /// <summary>
        ///  Initializes a new instance of the exception class with a specified
        ///  error code and error message.
        /// </summary>
        /// <param name="errorCode">The protocol error code returned by the service or generated by client. This is the code you can rely on for exception handling.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public AdalServiceException(string errorCode, string message)
            : base(errorCode, message)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the exception class with a specified
        ///  error code and a reference to the inner exception that is the cause of
        ///  this exception.
        /// </summary>
        /// <param name="errorCode">The protocol error code returned by the service or generated by client. This is the code you can rely on for exception handling.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified. It may especially contain the actual error message returned by the service.</param>
        internal AdalServiceException(string errorCode, Exception innerException)
            : this(errorCode, GetErrorMessage(errorCode), null, innerException)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the exception class with a specified
        ///  error code, error message and a reference to the inner exception that is the cause of
        ///  this exception.
        /// </summary>
        /// <param name="errorCode">The protocol error code returned by the service or generated by client. This is the code you can rely on for exception handling.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="serviceErrorCodes">The specific error codes that may be returned by the service.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified. It may especially contain the actual error message returned by the service.</param>
        internal AdalServiceException(string errorCode, string message, string[] serviceErrorCodes, Exception innerException)
            : base(errorCode, message, (innerException is HttpRequestWrapperException) ? innerException.InnerException : innerException)
        {
            var httpRequestWrapperException = (innerException as HttpRequestWrapperException);
            if (httpRequestWrapperException != null)
            {
                IHttpWebResponse response = httpRequestWrapperException.WebResponse;
                if (response != null)
                {
                    this.StatusCode = (int)response.StatusCode;
                }
                else if (innerException.InnerException is TaskCanceledException)
                {
                    var taskCanceledException = ((TaskCanceledException)(innerException.InnerException));
                    if (!taskCanceledException.CancellationToken.IsCancellationRequested)
                    {
                        this.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    }
                    else
                    {
                        // There is no HttpStatusCode for user cancelation
                        this.StatusCode = 0;
                    }
                }
                else
                {
                    this.StatusCode = 0;
                }
            }

            this.ServiceErrorCodes = serviceErrorCodes;
        }

        /// <summary>
        /// Gets the status code returned from http layer. This status code is either the HttpStatusCode in the inner HttpRequestException response or
        /// NavigateError Event Status Code in browser based flow (See http://msdn.microsoft.com/en-us/library/bb268233(v=vs.85).aspx).
        /// You can use this code for purposes such as implementing retry logic or error investigation.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets the specific error codes that may be returned by the service.
        /// </summary>
        public string[] ServiceErrorCodes { get; set; }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            return base.ToString() + string.Format("\n\tStatusCode: {0}", this.StatusCode);
        }

    }
}
