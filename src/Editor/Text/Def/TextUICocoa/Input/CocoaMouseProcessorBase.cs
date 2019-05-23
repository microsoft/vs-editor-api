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

        public virtual void PreprocessDraggingEntered(DragEvent e) { }
        public virtual void PostprocessDraggingEntered(DragEvent e) { }

        public virtual void PreprocessDraggingUpdated(DragEvent e) { }
        public virtual void PostprocessDraggingUpdated(DragEvent e) { }

        public virtual void PreprocessDraggingExited(DragEvent e) { }
        public virtual void PostprocessDraggingExited(DragEvent e) { }

        public virtual void PreprocessPrepareForDragOperation(DragEvent e) { }
        public virtual void PostprocessPrepareForDragOperation(DragEvent e) { }

        public virtual void PreprocessPerformDragOperation(DragEvent e) { }
        public virtual void PostprocessPerformDragOperation(DragEvent e) { }


        public virtual void PreprocessDraggingEnded(DragEvent e) { }
        public virtual void PostprocessDraggingEnded(DragEvent e) { }

        #endregion
    }
}