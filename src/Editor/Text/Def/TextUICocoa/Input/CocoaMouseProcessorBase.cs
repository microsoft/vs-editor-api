//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Provides a base implementation for mouse bindings, so that clients can
    /// override only the the methods they need.
    /// </summary>
    public abstract class CocoaMouseProcessorBase : ICocoaMouseProcessor
    {
        #region IMouseProcessor Members

        /// <summary>
        /// Handles the mouse left button down event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseLeftButtonDown(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse left button down event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseLeftButtonDown(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse right button down event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseRightButtonDown(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse right button down event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseRightButtonDown(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse left button up event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseLeftButtonUp(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse left button up event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseLeftButtonUp(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse right button up event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseRightButtonUp(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse right button up event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseRightButtonUp(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse up event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseUp(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse up event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseUp(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse down event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseDown(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse down event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseDown(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse move event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseMove(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse move event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseMove(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse wheel event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseWheel(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse wheel event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseWheel(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse enter event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseEnter(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse enter event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseEnter(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse leave event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseLeave(MouseEvent e) { }

        /// <summary>
        /// Handles the mouse leave event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseLeave(MouseEvent e) { }

        /// <summary>
        /// Handles the drag leave event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessDragLeave(MouseEvent e) { }

        /// <summary>
        /// Handles the drag leave event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessDragLeave(MouseEvent e) { }

        /// <summary>
        /// Handles the drag over event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessDragOver(MouseEvent e) { }

        /// <summary>
        /// Handles the drag over event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessDragOver(MouseEvent e) { }

        /// <summary>
        /// Handles the drag enter event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessDragEnter(MouseEvent e) { }

        /// <summary>
        /// Handles the drag enter event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessDragEnter(MouseEvent e) { }

        /// <summary>
        /// Handles the drop event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessDrop(MouseEvent e) { }

        /// <summary>
        /// Handles the drop event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessDrop(MouseEvent e) { }

        /// <summary>
        /// Handles the query continue drag event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessQueryContinueDrag(MouseEvent e) { }

        /// <summary>
        /// Handles the query continue drag event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessQueryContinueDrag(MouseEvent e) { }

        /// <summary>
        /// Handles the feedback event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessGiveFeedback(MouseEvent e) { }

        /// <summary>
        /// Handles the feedback event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessGiveFeedback(MouseEvent e) { }

        #endregion
    }
}