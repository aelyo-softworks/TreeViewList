using System;
using System.Drawing;

namespace TreeViewList
{
    public class DrawColumnEventArgs : DrawEventArgs
    {
        public DrawColumnEventArgs(Graphics graphics, Column column, Rectangle layout, StringFormat format)
            : base(graphics)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(column);
            ArgumentNullException.ThrowIfNull(format);
            Column = column;
            Layout = layout;
            Format = format;
        }

        public Column Column { get; }
        public Rectangle Layout { get; }
        public StringFormat Format { get; }
    }
}
