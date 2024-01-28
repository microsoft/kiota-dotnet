// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with decimal value.
    /// </summary>
    /// <param name="value">The decimal value associated with the node.</param>
    public class UntypedDecimal(decimal value) : UntypedNode
    {
        /// <summary>
        /// The value associated with untyped decimal node.
        /// </summary>
        public new decimal Value { get; } = value;
    }
}
