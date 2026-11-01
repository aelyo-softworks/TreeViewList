using ShellN.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TreeViewList;

namespace ShellExplorer
{
    public partial class Main : Form
    {
        private readonly TreeViewListControl _tvl = new();

        public Main()
        {
            InitializeComponent();
            Controls.Add(_tvl);
            _tvl.Dock = DockStyle.Fill;
            _tvl.DrawingCell += OnDrawingCell;

            _tvl.Columns.Add("Name");
            _tvl.Columns.Add("Date modified");
            _tvl.Columns.Add("Type");

            var sizeColumn = _tvl.Columns.Add("Size");
            sizeColumn.HorizontalAlignment = StringAlignment.Far;
            _tvl.RowExpanded += OnRowExpanded;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var desktop = ShellFolder.Desktop;
            LoadFolder(_tvl, desktop);
        }

        private void OnDrawingCell(object? sender, DrawCellEventArgs e)
        {
            if (e.Column.Index == 0)
            {
                var iconWidth = 0;
                if (e.Row.Tag is Model model)
                {
                    iconWidth = DrawRowIcon(model.Item, e);
                }

                if (iconWidth > 0)
                {
                    iconWidth += 2;
                    var layout = e.Layout;
                    layout.X += iconWidth;
                    layout.Width = Math.Max(0, layout.Width - iconWidth);
                    e.Layout = layout;
                }
                return;
            }
        }

        private int DrawRowIcon(ShellItem item, DrawCellEventArgs e)
        {
            var handle = item.GetIconHandle(ShellN.SHIL.SHIL_SMALL);
            using var icon = Icon.FromHandle(handle);
            if (icon == null)
                return 0;

            var x = _tvl.ExpanderPadding;
            var y = (e.Layout.Height - icon.Height) / 2;
            e.Graphics.DrawIcon(icon, new Rectangle(e.Layout.X + x, e.Layout.Y + y, Math.Min(icon.Width, e.Column.Width - x + 1), icon.Height));
            return icon.Width;
        }

        private void LoadFolder(IRowContainer container, ShellFolder folder)
        {
            foreach (var item in folder.EnumerateChildren().Order(ItemComparer.Instance))
            {
                if (item != null)
                {
                    AddRow(container, item);
                }
            }
        }

        private void OnRowExpanded(object? sender, RowExpandedEventArgs e)
        {
            if (e.Row.Tag is Model model)
            {
                if (!model.Loaded && model.Item is ShellFolder folder)
                {
                    model.Loaded = true;
                    LoadFolder(e.Row, folder);
                }
            }
        }

        private void AddRow(IRowContainer container, ShellItem item)
        {
            var row = container.Rows.Add();
            row.Tag = new Model(item);
            row.Cells.Add(item.SIGDN_NORMALDISPLAY);
            row.Cells.Add(item.DateModified);
            row.Cells.Add(item.Type);
            row.Cells.Add(item.Size);

            if (item is ShellFolder folder)
            {
                if (HasAnyChildren(folder))
                {
                    row.IsExpandable = true;
                }
            }
            _tvl.Invalidate();
        }

        private static bool HasAnyChildren(ShellFolder folder)
        {
            foreach (var item in folder.EnumerateChildren())
            {
                if (item != null)
                {
                    item.Dispose();
                    return true;
                }
            }
            return false;
        }

        private sealed class Model(ShellItem item) : IDisposable
        {
            public bool Loaded;
            public ShellItem Item { get; } = item;

            public void Dispose() => Item.Dispose();
        }

        private sealed class ItemComparer : IComparer<ShellItem>
        {
            public static ItemComparer Instance { get; } = new();

            public int Compare(ShellItem? x, ShellItem? y)
            {
                ArgumentNullException.ThrowIfNull(x);
                ArgumentNullException.ThrowIfNull(y);

                if (x.IsFolder)
                {
                    if (!y.IsFolder)
                        return -1;
                }
                else if (y.IsFolder)
                    return 1;

                return x.SIGDN_NORMALDISPLAY?.CompareTo(y.SIGDN_NORMALDISPLAY) ?? 0;
            }
        }
    }
}