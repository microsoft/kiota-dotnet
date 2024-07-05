// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options
{
    /// <summary>
    /// The User Agent Handler Option request class
    /// </summary>
    public class UserAgentHandlerOption : IRequestOption
    {
        /// <summary>
        /// Whether to append the kiota version to the user agent header
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// The product name to append to the user agent header
        /// </summary>
        public string ProductName { get; set; } = "kiota-dotnet";
        /// <summary>
        /// The product version to append to the user agent header
        /// </summary>
        public string ProductVersion { get; set; } = Microsoft.Kiota.Http.Generated.Version.Current();
    }
}
