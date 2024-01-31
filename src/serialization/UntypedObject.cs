// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with object value.
    /// </summary>
    /// <param name="properties">Properties associated with the node.</param>
    public class UntypedObject(IDictionary<string, UntypedNode> properties) : UntypedNode
    {
        private readonly IDictionary<string, UntypedNode> _properties = properties;
        /// <summary>
        /// Gets properties associated with untyped object node.
        /// </summary>
        /// <returns>Properties associated with untyped object node.</returns>
        public new IDictionary<string, UntypedNode> GetValue() => _properties;
    }
}
