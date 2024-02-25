// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Runtime.Serialization;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#endif

namespace Microsoft.Kiota.Abstractions.Extensions
{
    /// <summary>
    /// The class for extension methods for <see cref="Enum"/> types
    /// </summary>
    internal static class EnumExtensions
    {
        /// <summary>
        ///     Returns the value of the <see cref="EnumMemberAttribute"/> on an Enum field, if present.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <param name="name">The enum field name to check for <see cref="EnumMemberAttribute"/>.</param>
        /// <returns>The value of an <see cref="EnumMemberAttribute"/> for the named field, or <see langword="null" />.</returns>
#if NET5_0_OR_GREATER
        internal static string? GetEnumMemberName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(this T value, string name) where T : Enum
#else
        internal static string? GetEnumMemberName<T>(this T value, string name) where T : Enum
#endif
        {
#if NET5_0_OR_GREATER
            if(!RuntimeFeature.IsDynamicCodeSupported)
                return typeof(T).GetField(name)?.GetCustomAttribute<EnumMemberAttribute>()?.Value;
#endif

            // We can suppress ILC warning IL2075 as dynamic code is supported, so the field has not been trimmed
#pragma warning disable IL2075
            var type = value.GetType();

            if(type.GetField(name)?.GetCustomAttribute<EnumMemberAttribute>() is { } runtimeTypeAttribute)
                return runtimeTypeAttribute.Value;
#pragma warning restore IL2075

            return null;
        }
    }
}