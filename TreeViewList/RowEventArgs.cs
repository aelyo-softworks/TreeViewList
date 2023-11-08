using System;
using System.ComponentModel;

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
