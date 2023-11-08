using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShellExplorer.Shell;
using ShellExplorer.Utilities;
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
            var desktop = Folder.Desktop;
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

        private int DrawRowIcon(Item item, DrawCellEventArgs e)
        {
            using var icon = item.GetIconFromImageList(Interop.SHIL.SHIL_SMALL);
            if (icon == null)
                return 0;

            var x = _tvl.ExpanderPadding;
            var y = (e.Layout.Height - icon.Height) / 2;
            e.Graphics.DrawIcon(icon, new Rectangle(e.Layout.X + x, e.Layout.Y + y, Math.Min(icon.Width, e.Column.Width - x + 1), icon.Height));
            return icon.Width;
        }

        private static IEnumerable<Item> EnumerateChildren(Folder folder)
        {
            if (folder.ClassId == Folder.ShellFSFolder)
            {
                var path = folder.SIGDN_FILESYSPATH;
                if (Win32FindData.PathIsDirectory(path))
                    return folder.Children.OrderBy(c => c, ItemComparer.Instance);
            }

            return folder.Children;
        }

        private void LoadFolder(IRowContainer container, Folder folder)
        {
            foreach (var item in EnumerateChildren(folder))
            {
                AddRow(container, item);
            }
        }

        private void OnRowExpanded(object? sender, RowExpandedEventArgs e)
        {
            if (e.Row.Tag is Model model)
            {
                if (!model.Loaded && model.Item is Folder folder)
                {
                    model.Loaded = true;
                    LoadFolder(e.Row, folder);
                }
            }
        }

        private void AddRow(IRowContainer container, Item item)
        {
            var row = container.Rows.Add();
            row.Tag = new Model(item);
            row.Cells.Add(item.SIGDN_NORMALDISPLAY);
            row.Cells.Add(item.DateModified);
            row.Cells.Add(item.ItemTypeText);
            row.Cells.Add(item.Size);

            if (item is Folder folder)
            {
                if (folder.HasAnyChildren)
                {
                    row.IsExpandable = true;
                }
            }
            _tvl.Invalidate();
        }

        private sealed class Model
        {
            public Model(Item item)
            {
                Item = item;
            }

            public bool Loaded;
            public Item Item { get; }
        }

        private sealed class ItemComparer : IComparer<Item>
        {
            public static ItemComparer Instance = new();

            public int Compare(Item? x, Item? y)
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

                return x.SIGDN_NORMALDISPLAY.CompareTo(y.SIGDN_NORMALDISPLAY);
            }
        }
    }
}