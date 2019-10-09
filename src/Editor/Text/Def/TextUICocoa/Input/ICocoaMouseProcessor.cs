//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//

namespace Microsoft.VisualStudio.Text.Editor
{
    /// <summary>
    /// Provides extensions for mouse bindings.
    /// </summary>
    public interface ICocoaMouseProcessor
    {
        /// <summary>
        /// Handles a mouse left button down event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseLeftButtonDown(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse left button down event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseLeftButtonDown(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse right button down event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseRightButtonDown(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse right button down event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseRightButtonDown(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse left button up event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseLeftButtonUp(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse left button up event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseLeftButtonUp(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse right button up event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseRightButtonUp(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse right button up event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseRightButtonUp(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse up event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseUp(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse up event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseUp(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse down event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseDown(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse down event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseDown(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse move event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseMove(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse move event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseMove(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse wheel event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseWheel(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse wheel event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseWheel(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse enter event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseEnter(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse enter event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseEnter(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse leave event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseLeave(MouseEventArgs e);

        /// <summary>
        /// Handles a mouse leave event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseLeave(MouseEventArgs e);

        void PreprocessDraggingEntered(DragEventArgs e);
        void PostprocessDraggingEntered(DragEventArgs e);

        void PreprocessDraggingUpdated(DragEventArgs e);
        void PostprocessDraggingUpdated(DragEventArgs e);

        void PreprocessDraggingExited(DragEventArgs e);
        void PostprocessDraggingExited(DragEventArgs e);

        void PreprocessPrepareForDragOperation(DragEventArgs e);
        void PostprocessPrepareForDragOperation(DragEventArgs e);

        void PreprocessPerformDragOperation(DragEventArgs e);
        void PostprocessPerformDragOperation(DragEventArgs e);

        void PreprocessDraggingEnded(DragEventArgs e);
        void PostprocessDraggingEnded(DragEventArgs e);
    }
}