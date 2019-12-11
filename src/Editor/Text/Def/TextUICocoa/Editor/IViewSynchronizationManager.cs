//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
namespace Microsoft.VisualStudio.Text.Formatting
{
    using Microsoft.VisualStudio.Text.Editor;

    /// <summary>
    /// Manage the simultaneous layout to two <see cref="ITextView"/>.
    /// </summary>
    public interface IViewSynchronizationManager
    {
        ITextView GetSubordinateView(ITextView masterView);

        bool TryGetAnchorPointInSubordinateView(SnapshotPoint anchorPoint, out SnapshotPoint correspondingAnchorPoint);

        SnapshotPoint GetAnchorPointAboveInSubordinateView(SnapshotPoint anchorPoint);

        void WhichPairedLinesShouldBeDisplayed(SnapshotPoint masterAnchorPoint, SnapshotPoint subordinateAnchorPoint, out bool layoutMaster, out bool layoutSubordinate, bool goingUp);
    }
}