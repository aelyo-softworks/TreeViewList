/*
MIT License

Copyright (c) 2023 Aelyo Softworks

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading.Tasks;
global using global::System.Threading;
global using global::System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using System.Windows.Forms;

namespace TreeViewList
{
    public abstract class BaseList<T> : IEnumerable<T>
    {
        private readonly List<T> _list = new();

        protected BaseList() { }

        public int Count => _list.Count;
        public T this[int index] => _list[index];
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        protected virtual void Add(T item) => _list.Add(item);
        protected virtual void Insert(int index, T item) => _list.Insert(index, item);
        protected virtual void RemoveAt(int index) => _list.RemoveAt(index);
        protected virtual bool Remove(T item) => _list.Remove(item);
        protected virtual void Clear() => _list.Clear();
        protected virtual int IndexOf(T item) => _list.IndexOf(item);
    }
}
namespace TreeViewList
{
    public class Cell
    {
        public Cell(object? value)
        {
            Value = value;
        }

        public virtual object? Value { get; set; }
        public string ValueAsString => Value?.ToString() ?? string.Empty;

        public override string ToString() => ValueAsString;
    }
}

namespace TreeViewList
{
    public class CellCollection : BaseList<Cell>
    {
        public CellCollection(Row row)
        {
            ArgumentNullException.ThrowIfNull(row);
            Row = row;
        }

        public Row Row { get; }

        public virtual Cell Add(object? value)
        {
            var cell = new Cell(value);
            base.Add(cell);
            return cell;
        }

        public virtual Cell Insert(int index, object value)
        {
            var cell = new Cell(value);
            base.Insert(index, cell);
            return cell;
        }

        public new virtual void RemoveAt(int index)
        {
            var cell = base[index];
            base.RemoveAt(index);
        }

        public new virtual bool Remove(Cell column)
        {
            ArgumentNullException.ThrowIfNull(column);
            return base.Remove(column);
        }

        public new virtual int IndexOf(Cell column) => base.IndexOf(column);
        public new virtual void Clear()
        {
            foreach (var cell in this)
            {
            }
            base.Clear();
        }
    }
}

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

namespace TreeViewList
{
    public class ColumnCollection : BaseList<Column>
    {
        public ColumnCollection(TreeViewListControl control)
        {
            ArgumentNullException.ThrowIfNull(control);
            Control = control;
        }

        public TreeViewListControl Control { get; }

        private void AddComputeExtentWidth(Column column)
        {
            Control.ExtentWidth += column.Width;
            if (Control.Columns.Count > 1)
            {
                Control.ExtentWidth += Control.LineWidth;
            }
        }

        private void RemoveComputeExtentWidth(Column column)
        {
            Control.ExtentWidth -= column.Width;
            if (Control.Columns.Count > 0)
            {
                Control.ExtentWidth -= Control.LineWidth;
            }
        }

        public virtual Column Add(string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            var column = new Column(Control, text);
            base.Add(column);
            AddComputeExtentWidth(column);
            return column;
        }

        public virtual Column Insert(int index, string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            var column = new Column(Control, text);
            base.Insert(index, column);
            AddComputeExtentWidth(column);
            return column;
        }

        public new virtual void RemoveAt(int index)
        {
            var column = base[index];
            base.RemoveAt(index);
            RemoveComputeExtentWidth(column);
        }

        public new virtual bool Remove(Column column)
        {
            ArgumentNullException.ThrowIfNull(column);
            var ret = base.Remove(column);
            if (ret)
            {
                RemoveComputeExtentWidth(column);
            }
            return ret;
        }

        public new virtual int IndexOf(Column column) => base.IndexOf(column);
        public new virtual void Clear()
        {
            Control.ExtentWidth = 2 * Control.LineWidth;
            base.Clear();
        }

        public override string ToString() => string.Join(", ", this);
    }
}
namespace TreeViewList
{
    public class DrawCellEventArgs : DrawEventArgs
    {
        public DrawCellEventArgs(Graphics graphics, Row row, Column column, Cell cell, Rectangle layout, StringFormat format)
            : base(graphics)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(row);
            ArgumentNullException.ThrowIfNull(column);
            ArgumentNullException.ThrowIfNull(cell);
            ArgumentNullException.ThrowIfNull(format);
            Row = row;
            Column = column;
            Cell = cell;
            Layout = layout;
            Format = format;
        }

        public Row Row { get; }
        public Column Column { get; }
        public Cell Cell { get; }
        public StringFormat Format { get; }
        public virtual Rectangle Layout { get; set; }
    }
}

namespace TreeViewList
{
    public class DrawColumnEventArgs : DrawEventArgs
    {
        public DrawColumnEventArgs(Graphics graphics, Column column, Rectangle layout, StringFormat format)
            : base(graphics)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(column);
            ArgumentNullException.ThrowIfNull(format);
            Column = column;
            Layout = layout;
            Format = format;
        }

        public Column Column { get; }
        public Rectangle Layout { get; }
        public StringFormat Format { get; }
    }
}

namespace TreeViewList
{
    public class DrawEventArgs : HandledEventArgs
    {
        public DrawEventArgs(Graphics graphics)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            Graphics = graphics;
        }

        public Graphics Graphics { get; }
    }
}

namespace TreeViewList
{
    public class DrawRowEventArgs : DrawEventArgs
    {
        public DrawRowEventArgs(Graphics graphics, Row row, Rectangle layout)
            : base(graphics)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(row);
            Row = row;
            Layout = layout;
        }

        public Row Row { get; }
        public Rectangle Layout { get; }
    }
}

namespace TreeViewList
{
    public interface IRowContainer
    {
        TreeViewListControl Control { get; }
        RowCollection Rows { get; }
    }
}
namespace TreeViewList
{
    public class Row : IRowContainer
    {
        private bool? _isExpandable;
        private bool _isSelected;
        private bool _isExpanded;
        private static long _key;

        protected internal Row(IRowContainer parent)
        {
            ArgumentNullException.ThrowIfNull(parent);
            Container = parent;
            ChildRows = new RowCollection(this);
            Cells = new CellCollection(this);
            Key = Interlocked.Increment(ref _key);
        }

        internal long Key { get; }
        public int Index => Container.Rows.IndexOf(this);
        public IRowContainer Container { get; }
        public RowCollection ChildRows { get; }
        public CellCollection Cells { get; }
        public Row? ParentRow => Container as Row;
        public TreeViewListControl Control => Container as TreeViewListControl ?? Container.Control;
        RowCollection IRowContainer.Rows => ChildRows;
        public virtual object? Tag { get; set; }
        public string FirstCellText => Cells.Count > 0 ? Cells[0].ValueAsString : string.Empty;

        public virtual bool IsSelected { get => _isSelected; set => SetSelected(value, Control.SelectionMode); }
        internal void SetSelected(bool value, SelectionMode mode)
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            Control.ToggleSelected(this, mode);
        }

        public virtual bool? IsExpandable
        {
            get => _isExpandable ?? ChildRows.Count > 0;
            set
            {
                if (_isExpandable == value)
                    return;

                _isExpandable = value;
            }
        }

        public virtual bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value)
                    return;

                _isExpanded = value;
                if (_isExpanded)
                {
                    Control.ExpandRow(this);
                }
                else
                {
                    Control.CollapseRow(this);
                }
            }
        }

        public Row? PreviousRow
        {
            get
            {
                var index = Index;
                if (index == 0)
                    return null;

                return Container.Rows[index - 1];
            }
        }

        public Row? NextRow
        {
            get
            {
                var index = Index;
                if (index + 1 >= Container.Rows.Count)
                    return null;

                return Container.Rows[index + 1];
            }
        }

        public IEnumerable<Row> AllChildRows
        {
            get
            {
                foreach (var child in ChildRows)
                {
                    yield return child;
                    foreach (var grandChild in child.AllChildRows)
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        public IEnumerable<Row> ParentRows
        {
            get
            {
                var parentRow = ParentRow;
                if (parentRow != null)
                {
                    yield return parentRow;
                    foreach (var grandParent in parentRow.ParentRows)
                    {
                        yield return grandParent;
                    }
                }
            }
        }

        public IEnumerable<IRowContainer> Containers
        {
            get
            {
                var container = Container;
                yield return container;
                if (container is Row row)
                {
                    foreach (var grandParent in row.Containers)
                    {
                        yield return grandParent;
                    }
                }
            }
        }

        public override string ToString() => FirstCellText;

        public virtual void EnsureVisible()
        {
            ExpandHierarchy();
            Control.EnsureVisible(this);
        }

        public virtual void ExpandHierarchy()
        {
            foreach (var parent in ParentRows)
            {
                parent.Expand();
            }
        }

        public virtual void Expand() => IsExpanded = true;
        public virtual void Collapse() => IsExpanded = false;
        public virtual void ExpandAll()
        {
            IsExpanded = true;
            foreach (var row in ChildRows)
            {
                row.ExpandAll();
            }
        }

        public virtual void CollapseAll()
        {
            IsExpanded = false;
            foreach (var row in ChildRows)
            {
                row.CollapseAll();
            }
        }
    }
}

namespace TreeViewList
{
    public class RowCollapsedEventArgs : RowEventArgs
    {
        public RowCollapsedEventArgs(Row row)
            : base(row)
        {
        }
    }
}

namespace TreeViewList
{
    public class RowCollection : BaseList<Row>
    {
        public RowCollection(IRowContainer container)
        {
            ArgumentNullException.ThrowIfNull(container);
            Container = container;
        }

        public IRowContainer Container { get; }
        public TreeViewListControl Control => Container as TreeViewListControl ?? Container.Control;

        public virtual Row Add()
        {
            var row = new Row(Container);
            base.Add(row);
            Control.AddRow(row);
            return row;
        }

        public virtual Row Insert(int index)
        {
            var row = new Row(Container);
            base.Insert(index, row);
            Control.AddRow(row);
            return row;
        }

        public new virtual void RemoveAt(int index)
        {
            var row = base[index];
            base.RemoveAt(index);
            Control.RemoveRow(row);
        }

        public new virtual bool Remove(Row row)
        {
            ArgumentNullException.ThrowIfNull(row);
            var ret = base.Remove(row);
            if (ret)
            {
                Control.RemoveRow(row);
            }
            return ret;
        }

        public new virtual int IndexOf(Row column) => base.IndexOf(column);
        public new virtual void Clear()
        {
            Control.RemoveRows(Container);
            base.Clear();
        }

        public override string ToString() => string.Join(", ", this);
    }
}
namespace TreeViewList
{
    public abstract class RowEventArgs : HandledEventArgs
    {
        protected RowEventArgs(Row row)
        {
            ArgumentNullException.ThrowIfNull(row);
            Row = row;
        }

        public Row Row { get; }
    }
}

namespace TreeViewList
{
    public class RowExpandedEventArgs : RowEventArgs
    {
        public RowExpandedEventArgs(Row row)
            : base(row)
        {
        }
    }
}

namespace TreeViewList
{
    public partial class TreeViewListControl : Control, IRowContainer
    {
        public static int DefaultDefaultColumnWidth { get; set; } = 200;
        public static int DefaultDefaultMinColumnWidth { get; set; } = 40;
        public static int DefaultDefaultColumnPadding { get; set; } = 5;
        public static int DefaultRowHeight { get; set; } = 26;
        public static int DefaultRowOverhang { get; set; } = 11;
        public static int DefaultLineWidth { get; set; } = 1;
        public static int MouseTolerance { get; set; } = 4;
        public static Color DefaultLineColor { get; set; } = SystemColors.Control;

        public event EventHandler<DrawColumnEventArgs>? DrawingColumnHeader;
        public event EventHandler<DrawColumnEventArgs>? DrawnColumnHeader;
        public event EventHandler<DrawCellEventArgs>? DrawingCell;
        public event EventHandler<DrawCellEventArgs>? DrawnCell;
        public event EventHandler<RowExpandedEventArgs>? RowExpanded;
        public event EventHandler<RowCollapsedEventArgs>? RowCollapsed;
        public event EventHandler<DrawRowEventArgs>? DrawingRow;
        public event EventHandler<DrawRowEventArgs>? DrawnRow;
        public event EventHandler<EventArgs>? SelectionChanged;

        private readonly Dictionary<long, int> _rowsIndex = new();
        private readonly List<CachedRow> _cachedRows = new();
        private readonly List<Row> _selectedRows = new();
        private Brush? _backColorBrush;
        private Brush? _rowSelectedBrush;
        private SelectionMode _selectionMode;
        private int _extentHeight;
        private int _extentWidth;
        private Pen? _linePen;
        private int _rowHeight;
        private int _headerHeight;
        private int _rowOverhang;
        private int _defaultColumnWidth;
        private int _defaultMinColumnWidth;
        private bool _drawColumnsHeader;
        private bool _drawLastColumnRightLine;
        private bool _drawLastRowBottomLine;
        private MovingColumnHeader? _movingColumnHeader;
        private Row? _rowUnderMouse;
        private Row? _focusedRow;

        public TreeViewListControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            BackColor = Color.White;

            Rows = new RowCollection(this);
            Columns = new ColumnCollection(this);

            DefaultColumnWidth = DefaultDefaultColumnWidth;
            DefaultMinColumnWidth = DefaultDefaultMinColumnWidth;
            RowHeight = DefaultRowHeight;
            HeaderHeight = DefaultRowHeight;
            RowOverhang = DefaultRowOverhang;
            LineWidth = DefaultLineWidth;
            LineColor = DefaultLineColor;
            DrawColumnsHeader = true;
            DrawLastColumnRightLine = true;
            DrawLastRowBottomLine = true;

            ExtentWidth = 2 * LineWidth;
            ExtentHeight = LineWidth + (DrawColumnsHeader ? LineWidth + HeaderHeight : 0);

            HorizontalScrollBar.Visible = true;
            HorizontalScrollBar.ValueChanged += (s, e) => OnHorizontalScrollBarValueChanged();
            Controls.Add(HorizontalScrollBar);
            VerticalScrollBar.Visible = true;
            VerticalScrollBar.ValueChanged += (s, e) => OnVerticalScrollBarValueChanged();
            Controls.Add(VerticalScrollBar);
            UpdateScrollBars();
        }

        protected HScrollBar HorizontalScrollBar { get; } = new();
        protected VScrollBar VerticalScrollBar { get; } = new();
        public virtual int ExpanderSize => LogicalToDeviceUnits(9);
        public virtual int ExpanderPadding => LogicalToDeviceUnits(4);
        public Row? RowUnderMouse => _rowUnderMouse;
        public Row? FocusedRow => _focusedRow;

        TreeViewListControl IRowContainer.Control => this;

        public RowCollection Rows { get; }
        public ColumnCollection Columns { get; }

        private bool IsSelectionMultiple => SelectionMode == SelectionMode.MultiSimple || SelectionMode == SelectionMode.MultiExtended;
        public virtual SelectionMode SelectionMode
        {
            get => _selectionMode;
            set
            {
                if (_selectionMode == value)
                    return;

                _selectionMode = value;
                switch (_selectionMode)
                {
                    case SelectionMode.None:
                        if (_selectedRows.Count > 0)
                        {
                            _selectedRows.Clear();
                            OnSelectionChanged(this, EventArgs.Empty);
                        }
                        break;

                    case SelectionMode.One:
                        if (_selectedRows.Count > 1)
                        {
                            _selectedRows.RemoveRange(1, _selectedRows.Count - 1);
                            OnSelectionChanged(this, EventArgs.Empty);
                        }
                        break;
                }
            }
        }

        // includes header
        public int ExtentHeight
        {
            get => _extentHeight;
            internal set
            {
                if (_extentHeight == value)
                    return;

                _extentHeight = value;
                UpdateScrollBarsVisibility();
                UpdateScrollBars();
            }
        }

        public int ExtentWidth
        {
            get => _extentWidth;
            internal set
            {
                if (_extentWidth == value)
                    return;

                _extentWidth = value;
                UpdateScrollBarsVisibility();
                UpdateScrollBars();
            }
        }

        public virtual bool DrawColumnsHeader
        {
            get => _drawColumnsHeader;
            set
            {
                if (_drawColumnsHeader == value)
                    return;

                _drawColumnsHeader = value;
            }
        }

        public virtual bool DrawLastColumnRightLine
        {
            get => _drawLastColumnRightLine;
            set
            {
                if (_drawLastColumnRightLine == value)
                    return;

                _drawLastColumnRightLine = value;
            }
        }

        public virtual bool DrawLastRowBottomLine
        {
            get => _drawLastRowBottomLine;
            set
            {
                if (_drawLastRowBottomLine == value)
                    return;

                _drawLastRowBottomLine = value;
            }
        }

        public virtual int VerticalOffset
        {
            get => VerticalScrollBar.Value;
            set
            {
                if (VerticalScrollBar.Value == value)
                    return;

                value = Math.Max(0, Math.Min(value, ExtentHeight - Height));
                VerticalScrollBar.Value = value;
            }
        }

        public virtual int HorizontalOffset
        {
            get => HorizontalScrollBar.Value;
            set
            {
                if (HorizontalScrollBar.Value == value)
                    return;

                value = Math.Max(0, Math.Min(value, ExtentWidth - Width));
                HorizontalScrollBar.Value = value;
            }
        }

        private int RowHeightWithLine => RowHeight + LineWidth;
        public virtual int RowHeight
        {
            get => _rowHeight;
            set
            {
                if (_rowHeight == value)
                    return;

                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _rowHeight = value;
            }
        }

        private int HeaderHeightWithLine => DrawColumnsHeader ? HeaderHeight + LineWidth : 0;
        public virtual int HeaderHeight
        {
            get => _headerHeight;
            set
            {
                if (_headerHeight == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _headerHeight = value;
            }
        }

        public virtual int RowOverhang
        {
            get => _rowOverhang;
            set
            {
                if (_rowOverhang == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _rowOverhang = value;
            }
        }

        public virtual int DefaultColumnWidth
        {
            get => _defaultColumnWidth;
            set
            {
                if (_defaultColumnWidth == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _defaultColumnWidth = value;
            }
        }

        public virtual int DefaultMinColumnWidth
        {
            get => _defaultMinColumnWidth;
            set
            {
                if (_defaultMinColumnWidth == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _defaultMinColumnWidth = value;
            }
        }

        public virtual Color LineColor
        {
            get => _linePen?.Color ?? DefaultLineColor;
            set
            {
                if (LineColor == value)
                    return;

                var width = LineWidth;

                _linePen?.Dispose();
                _linePen = new Pen(value, width);
            }
        }

        public virtual int LineWidth
        {
            get => (int)(_linePen?.Width ?? DefaultLineWidth);
            set
            {
                if (LineWidth == value)
                    return;

                var before = LineWidth;
                var color = LineColor;
                _linePen?.Dispose();
                _linePen = new Pen(color, value);

                ExtentWidth -= 2 * LineWidth + before * Math.Max(0, Columns.Count - 1);
                ExtentWidth += 2 * LineWidth + value * Math.Max(0, Columns.Count - 1);
            }
        }

        public virtual Brush? RowSelectedBrush
        {
            get => _rowSelectedBrush;
            set
            {
                if (_rowSelectedBrush == value)
                    return;

                _rowSelectedBrush?.Dispose();
                _rowSelectedBrush = value;
            }
        }

        public IEnumerable<Row> AllRows
        {
            get
            {
                foreach (var row in Rows)
                {
                    yield return row;
                    foreach (var child in row.AllChildRows)
                    {
                        yield return child;
                    }
                }
            }
        }

        public Row? SelectedRow => _selectedRows.FirstOrDefault();
        public IEnumerable<Row> SelectedRows => _selectedRows;
        public IEnumerable<Row> VisibleRows => VisibleCachedRows.Select(r => r.Row);
        protected IEnumerable<CachedRow> VisibleCachedRows
        {
            get
            {
                GetVisibleRows(out var index, out var count);
                if (count == 0)
                    yield break;

                for (var i = index; i < _cachedRows.Count; i++)
                {
                    yield return _cachedRows[i];
                    count--;
                    if (count == 0)
                        yield break;
                }
            }
        }

        public virtual Row? GetRow(Point point) => GetCachedRow(point)?.Row;
        protected virtual CachedRow? GetCachedRow(Point point)
        {
            var delta = point.Y - HeaderHeightWithLine + VerticalOffset;
            if (delta < 0)
                return null;

            var index = delta / RowHeightWithLine;
            if (index < 0 || index >= _cachedRows.Count)
                return null;

            return _cachedRows[index];
        }

        private void GetVisibleRows(out int index, out int count)
        {
            index = VerticalOffset / RowHeightWithLine;
            count = 1 + (ClientSize.Height - HeaderHeightWithLine) / RowHeightWithLine;
        }

        public virtual Rectangle GetRowBounds(Row row) => GetRowBounds(row, out _);
        private Rectangle GetRowBounds(Row row, out int index)
        {
            ArgumentNullException.ThrowIfNull(nameof(row));
            if (row.Control != this)
                throw new ArgumentException(null, nameof(row));

            if (!_rowsIndex.TryGetValue(row.Key, out index))
                return Rectangle.Empty;

            return new Rectangle(LineWidth, LineWidth + index * RowHeightWithLine, ExtentWidth - 2 * LineWidth, RowHeight);
        }

        public Rectangle GetRowClientBounds(Row row) => GetRowClientBounds(row, out _);
        private Rectangle GetRowClientBounds(Row row, out int index)
        {
            var bounds = GetRowBounds(row, out index);
            if (!bounds.IsEmpty)
            {
                bounds.X -= HorizontalOffset;
                bounds.Y += -VerticalOffset + HeaderHeightWithLine;
            }
            return bounds;
        }

        public Rectangle GetRowClientExtenderBounds(Row row)
        {
            var bounds = GetRowClientBounds(row, out var index);
            if (!bounds.IsEmpty && Columns.Count > 0)
            {
                var cachedRow = _cachedRows[index];
                var size = ExpanderPadding * 2 + ExpanderSize;
                var expanded = new Rectangle(bounds.X + cachedRow.Level * RowOverhang, bounds.Y + (bounds.Height - size) / 2, size, size);
                bounds.Intersect(expanded);
            }
            return bounds;
        }

        protected virtual void OnHorizontalScrollBarValueChanged()
        {
            Invalidate();
        }

        protected virtual void OnVerticalScrollBarValueChanged()
        {
            Invalidate();
        }

        private void UpdateScrollBarsVisibility()
        {
            var invisibleWidth = Math.Max(0, ExtentWidth - ClientSize.Width);
            HorizontalScrollBar.Visible = invisibleWidth > 0;
            HorizontalScrollBar.LargeChange = ClientSize.Width;
            HorizontalScrollBar.Maximum = ExtentWidth;
            HorizontalOffset = Math.Min(invisibleWidth, HorizontalOffset);

            var invisibleHeight = Math.Max(0, ExtentHeight - Height);
            VerticalScrollBar.Visible = invisibleHeight > 0;
            VerticalScrollBar.LargeChange = ClientSize.Height;
            VerticalScrollBar.Maximum = ExtentHeight;
            VerticalOffset = Math.Min(invisibleHeight, VerticalOffset);
        }

        private void UpdateScrollBars()
        {
            var size = ClientSize;
            if (size.Width <= VerticalScrollBar.Width || size.Height < HorizontalScrollBar.Height)
                return;

            HorizontalScrollBar.Width = size.Width - (VerticalScrollBar.Visible ? VerticalScrollBar.Width : 0);
            HorizontalScrollBar.Top = size.Height - HorizontalScrollBar.Height;
            VerticalScrollBar.Height = size.Height - (HorizontalScrollBar.Visible ? HorizontalScrollBar.Height : 0);
            VerticalScrollBar.Left = size.Width - VerticalScrollBar.Width;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _linePen?.Dispose();
            _backColorBrush?.Dispose();
            _rowSelectedBrush?.Dispose();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            _backColorBrush?.Dispose();
            _backColorBrush = new SolidBrush(BackColor);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);

            DrawRows(e.Graphics);

            if (DrawColumnsHeader)
            {
                DrawHeader(e.Graphics);
            }

            DrawLines(e.Graphics);
            DrawScrollBarsCorner(e.Graphics);

            if (Focused)
            {
                DrawFocus(e.Graphics);
            }
        }

        protected override void RescaleConstantsForDpi(int deviceDpiOld, int deviceDpiNew)
        {
            base.RescaleConstantsForDpi(deviceDpiOld, deviceDpiNew);
            RowHeight = RowHeight * deviceDpiNew / deviceDpiOld;
            HeaderHeight = HeaderHeight * deviceDpiNew / deviceDpiOld;
            LineWidth = LineWidth * deviceDpiNew / deviceDpiOld;
            RowOverhang = RowOverhang * deviceDpiNew / deviceDpiOld;
            DefaultColumnWidth = DefaultColumnWidth * deviceDpiNew / deviceDpiOld;
            DefaultDefaultMinColumnWidth = DefaultDefaultMinColumnWidth * deviceDpiNew / deviceDpiOld;

            foreach (var column in Columns)
            {
                column.Width = column.Width * deviceDpiNew / deviceDpiOld;
                column.MinWidth = column.MinWidth * deviceDpiNew / deviceDpiOld;
                column.HorizontalPadding = column.HorizontalPadding * deviceDpiNew / deviceDpiOld;
                column.VerticalPadding = column.VerticalPadding * deviceDpiNew / deviceDpiOld;
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (_focusedRow != null)
                    {
                        var index = _rowsIndex[_focusedRow.Key];
                        if (index != 0)
                        {
                            var cachedRow = _cachedRows[index - 1];
                            SetFocusedRow(cachedRow.Row);

                            if (IsSelectionMultiple && ModifierKeys.HasFlag(Keys.Shift))
                            {
                                _cachedRows[index].Row.SetSelected(true, SelectionMode.MultiSimple);
                                _focusedRow.SetSelected(true, SelectionMode.MultiSimple);
                            }
                        }
                    }
                    break;

                case Keys.Down:
                    if (_focusedRow != null)
                    {
                        var index = _rowsIndex[_focusedRow.Key];
                        if (index + 1 < _rowsIndex.Count)
                        {
                            var cachedRow = _cachedRows[index + 1];
                            SetFocusedRow(cachedRow.Row);

                            if (IsSelectionMultiple && ModifierKeys.HasFlag(Keys.Shift))
                            {
                                _cachedRows[index].Row.SetSelected(true, SelectionMode.MultiSimple);
                                _focusedRow.SetSelected(true, SelectionMode.MultiSimple);
                            }
                        }
                    }
                    break;

                case Keys.Left:
                    if (_focusedRow != null)
                    {
                        if (_focusedRow.IsExpanded)
                        {
                            _focusedRow.Collapse();
                        }
                        else
                        {
                            SetFocusedRow(_focusedRow?.ParentRow);
                        }
                    }
                    break;

                case Keys.Right:
                    if (_focusedRow != null)
                    {
                        if (_focusedRow.IsExpanded)
                        {
                            var childRow = _focusedRow.ChildRows.FirstOrDefault();
                            if (childRow != null)
                            {
                                SetFocusedRow(childRow);
                            }
                        }
                        else
                        {
                            _focusedRow.Expand();
                        }
                    }
                    break;

                case Keys.Space:
                    if (_focusedRow != null)
                    {
                        _focusedRow.IsSelected = !_focusedRow.IsSelected;
                    }
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (Capture)
            {
                _movingColumnHeader?.MouseMove(e.Location);
                return;
            }

            var left = GetSplitColumn(e.Location);
            Cursor = left != null ? Cursors.VSplit : Cursors.Default;

            _rowUnderMouse = GetRow(e.Location);
            if (_rowUnderMouse != null && _rowUnderMouse.IsExpandable == true && GetRowClientExtenderBounds(_rowUnderMouse).Contains(e.Location))
            {
                Cursor = Cursors.Hand;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (e.Button == MouseButtons.Left)
            {
                var left = GetSplitColumn(e.Location);
                if (left != null)
                {
                    Capture = true;
                    _movingColumnHeader = new MovingColumnHeader(left, e.Location);
                    return;
                }
            }

            if (_rowUnderMouse != null)
            {
                SetFocusedRow(_rowUnderMouse);

                if (_rowUnderMouse.IsExpandable == true && GetRowClientExtenderBounds(_rowUnderMouse).Contains(e.Location))
                {
                    _rowUnderMouse.IsExpanded = !_rowUnderMouse.IsExpanded;
                }
                else
                {
                    _rowUnderMouse.IsSelected = !_rowUnderMouse.IsSelected;
                }
            }
        }

        internal void ToggleSelected(Row row, SelectionMode mode)
        {
            switch (mode)
            {
                case SelectionMode.One:
                    handleOne(row.IsSelected);
                    break;

                case SelectionMode.MultiSimple:
                    handleMulti(row.IsSelected);
                    break;

                case SelectionMode.MultiExtended:
                    if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        handleMulti(row.IsSelected);
                    }
                    else
                    {
                        handleOne(row.IsSelected);
                    }
                    break;
            }

            void handleOne(bool selected)
            {
                foreach (var selectedRow in _selectedRows.ToArray())
                {
                    if (selectedRow == row && selected)
                        continue;

                    selectedRow.IsSelected = false;
                    _selectedRows.Remove(selectedRow);
                    InvalidateRow(selectedRow);
                }

                if (selected && _selectedRows.Count == 0)
                {
                    _selectedRows.Add(row);
                    InvalidateRow(row);
                }
            }

            void handleMulti(bool selected)
            {
                var existing = _selectedRows.FirstOrDefault(r => r == row);
                if (existing == null && !selected)
                    return;

                if (existing == null)
                {
                    _selectedRows.Add(row);
                }
                else
                {
                    _selectedRows.Remove(existing);
                }

                InvalidateRow(row);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (Capture)
            {
                if (_movingColumnHeader != null)
                {
                    _movingColumnHeader = null;
                }
            }
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            UpdateScrollBarsVisibility();
            UpdateScrollBars();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UpdateScrollBars();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            var scrollLines = SystemInformation.MouseWheelScrollLines;
            if (scrollLines > 0)
            {
                // lines
                var offset = (RowHeight + LineWidth) * e.Delta / 120 * SystemInformation.MouseWheelScrollLines;
                VerticalOffset -= offset;
            }
            else if (scrollLines < 0)
            {
                // pages
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            InvalidateRow(_focusedRow);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            InvalidateRow(_focusedRow);
        }

        protected virtual void DrawFocus(Graphics graphics)
        {
            ArgumentNullException.ThrowIfNull(graphics);

            if (_focusedRow != null)
            {
                var bounds = GetRowClientBounds(_focusedRow);
                ControlPaint.DrawFocusRectangle(graphics, bounds);
            }
        }

        protected virtual void DrawRows(Graphics graphics)
        {
            ArgumentNullException.ThrowIfNull(graphics);

            var y = HeaderHeightWithLine + LineWidth;
            var x = -HorizontalOffset + LineWidth;
            var width = ExtentWidth - 2 * LineWidth;
            if (width <= 2 * LineWidth)
                return;

            var height = RowHeight;
            foreach (var cachedRow in VisibleCachedRows)
            {
                var overhang = cachedRow.Level * RowOverhang;
                var layout = new Rectangle(x + overhang, y, Math.Max(0, width - overhang), height);
                DrawRow(graphics, cachedRow.Row, layout);
                y += RowHeightWithLine;
            }
        }

        protected virtual void DrawRow(Graphics graphics, Row row, Rectangle layout)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(row);

            var e = new DrawRowEventArgs(graphics, row, layout);
            OnDrawingRow(this, e);
            if (e.Handled)
                return;

            if (row.IsSelected)
            {
                DrawSelection(graphics, layout);
            }

            var cellCount = Math.Min(Columns.Count, row.Cells.Count);
            var x = LineWidth;
            var cellLayout = new Rectangle(0, layout.Y, 0, layout.Height);
            for (var i = 0; i < cellCount; i++)
            {
                var cell = row.Cells[i];
                var col = Columns[i];

                cellLayout.Width = col.Width;
                if (i == 0)
                {
                    // keep overhang offset for 1st cell
                    cellLayout.X = layout.X;
                    cellLayout = DrawExpander(graphics, row, cellLayout);
                }
                else
                {
                    cellLayout.X = x;
                }

                var format = (StringFormat)StringFormat.GenericTypographic.Clone();
                if (i == 0)
                {
                    format.LineAlignment = StringAlignment.Center;
                    format.Alignment = StringAlignment.Near;
                }
                else
                {
                    format.LineAlignment = col.VerticalAlignment;
                    format.Alignment = col.HorizontalAlignment;
                }

                format.FormatFlags = col.FormatFlags;
                format.Trimming = col.Trimming;

                DrawCell(graphics, row, col, cell, cellLayout, format);

                x += col.Width + LineWidth;
            }

            OnDrawnRow(this, new DrawRowEventArgs(graphics, row, layout));
        }

        protected virtual void DrawCell(Graphics graphics, Row row, Column column, Cell cell, Rectangle layout, StringFormat format)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(row);
            ArgumentNullException.ThrowIfNull(column);
            ArgumentNullException.ThrowIfNull(cell);

            var e = new DrawCellEventArgs(graphics, row, column, cell, layout, format);
            OnDrawingCell(this, e);
            if (e.Handled)
                return;

            layout = e.Layout;
            layout.X += column.HorizontalPadding;
            layout.Y += column.VerticalPadding;
            layout.Width = Math.Max(0, layout.Width - column.HorizontalPadding * 2);
            layout.Height = Math.Max(0, layout.Height - column.VerticalPadding * 2);

            // sending width = 0 causes the string to be displayed (!)
            if (layout.Width == 0 || layout.Height == 0)
                return;

            graphics.DrawString(cell.Value?.ToString() ?? string.Empty, column.Font ?? Font, column.TextBrush ?? SystemBrushes.ControlText, layout, format);
            OnDrawnCell(this, new DrawCellEventArgs(graphics, row, column, cell, layout, format));
        }

        protected virtual Rectangle DrawExpander(Graphics graphics, Row row, Rectangle layout)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(row);
            if (Columns.Count == 0)
                return layout;

            var col0Width = Columns[0].Width;
            var bounds = new Rectangle(layout.Left + ExpanderPadding, layout.Top + (RowHeight - ExpanderSize) / 2, ExpanderSize, ExpanderSize);
            if (bounds.Left >= col0Width)
                return layout;

            if (bounds.Width == 0 || bounds.Height == 0)
                return layout;

            if (row.IsExpandable == true)
            {
                var clip = bounds;
                clip.X = layout.X;
                clip.Width = col0Width;
                var renderer = new VisualStyleRenderer(row.IsExpanded ? VisualStyleElement.TreeView.Glyph.Opened : VisualStyleElement.TreeView.Glyph.Closed);
                renderer.DrawBackground(graphics, bounds, clip);
            }

            layout.X += ExpanderPadding + ExpanderSize;
            layout.Width = Math.Max(0, layout.Width - (ExpanderPadding + ExpanderSize));
            return layout;
        }

        protected virtual void DrawSelection(Graphics graphics, Rectangle layout)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            if (RowSelectedBrush == null)
                return;

            Region? clip = null;
            if (!graphics.IsClipEmpty)
            {
                clip = graphics.Clip;
                graphics.ResetClip();
            }

            //layout.Width = ExtentWidth;
            Trace.WriteLine("DrawSelection bounds:" + layout);
            graphics.FillRectangle(RowSelectedBrush, layout);

            if (clip != null)
            {
                graphics.Clip = clip;
            }
        }

        protected virtual void DrawLines(Graphics graphics)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            if (Columns.Count == 0)
                return;

            var width = ExtentWidth - 1;
            var height = ExtentHeight - 1;
            _linePen ??= new Pen(LineColor, LineWidth);

            // rows
            graphics.DrawLine(_linePen, new Point(0, 0), new Point(width, 0));
            var y = HeaderHeightWithLine;
            foreach (var cachedRow in VisibleCachedRows)
            {
                graphics.DrawLine(_linePen, new Point(0, y), new Point(width, y));
                y += RowHeightWithLine;
            }

            if (DrawLastRowBottomLine)
            {
                graphics.DrawLine(_linePen, new Point(0, y), new Point(width, y));
            }

            // columns
            var x = -HorizontalOffset;
            foreach (var column in Columns)
            {
                graphics.DrawLine(_linePen, new Point(x, 0), new Point(x, height));
                x += column.Width + LineWidth;
            }

            if (DrawLastColumnRightLine)
            {
                graphics.DrawLine(_linePen, new Point(width, 0), new Point(width, height));
            }

            // header
            if (DrawColumnsHeader)
            {
                graphics.DrawLine(_linePen, new Point(0, HeaderHeightWithLine), new Point(width, HeaderHeightWithLine));
            }
        }

        protected virtual void DrawHeader(Graphics graphics)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            _backColorBrush ??= new SolidBrush(BackColor);
            graphics.FillRectangle(_backColorBrush, new Rectangle(LineWidth, LineWidth, Width - 2 * LineWidth, HeaderHeight));
            var left = -HorizontalOffset + LineWidth;
            foreach (var column in Columns)
            {
                var layout = new Rectangle(left, LineWidth, column.Width, HeaderHeight);
                DrawColumnHeader(graphics, column, layout);
                left += column.Width + LineWidth;
            }
        }

        protected internal virtual void DrawColumnHeader(Graphics graphics, Column column, Rectangle layout)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(column);

            var format = new StringFormat(StringFormatFlags.FitBlackBox)
            {
                LineAlignment = column.HeaderVerticalAlignment,
                Alignment = column.HeaderHorizontalAlignment,
                Trimming = column.HeaderTrimming,
                FormatFlags = column.HeaderFormatFlags,
            };

            var e = new DrawColumnEventArgs(graphics, column, layout, format);
            OnDrawingColumnHeader(this, e);
            if (e.Handled)
                return;

            graphics.DrawString(column.Text, column.HeaderFont ?? Font, column.HeaderTextBrush ?? SystemBrushes.ControlText, layout, format);
            OnDrawnColumnHeader(this, new DrawColumnEventArgs(graphics, column, layout, format));
        }

        protected virtual void OnSelectionChanged(object? sender, EventArgs e) => SelectionChanged?.Invoke(sender, e);
        protected virtual void OnRowExpanded(object? sender, RowExpandedEventArgs e) => RowExpanded?.Invoke(sender, e);
        protected virtual void OnRowCollapsed(object? sender, RowCollapsedEventArgs e) => RowCollapsed?.Invoke(sender, e);
        protected virtual void OnDrawingCell(object? sender, DrawCellEventArgs e) => DrawingCell?.Invoke(sender, e);
        protected virtual void OnDrawnCell(object? sender, DrawCellEventArgs e) => DrawnCell?.Invoke(sender, e);
        protected virtual void OnDrawingColumnHeader(object? sender, DrawColumnEventArgs e) => DrawingColumnHeader?.Invoke(sender, e);
        protected virtual void OnDrawnColumnHeader(object? sender, DrawColumnEventArgs e) => DrawnColumnHeader?.Invoke(sender, e);
        protected virtual void OnDrawingRow(object? sender, DrawRowEventArgs e) => DrawingRow?.Invoke(sender, e);
        protected virtual void OnDrawnRow(object? sender, DrawRowEventArgs e) => DrawnRow?.Invoke(sender, e);

        private void SetFocusedRow(Row? row)
        {
            if (row == null)
                return;

            if (_focusedRow == row)
                return;

            InvalidateRow(_focusedRow);
            _focusedRow = row;
            InvalidateRow(_focusedRow);
            EnsureVisible(row);
        }

        private void DrawScrollBarsCorner(Graphics graphics)
        {
            var dr = DisplayRectangle;
            var cr = ClientRectangle;
            var w = cr.Width - dr.Width;
            if (w <= 0)
                return;

            var h = cr.Height - dr.Height;
            if (h <= 0)
                return;

            graphics.FillRectangle(SystemBrushes.Control, new Rectangle(dr.Right, dr.Bottom, w, h));
        }

        private Column? GetSplitColumn(Point point)
        {
            if (Columns.Count == 0)
                return null;

            var width = Columns[0].Width;
            for (var i = 1; i < Columns.Count; i++)
            {
                var delta = point.X - width;
                if (Math.Abs(delta) <= MouseTolerance)
                    return Columns[i - 1];

                width += Columns[i].Width + LineWidth;
            }

            if (DrawLastColumnRightLine)
            {
                var delta = point.X - width;
                if (Math.Abs(delta) <= MouseTolerance)
                    return Columns[Columns.Count - 1];
            }
            return null;
        }

        internal void InvalidateRow(Row? row)
        {
            if (row == null)
                return;

            var bounds = GetRowClientBounds(row);
            if (!bounds.IsEmpty)
            {
                Trace.WriteLine("row:" + row + " bounds:" + bounds);
                Invalidate(bounds);
            }
        }

        internal void AddRow(Row row)
        {
            Row? prev;
            int prevIndex;
            if (row.Container is TreeViewListControl)
            {
                // a root row, it will always be in cache
                prev = row.PreviousRow;
                if (prev == null)
                {
                    insert(0, row, 0);
                    return;
                }

                prevIndex = _rowsIndex[prev.Key];
                insert(prevIndex + 1, row, 0);
                ComputeExtentHeight();
                return;
            }

            if (row.Container is Row parentRow)
            {
                if (!parentRow.IsExpanded)
                    return;

                if (!_rowsIndex.TryGetValue(parentRow.Key, out var parentIndex))
                    return;

                var cachedParentRow = _cachedRows[parentIndex];
                prev = row.PreviousRow;
                if (prev == null)
                {
                    insert(parentIndex + 1, row, cachedParentRow.Level + 1);
                    return;
                }

                prevIndex = _rowsIndex[prev.Key];
                insert(prevIndex + 1, row, cachedParentRow.Level + 1);
                ComputeExtentHeight();
                return;
            }

            void insert(int index, Row row, int level)
            {
                ReIndexCacheToEnd(index, 1);
                InsertRow(index, row, level);
            }
        }

        private void InsertRow(int index, Row row, int level)
        {
            _cachedRows.Insert(index, new CachedRow(row, level));
            _rowsIndex.Add(row.Key, index);

            if (_cachedRows.Count == 1)
            {
                _focusedRow = _cachedRows[0].Row;
            }
        }

        internal void RemoveRow(Row row)
        {
            RemoveChildrenRows(row, out var index);
            if (index >= 0)
            {
                _cachedRows.RemoveAt(index);
                _rowsIndex.Remove(row.Key);
                RemoveFromKnownRows(row);
                ReIndexCacheToEnd(index, 0);
                ComputeExtentHeight();
                Invalidate();
            }
        }

        private void RemoveFromKnownRows(Row row)
        {
            if (row == _focusedRow)
            {
                _focusedRow = null;
            }

            if (row == _rowUnderMouse)
            {
                _focusedRow = null;
            }
        }

        private void ClearRows()
        {
            _cachedRows.Clear();
            _rowsIndex.Clear();
            _focusedRow = null;
            _rowUnderMouse = null;
        }

        internal void RemoveRows(IRowContainer container)
        {
            if (container is Row row)
            {
                RemoveChildrenRows(row);
                return;
            }

            if (container == this)
            {
                ClearRows();
                Invalidate();
                return;
            }
        }

        internal void ExpandRow(Row row)
        {
            if (_rowsIndex.TryGetValue(row.Key, out var index))
            {
                var cachedRow = _cachedRows[index];
                var newIndex = AddChildrenRows(row, cachedRow.Level, index);
                if (newIndex > index)
                {
                    ReIndexCacheToEnd(newIndex + 1, 0);
                    ComputeExtentHeight();
                    Invalidate();
                }
                else
                {
                    InvalidateExpander(row);
                }
            }
            OnRowExpanded(this, new RowExpandedEventArgs(row));
        }

        private int AddChildrenRows(Row row, int level, int index)
        {
            foreach (var child in row.ChildRows)
            {
                index++;
                InsertRow(index, child, level + 1);
                if (child.IsExpanded)
                {
                    index = AddChildrenRows(child, level + 1, index);
                }
            }
            return index;
        }

        internal void CollapseRow(Row row)
        {
            RemoveChildrenRows(row);
            OnRowCollapsed(this, new RowCollapsedEventArgs(row));
        }

        private void RemoveChildrenRows(Row row) => RemoveChildrenRows(row, out _, out _);
        private void RemoveChildrenRows(Row row, out int index) => RemoveChildrenRows(row, out index, out _);
        private void RemoveChildrenRows(Row row, out int index, out CachedRow? cachedRow)
        {
            if (_rowsIndex.TryGetValue(row.Key, out index))
            {
                cachedRow = _cachedRows[index];
                var removedKeys = RemoveChildrenRows(cachedRow.Level, index);
                if (removedKeys.Count > 0)
                {
                    for (var i = 0; i < removedKeys.Count; i++)
                    {
                        var irow = _cachedRows[index + 1];
                        _cachedRows.RemoveAt(index + 1);
                        RemoveFromKnownRows(irow.Row);
                    }
                    removedKeys.ForEach(k => _rowsIndex.Remove(k));

                    ReIndexCacheToEnd(index + 1, 0);
                    ComputeExtentHeight();
                    Invalidate();
                }
                else
                {
                    InvalidateExpander(row);
                }
            }
            else
            {
                index = -1;
                cachedRow = null;
            }
        }

        private List<long> RemoveChildrenRows(int level, int index)
        {
            var keys = new List<long>();
            for (var i = index + 1; i < _cachedRows.Count; i++)
            {
                var cachedRow = _cachedRows[i];
                if (cachedRow.Level <= level)
                    break;

                keys.Add(cachedRow.Row.Key);
            }
            return keys;
        }

        private void ReIndexCacheToEnd(int startIndex, int offset)
        {
            for (var i = startIndex; i < _cachedRows.Count; i++)
            {
                _rowsIndex[_cachedRows[i].Row.Key] = i + offset;
            }
        }

        internal void EnsureVisible(Row row)
        {
            var rowIndex = _rowsIndex[row.Key];
            GetVisibleRows(out var index, out int count);
            int offset;
            if (rowIndex >= index + count)
            {
                offset = RowHeightWithLine * (rowIndex - count + 1);
            }
            else if (rowIndex < index)
            {
                offset = RowHeightWithLine * rowIndex;
            }
            else
                return;

            VerticalOffset = offset;
        }

        private void InvalidateExpander(Row row)
        {
            Invalidate(); // TODO: optimize
        }

        private void ComputeExtentHeight()
        {
            ExtentHeight = HeaderHeightWithLine + _cachedRows.Count * RowHeightWithLine + (DrawLastColumnRightLine ? LineWidth : 0);
        }

        protected class CachedRow
        {
            public CachedRow(Row row, int level)
            {
                ArgumentNullException.ThrowIfNull(row);
                Row = row;
                Level = level;
            }

            public Row Row { get; }
            public int Level { get; }

            public override string ToString() => new string('-', Level) + " " + Row.ToString();
        }

        private sealed class MovingColumnHeader
        {
            public MovingColumnHeader(Column column, Point position)
            {
                Column = column;
                Position = position;
                Width = column.Width;
            }

            public int Width;
            public Column Column;
            public Point Position;

            public void MouseMove(Point position)
            {
                var newWidth = Width + position.X - Position.X;
                if (newWidth <= 0)
                    return;

                var oldWidth = Column.Width;
                if (oldWidth == newWidth)
                    return;

                Column.Width = newWidth;
                var delta = Column.Width - oldWidth;
                Column.Control.ExtentWidth += delta;
                Column.Control.Invalidate();
            }
        }
    }
}

