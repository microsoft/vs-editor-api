using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Implementation
{
    /// <summary>
    /// Telemetry data pertinent to a single <see cref="AsyncCompletionSession"/>
    /// </summary>
    internal class CompletionSessionTelemetry
    {
        private readonly CompletionTelemetryHost _telemetryHost;

        /// <summary>
        /// Tracks time spent on the worker thread - getting data, filtering and sorting. Used for telemetry.
        /// </summary>
        internal Stopwatch ComputationStopwatch { get; } = new Stopwatch();

        /// <summary>
        /// Tracks time spent on the UI thread - either rendering or committing. Used for telemetry.
        /// </summary>
        internal Stopwatch UiStopwatch { get; } = new Stopwatch();

        // Names of parts that participated in completion
        internal string ItemManagerName { get; private set; }
        internal string PresenterProviderName { get; private set; }
        internal string CommitManagerName { get; private set; }

        // "Setup" is work done on UI thread by IAsyncCompletionBroker
        // since there are many participating MEF parts, we record their names together with the time
        internal Dictionary<string, long> CommitManagerSetupDuration { get; } = new Dictionary<string, long>();
        internal Dictionary<string, long> ItemSourceSetupDuration { get; } = new Dictionary<string, long>();

        // "Get Context" is work done by IAsyncCompletionItemSource
        // multiple sources may participate in a single completion session
        internal Dictionary<string, long> ItemSourceGetContextDuration { get; } = new Dictionary<string, long>();

        // "Processing" is work done by IAsyncCompletionItemManager
        internal long InitialProcessingDuration { get; private set; }
        internal long TotalProcessingDuration { get; private set; }
        internal int TotalProcessingCount { get; private set; }

        // "Rendering" is work done on UI thread by ICompletionPresenter
        internal long InitialRenderingDuration { get; private set; }
        internal long TotalRenderingDuration { get; private set; }
        internal int TotalRenderingCount { get; private set; }

        // "Closing" is also work done on UI thread by ICompletionPresenter
        internal long ClosingDuration { get; private set; }

        // "Commit" is work done on UI thread by IAsyncCompletionCommitManager
        internal long CommitDuration { get; private set; }

        // The following work is a mix of "Get Context" and "Processing" and blocks UI thread
        internal long BlockingComputationDuration { get; private set; }

        // Additional parameters related to work done by IAsyncCompletionItemManager
        internal bool UserEverScrolled { get; private set; }
        internal bool UserEverSetFilters { get; private set; }
        internal int FinalItemCount { get; private set; }
        internal int NumberOfKeystrokes { get; private set; }

        public CompletionSessionTelemetry(CompletionTelemetryHost telemetryHost)
        {
            _telemetryHost = telemetryHost;
        }

        internal void RecordProcessing(long duration, int itemCount)
        {
            if (TotalProcessingCount == 0)
            {
                InitialProcessingDuration = duration;
            }
            else
            {
                TotalProcessingDuration += duration;
                FinalItemCount = itemCount;
            }
            TotalProcessingCount++;
        }

        internal void RecordRendering(long duration)
        {
            if (TotalRenderingCount == 0)
                InitialRenderingDuration = duration;
            TotalRenderingCount++;
            TotalRenderingDuration += duration;
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

        internal void RecordCommitted(long duration,
            IAsyncCompletionCommitManager manager)
        {
            CommitManagerName = CompletionTelemetryHost.GetCommitManagerName(manager);
            CommitDuration = duration;
        }

        internal void RecordClosing(long duration)
        {
            ClosingDuration += duration;
        }

        internal void Save(
            IAsyncCompletionItemManager itemManager,
            ICompletionPresenterProvider presenterProvider)
        {
            ItemManagerName = CompletionTelemetryHost.GetItemManagerName(itemManager);
            PresenterProviderName = CompletionTelemetryHost.GetPresenterProviderName(presenterProvider);
            _telemetryHost.Add(this);
        }

        internal void RecordObtainingCommitManagerData(IAsyncCompletionCommitManager manager, long elapsedMilliseconds)
        {
            var name = CompletionTelemetryHost.GetCommitManagerName(manager);
            CommitManagerSetupDuration[name] = elapsedMilliseconds;
        }

        internal void RecordObtainingSourceSpan(IAsyncCompletionSource source, long elapsedMilliseconds)
        {
            var name = CompletionTelemetryHost.GetSourceName(source);
            ItemSourceSetupDuration[name] = elapsedMilliseconds;
        }

        internal void RecordObtainingSourceContext(IAsyncCompletionSource source, long elapsedMilliseconds)
        {
            var name = CompletionTelemetryHost.GetSourceName(source);
            ItemSourceGetContextDuration[name] = elapsedMilliseconds;
        }

        internal void RecordBlockingWaitForComputation(long elapsedMilliseconds)
        {
            BlockingComputationDuration = elapsedMilliseconds;
        }
    }

    /// <summary>
    /// Aggregates <see cref="CompletionSessionTelemetry"/>.
    /// </summary>
    internal class CompletionTelemetryHost
    {
        private class AggregateCommitManagerData
        {
            internal long TotalCommitTime;
            internal long TotalSetupTime;

            // These values are used to calculate averages
            internal long CommitCount;
            internal long SetupCount;

            // We persist the slowest duration for operations on the UI thread
            internal long MaxCommitTime;
            internal long MaxSetupTime;
        }

        private class AggregateSourceData
        {
            internal long TotalGetContextTime;
            internal long TotalSetupTime;

            // These values are used to calculate averages
            internal long GetContextCount;
            internal long SetupCount;

            // We persist the slowest duration for operations on the UI thread
            internal long MaxSetupTime;
        }

        private class AggregateItemManagerData
        {
            internal long InitialProcessTime;
            internal long TotalProcessTime;
            internal long TotalBlockingComputationTime;
            internal long MaxBlockingComputationTime;

            internal int TotalKeystrokes;
            internal int UserEverScrolled;
            internal int UserEverSetFilters;
            internal int FinalItemCount;

            // These values are used to calculate averages
            internal int SessionCount;
            // This value is used to calculate average processing time. One session may have multiple processing operations.
            internal int ProcessCount;
        }

        private class AggregatePresenterData
        {
            internal long InitialRenderTime;
            internal long TotalRenderTime;
            internal long TotalClosingTime;

            // These values are used to calculate averages
            internal int RenderCount;
            internal int ClosingCount;

            // We persist the slowest duration for operations on the UI thread
            internal long MaxRenderTime;
            internal long MaxClosingTime;
        }

        Dictionary<string, AggregateCommitManagerData> CommitManagerData = new Dictionary<string, AggregateCommitManagerData>();
        Dictionary<string, AggregateItemManagerData> ItemManagerData = new Dictionary<string, AggregateItemManagerData>();
        Dictionary<string, AggregatePresenterData> PresenterData = new Dictionary<string, AggregatePresenterData>();
        Dictionary<string, AggregateSourceData> SourceData = new Dictionary<string, AggregateSourceData>();

        private readonly ILoggingServiceInternal _logger;
        private readonly AsyncCompletionBroker _broker;

        public CompletionTelemetryHost(ILoggingServiceInternal logger, AsyncCompletionBroker broker)
        {
            _logger = logger;
            _broker = broker;
        }

        internal static string GetSourceName(IAsyncCompletionSource source) => source?.GetType().ToString() ?? string.Empty;
        internal static string GetCommitManagerName(IAsyncCompletionCommitManager commitManager) => commitManager?.GetType().ToString() ?? string.Empty;
        internal static string GetItemManagerName(IAsyncCompletionItemManager itemManager) => itemManager?.GetType().ToString() ?? string.Empty;
        internal static string GetPresenterProviderName(ICompletionPresenterProvider provider) => provider?.GetType().ToString() ?? string.Empty;

        /// <summary>
        /// Adds data from <see cref="CompletionSessionTelemetry" /> to appropriate buckets.
        /// </summary>
        /// <param name=""></param>
        internal void Add(CompletionSessionTelemetry telemetry)
        {
            if (_logger == null)
                return;

            AddSourceData(telemetry, SourceData);
            AddItemManagerData(telemetry, ItemManagerData);
            AddCommitManagerData(telemetry, CommitManagerData);
            AddPresenterData(telemetry, PresenterData);
        }

        /// <summary>
        /// Sends batch of collected data.
        /// </summary>
        internal void Send()
        {
            if (_logger == null)
                return;

            foreach (var data in ItemManagerData)
            {
                if (data.Value.SessionCount == 0)
                    continue;
                if (data.Value.ProcessCount == 0)
                    continue;

                _logger.PostEvent(TelemetryEventType.Operation,
                    ItemManagerEventName,
                    TelemetryResult.Success,
                    (ItemManagerName, data.Key),
                    (ItemManagerAverageFinalItemCount, data.Value.FinalItemCount / (double)data.Value.SessionCount),
                    (ItemManagerAverageInitialProcessDuration, data.Value.InitialProcessTime / (double)data.Value.SessionCount),
                    (ItemManagerAverageFilterDuration, data.Value.TotalProcessTime / (double)data.Value.ProcessCount),
                    (ItemManagerAverageKeystrokeCount, data.Value.TotalKeystrokes / (double)data.Value.SessionCount),
                    (ItemManagerAverageScrolled, data.Value.UserEverScrolled / (double)data.Value.SessionCount),
                    (ItemManagerAverageSetFilters, data.Value.UserEverSetFilters / (double)data.Value.SessionCount),
                    (ItemManagerAverageBlockingComputationDuration, data.Value.TotalBlockingComputationTime / (double)data.Value.SessionCount),
                    (ItemManagerMaxBlockingComputationDuration, data.Value.MaxBlockingComputationTime)
                );
            }

            foreach (var data in SourceData)
            {
                if (data.Value.SetupCount == 0)
                    continue;
                if (data.Value.GetContextCount == 0)
                    data.Value.GetContextCount = 1; // the result of division will remain 0 and the division won't throw

                _logger.PostEvent(TelemetryEventType.Operation,
                    SourceEventName,
                    TelemetryResult.Success,
                    (SourceName, data.Key),
                    (SourceAverageGetContextDuration, data.Value.TotalGetContextTime / (double)data.Value.GetContextCount),
                    (SourceAverageSetupDuration, data.Value.TotalSetupTime / (double)data.Value.SetupCount),
                    (SourceMaxSetupDuration, data.Value.MaxSetupTime)
                );
            }

            foreach (var data in CommitManagerData)
            {
                if (data.Value.CommitCount == 0)
                    continue;

                _logger.PostEvent(TelemetryEventType.Operation,
                    CommitManagerEventName,
                    TelemetryResult.Success,
                    (CommitManagerName, data.Key),
                    (CommitManagerAverageCommitDuration, data.Value.TotalCommitTime / (double)data.Value.CommitCount),
                    (CommitManagerAverageSetupDuration, data.Value.TotalSetupTime / (double)data.Value.SetupCount),
                    (CommitManagerMaxCommitDuration, data.Value.MaxCommitTime),
                    (CommitManagerMaxSetupDuration, data.Value.MaxSetupTime)
                );
            }

            foreach (var data in PresenterData)
            {
                if (data.Value.RenderCount == 0)
                    continue;

                _logger.PostEvent(TelemetryEventType.Operation,
                    PresenterEventName,
                    TelemetryResult.Success,
                    (PresenterName, data.Key),
                    (PresenterAverageInitialRendering, data.Value.InitialRenderTime / (double)data.Value.ClosingCount),
                    (PresenterAverageRendering, data.Value.TotalRenderTime / (double)data.Value.RenderCount),
                    (PresenterAverageClosing, data.Value.TotalClosingTime / (double)data.Value.ClosingCount),
                    (PresenterMaxRendering, data.Value.MaxRenderTime),
                    (PresenterMaxClosing, data.Value.MaxClosingTime)
                );
            }
        }

        /// <summary>
        /// Tracks obtaining applicable span and getting items
        /// </summary>
        /// <param name="telemetry">Telemetry from <see cref="IAsyncCompletionSession"/></param>
        /// <param name="sourceData">Data aggregator</param>
        private static void AddSourceData(CompletionSessionTelemetry telemetry, Dictionary<string, AggregateSourceData> sourceData)
        {
            foreach (var setupData in telemetry.ItemSourceSetupDuration)
            {
                if (!sourceData.ContainsKey(setupData.Key))
                    sourceData[setupData.Key] = new AggregateSourceData();
                var aggregateSourceData = sourceData[setupData.Key];

                aggregateSourceData.TotalSetupTime += setupData.Value;
                aggregateSourceData.SetupCount++;

                aggregateSourceData.MaxSetupTime = Math.Max(aggregateSourceData.MaxSetupTime, setupData.Value);
            }

            foreach (var getContextData in telemetry.ItemSourceGetContextDuration)
            {
                if (!sourceData.ContainsKey(getContextData.Key))
                    sourceData[getContextData.Key] = new AggregateSourceData();
                var aggregateSourceData = sourceData[getContextData.Key];

                aggregateSourceData.TotalGetContextTime += getContextData.Value;
                aggregateSourceData.GetContextCount++;
            }
        }

        /// <summary>
        /// Tracks sorting and filtering items
        /// </summary>
        /// <param name="telemetry">Telemetry from <see cref="IAsyncCompletionSession"/></param>
        /// <param name="sourceData">Data aggregator</param>
        private static void AddItemManagerData(CompletionSessionTelemetry telemetry, Dictionary<string, AggregateItemManagerData> itemManagerData)
        {
            var itemManagerKey = telemetry.ItemManagerName;
            if (!itemManagerData.ContainsKey(itemManagerKey))
                itemManagerData[itemManagerKey] = new AggregateItemManagerData();
            var aggregateItemManagerData = itemManagerData[itemManagerKey];

            aggregateItemManagerData.InitialProcessTime += telemetry.InitialProcessingDuration;
            aggregateItemManagerData.TotalProcessTime += telemetry.TotalProcessingDuration;
            aggregateItemManagerData.TotalBlockingComputationTime += telemetry.BlockingComputationDuration;
            aggregateItemManagerData.ProcessCount += telemetry.TotalProcessingCount;
            aggregateItemManagerData.TotalKeystrokes += telemetry.NumberOfKeystrokes;
            aggregateItemManagerData.UserEverScrolled += telemetry.UserEverScrolled ? 1 : 0;
            aggregateItemManagerData.UserEverSetFilters += telemetry.UserEverSetFilters ? 1 : 0;
            aggregateItemManagerData.FinalItemCount += telemetry.FinalItemCount;
            aggregateItemManagerData.SessionCount++;

            aggregateItemManagerData.MaxBlockingComputationTime = Math.Max(aggregateItemManagerData.MaxBlockingComputationTime, telemetry.BlockingComputationDuration);
        }

        /// <summary>
        /// Tracks obtaining commit characters and committing
        /// </summary>
        /// <param name="telemetry">Telemetry from <see cref="IAsyncCompletionSession"/></param>
        /// <param name="sourceData">Data aggregator</param>
        private static void AddCommitManagerData(CompletionSessionTelemetry telemetry, Dictionary<string, AggregateCommitManagerData> commitManagerData)
        {
            var commitKey = telemetry.CommitManagerName;
            if (!string.IsNullOrEmpty(commitKey))
            {
                // commitKey is empty when session is dismissed without committing.
                if (!commitManagerData.ContainsKey(commitKey))
                    commitManagerData[commitKey] = new AggregateCommitManagerData();
                var aggregateCommitManagerData = commitManagerData[commitKey];

                aggregateCommitManagerData.TotalCommitTime += telemetry.CommitDuration;
                aggregateCommitManagerData.CommitCount++;

                aggregateCommitManagerData.MaxCommitTime = Math.Max(aggregateCommitManagerData.MaxCommitTime, telemetry.CommitDuration);
            }

            foreach (var commitManagerSetupData in telemetry.CommitManagerSetupDuration)
            {
                if (!commitManagerData.ContainsKey(commitManagerSetupData.Key))
                    commitManagerData[commitManagerSetupData.Key] = new AggregateCommitManagerData();
                var aggregateCommitManagerData = commitManagerData[commitManagerSetupData.Key];

                aggregateCommitManagerData.TotalSetupTime += commitManagerSetupData.Value;
                aggregateCommitManagerData.SetupCount++;

                aggregateCommitManagerData.MaxSetupTime = Math.Max(aggregateCommitManagerData.MaxSetupTime, commitManagerSetupData.Value);
            }
        }

        /// <summary>
        /// Tracks opening, updating and closing the GUI
        /// </summary>
        /// <param name="telemetry">Telemetry from <see cref="IAsyncCompletionSession"/></param>
        /// <param name="sourceData">Data aggregator</param>
        private static void AddPresenterData(CompletionSessionTelemetry telemetry, Dictionary<string, AggregatePresenterData> presenterData)
        {
            var presenterKey = telemetry.PresenterProviderName;
            if (!presenterData.ContainsKey(presenterKey))
                presenterData[presenterKey] = new AggregatePresenterData();
            var aggregatePresenterData = presenterData[presenterKey];

            aggregatePresenterData.InitialRenderTime += telemetry.InitialRenderingDuration;
            aggregatePresenterData.TotalRenderTime += telemetry.TotalRenderingDuration;
            aggregatePresenterData.RenderCount += telemetry.TotalRenderingCount;
            aggregatePresenterData.TotalClosingTime += telemetry.ClosingDuration;
            aggregatePresenterData.ClosingCount++;

            aggregatePresenterData.MaxRenderTime = Math.Max(aggregatePresenterData.MaxRenderTime, telemetry.InitialRenderingDuration);
            aggregatePresenterData.MaxClosingTime = Math.Max(aggregatePresenterData.MaxClosingTime, telemetry.ClosingDuration);
        }

        // Property and event names
        internal const string PresenterEventName = "VS/Editor/Completion/PresenterData";
        internal const string PresenterName = "Property.Presenter.Name";
        internal const string PresenterAverageInitialRendering = "Property.Presenter.InitialRenderDuration";
        internal const string PresenterAverageRendering = "Property.Presenter.AllRenderDuration";
        internal const string PresenterAverageClosing = "Property.Presenter.AllClosingDuration";
        internal const string PresenterMaxRendering = "Property.Presenter.MaxRenderDuration";
        internal const string PresenterMaxClosing = "Property.Presenter.MaxClosingDuration";

        internal const string ItemManagerEventName = "VS/Editor/Completion/ItemManagerData";
        internal const string ItemManagerName = "Property.ItemManager.Name";
        internal const string ItemManagerAverageFinalItemCount = "Property.ItemManager.FinalItemCount";
        internal const string ItemManagerAverageInitialProcessDuration = "Property.ItemManager.InitialDuration";
        internal const string ItemManagerAverageFilterDuration = "Property.ItemManager.AnyDuration";
        internal const string ItemManagerAverageKeystrokeCount = "Property.ItemManager.KeystrokeCount";
        internal const string ItemManagerAverageScrolled = "Property.ItemManager.Scrolled";
        internal const string ItemManagerAverageSetFilters = "Property.ItemManager.SetFilters";
        internal const string ItemManagerAverageBlockingComputationDuration = "Property.ItemManager.BlockingComputationDuration";
        internal const string ItemManagerMaxBlockingComputationDuration = "Property.ItemManager.MaxBlockingComputationDuration";

        internal const string CommitManagerEventName = "VS/Editor/Completion/CommitManagerData";
        internal const string CommitManagerName = "Property.CommitManager.Name";
        internal const string CommitManagerAverageCommitDuration = "Property.Commit.CommitDuration";
        internal const string CommitManagerAverageSetupDuration = "Property.Commit.SetupDuration";
        internal const string CommitManagerMaxCommitDuration = "Property.Commit.MaxCommitDuration";
        internal const string CommitManagerMaxSetupDuration = "Property.Commit.MaxSetupDuration";

        internal const string SourceEventName = "VS/Editor/Completion/SourceData";
        internal const string SourceName = "Property.Source.Name";
        internal const string SourceAverageGetContextDuration = "Property.Source.GetContextDuration";
        internal const string SourceAverageSetupDuration = "Property.Source.SetupDuration";
        internal const string SourceMaxSetupDuration = "Property.Source.MaxSetupDuration";
    }
}
