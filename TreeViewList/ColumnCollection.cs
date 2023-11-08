using System;

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