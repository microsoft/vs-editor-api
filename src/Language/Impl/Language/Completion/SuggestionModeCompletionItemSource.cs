using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Internal item source used during lifetime of the suggestion mode item.
    /// </summary>
    internal class SuggestionModeCompletionItemSource : IAsyncCompletionItemSource
    {
        static IAsyncCompletionItemSource _instance;
        internal static IAsyncCompletionItemSource Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SuggestionModeCompletionItemSource();
                return _instance;
            }
        }

        void IAsyncCompletionItemSource.CustomCommit(ITextView view, ITextBuffer buffer, CompletionItem item, ITrackingSpan applicableSpan, char typeChar, CancellationToken token)
        {
            throw new NotImplementedException("Suggestion mode item does not have custom commit behavior");
        }

        Task<CompletionContext> IAsyncCompletionItemSource.GetCompletionContextAsync(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            throw new NotImplementedException("This item source is not meant to be registered. It is used only to provide tooltip.");
        }

        Task<object> IAsyncCompletionItemSource.GetDescriptionAsync(CompletionItem item, CancellationToken token)
        {
            return Task.FromResult<object>(string.Empty);
        }

        ImmutableArray<char> IAsyncCompletionItemSource.GetPotentialCommitCharacters()
        {
            throw new NotImplementedException("This item source is not meant to be registered. It is used only to provide tooltip.");
        }

        Task IAsyncCompletionItemSource.HandleViewClosedAsync(ITextView view)
        {
            throw new NotImplementedException("This item source is not meant to be registered. It is used only to provide tooltip.");
        }

        bool IAsyncCompletionItemSource.ShouldCommitCompletion(char typeChar, SnapshotPoint location)
        {
            return false; // Typing should not commit the suggestion mode item.
        }

        bool IAsyncCompletionItemSource.ShouldTriggerCompletion(char typeChar, SnapshotPoint location)
        {
            throw new NotImplementedException("This item source is not meant to be registered. It is used only to provide tooltip.");
        }
    }
}
