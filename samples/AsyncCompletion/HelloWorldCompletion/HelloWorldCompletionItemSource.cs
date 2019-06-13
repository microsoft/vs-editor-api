using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

namespace HelloWorldCompletion
{
    public class HelloWorldCompletionSource : IAsyncCompletionSource
    {
        private static ImageElement CompletionItemIcon = new ImageElement(new ImageId(new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"), 3335), "Hello Icon");
        private ImmutableArray<CompletionItem> sampleItems;

        public HelloWorldCompletionSource()
        {
            sampleItems = ImmutableArray.Create(
                new CompletionItem("Hello", this, CompletionItemIcon),
                new CompletionItem("World", this, CompletionItemIcon));
        }

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            // Since we are plugging in to CSharp content type,
            // allow the CSharp language service to pick the Applicable To Span.
            return CompletionStartData.ParticipatesInCompletionIfAny;
            // Alternatively, we've got to provide location for completion
            // return new CompletionStartData(CompletionParticipation.ProvidesItems, ...
        }

        public async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            session.Properties["LineNumber"] = triggerLocation.GetContainingLine().LineNumber;
            return new CompletionContext(sampleItems);
        }

        public async Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            var content = new ContainerElement(
                ContainerElementStyle.Wrapped,
                CompletionItemIcon,
                new ClassifiedTextElement(
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "Hello!"),
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, " This is a sample item")));
            var lineInfo = new ClassifiedTextElement(
                    new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Comment,
                        "You are on line " + ((int)(session.Properties["LineNumber"]) + 1).ToString()));
            var timeInfo = new ClassifiedTextElement(
                    new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Identifier,
                        "and it is " + DateTime.Now.ToShortTimeString()));

            return new ContainerElement(
                ContainerElementStyle.Stacked,
                content,
                lineInfo,
                timeInfo);
        }
    }
}
