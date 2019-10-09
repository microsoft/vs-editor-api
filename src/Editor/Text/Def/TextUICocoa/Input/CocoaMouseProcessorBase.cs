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
        public virtual void PreprocessMouseLeftButtonDown(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse left button down event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseLeftButtonDown(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse right button down event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseRightButtonDown(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse right button down event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseRightButtonDown(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse left button up event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseLeftButtonUp(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse left button up event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseLeftButtonUp(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse right button up event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseRightButtonUp(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse right button up event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseRightButtonUp(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse up event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseUp(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse up event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseUp(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse down event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseDown(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse down event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseDown(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse move event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseMove(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse move event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseMove(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse wheel event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseWheel(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse wheel event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseWheel(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse enter event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseEnter(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse enter event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseEnter(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse leave event before the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PreprocessMouseLeave(MouseEventArgs e) { }

        /// <summary>
        /// Handles the mouse leave event after the default handler.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        public virtual void PostprocessMouseLeave(MouseEventArgs e) { }

        public virtual void PreprocessDraggingEntered(DragEventArgs e) { }
        public virtual void PostprocessDraggingEntered(DragEventArgs e) { }

        public virtual void PreprocessDraggingUpdated(DragEventArgs e) { }
        public virtual void PostprocessDraggingUpdated(DragEventArgs e) { }

        public virtual void PreprocessDraggingExited(DragEventArgs e) { }
        public virtual void PostprocessDraggingExited(DragEventArgs e) { }

        public virtual void PreprocessPrepareForDragOperation(DragEventArgs e) { }
        public virtual void PostprocessPrepareForDragOperation(DragEventArgs e) { }

        public virtual void PreprocessPerformDragOperation(DragEventArgs e) { }
        public virtual void PostprocessPerformDragOperation(DragEventArgs e) { }


        public virtual void PreprocessDraggingEnded(DragEventArgs e) { }
        public virtual void PostprocessDraggingEnded(DragEventArgs e) { }

        #endregion
    }
}