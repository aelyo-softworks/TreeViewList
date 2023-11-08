using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

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
