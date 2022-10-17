// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Kiota.Abstractions
{
    /// <summary>
    ///     Defines the <see cref="IRequestOption"/> for holding a <see cref="IResponseHandler"/>
    /// </summary>
    public class ResponseHandlerOption : IRequestOption
    {
        /// <summary>
        /// The <see cref="IResponseHandler"/> to use for a request
        /// </summary>
        public IResponseHandler ResponseHandler { get; set; }
    }
}
