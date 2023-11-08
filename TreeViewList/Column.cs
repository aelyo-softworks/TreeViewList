using System;
using System.Collections.Generic;
using System.Drawing;

namespace TreeViewList
{
    public class Column
    {
        private int _width = TreeViewListControl.DefaultDefaultColumnWidth;
        private int _minWidth = TreeViewListControl.DefaultDefaultMinColumnWidth;
        private int _verticalPadding = TreeViewListControl.DefaultDefaultColumnPadding;
        private int _horizontalPadding = TreeViewListControl.DefaultDefaultColumnPadding;
        private StringAlignment _headerVerticalAlignment = StringAlignment.Center;
        private StringAlignment _headerHorizontalAlignment = StringAlignment.Center;
        private StringFormatFlags _headerFormatFlags = StringFormatFlags.FitBlackBox;
        private StringTrimming _headerTrimming = StringTrimming.None;
        private StringAlignment _verticalAlignment = StringAlignment.Center;
        private StringAlignment _horizontalAlignment = StringAlignment.Center;
        private StringFormatFlags _formatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.LineLimit;
        private StringTrimming _trimming = StringTrimming.EllipsisCharacter;
        private Font? _font;
        private Font? _headerFont;
        private Brush? _textBrush;
        private Brush? _headerTextBrush;

        protected internal Column(TreeViewListControl control, string text)
        {
            ArgumentNullException.ThrowIfNull(control);
            ArgumentNullException.ThrowIfNull(text);
            Control = control;
            Text = text;
        }

        public int Index => Control.Columns.IndexOf(this);
        public TreeViewListControl Control { get; }
        public string Text { get; }
        public virtual object? Tag { get; set; }

        public virtual int Width
        {
            get => _width;
            set
            {
                if (_width == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _width = Math.Max(MinWidth, value);
            }
        }

        public virtual int MinWidth
        {
            get => _minWidth;
            set
            {
                if (_minWidth == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _minWidth = value;
            }
        }

        public virtual int VerticalPadding
        {
            get => _verticalPadding;
            set
            {
                if (_verticalPadding == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _verticalPadding = value;
            }
        }

        public virtual int HorizontalPadding
        {
            get => _horizontalPadding;
            set
            {
                if (_horizontalPadding == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _horizontalPadding = value;
            }
        }

        public virtual Brush? HeaderTextBrush
        {
            get => _headerTextBrush;
            set
            {
                if (_headerTextBrush == value)
                    return;

                _headerTextBrush?.Dispose();
                _headerTextBrush = value;
            }
        }

        public virtual Font? HeaderFont
        {
            get => _headerFont;
            set
            {
                if (_headerFont == value)
                    return;

                _headerFont?.Dispose();
                _headerFont = value;
            }
        }

        public virtual Brush? TextBrush
        {
            get => _textBrush;
            set
            {
                if (_textBrush == value)
                    return;

                _textBrush?.Dispose();

                _textBrush = value;
            }
        }

        public virtual Font? Font
        {
            get => _font;
            set
            {
                if (_font == value)
                    return;

                try
                {
                    _font?.Dispose();
                }
                catch
                {
                    // continue
                }

                _font = value;
            }
        }

        public virtual StringAlignment HeaderVerticalAlignment
        {
            get => _headerVerticalAlignment;
            set
            {
                if (_headerVerticalAlignment == value)
                    return;

                _headerVerticalAlignment = value;
            }
        }

        public virtual StringAlignment HeaderHorizontalAlignment
        {
            get => _headerHorizontalAlignment;
            set
            {
                if (_headerHorizontalAlignment == value)
                    return;

                _headerHorizontalAlignment = value;
            }
        }

        public virtual StringFormatFlags HeaderFormatFlags
        {
            get => _headerFormatFlags;
            set
            {
                if (_headerFormatFlags == value)
                    return;

                _headerFormatFlags = value;
            }
        }

        public virtual StringTrimming HeaderTrimming
        {
            get => _headerTrimming;
            set
            {
                if (_headerTrimming == value)
                    return;

                _headerTrimming = value;
            }
        }

        public virtual StringAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                if (_verticalAlignment == value)
                    return;

                _verticalAlignment = value;
            }
        }

        public virtual StringAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                if (_horizontalAlignment == value)
                    return;

                _horizontalAlignment = value;
            }
        }

        public virtual StringFormatFlags FormatFlags
        {
            get => _formatFlags;
            set
            {
                if (_formatFlags == value)
                    return;

                _formatFlags = value;
            }
        }

        public virtual StringTrimming Trimming
        {
            get => _trimming;
            set
            {
                if (_trimming == value)
                    return;

                _trimming = value;
            }
        }

        internal IEnumerable<Column> FollowingColumns
        {
            get
            {
                var index = Index;
                if (index < 0)
                    yield break;

                for (var i = index + 1; i < Control.Columns.Count; i++)
                {
                    yield return Control.Columns[i];
                }
            }
        }

        public override string ToString() => Text;
    }
}
