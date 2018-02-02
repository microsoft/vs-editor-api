namespace Microsoft.VisualStudio.Text.Operations.Implementation
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Operations;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// Defines Expand Contract Selection Option.
    /// </summary>
    [Export(typeof(EditorOptionDefinition))]
    [Name(ExpandContractSelectionOptions.ExpandContractSelectionEnabledOptionId)]
    internal sealed class ExpandContractSelectionEnabled : EditorOptionDefinition<bool>
    {
        /// <summary>
        /// Gets the default value, which is <c>false</c>.
        /// </summary>
        public override bool Default => true;

        /// <summary>
        /// Gets the default text view host value.
        /// </summary>
        public override EditorOptionKey<bool> Key => ExpandContractSelectionOptions.ExpandContractSelectionEnabledKey;
    }
}
