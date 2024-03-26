// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with the collection of untyped values.
    /// </summary>
    /// <param name="value">The collection of child nodes.</param>
    public class UntypedArray(IEnumerable<UntypedNode> value) : UntypedNode
    {
        private readonly IEnumerable<UntypedNode> _value = value;
        /// <summary>
        /// Gets the collection of untyped child nodes.
        /// </summary>
        /// <returns>The collection of untyped child nodes.</returns>
        public new IEnumerable<UntypedNode> GetValue() => _value;
    }
}
