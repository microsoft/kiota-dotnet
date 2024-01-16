// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

#if NET5_0_OR_GREATER
#endif

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with boolean value.
    /// </summary>
    /// <param name="value">The boolean value associated with the node.</param>
    public class UntypedBoolean(bool value) : UntypedNode
    {
        /// <summary>
        /// 
        /// </summary>
        public new bool Value { get; } = value;
    }
}
