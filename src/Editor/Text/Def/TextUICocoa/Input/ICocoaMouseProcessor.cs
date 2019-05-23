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
        void PreprocessMouseLeftButtonDown(MouseEvent e);

        /// <summary>
        /// Handles a mouse left button down event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseLeftButtonDown(MouseEvent e);

        /// <summary>
        /// Handles a mouse right button down event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseRightButtonDown(MouseEvent e);

        /// <summary>
        /// Handles a mouse right button down event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseRightButtonDown(MouseEvent e);

        /// <summary>
        /// Handles a mouse left button up event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseLeftButtonUp(MouseEvent e);

        /// <summary>
        /// Handles a mouse left button up event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseLeftButtonUp(MouseEvent e);

        /// <summary>
        /// Handles a mouse right button up event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseRightButtonUp(MouseEvent e);

        /// <summary>
        /// Handles a mouse right button up event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseRightButtonUp(MouseEvent e);

        /// <summary>
        /// Handles a mouse up event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseUp(MouseEvent e);

        /// <summary>
        /// Handles a mouse up event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseUp(MouseEvent e);

        /// <summary>
        /// Handles a mouse down event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseDown(MouseEvent e);

        /// <summary>
        /// Handles a mouse down event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseDown(MouseEvent e);

        /// <summary>
        /// Handles a mouse move event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseMove(MouseEvent e);

        /// <summary>
        /// Handles a mouse move event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseMove(MouseEvent e);

        /// <summary>
        /// Handles a mouse wheel event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseWheel(MouseEvent e);

        /// <summary>
        /// Handles a mouse wheel event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseWheel(MouseEvent e);

        /// <summary>
        /// Handles a mouse enter event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseEnter(MouseEvent e);

        /// <summary>
        /// Handles a mouse enter event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseEnter(MouseEvent e);

        /// <summary>
        /// Handles a mouse leave event before the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PreprocessMouseLeave(MouseEvent e);

        /// <summary>
        /// Handles a mouse leave event after the default handler.
        /// </summary>
        /// <param name="e">
        /// Event arguments that describe the event.
        /// </param>
        void PostprocessMouseLeave(MouseEvent e);

        void PreprocessDraggingEntered(DragEvent e);
        void PostprocessDraggingEntered(DragEvent e);

        void PreprocessDraggingUpdated(DragEvent e);
        void PostprocessDraggingUpdated(DragEvent e);

        void PreprocessDraggingExited(DragEvent e);
        void PostprocessDraggingExited(DragEvent e);

        void PreprocessPrepareForDragOperation(DragEvent e);
        void PostprocessPrepareForDragOperation(DragEvent e);

        void PreprocessPerformDragOperation(DragEvent e);
        void PostprocessPerformDragOperation(DragEvent e);

        void PreprocessDraggingEnded(DragEvent e);
        void PostprocessDraggingEnded(DragEvent e);
    }
}