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
        private readonly string? _value = value;
        /// <summary>
        /// Gets the string associated with untyped string node.
        /// </summary>
        /// <returns>The string associated with untyped string node.</returns>
        public new string? GetValue() => _value;
    }
}
