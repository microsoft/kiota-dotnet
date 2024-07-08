// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Base class for untyped node.
    /// </summary>
    public class UntypedNode : IParsable
    {
        private static readonly IDictionary<string, Action<IParseNode>> _fieldDeserializers = new ReadOnlyDictionary<string, Action<IParseNode>>(new Dictionary<string, Action<IParseNode>>());
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => _fieldDeserializers;
        /// <summary>
        /// Serializes information the current object
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        /// </summary>
        public virtual void Serialize(ISerializationWriter writer) => _ = writer ?? throw new ArgumentNullException(nameof(writer));
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static UntypedNode CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new();
        }
        /// <summary>
        /// Gets the value assigned to untyped node.
        /// </summary>
        /// <returns>The value assigned to untyped node.</returns>
        public object? GetValue() => throw new NotImplementedException();
    }
}
