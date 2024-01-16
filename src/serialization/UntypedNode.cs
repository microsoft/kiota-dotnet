// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

#if NET5_0_OR_GREATER
#endif

using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions.Store;

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Base class for untyped node.
    /// </summary>
    public abstract class UntypedNode : IParsable, IAdditionalDataHolder, IBackedModel
    {
        /// <summary>
        /// The value assigned to untyped node.
        /// </summary>
        public object? Value => null;
        /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
        public IDictionary<string, object> AdditionalData
        {
            get { return BackingStore?.Get<IDictionary<string, object>>("additionalData")!; }
            set { BackingStore?.Set("additionalData", value); }
        }
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>
        /// Instantiates a new entity and sets the default values.
        /// </summary>
        public UntypedNode()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
            AdditionalData = new Dictionary<string, object>();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>();
        }
        /// <summary>
        /// Serializes information the current object
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        /// </summary>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}
