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
        /// <summary>
        /// The value associated with untyped float node.
        /// </summary>
        public new float Value { get; } = value;
    }
}
