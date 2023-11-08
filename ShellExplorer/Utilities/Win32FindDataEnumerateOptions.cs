using System;

namespace ShellExplorer.Utilities
{
    [Flags]
    public enum Win32FindDataEnumerateOptions
    {
        None = 0x0,
        Recursive = 0x1,
        DepthFirst = 0x2,
        FilesOnly = 0x4,
        DirectoriesOnly = 0x8,
    }
}
