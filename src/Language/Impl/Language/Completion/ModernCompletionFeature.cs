using System.Diagnostics;
using Microsoft.VisualStudio.Text.Utilities;

namespace Microsoft.VisualStudio.Language.Intellisense.Implementation
{
    /// <summary>
    /// Provides information whether modern completion should be enabled, given the buffer's content type.
    /// </summary>
    internal static class ModernCompletionFeature
    {
        private const string TreatmentFlightName = "CompletionAPI";
        private static bool _treatmentFlightEnabled;
        private static bool _initialized;

        /// <summary>
        /// Returns whether or not modern completion should be enabled.
        /// </summary>
        /// <returns>true if experiment is enabled.</returns>
        public static bool GetFeatureState(IExperimentationServiceInternal experimentationService)
        {
            if (_initialized)
            {
                return _treatmentFlightEnabled;
            }

            _treatmentFlightEnabled = experimentationService.IsCachedFlightEnabled(TreatmentFlightName);
            Debug.Assert(_treatmentFlightEnabled);
            _initialized = true;
            return _treatmentFlightEnabled;
        }
    }
}
