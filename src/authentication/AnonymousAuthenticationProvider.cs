// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Abstractions.Authentication
{
    /// <summary>
    /// This authentication provider does not perform any authentication.
    /// </summary>
    public class AnonymousAuthenticationProvider : IAuthenticationProvider
    {
        /// <inheritdoc />
        public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object> additionalAuthenticationContext = default, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
