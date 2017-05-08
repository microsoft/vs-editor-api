// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Classification
{
    using System;

    /// <summary>
    /// Provides information for the <see cref="IClassifier.ClassificationChanged"/> event.
    /// </summary>
    public class ClassificationChangedEventArgs : EventArgs
    {
        SnapshotSpan changeSpan;

        /// <summary>
        /// Initializes a new instance of a <see cref="ClassificationChangedEventArgs"/> object.
        /// </summary>
        /// <param name="changeSpan">
        /// The span of the classification that changed.
        /// </param>
        public ClassificationChangedEventArgs(SnapshotSpan changeSpan)
        {
            this.changeSpan = changeSpan;
        }

        /// <summary>
        /// Gets the span of the classification that changed.
        /// </summary>
        public SnapshotSpan ChangeSpan
        {
            get { return this.changeSpan; }
        }
    }
}