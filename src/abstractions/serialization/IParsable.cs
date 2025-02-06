// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    ///     Defines a serializable model object.
    /// </summary>
    /// <remarks>In the Microsoft.Kiota.Serialization namespace, you can find extension methods for serializing this object.</remarks>
    public interface IParsable
    {
        /// <summary>
        ///   Gets the deserialization information for this object.
        /// </summary>
        /// <returns>The deserialization information for this object where each entry is a property key with its deserialization callback.</returns>
        IDictionary<string, Action<IParseNode>> GetFieldDeserializers();
        /// <summary>
        ///  Writes the objects properties to the current writer.
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter">writer</see> to write to.</param>
        void Serialize(ISerializationWriter writer);
    }
}
