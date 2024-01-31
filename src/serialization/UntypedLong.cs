// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with long value.
    /// </summary>
    /// <param name="value">The long value associated with the node.</param>
    public class UntypedLong(long value) : UntypedNode
    {
        /// <summary>
        /// The value associated with untyped long node.
        /// </summary>
        private readonly long _value = value;
        /// <summary>
        /// Gets the value associated with untyped long node.
        /// </summary>
        /// <returns>The value associated with untyped long node.</returns>
        public new long GetValue() => _value;
    }
}
