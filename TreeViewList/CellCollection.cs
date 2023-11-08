using System;

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
