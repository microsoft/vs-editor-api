using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    internal class CompletionSessionTelemetry
    {
        private readonly CompletionTelemetryHost _telemetryHost;

        // Collected data
        internal string CompletionService { get; private set; }
        internal string CompletionPresenterProvider { get; private set; }
        internal string CompletionSource { get; private set; }

        internal long InitialProcessingDuration { get; private set; }
        internal long TotalProcessingDuration { get; private set; }
        internal int TotalProcessingCount { get; private set; }

        internal long InitialRenderingDuration { get; private set; }
        internal long TotalRenderingDuration { get; private set; }
        internal int TotalRenderingCount { get; private set; }

        internal long CommitDuration { get; private set; }

        internal bool UserEverScrolled { get; private set; }
        internal bool UserEverSetFilters { get; private set; }
        internal int FinalItemCount { get; private set; }
        internal int NumberOfKeystrokes { get; private set; }

        public CompletionSessionTelemetry(CompletionTelemetryHost telemetryHost, IAsyncCompletionService completionService, ICompletionPresenterProvider presenterProvider)
        {
            _telemetryHost = telemetryHost;
            CompletionService = _telemetryHost.GetCompletionServiceName(completionService);
            CompletionPresenterProvider = _telemetryHost.GetCompletionPresenterProviderName(presenterProvider);
        }

        internal void RecordProcessing(long processingTime, int itemCount)
        {
            if (TotalProcessingCount == 0)
            {
                InitialProcessingDuration = processingTime;
            }
            else
            {
                TotalProcessingDuration += processingTime;
                FinalItemCount = itemCount;
            }
            TotalProcessingCount++;
        }

        internal void RecordRendering(long processingTime)
        {
            if (TotalRenderingCount == 0)
                InitialRenderingDuration = processingTime;
            TotalRenderingCount++;
            TotalRenderingDuration += processingTime;
        }

        internal void RecordScrolling()
        {
            UserEverScrolled = true;
        }

        internal void RecordChangingFilters()
        {
            UserEverSetFilters = true;
        }

        internal void RecordKeystroke()
        {
            NumberOfKeystrokes++;
        }

        internal void RecordCommitted(long commitDuration, CompletionItem committedItem)
        {
            CompletionSource = committedItem.UseCustomCommit ? _telemetryHost.GetItemSourceName(committedItem.Source) : String.Empty;
            CommitDuration = commitDuration;
            _telemetryHost.Add(this);
        }
    }

    internal class CompletionTelemetryHost
    {
        private class AggregateSourceData
        {
            internal long TotalCommitTime;
            internal long CommitCount;
        }

        private class AggregateServiceData
        {
            internal long TotalProcessTime;
            internal long InitialProcessTime;
            internal int ProcessCount;
            internal int TotalKeystrokes;
            internal int UserEverScrolled;
            internal int UserEverSetFilters;
            internal int FinalItemCount;
            internal int DataCount;
        }

        private class AggregatePresenterData
        {
            internal long TotalRenderTime;
            internal long InitialRenderTime;
            internal int RenderCount;
        }

        Dictionary<string, AggregateSourceData> SourceData = new Dictionary<string, AggregateSourceData>(4);
        Dictionary<string, AggregateServiceData> ServiceData = new Dictionary<string, AggregateServiceData>(4);
        Dictionary<string, AggregatePresenterData> PresenterData = new Dictionary<string, AggregatePresenterData>(4);

        private readonly ILoggingServiceInternal _logger;
        private readonly AsyncCompletionBroker _broker;

        public CompletionTelemetryHost(ILoggingServiceInternal logger, AsyncCompletionBroker broker)
        {
            _logger = logger;
            _broker = broker;
        }

        internal string GetItemSourceName(IAsyncCompletionItemSource source) => _broker.GetItemSourceName(source);
        internal string GetCompletionServiceName(IAsyncCompletionService service) => _broker.GetCompletionServiceName(service);
        internal string GetCompletionPresenterProviderName(ICompletionPresenterProvider provider) => _broker.GetCompletionPresenterProviderName(provider);

        /// <summary>
        /// Adds data from <see cref="CompletionSessionTelemetry" /> to appropriate buckets.
        /// </summary>
        /// <param name=""></param>
        internal void Add(CompletionSessionTelemetry telemetry)
        {
            if (_logger == null)
                return;

            var presenterKey = telemetry.CompletionPresenterProvider;
            if (!PresenterData.ContainsKey(presenterKey))
                PresenterData[presenterKey] = new AggregatePresenterData();
            var aggregatePresenterData = PresenterData[presenterKey];

            var serviceKey = telemetry.CompletionService;
            if (!ServiceData.ContainsKey(serviceKey))
                ServiceData[serviceKey] = new AggregateServiceData();
            var aggregateServiceData = ServiceData[serviceKey];

            var sourceKey = telemetry.CompletionSource;
            if (!SourceData.ContainsKey(sourceKey))
                SourceData[sourceKey] = new AggregateSourceData();
            var aggregateSourceData = SourceData[sourceKey];

            aggregatePresenterData.InitialRenderTime += telemetry.InitialRenderingDuration;
            aggregatePresenterData.TotalRenderTime += telemetry.TotalRenderingDuration;
            aggregatePresenterData.RenderCount += telemetry.TotalRenderingCount;

            aggregateServiceData.DataCount++;
            aggregateServiceData.FinalItemCount += telemetry.FinalItemCount;
            aggregateServiceData.InitialProcessTime += telemetry.InitialProcessingDuration;
            aggregateServiceData.ProcessCount += telemetry.TotalProcessingCount;
            aggregateServiceData.TotalProcessTime += telemetry.TotalProcessingDuration;
            aggregateServiceData.TotalKeystrokes += telemetry.NumberOfKeystrokes;
            aggregateServiceData.TotalProcessTime += telemetry.TotalProcessingDuration;
            aggregateServiceData.UserEverScrolled += telemetry.UserEverScrolled ? 1 : 0;
            aggregateServiceData.UserEverSetFilters += telemetry.UserEverSetFilters ? 1 : 0;

            aggregateSourceData.TotalCommitTime += telemetry.CommitDuration;
            aggregateSourceData.CommitCount++;
        }

        /// <summary>
        /// Sends batch of collected data.
        /// </summary>
        internal void Send()
        {
            if (_logger == null)
                return;

            foreach (var data in PresenterData)
            {
                if (data.Value.RenderCount == 0)
                    continue;

                _logger.PostEvent(PresenterEventName,
                    (PresenterName, data.Key),
                    (PresenterAverageInitialRendering, data.Value.InitialRenderTime / data.Value.RenderCount),
                    (PresenterAverageRendering, data.Value.TotalRenderTime / data.Value.RenderCount)
                );
            }

            foreach (var data in ServiceData)
            {
                if (data.Value.DataCount == 0)
                    continue;

                _logger.PostEvent(ServiceEventName,
                    (ServiceName, data.Key),
                    (ServiceAverageFinalItemCount, data.Value.FinalItemCount / data.Value.DataCount),
                    (ServiceAverageInitialProcessTime, data.Value.InitialProcessTime / data.Value.DataCount),
                    (ServiceAverageFilterTime, data.Value.TotalProcessTime / data.Value.ProcessCount),
                    (ServiceAverageKeystrokeCount, data.Value.TotalKeystrokes / data.Value.DataCount),
                    (ServiceAverageScrolled, data.Value.UserEverScrolled / data.Value.DataCount),
                    (ServiceAverageSetFilters, data.Value.UserEverSetFilters / data.Value.DataCount)
                );
            }

            foreach (var data in SourceData)
            {
                if (data.Value.CommitCount == 0)
                    continue;

                _logger.PostEvent(SourceEventName,
                    (SourceName, data.Key),
                    (SourceAverageCommit, data.Value.TotalCommitTime / data.Value.CommitCount)
                );
            }
        }

        // Property and event names
        internal const string PresenterEventName = "VS/Editor/Completion/PresenterData";
        internal const string PresenterName = "Property.Rendering.Name";
        internal const string PresenterAverageInitialRendering = "Property.Rendering.InitialDuration";
        internal const string PresenterAverageRendering = "Property.Rendering.AnyDuration";

        internal const string ServiceEventName = "VS/Editor/Completion/ServiceData";
        internal const string ServiceName = "Property.Service.Name";
        internal const string ServiceAverageFinalItemCount = "Property.Service.Name";
        internal const string ServiceAverageInitialProcessTime = "Property.Service.Name";
        internal const string ServiceAverageFilterTime = "Property.Service.Name";
        internal const string ServiceAverageKeystrokeCount = "Property.Service.Name";
        internal const string ServiceAverageScrolled = "Property.Service.Name";
        internal const string ServiceAverageSetFilters = "Property.Service.Name";

        internal const string SourceEventName = "VS/Editor/Completion/SourceData";
        internal const string SourceName = "Property.Commit.Name";
        internal const string SourceAverageCommit = "Property.Commit.Duration";
    }
}
