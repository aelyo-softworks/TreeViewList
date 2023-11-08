using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ShellExplorer.Utilities
{
    public sealed class Win32FindData
    {
        private Win32FindData(string fullName)
        {
            FullName = fullName;
        }

        public string FullName { get; }
        public long Length { get; private set; }
        public FileAttributes Attributes { get; private set; }
        public DateTime CreationTimeUtc { get; private set; }
        public DateTime LastAccessTimeUtc { get; private set; }
        public DateTime LastWriteTimeUtc { get; private set; }
        public DateTime LastWriteTime => LastAccessTimeUtc == DateTime.MinValue ? DateTime.MinValue : LastWriteTimeUtc.ToLocalTime();
        public DateTime CreationTime => CreationTimeUtc == DateTime.MinValue ? DateTime.MinValue : CreationTimeUtc.ToLocalTime();
        public DateTime LastAccessTime => LastAccessTimeUtc == DateTime.MinValue ? DateTime.MinValue : LastAccessTimeUtc.ToLocalTime();
        public string Extension => FullName == null ? string.Empty : Path.GetExtension(FullName);
        public bool IsDirectory => Attributes.HasFlag(FileAttributes.Directory);
        public bool IsHidden => Attributes.HasFlag(FileAttributes.Hidden);
        public string Name => Path.GetFileName(FullName);

        public override string ToString() => FullName ?? string.Empty;

        public bool HasExtension(string extension) => HasExtension(new string[] { extension });
        public bool HasExtension(params string[] extensions) => HasExtension((IEnumerable<string>)extensions);
        public bool HasExtension(IEnumerable<string> extensions)
        {
            if (extensions == null)
                return false;

            var ext = Extension;
            foreach (var extension in extensions)
            {
                if (EqualsIgnoreCase(extension, ext))
                    return true;
            }
            return false;
        }

        private static string? Nullify(string? text)
        {
            if (text == null)
                return null;

            if (string.IsNullOrWhiteSpace(text))
                return null;

            var t = text.Trim();
            return t.Length == 0 ? null : t;
        }

        private static bool EqualsIgnoreCase(string? thisString, string? text, bool trim = false)
        {
            if (trim)
            {
                thisString = Nullify(thisString);
                text = Nullify(text);
            }

            if (thisString == null)
                return text == null;

            if (text == null)
                return false;

            if (thisString.Length != text.Length)
                return false;

            return string.Compare(thisString, text, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool DeleteFile(string path, bool unprotect = true)
        {
            ArgumentNullException.ThrowIfNull(path);
            var atts = PathGetAttributes(path);
            if (atts == null || atts.Value.HasFlag(FileAttributes.Directory))
                return false;

            if (unprotect && atts.Value.HasFlag(FileAttributes.ReadOnly))
            {
                PathSetAttributes(path, atts.Value & ~FileAttributes.ReadOnly);
            }

            return DeleteFileW(path);
        }

        public static bool PathSetAttributes(string path, FileAttributes attributes)
        {
            ArgumentNullException.ThrowIfNull(path);
            return SetFileAttributes(path, attributes);
        }

        public static bool PathExists(string? path) => PathGetAttributes(path).HasValue;
        public static FileAttributes? PathGetAttributes(string? path)
        {
            if (path == null)
                return null;

            var data = new WIN32_FILE_ATTRIBUTE_DATA();
            if (!GetFileAttributesEx(path, GetFileExInfoStandard, ref data))
                return null;

            return data.fileAttributes;
        }

        public static bool PathIsDirectory(string? path)
        {
            var atts = PathGetAttributes(path);
            if (!atts.HasValue)
                return false;

            return atts.Value.HasFlag(FileAttributes.Directory);
        }

        public static long? PathGetSize(string? path)
        {
            if (path == null)
                return null;

            var data = new WIN32_FILE_ATTRIBUTE_DATA();
            if (!GetFileAttributesEx(path, GetFileExInfoStandard, ref data))
                return null;

            if (data.fileAttributes.HasFlag(FileAttributes.Directory))
                return null;

            return data.fileSize;
        }

        public static bool PathIsFile(string? path)
        {
            var atts = PathGetAttributes(path);
            if (!atts.HasValue)
                return false;

            return !atts.Value.HasFlag(FileAttributes.Directory);
        }

        public static bool DirectoryIsNotEmpty(string path) => EnumerateFileSystemEntries(path).Any(f => !f.IsHidden);
        public static bool RemoveDirectory(string path) => path == null ? throw new ArgumentNullException(nameof(path)) : RemoveDirectoryW(path);
        public static bool DeleteDirectory(string path, bool recursive = true, bool unprotectFiles = true) => DeleteDirectory(path, false, recursive, unprotectFiles);
        public static bool DeleteDirectoryContent(string path, bool recursive = true, bool unprotectFiles = true) => DeleteDirectory(path, true, recursive, unprotectFiles);
        private static bool DeleteDirectory(string path, bool onlyContent, bool recursive, bool unprotect)
        {
            ArgumentNullException.ThrowIfNull(path);
            if (!PathIsDirectory(path))
                return true;

            var errors = false;
            var options = Win32FindDataEnumerateOptions.DepthFirst;
            if (recursive)
            {
                options |= Win32FindDataEnumerateOptions.Recursive;
                foreach (var data in EnumerateFileSystemEntries(path, options))
                {
                    if (data.IsDirectory)
                    {
                        if (!RemoveDirectoryW(data.FullName!))
                        {
                            errors = true;
                        }
                    }
                    else if (!DeleteFile(data.FullName!, unprotect))
                    {
                        errors = true;
                    }
                }
            }

            if (!onlyContent && !RemoveDirectory(path))
            {
                errors = true;
            }
            return !errors;
        }

        public static Win32FindData? FromPath(string? path)
        {
            if (path == null)
                return null;

            var di = new DirectoryInfo(path);
            if (di.Exists)
                return new Win32FindData(di.FullName)
                {
                    Attributes = di.Attributes,
                    CreationTimeUtc = di.CreationTimeUtc,
                    LastAccessTimeUtc = di.LastAccessTimeUtc,
                    LastWriteTimeUtc = di.LastWriteTimeUtc
                };

            var fi = new FileInfo(path);
            return fi.Exists ? new Win32FindData(fi.FullName)
            {
                Attributes = fi.Attributes,
                CreationTimeUtc = fi.CreationTimeUtc,
                LastAccessTimeUtc = fi.LastAccessTimeUtc,
                LastWriteTimeUtc = fi.LastWriteTimeUtc,
                Length = fi.Length
            }
            : null;
        }

        public static IEnumerable<Win32FindData> EnumerateFileSystemEntries(string directoryPath, Win32FindDataEnumerateOptions options = Win32FindDataEnumerateOptions.None, bool throwOnError = false)
        {
            ArgumentNullException.ThrowIfNull(directoryPath);
            directoryPath = Path.GetFullPath(directoryPath);
            if (!PathIsDirectory(directoryPath))
                yield break;

            var findPath = Normalize(directoryPath, true);
            if (!findPath.EndsWith("*"))
            {
                findPath = Path.Combine(findPath, "*");
            }

            var handle = FindFirstFile(findPath, out WIN32_FIND_DATA data);
            if (handle == INVALID_HANDLE_VALUE)
            {
                if (throwOnError)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                yield break;
            }

            do
            {
                if (Include(ref data))
                {
                    if (data.IsDirectory)
                    {
                        if (options.HasFlag(Win32FindDataEnumerateOptions.Recursive))
                        {
                            if (!options.HasFlag(Win32FindDataEnumerateOptions.DepthFirst) && !options.HasFlag(Win32FindDataEnumerateOptions.FilesOnly))
                            {
                                var fd = ToWin32FindData(data, directoryPath);
                                yield return fd;
                            }

                            foreach (var child in EnumerateFileSystemEntries(Path.Combine(directoryPath, data.cFileName), options, throwOnError))
                            {
                                yield return child;
                            }

                            if (options.HasFlag(Win32FindDataEnumerateOptions.DepthFirst) && !options.HasFlag(Win32FindDataEnumerateOptions.FilesOnly))
                            {
                                var fd = ToWin32FindData(data, directoryPath);
                                yield return fd;
                            }
                        }
                    }

                    if (!options.HasFlag(Win32FindDataEnumerateOptions.DirectoriesOnly))
                    {
                        var fd = ToWin32FindData(data, directoryPath);
                        yield return fd;
                    }
                }

                if (!FindNextFile(handle, out data))
                {
                    if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_FILES)
                    {
                        FindClose(handle);
                        yield break;
                    }
                }
            }
            while (true);
        }

        private static string Normalize(string path, bool expandEnvironmentVariables)
        {
            var expanded = expandEnvironmentVariables ? Environment.ExpandEnvironmentVariables(path) : path;
            if (expanded.StartsWith(_prefix))
                return expanded;

            return expanded.StartsWith(@"\\") ? string.Concat(_uncPrefix, expanded.AsSpan(2)) : _prefix + expanded;
        }

        private static bool Include(ref WIN32_FIND_DATA data) => data.cFileName != "." && data.cFileName != "..";
        private static Win32FindData ToWin32FindData(WIN32_FIND_DATA data, string directoryPath) => new(Path.Combine(directoryPath, data.cFileName))
        {
            Attributes = data.fileAttributes,
            CreationTimeUtc = DateTimeFromFileTimeUtc(data.ftCreationTimeHigh, data.ftCreationTimeLow),
            LastAccessTimeUtc = DateTimeFromFileTimeUtc(data.ftLastAccessTimeHigh, data.ftLastAccessTimeLow),
            LastWriteTimeUtc = DateTimeFromFileTimeUtc(data.ftLastWriteTimeHigh, data.ftLastWriteTimeLow),
            Length = data.fileSizeLow | (long)data.fileSizeHigh << 32
        };

        private static DateTime DateTimeFromFileTimeUtc(uint hi, uint lo)
        {
            try
            {
                var time = (long)hi << 32 | lo;
                return DateTime.FromFileTimeUtc(time);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private extern static bool GetFileAttributesEx(string name, int fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private extern static bool RemoveDirectoryW(string lpPathName);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private extern static bool DeleteFileW(string lpPathName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private extern static bool SetFileAttributes(string lpFileName, FileAttributes dwFileAttributes);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

        private const int GetFileExInfoStandard = 0;
        private const int ERROR_NO_MORE_FILES = 18;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);
        private const string _prefix = @"\\?\";
        private const string _uncPrefix = _prefix + @"UNC\";

        private struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public FileAttributes fileAttributes;
            public FILE_TIME ftCreationTime;
            public FILE_TIME ftLastAccessTime;
            public FILE_TIME ftLastWriteTime;
            public long fileSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FILE_TIME
        {
            public uint ftTimeLow;
            public uint ftTimeHigh;

            public FILE_TIME(long fileTime)
            {
                ftTimeLow = (uint)fileTime;
                ftTimeHigh = (uint)(fileTime >> 32);
            }

            public readonly bool IsZero => ftTimeHigh == 0 && ftTimeLow == 0;
            public readonly long ToTicks() => ((long)ftTimeHigh << 32) + ftTimeLow;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATA
        {
            public FileAttributes fileAttributes;
            public uint ftCreationTimeLow;
            public uint ftCreationTimeHigh;
            public uint ftLastAccessTimeLow;
            public uint ftLastAccessTimeHigh;
            public uint ftLastWriteTimeLow;
            public uint ftLastWriteTimeHigh;
            public uint fileSizeHigh;
            public uint fileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;

            public readonly bool IsDirectory => fileAttributes.HasFlag(FileAttributes.Directory);
            public override readonly string ToString() => cFileName;
        }
    }
}
