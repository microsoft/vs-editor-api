// Copyright (c) Microsoft Corporation
// All rights reserved

using System;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Represents an object that can provide a unique ID for telemetry purposes.
    /// <typeparam name="Tid">Type of the telemetry ID.</typeparam>
    /// </summary>
    public interface ITelemetryIdProvider<Tid>
    {
        /// <summary>
        /// Tries to get a unique ID for telemetry purposes.
        /// </summary>
        /// <returns><c>true</c> if a unique telemetry ID was returned, <c>false</c> if this object refuses to participate in telemetry logging.</returns>
        bool TryGetTelemetryId(out Tid telemetryId);
    }
}
