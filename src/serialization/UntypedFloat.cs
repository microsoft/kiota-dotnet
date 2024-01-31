// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with float value.
    /// </summary>
    /// <param name="value">The float value associated with the node.</param>
    public class UntypedFloat(float value) : UntypedNode
    {
        private readonly float _value = value;
        /// <summary>
        /// Gets the value associated with untyped float node.
        /// </summary>
        /// <returns>The value associated with untyped float node.</returns>
        public override object GetValue() => _value;
    }
}
