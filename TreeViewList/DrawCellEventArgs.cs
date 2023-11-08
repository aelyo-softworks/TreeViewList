using System;
using System.Drawing;

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
