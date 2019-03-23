//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License. See License.txt in the project root for license information.
//
// This file contain implementations details that are subject to change without notice.
// Use at your own risk.
//
using System;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Text.Utilities
{
    class DifferenceBrushManager
    {
        public static DifferenceBrushManager GetBrushManager(ITextView3 view, IEditorFormatMapService formatMapService)
        {
            return view.Properties.GetOrCreateSingletonProperty(() => new DifferenceBrushManager(view, formatMapService.GetEditorFormatMap(view)));
        }

        public static DifferenceBrushManager GetBrushManager(ITextView3 view, IEditorFormatMap formatMap)
        {
            return view.Properties.GetOrCreateSingletonProperty(() => new DifferenceBrushManager(view, formatMap));
        }

        // internal for unit testing
        internal static readonly SolidColorBrush _defaultRemovedLineBrush = Brushes.PaleVioletRed;
        internal static readonly SolidColorBrush _defaultAddedLineBrush = Brushes.LightYellow;
        internal static readonly SolidColorBrush _defaultRemovedWordBrush = Brushes.Red;
        internal static readonly SolidColorBrush _defaultAddedWordBrush = Brushes.Yellow;

        IEditorFormatMap _formatMap;

        #region Public properties (brushes) and changed event

        public Brush RemovedLineBrush { get; private set; }
        public Brush AddedLineBrush { get; private set; }

        public Brush RemovedWordBrush { get; private set; }
        public Brush RemovedWordForegroundBrush { get; private set; }
        public Pen RemovedWordForegroundPen { get; private set; }
        public Brush AddedWordBrush { get; private set; }
        public Brush AddedWordForegroundBrush { get; private set; }
        public Pen AddedWordForegroundPen { get; private set; }

        public Brush ViewportBrush { get; private set; }
        public Pen ViewportPen { get; private set; }
        public Brush OverviewBrush { get; private set; }

        public event EventHandler<EventArgs> BrushesChanged;

        #endregion

        internal DifferenceBrushManager(ITextView3 view, IEditorFormatMap formatMap)
        {
            _formatMap = formatMap;

            InitializeBrushes();

            _formatMap.FormatMappingChanged += FormatMapChanged;
            view.Closed += (s,a) => { _formatMap.FormatMappingChanged -= FormatMapChanged; };
        }

        void InitializeBrushes()
        {
            RemovedLineBrush = GetBrushValue("deltadiff.remove.line", _defaultRemovedLineBrush);
            RemovedWordBrush = GetBrushValue("deltadiff.remove.word", _defaultRemovedWordBrush);
            RemovedWordForegroundBrush = GetBrushValue("deltadiff.remove.word", _defaultRemovedWordBrush, EditorFormatDefinition.ForegroundBrushId);
            RemovedWordForegroundPen = new Pen(RemovedWordForegroundBrush, 2.0);
            RemovedWordForegroundPen.Freeze();

            AddedLineBrush = GetBrushValue("deltadiff.add.line", _defaultAddedLineBrush);
            AddedWordBrush = GetBrushValue("deltadiff.add.word", _defaultAddedWordBrush);
            AddedWordForegroundBrush = GetBrushValue("deltadiff.add.word", _defaultAddedWordBrush, EditorFormatDefinition.ForegroundBrushId);
            AddedWordForegroundPen = new Pen(AddedWordForegroundBrush, 2.0);
            AddedWordForegroundPen.Freeze();

            ViewportBrush = GetBrushValue("deltadiff.overview.color", Brushes.DarkGray, EditorFormatDefinition.ForegroundBrushId);
            ViewportPen = new Pen(ViewportBrush, 2.0);
            ViewportPen.Freeze();

            OverviewBrush = GetBrushValue("deltadiff.overview.color", Brushes.Gray);

            var temp = BrushesChanged;
            if (temp != null)
                temp(this, EventArgs.Empty);
        }

        Brush GetBrushValue(string formatName, Brush defaultValue, string resource = EditorFormatDefinition.BackgroundBrushId)
        {
            var formatProperties = _formatMap.GetProperties(formatName);
            if (formatProperties != null && formatProperties.Contains(resource))
            {
                var brushValue = formatProperties[resource] as Brush;
                if (brushValue != null)
                    return brushValue;
            }

            return defaultValue;
        }

        void FormatMapChanged(object sender, FormatItemsEventArgs e)
        {
            bool updateRequired = e.ChangedItems.Any(item =>
                    string.Equals(item, "deltadiff.add.word", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item, "deltadiff.add.line", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item, "deltadiff.remove.word", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item, "deltadiff.remove.line", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item, "deltadiff.overview.color", System.StringComparison.OrdinalIgnoreCase));

            if (updateRequired)
            {
                InitializeBrushes();
            }
        }
    }
}
