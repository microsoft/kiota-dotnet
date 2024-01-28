// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with double value.
    /// </summary>
    /// <param name="value">The double value associated with the node.</param>
    public class UntypedDouble(double value) : UntypedNode
    {
        /// <summary>
        /// The value associated with untyped double node.
        /// </summary>
        public new double Value { get; } = value;
    }
}
