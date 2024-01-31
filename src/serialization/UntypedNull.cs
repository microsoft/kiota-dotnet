// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node without the value.
    /// </summary>
    public class UntypedNull : UntypedNode
    {
        /// <summary>
        /// Gets the value associated with untyped null node.
        /// </summary>
        /// <returns>The value associated with untyped null node.</returns>
        public override object? GetValue() => null;
    }
}
