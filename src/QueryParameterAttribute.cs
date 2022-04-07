// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------
using System;

namespace Microsoft.Kiota.Abstractions;

/// <summary>
/// This attribute allows mapping between the query parameter name in the template and the property name in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class QueryParameterAttribute : Attribute
{
    private readonly string templateName;
    
    ///<summary>
    /// Creates a new instance of the attribute
    ///</summary>
    ///<param name="templateName">The name of the parameter in the template.</param>
    public QueryParameterAttribute(string templateName)
    {
        if(string.IsNullOrEmpty(templateName)) throw new ArgumentNullException(nameof(templateName));
        this.templateName = templateName;
    }
    
    ///<summary>
    /// The name of the parameter in the template.
    ///</summary>
    public string TemplateName
    {
        get { return templateName; }
    }
}