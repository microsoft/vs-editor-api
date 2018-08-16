using System.ComponentModel.Composition;

namespace Microsoft.VisualStudio.Utilities.Features.Implementation
{
    /// <summary>
    /// Contains exports for <see cref="FeatureDefinition"/>s shared in <see cref="PredefinedEditorFeatureNames"/>
    /// </summary>
    internal class StandardEditorFeatureDefinitions
    {
        [Export]
        [Name(PredefinedEditorFeatureNames.Editor)]
        public FeatureDefinition EditorDefinition;

        [Export]
        [Name(PredefinedEditorFeatureNames.Popup)]
        public FeatureDefinition PopupDefinition;

        [Export]
        [Name(PredefinedEditorFeatureNames.InteractivePopup)]
        [BaseDefinition(PredefinedEditorFeatureNames.Popup)]
        public FeatureDefinition InteractivePopupDefinition;

        [Export]
        [Name(PredefinedEditorFeatureNames.Completion)]
        [BaseDefinition(PredefinedEditorFeatureNames.InteractivePopup)]
        [BaseDefinition(PredefinedEditorFeatureNames.Editor)]
        public FeatureDefinition CompletionDefinition;
    }
}
