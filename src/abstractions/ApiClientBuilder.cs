// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;

namespace Microsoft.Kiota.Abstractions
{
    /// <summary>
    ///     Provides a builder for creating an ApiClient and register the default serializers/deserializers.
    /// </summary>
    public static class ApiClientBuilder
    {
        /// <summary>
        /// Registers the default serializer to the registry.
        /// </summary>
        /// <typeparam name="T">The type of the serialization factory to register</typeparam>
        public static void RegisterDefaultSerializer<T>() where T : ISerializationWriterFactory, new()
        {
            var serializationWriterFactory = new T();
            SerializationWriterFactoryRegistry.DefaultInstance
                                            .ContentTypeAssociatedFactories
                                            .TryAdd(serializationWriterFactory.ValidContentType, serializationWriterFactory);
        }
        /// <summary>
        /// Registers the default deserializer to the registry.
        /// </summary>
        /// <typeparam name="T">The type of the parse node factory to register</typeparam>
        public static void RegisterDefaultDeserializer<T>() where T : IParseNodeFactory, new()
        {
            var deserializerFactory = new T();
            ParseNodeFactoryRegistry.DefaultInstance
                                    .ContentTypeAssociatedFactories
                                    .TryAdd(deserializerFactory.ValidContentType, deserializerFactory);
        }
        /// <summary>
        /// Enables the backing store on default serialization writers and the given serialization writer.
        /// </summary>
        /// <param name="original">The serialization writer to enable the backing store on.</param>
        /// <returns>A new serialization writer with the backing store enabled.</returns>
        public static ISerializationWriterFactory EnableBackingStoreForSerializationWriterFactory(ISerializationWriterFactory original) => original switch
        {
            null => throw new ArgumentNullException(nameof(original)),
            SerializationWriterFactoryRegistry r => EnableBackingStoreForSerializationRegistry(r),
            //We are already enabled so use it.
            BackingStoreSerializationWriterProxyFactory => original,
            _ => new BackingStoreSerializationWriterProxyFactory(original)
        };
        /// <summary>
        /// Enables the backing store on default parse nodes factories and the given parse node factory.
        /// </summary>
        /// <param name="original">The parse node factory to enable the backing store on.</param>
        /// <returns>A new parse node factory with the backing store enabled.</returns>
        public static IParseNodeFactory EnableBackingStoreForParseNodeFactory(IParseNodeFactory original) => original switch
        {
            null => throw new ArgumentNullException(nameof(original)),
            ParseNodeFactoryRegistry r => EnableBackingStoreForParseNodeRegistry(r),
            //We are already enabled so use it.
            BackingStoreParseNodeFactory => original,
            _ => new BackingStoreParseNodeFactory(original)
        };

        private static ParseNodeFactoryRegistry EnableBackingStoreForParseNodeRegistry(ParseNodeFactoryRegistry registry)
        {
            if(registry != ParseNodeFactoryRegistry.DefaultInstance)
                EnableBackingStoreForParseNodeRegistry(ParseNodeFactoryRegistry.DefaultInstance);
            var keysToUpdate = registry
                                .ContentTypeAssociatedFactories
                                .Where(static x => x.Value is not BackingStoreParseNodeFactory or ParseNodeFactoryRegistry)
                                .Select(static x => x.Key)
                                .ToArray();
            foreach(var key in keysToUpdate)
            {
                registry.ContentTypeAssociatedFactories[key] = new BackingStoreParseNodeFactory(registry.ContentTypeAssociatedFactories[key]);
            }
            return registry;
        }

        private static SerializationWriterFactoryRegistry EnableBackingStoreForSerializationRegistry(SerializationWriterFactoryRegistry registry)
        {
            if(registry != SerializationWriterFactoryRegistry.DefaultInstance)
                EnableBackingStoreForSerializationRegistry(SerializationWriterFactoryRegistry.DefaultInstance);
            var keysToUpdate = registry
                .ContentTypeAssociatedFactories
                .Where(static x => x.Value is not BackingStoreSerializationWriterProxyFactory or SerializationWriterFactoryRegistry)
                .Select(static x => x.Key)
                .ToArray();

            foreach(var key in keysToUpdate)
            {
                registry.ContentTypeAssociatedFactories[key] = new BackingStoreSerializationWriterProxyFactory(registry.ContentTypeAssociatedFactories[key]);
            }
            return registry;
        }
    }
}
