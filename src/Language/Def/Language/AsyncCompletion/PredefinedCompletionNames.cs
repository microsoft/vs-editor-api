using Microsoft.VisualStudio.Commanding;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion
{
    /// <summary>
    /// Provides names used by the Async Completion feature.
    /// </summary>
    public static class PredefinedCompletionNames
    {
        /// <summary>
        /// Name of the default <see cref="IAsyncCompletionItemManagerProvider"/>. Use to order your MEF part.
        /// </summary>
        public const string DefaultCompletionItemManager = "DefaultCompletionItemManager";

        /// <summary>
        /// Name of the default <see cref="ICompletionPresenterProvider"/>. Use to order your MEF part.
        /// </summary>
        public const string DefaultCompletionPresenter = "DefaultCompletionPresenter";

        /// <summary>
        /// Name of the completion's <see cref="ICommandHandler"/>. Use to order your MEF part.
        /// </summary>
        public const string CompletionCommandHandler = "CompletionCommandHandler";

        /// <summary>
        /// Name of the editor option that stores user's preference for the completion mode.
        /// </summary>
        public const string SuggestionModeInCompletionOptionName = "SuggestionModeInCompletion";

        /// <summary>
        /// Name of the editor option that stores user's preference for the completion mode during debugging.
        /// </summary>
        public const string SuggestionModeInDebuggerCompletionOptionName = "SuggestionModeInCompletionDuringDebugging";
    }
}
