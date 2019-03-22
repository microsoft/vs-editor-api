using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.VisualStudio.Utilities.Features.Implementation
{
    /// <summary>
    /// Describes metadata required of <see cref="FeatureDefinition"/> imports.
    /// </summary>
    public interface IFeatureDefinitionMetadata
    {
        /// <summary>
        /// Name of the feature
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Optionally, a collection of names of parent features
        /// </summary>
        [System.ComponentModel.DefaultValue(null)]
        IEnumerable<string> BaseDefinition { get; }
    }
}
