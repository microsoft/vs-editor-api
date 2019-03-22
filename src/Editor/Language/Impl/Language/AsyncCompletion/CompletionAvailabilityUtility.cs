using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Utilities;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Implementation
{
    /// <summary>
    /// Provides information whether modern completion should be enabled,
    /// based on the state of <see cref="IExperimentationServiceInternal"/> and <see cref="IFeatureServiceFactory" />
    /// for the given <see cref="IContentType"/> and <see cref="ITextView"/>.
    /// </summary>
    [Export]
    internal class CompletionAvailabilityUtility
    {
        [Import]
        private IExperimentationServiceInternal ExperimentationService;

        [Import]
        private IFeatureServiceFactory FeatureServiceFactory;

        [Import]
        private AsyncCompletionBroker Broker; // We're using internal method to check if relevant MEF parts exist.

        // Black list by content type
        private const string CompletionFlightName = "CompletionAPI";
        private const string RoslynLanguagesContentType = "Roslyn Languages";
        private const string RazorContentType = "Razor";
        private bool _treatmentFlightDataInitialized;

        // Quick access data:
        private bool _treatmentFlightEnabled;
        private IFeatureCookie _globalCompletionCookie;
        private IFeatureCookie GlobalCompletionCookie =>
            _globalCompletionCookie
            ?? (_globalCompletionCookie = FeatureServiceFactory.GlobalFeatureService.GetCookie(PredefinedEditorFeatureNames.Completion));

        /// <summary>
        /// Returns whether completion is available for the given <see cref="IContentType" />.
        /// </summary>
        /// <returns>true if experiment is enabled, feature is enabled in the global scope, and broker has providers that match the supplied <see cref="IContentType" /></returns>
        internal bool IsAvailable(IContentType contentType)
        {
            if (!GlobalCompletionCookie.IsEnabled)
                return false;

            if (!Broker.HasCompletionProviders(contentType))
                return false;

            // Roslyn and Razor providers exist in the MEF cache, but Roslyn is not ready for public rollout yet.
            // However, We do want other languages (e.g. AXML, EditorConfig) to work with Async Completion API
            // We will remove this check once Roslyn fully embraces Async Completion API.
            if (!IsExperimentEnabled() && (contentType.IsOfType(RoslynLanguagesContentType) || contentType.IsOfType(RazorContentType)))
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether completion is available for the given <see cref="IContentType"/> in the given <see cref="ITextView" />.
        /// </summary>
        /// <returns>true if experiment is enabled, feature is enabled in the <see cref="ITextView" />'s scope, and broker has providers that match the supplied <see cref="IContentType" /></returns>
        internal bool IsAvailable(IContentType contentType, ITextView textView)
        {
            if (!Broker.HasCompletionProviders(contentType))
                return false;

            var featureService = FeatureServiceFactory.GetOrCreate(textView);
            if (!featureService.IsEnabled(PredefinedEditorFeatureNames.Completion))
                return false;

            // Roslyn and Razor providers exist in the MEF cache, but Roslyn is not ready for public rollout yet.
            // However, We do want other languages (e.g. AXML, EditorConfig) to work with Async Completion API
            // We will remove this check once Roslyn fully embraces Async Completion API.
            if (!IsExperimentEnabled() && (contentType.IsOfType(RoslynLanguagesContentType) || contentType.IsOfType(RazorContentType)))
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether completion is available in the given <see cref="ITextView" />.
        /// Note: the second parameter <see cref="IContentType"/> is to be removed in dev16 when the experiment ends.
        /// </summary>
        /// <returns>true if experiment is enabled and feature is enabled in <see cref="ITextView"/>'s scope</returns>
        internal bool IsAvailable(ITextView textView, IContentType contentTypeToCheckBlacklist)
        {
            var featureService = FeatureServiceFactory.GetOrCreate(textView);
            if (!featureService.IsEnabled(PredefinedEditorFeatureNames.Completion))
                return false;

            // Roslyn and Razor providers exist in the MEF cache, but Roslyn is not ready for public rollout yet.
            // However, We do want other languages (e.g. AXML, EditorConfig) to work with Async Completion API
            // We will remove this check once Roslyn fully embraces Async Completion API.
            if (!IsExperimentEnabled() && (contentTypeToCheckBlacklist.IsOfType(RoslynLanguagesContentType) || contentTypeToCheckBlacklist.IsOfType(RazorContentType)))
                return false;

            return true;
        }

        private bool IsExperimentEnabled()
        {
            if (_treatmentFlightDataInitialized)
                return _treatmentFlightEnabled;

            _treatmentFlightEnabled = ExperimentationService.IsCachedFlightEnabled(CompletionFlightName);
            _treatmentFlightDataInitialized = true;
            return _treatmentFlightEnabled;
        }
    }
}
