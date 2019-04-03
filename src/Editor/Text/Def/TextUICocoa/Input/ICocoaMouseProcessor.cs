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

        /// <summary>
        /// Handles a drag leave event before the default handler.
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PreprocessDragLeave(MouseEvent e);

        /// <summary>
        /// Handles a drag leave event after the default handler.
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PostprocessDragLeave(MouseEvent e);

        /// <summary>
        /// Handles a drag over event before the default handler.
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PreprocessDragOver(MouseEvent e);

        /// <summary>
        /// Handles a drag over event after the default handler.
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PostprocessDragOver(MouseEvent e);

        /// <summary>
        /// Handles a drag enter event before the default handler.
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PreprocessDragEnter(MouseEvent e);

        /// <summary>
        /// Handles a drag enter event after the default handler.
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PostprocessDragEnter(MouseEvent e);

        /// <summary>
        /// Handles a drop event before the default handler.
        /// </summary>
        /// <param name="e">
        /// <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PreprocessDrop(MouseEvent e);

        /// <summary>
        /// Handles a drop event after the default handler.
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PostprocessDrop(MouseEvent e);

        /// <summary>
        /// Handles a QueryContinueDrag event before the default handler. 
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PreprocessQueryContinueDrag(MouseEvent e);

        /// <summary>
        /// Handles a QueryContinueDrag event after the default handler. 
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PostprocessQueryContinueDrag(MouseEvent e);

        /// <summary>
        /// Handles a GiveFeedback event before the default handler. 
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PreprocessGiveFeedback(MouseEvent e);

        /// <summary>
        /// Handles a GiveFeedback event after the default handler. 
        /// </summary>
        /// <param name="e">
        /// A <see cref="MouseEvent"/> describing the drag operation.
        /// </param>
        void PostprocessGiveFeedback(MouseEvent e);
    }
}