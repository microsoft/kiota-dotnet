// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        public static ISerializationWriterFactory EnableBackingStoreForSerializationWriterFactory(ISerializationWriterFactory original)
        {
            ISerializationWriterFactory result = original ?? throw new ArgumentNullException(nameof(original));
            if(original is SerializationWriterFactoryRegistry registry)
            {
                EnableBackingStoreForSerializationRegistry(registry);
                if(registry != SerializationWriterFactoryRegistry.DefaultInstance)// if the registry is the default instance, we already enabled it above. No need to do it twice
                    EnableBackingStoreForSerializationRegistry(SerializationWriterFactoryRegistry.DefaultInstance);
            }
            if(result is BackingStoreSerializationWriterProxyFactory)
                //We are already enabled so use it.
                return result;
            else
                result = new BackingStoreSerializationWriterProxyFactory(original);

            return result;
        }
        /// <summary>
        /// Enables the backing store on default parse nodes factories and the given parse node factory.
        /// </summary>
        /// <param name="original">The parse node factory to enable the backing store on.</param>
        /// <returns>A new parse node factory with the backing store enabled.</returns>
        public static IParseNodeFactory EnableBackingStoreForParseNodeFactory(IParseNodeFactory original)
        {
            var result = original ?? throw new ArgumentNullException(nameof(original));
            if(original is ParseNodeFactoryRegistry registry)
            {
                EnableBackingStoreForParseNodeRegistry(registry);
                if(registry != ParseNodeFactoryRegistry.DefaultInstance)// if the registry is the default instance, we already enabled it above. No need to do it twice
                    EnableBackingStoreForParseNodeRegistry(ParseNodeFactoryRegistry.DefaultInstance);
            }
            if(result is BackingStoreParseNodeFactory)
                //We are already enabled so use it.
                return result;
            else
                result = new BackingStoreParseNodeFactory(original);

            return result;
        }

        private static void EnableBackingStoreForParseNodeRegistry(ParseNodeFactoryRegistry registry)
        {
            var keysToUpdate = new List<string>();
            foreach(var entry in registry.ContentTypeAssociatedFactories)
            {
                if(entry.Value is not BackingStoreParseNodeFactory)
                {
                    keysToUpdate.Add(entry.Key);
                }
            }

            foreach(var key in keysToUpdate)
            {
                registry.ContentTypeAssociatedFactories[key] = new BackingStoreParseNodeFactory(registry.ContentTypeAssociatedFactories[key]);
            }
        }

        private static void EnableBackingStoreForSerializationRegistry(SerializationWriterFactoryRegistry registry)
        {
            var keysToUpdate = new List<string>();
            foreach(var entry in registry.ContentTypeAssociatedFactories)
            {
                if(entry.Value is not BackingStoreSerializationWriterProxyFactory)
                {
                    keysToUpdate.Add(entry.Key);
                }
            }

            foreach(var key in keysToUpdate)
            {
                registry.ContentTypeAssociatedFactories[key] = new BackingStoreSerializationWriterProxyFactory(registry.ContentTypeAssociatedFactories[key]);
            }
        }
    }
}
