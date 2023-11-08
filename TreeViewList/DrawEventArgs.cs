using System;
using System.ComponentModel;
using System.Drawing;

namespace TreeViewList
{
    public class DrawEventArgs : HandledEventArgs
    {
        public DrawEventArgs(Graphics graphics)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            Graphics = graphics;
        }

        public Graphics Graphics { get; }
    }
}
