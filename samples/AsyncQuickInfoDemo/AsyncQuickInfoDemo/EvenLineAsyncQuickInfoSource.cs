namespace AsyncQuickInfoDemo
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Core.Imaging;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Language.StandardClassification;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Adornments;

    internal sealed class EvenLineAsyncQuickInfoSource : IAsyncQuickInfoSource
    {
        // Copied from KnownMonikers, because Mono doesn't support ImageMoniker type.
        private static readonly ImageId AssemblyWarningImageId = new ImageId(
            new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"),
            200);

        private ITextBuffer textBuffer;

        public EvenLineAsyncQuickInfoSource(ITextBuffer textBuffer)
        {
            this.textBuffer = textBuffer;
        }

        public void Dispose()
        {
            // This provider does not perform any cleanup.
        }

        public Task<QuickInfoItem> GetQuickInfoItemAsync(
            IAsyncQuickInfoSession session,
            CancellationToken cancellationToken)
        {
            var triggerPoint = session.GetTriggerPoint(this.textBuffer.CurrentSnapshot);
            if (triggerPoint != null)
            {
                var line = triggerPoint.Value.GetContainingLine();
                var lineNumber = triggerPoint.Value.GetContainingLine().LineNumber;
                var lineSpan = this.textBuffer.CurrentSnapshot.CreateTrackingSpan(
                    line.Extent,
                    SpanTrackingMode.EdgeInclusive);

                object content = null;

                // Check if this is an even line.
                if ((lineNumber % 2) == 1)
                {
                    content = new ContainerElement(
                        ContainerElementStyle.Wrapped,
                        new ImageElement(AssemblyWarningImageId),
                        new ClassifiedTextElement(
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "Even Or Odd: "),
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, "Even")));
                }
                else
                {
                    content = new ContainerElement(
                        ContainerElementStyle.Wrapped,
                        new ImageElement(AssemblyWarningImageId),
                        new ClassifiedTextElement(
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "Even Or Odd: "),
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, "Odd")));
                }

                var contentContainer = new ContainerElement(
                    ContainerElementStyle.Stacked,
                    content,
                    new ClassifiedTextElement(
                        new ClassifiedTextRun(
                            PredefinedClassificationTypeNames.Identifier,
                            "The current date and time is: " + DateTime.Now.ToString())));

                return Task.FromResult(
                    new QuickInfoItem(
                        lineSpan,
                        contentContainer));
            }

            return Task.FromResult<QuickInfoItem>(null);
        }
    }
}