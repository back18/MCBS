using QuanLib.Core;
using QuanLib.Core.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public partial class FileBrowserBox : MultiPagePanel
    {
        public FileBrowserBox()
        {
            DriveList_Page = new(this);
            FileList_Page = new(this);
            Menu_Page = new(this);
            PagePanels.Add(DriveList_Page.PageKey, DriveList_Page);
            PagePanels.Add(FileList_Page.PageKey, FileList_Page);
            PagePanels.Add(Menu_Page.PageKey, Menu_Page);

            FileListSort = FileListSort.FileName;
            FileListSortDirection = ListSortDirection.Ascending;
            _forwards = new();

            OpenFile += OnOpenFile;
            OpenDirectory += OnOpenDirectory;
        }

        private readonly Stack<string> _forwards;

        public readonly DriveListPage DriveList_Page;

        public readonly FileListPage FileList_Page;

        public readonly MenuPage Menu_Page;

        public FileListSort FileListSort { get; set; }

        public ListSortDirection FileListSortDirection { get; set; }

        public event EventHandler<FileBrowserBox, EventArgs<FileInfo>> OpenFile;

        public event EventHandler<FileBrowserBox, EventArgs<DirectoryInfo>> OpenDirectory;

        protected virtual void OnOpenFile(FileBrowserBox sender, EventArgs<FileInfo> e) { }

        protected virtual void OnOpenDirectory(FileBrowserBox sender, EventArgs<DirectoryInfo> e) { }

        public override void Initialize()
        {
            base.Initialize();

            ShowFileList();
        }

        protected override void OnTextChanged(Control sender, ValueChangedEventArgs<string> e)
        {
            base.OnTextChanged(sender, e);

            string path = e.NewValue;

            if (string.IsNullOrEmpty(path) && OperatingSystem.IsWindows())
            {
                ActivePageKey = DriveList_Page.PageKey;
                DriveList_Page.UpdateList();
            }
            else
            {
                ActivePageKey = FileList_Page.PageKey;
                FileList_Page.Text = path;
            }

            if (_forwards.Count > 0)
            {
                string forward = _forwards.Peek();
                if (forward == path)
                    _forwards.Pop();
                else if (!forward.StartsWith(path))
                    _forwards.Clear();
            }
        }

        public void Backward()
        {
            if (ActivePage == Menu_Page)
            {
                ShowFileList();
                return;
            }

            string path = Text;
            string? directory = Path.GetDirectoryName(path);

            if (path is "" or "/")
                return;

            _forwards.Push(path);
            Text = directory ?? string.Empty;
        }

        public void Forward()
        {
            if (_forwards.TryPop(out var forward))
                Text = forward;
        }

        public void ShowFileList()
        {
            if (string.IsNullOrEmpty(Text) && OperatingSystem.IsWindows())
                ActivePageKey = DriveList_Page.PageKey;
            else
                ActivePageKey = FileList_Page.PageKey;
        }

        public void ShowMenu()
        {
            ActivePageKey = Menu_Page.PageKey;
        }
    }
}
