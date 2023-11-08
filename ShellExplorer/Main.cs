using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShellExplorer.Shell;
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

            _tvl.Columns.Add("Name");
            _tvl.Columns.Add("Date modified");
            _tvl.Columns.Add("Type");
            _tvl.Columns.Add("Size");
            _tvl.RowExpanded += OnRowExpanded;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var desktop = Folder.Desktop;
            LoadFolder(_tvl, desktop);
        }

        private void LoadFolder(IRowContainer container, Folder folder)
        {
            Task.Run(() =>
            {
                foreach (var item in folder.Children)
                {
                    AddRow(container, item);
                }
            });
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
            BeginInvoke(() =>
            {
                var row = container.Rows.Add();
                row.Tag = new Model(item);
                row.Cells.Add(item.SIGDN_NORMALDISPLAY);

                if (item is Folder folder)
                {
                    if (folder.Children.Any())
                    {
                        row.IsExpandable = true;
                    }
                }
                _tvl.Invalidate();
            });
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
    }
}