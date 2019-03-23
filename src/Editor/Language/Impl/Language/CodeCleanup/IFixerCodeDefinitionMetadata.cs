using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.Language.CodeCleanUp
{
    /// <summary>
    /// Represents MEF metadata view corresponding to the <see cref="FixIdDefinition"/>s.
    /// </summary>
    internal interface IFixIdDefinitionMetadata
    {
        /// <summary>
        /// Fixer Id for example "IDE001"
        /// </summary>
        string FixId { get; }

        /// <summary>
        /// Key for use in the .editorconfig file
        /// </summary>
        string ConfigurationKey { get; }

        /// <summary>
        /// Optional help link to provide more information about the fixer code
        /// </summary>
        [DefaultValue(null)]
        string HelpLink { get; }

        /// <summary>
        /// Localized display name
        /// </summary>
        string LocalizedName { get; }
    }
}
