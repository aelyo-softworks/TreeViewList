using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using ShellExplorer.Interop;
using ShellExplorer.Utilities;

namespace ShellExplorer.Shell
{
    public class Folder : Item
    {
        private static readonly Lazy<Folder> _desktop = new(() => GetKnownFolder(KNOWNFOLDERID.FOLDERID_Desktop)!);
        public static Folder Desktop => _desktop.Value;

        public Folder(object? shellItem)
            : base(shellItem)
        {
        }

        public IEnumerable<Item> Children => EnumerateChildren();
        public IEnumerable<Item> EnumerateChildren(SHCONTF? flags = null)
        {
            //+ see https://devblogs.microsoft.com/oldnewthing/20150126-00/?p=44833
            IBindCtx? context = null;
            if (flags.HasValue)
            {
                context = Methods.CreateBindCtx();
                const string STR_ENUM_ITEMS_FLAGS = "SHCONTF";
                context.AddToBindCtx(STR_ENUM_ITEMS_FLAGS, flags.Value);
            }

            _shellItem.BindToHandler(context, BHID.BHID_EnumItems, typeof(IEnumShellItems).GUID, out var obj);
            if (obj == null)
                yield break;

            var enumItems = (IEnumShellItems)obj;
            do
            {
                IShellItem item;
                try
                {
                    var hr = enumItems.Next(1, out item, out _);
                    if (hr != 0)
                        break;
                }
                catch
                {
                    continue;
                }

                var child = ToItem(item);
                if (child != null)
                    yield return child;
            }
            while (true);
        }

        public static Folder? GetKnownFolder(Guid id, KNOWN_FOLDER_FLAG flags = 0, IntPtr? token = null, bool throwOnError = false)
        {
            Methods.SHGetKnownFolderItem(id, flags, token ?? IntPtr.Zero, typeof(IShellItem).GUID, out var obj).ThrowOnError(throwOnError);
            return ToItem(obj as IShellItem) as Folder;
        }
    }
}
