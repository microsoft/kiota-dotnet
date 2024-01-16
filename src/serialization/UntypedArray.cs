// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
#if NET5_0_OR_GREATER
#endif

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with the collection of untyped values.
    /// </summary>
    /// <param name="value">The collection of child nodes.</param>
    public class UntypedArray(IEnumerable<UntypedNode>? value) : UntypedNode
    {
        /// <summary>
        /// The collection of untyped child nodes.
        /// </summary>
        public new IEnumerable<UntypedNode>? Value { get; } = value;
    }
}
