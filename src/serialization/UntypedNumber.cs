// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with number value.
    /// </summary>
    /// <param name="value">The number value associated with the node.</param>
    public class UntypedNumber(string value) : UntypedNode
    {
        /// <summary>
        /// The number associated with untyped number node.
        /// </summary>
        public new string Value { get; } = value;
    }
}
