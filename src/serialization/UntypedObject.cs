// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
#if NET5_0_OR_GREATER
#endif

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with object value.
    /// </summary>
    /// <param name="properties">Properties associated with the node.</param>
    public class UntypedObject(IDictionary<string, UntypedNode>? properties) : UntypedNode
    {
        /// <summary>
        /// Properties associated with untyped object node.
        /// </summary>
        public IDictionary<string, UntypedNode>? Properties { get; } = properties;
    }
}
