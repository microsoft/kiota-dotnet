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
        private readonly double _value = value;
        /// <summary>
        /// Gets the value associated with untyped double node.
        /// </summary>
        /// <returns>The value associated with untyped double node.</returns>
        public override object GetValue() => _value;
    }
}
