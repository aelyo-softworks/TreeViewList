using System;

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