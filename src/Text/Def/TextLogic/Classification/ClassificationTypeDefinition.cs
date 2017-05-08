// Copyright (C) Microsoft Corporation.  All Rights Reserved.

namespace Microsoft.VisualStudio.Text.Classification
{
    /// <summary>
    /// Describes a data-only export for declaring classification types.
    /// </summary>
    /// <remarks> 
    /// Because you cannot subclass this type, you can use the [Export] attribute with no type.
    /// </remarks>
    /// <example>
    /// internal sealed class Components
    /// {
    ///    [Export]
    ///    [Name("keyword")]            // required
    ///    [BaseDefinition("text")]     // zero or more BaseDefinitions are allowed
    ///    internal ClassificationTypeDefinition keywordDefinition;
    ///    
    ///    { other components }
    /// }
    /// </example>
    public sealed class ClassificationTypeDefinition
    {
    }
}
