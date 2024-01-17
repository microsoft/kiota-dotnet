// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with string value.
    /// </summary>
    /// <param name="value">The string value associated with the node.</param>
    public class UntypedString(string? value) : UntypedNode
    {
        /// <summary>
        /// The string associated with untyped string node.
        /// </summary>
        public new string? Value { get; } = value;
    }
}
