using System;
using System.Drawing;

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
