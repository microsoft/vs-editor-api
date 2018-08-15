using System;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Text.MultiSelection.Implementation
{
    internal class SelectionUIProperties : AbstractSelectionPresentationProperties
    {
        private MultiSelectionBrokerFactory _factory;
        private MultiSelectionBroker _broker;
        private IEditorOptions _options;
        private SelectionTransformer _transformer;

        public SelectionUIProperties(MultiSelectionBrokerFactory factory, MultiSelectionBroker broker, SelectionTransformer transformer)
        {
            _factory = factory;
            _broker = broker;
            _options = _factory.EditorOptionsFactoryService.GetOptions(_broker.TextView);
            _transformer = transformer;
        }

        internal void SetPreferredXCoordinate(double value)
        {
            PreferredXCoordinate = value;
        }

        internal void SetPreferredYCoordinate(double value)
        {
            PreferredYCoordinate = value;
        }

        public override bool IsOverwriteMode
        {
            get
            {
                //Perf: ContainingTextViewLine has the costly side effects. Try to do every short circut possible to return early before using it.
                if ((!_options.IsOverwriteModeEnabled())
                    || _transformer.Selection.InsertionPoint.IsInVirtualSpace
                    || (!_transformer.Selection.IsEmpty))
                {
                    return false;
                }

                // Ok, we know overwrite mode is globally on, we don't have a selection, and we're not in virtual space.
                // Now the only other check is 'are we at the end of the current line?'
                return this.ContainingTextViewLine.End.Position != _transformer.Selection.InsertionPoint.Position.Position;
            }
        }

        public override TextBounds CaretBounds
        {
            get
            {
                var width = this.CaretWidth;
                var line = this.ContainingTextViewLine;
                double left;

                if (this.IsOverwriteMode)
                {
                    var charBounds = this.ContainingTextViewLine.GetExtendedCharacterBounds(_transformer.Selection.InsertionPoint);
                    left = charBounds.Left;
                }
                else
                {
                    left = GetXCoordinateFromVirtualBufferPosition(line, _transformer.Selection.InsertionPoint);
                }
                return new TextBounds(left, line.Top, width, line.Height, line.TextTop, line.TextHeight);
            }
        }

        public override ITextViewLine ContainingTextViewLine
        {
            get
            {
                var bufferPosition = _transformer.Selection.InsertionPoint.Position;
                ITextViewLine textLine = _broker.TextView.GetTextViewLineContainingBufferPosition(bufferPosition);

                if ((_transformer.Selection.InsertionPointAffinity == PositionAffinity.Predecessor) && (textLine.Start == bufferPosition) &&
                    (_broker.TextView.TextSnapshot.GetLineFromPosition(bufferPosition).Start != bufferPosition))
                {
                    //The desired location has precedessor affinity at the start of a word wrapped line, so we
                    //really want the line before this one.
                    textLine = _broker.TextView.GetTextViewLineContainingBufferPosition(bufferPosition - 1);
                }

                return textLine;
            }
        }

        public double CaretWidth
        {
            get
            {
                if (this.IsOverwriteMode)
                {
                    var bounds = this.ContainingTextViewLine.GetExtendedCharacterBounds(_transformer.Selection.InsertionPoint);
                    return bounds.Width;
                }
                else
                {
                    return (double)_options.GetOptionValue(DefaultTextViewOptions.CaretWidthId);
                }
            }
        }

        public override bool IsWithinViewport
        {
            get
            {
                // make sure the caret is on a line that's visible and that the caret is within the visual boundaries of the view
                return (((ContainingTextViewLine.VisibilityState == VisibilityState.FullyVisible)
                    && CaretBounds.Left >= _broker.TextView.ViewportLeft) && (CaretBounds.Right <= _broker.TextView.ViewportRight));
            }
        }

        /// <summary>
        /// Get the caret x coordinate for a virtual buffer position.
        /// </summary>
        /// <remarks>
        /// The x coordinate is always on the trailing edge of the previous character,
        /// *unless* the supplied buffer position is the first character on the line or
        /// is in virtual space.
        /// </remarks>
        internal static double GetXCoordinateFromVirtualBufferPosition(ITextViewLine textLine, VirtualSnapshotPoint bufferPosition)
        {
            return (bufferPosition.IsInVirtualSpace || bufferPosition.Position == textLine.Start) ?
                textLine.GetExtendedCharacterBounds(bufferPosition).Leading :
                textLine.GetExtendedCharacterBounds(bufferPosition.Position - 1).Trailing;
        }
    }
}
