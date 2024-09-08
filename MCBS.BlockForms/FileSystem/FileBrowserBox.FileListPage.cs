using MCBS.Application;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Clipping;
using QuanLib.Core.Events;
using QuanLib.Core.Extensions;
using QuanLib.Game;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public partial class FileBrowserBox
    {
        public class FileListPage : PagePanel
        {
            public FileListPage(FileBrowserBox owner, string pageKey = "FileList") : base(pageKey)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;

                FileMeun_ListMenuBox = new();
                Open_Button = new();
                Rename_Button = new();
                Cut_Button = new();
                Copy_Button = new();
                Delete_Button = new();
            }

            private readonly FileBrowserBox _owner;

            private readonly ListMenuBox<Control> FileMeun_ListMenuBox;

            private readonly Button Open_Button;

            private readonly Button Rename_Button;

            private readonly Button Cut_Button;

            private readonly Button Copy_Button;

            private readonly Button Delete_Button;

            public override void Initialize()
            {
                base.Initialize();

                FileMeun_ListMenuBox.ClientSize = new(68, 18 * 4 + 5);
                FileMeun_ListMenuBox.MaxDisplayPriority = int.MaxValue;
                FileMeun_ListMenuBox.DisplayPriority = int.MaxValue - 1;
                FileMeun_ListMenuBox.Spacing = 1;
                FileMeun_ListMenuBox.FirstHandleRightClick = true;

                Open_Button.Text = "打开";
                Open_Button.ClientSize = new(64, 16);
                Open_Button.RightClick += Open_Button_RightClick;
                Open_Button.RightClick += CloseFileMenu_RightClick;
                Open_Button.RightClick += ClearSelectedFileBoxs_RightClick;
                FileMeun_ListMenuBox.AddedChildControlAndLayout(Open_Button);

                Rename_Button.Text = "重命名";
                Rename_Button.ClientSize = new(64, 16);
                Rename_Button.RightClick += Rename_Button_RightClick;
                Rename_Button.RightClick += CloseFileMenu_RightClick;
                Rename_Button.RightClick += ClearSelectedFileBoxs_RightClick;
                FileMeun_ListMenuBox.AddedChildControlAndLayout(Rename_Button);

                Cut_Button.Text = "剪切";
                Cut_Button.ClientSize = new(64, 16);
                Cut_Button.RightClick += Cut_Button_RightClick;
                Cut_Button.RightClick += CloseFileMenu_RightClick;
                Cut_Button.RightClick += ClearSelectedFileBoxs_RightClick;
                FileMeun_ListMenuBox.AddedChildControlAndLayout(Cut_Button);

                Copy_Button.Text = "复制";
                Copy_Button.ClientSize = new(64, 16);
                Copy_Button.RightClick += Copy_Button_RightClick;
                Copy_Button.RightClick += CloseFileMenu_RightClick;
                Copy_Button.RightClick += ClearSelectedFileBoxs_RightClick;
                FileMeun_ListMenuBox.AddedChildControlAndLayout(Copy_Button);

                Delete_Button.Text = "删除";
                Delete_Button.ClientSize = new(64, 16);
                Delete_Button.RightClick += Delete_Button_RightClick;
                Delete_Button.RightClick += CloseFileMenu_RightClick;
                Delete_Button.RightClick += ClearSelectedFileBoxs_RightClick;
                FileMeun_ListMenuBox.AddedChildControlAndLayout(Delete_Button);

                UpdateList(true);
            }

            protected override void OnRightClick(Control sender, CursorEventArgs e)
            {
                base.OnRightClick(sender, e);

                FileBox? fileBox = GetHoverFileBox(e);
                if (fileBox is null)
                    return;

                fileBox.IsSelected = !fileBox.IsSelected;
                ChildControls.Remove(FileMeun_ListMenuBox);
            }

            protected override void OnDoubleRightClick(Control sender, CursorEventArgs e)
            {
                base.OnDoubleRightClick(sender, e);

                FileBox? fileBox = GetHoverFileBox(e);
                if (fileBox is null)
                    return;

                if (fileBox.IsSelected)
                    OpenMenu(e.Position, FileMeun_ListMenuBox);
                else if (fileBox.FileSystemInfo is DirectoryInfo directoryInfo)
                    _owner.Text = directoryInfo.FullName;
                else if (fileBox.FileSystemInfo is FileInfo fileInfo)
                    _owner.OpenFile.Invoke(_owner, new(fileInfo));
            }

            protected override void OnTextChanged(Control sender, ValueChangedEventArgs<string> e)
            {
                base.OnTextChanged(sender, e);

                UpdateList(true);
            }

            protected override void OnAfterFrame(Control sender, EventArgs e)
            {
                base.OnAfterFrame(sender, e);

                if (MinecraftBlockScreen.Instance.SystemTick % 20 == 0)
                    UpdateList(false);
            }

            public void UpdateList(bool reload)
            {
                FileBox[] fileBoxes = GetFileBoxs(reload);

                if (reload)
                {
                    UpdateList(fileBoxes);
                    OffsetPosition = Point.Empty;
                }
                else
                {
                    FileBox[] childControls = ChildControls.OfType<FileBox>().OrderBy(i => i.ClientLocation.Y).ToArray();
                    if (!childControls.OrderReferenceEquals(fileBoxes))
                        UpdateList(fileBoxes);
                }
            }

            private void UpdateList(IList<FileBox> fileBoxes)
            {
                ArgumentNullException.ThrowIfNull(fileBoxes, nameof(fileBoxes));

                ChildControls.Clear();

                foreach (FileBox fileBox in fileBoxes)
                    ChildControls.Add(fileBox);

                if (fileBoxes.Count == 0)
                {
                    PageSize = ClientSize;
                    OffsetPosition = Point.Empty;
                    return;
                }

                LayoutHelper.FillLayoutDown(this, fileBoxes, 0);
                PageSize = new(Math.Max(ClientSize.Width, fileBoxes[^1].Width), Math.Max(ClientSize.Height, fileBoxes[^1].BottomLocation + 1));
                OffsetPosition = new(0, Math.Clamp(OffsetPosition.Y, 0, PageSize.Height - ClientSize.Height));
            }

            private FileBox[] GetFileBoxs(bool isReload)
            {
                FileSystemInfo[] fileSystemInfos = GetFileSystemInfos();
                List<FileBox> result = [];

                if (isReload)
                {
                    foreach (FileSystemInfo fileSystemInfo in fileSystemInfos)
                    {
                        FileBox fileBox = CreateFileBox(fileSystemInfo);
                        result.Add(fileBox);
                    }
                }
                else
                {
                    Dictionary<string, FileBox> childControls = ChildControls.OfType<FileBox>().ToDictionary(i => i.FileSystemInfo.FullName, i => i);
                    foreach (FileSystemInfo fileSystemInfo in fileSystemInfos)
                    {
                        if (!childControls.TryGetValue(fileSystemInfo.FullName, out var fileBox))
                            fileBox = CreateFileBox(fileSystemInfo);
                        result.Add(fileBox);
                    }
                }

                return result.ToArray();
            }

            private FileSystemInfo[] GetFileSystemInfos()
            {
                DirectoryInfo currentDirectory;
                DirectoryInfo[] directoryInfos;
                FileInfo[] fileInfos;

                try
                {
                    currentDirectory = new(Text);
                    directoryInfos = currentDirectory.GetDirectories();
                    fileInfos = currentDirectory.GetFiles();
                }
                catch
                {
                    return [];
                }

                return FileListUtil.Sort(directoryInfos, fileInfos, _owner.FileListSort, _owner.FileListSortDirection);
            }

            private FileBox CreateFileBox(FileSystemInfo fileSystemInfo)
            {
                ArgumentNullException.ThrowIfNull(fileSystemInfo, nameof(fileSystemInfo));

                FileBox fileBox = new(fileSystemInfo);
                fileBox.Width = Math.Max(ClientSize.Width, fileBox.MinSize.Width);
                fileBox.Stretch = Direction.Right;
                return fileBox;
            }

            private void Open_Button_RightClick(Control sender, CursorEventArgs e)
            {
                FileBox[] fileBoxes = GetSelectedFileBoxs();
                foreach (FileBox fileBox in fileBoxes)
                {
                    if (fileBox.FileSystemInfo is DirectoryInfo directoryInfo)
                        _owner.OpenDirectory.Invoke(_owner, new(directoryInfo));
                    else if (fileBox.FileSystemInfo is FileInfo fileInfo)
                        _owner.OpenFile.Invoke(_owner, new(fileInfo));
                }
            }

            private void Rename_Button_RightClick(Control sender, CursorEventArgs e)
            {
                Form? form = GetForm();
                FileBox[] fileBoxes = GetSelectedFileBoxs();

                if (fileBoxes.Length != 1)
                {
                    if (form is not null)
                    {
                        _ = DialogBoxHelper.OpenMessageBoxAsync(
                            form,
                            "提醒",
                            "无法重命名多个文件或文件夹",
                            MessageBoxButtons.OK);
                        return;
                    }
                }

                ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents["System.FileRenameHandler"];
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [fileBoxes[0].FileSystemInfo.FullName], form);
            }

            private void Cut_Button_RightClick(Control sender, CursorEventArgs e)
            {
                FileBox[] fileBoxes = GetSelectedFileBoxs();
                string[] paths = fileBoxes.Select(s => s.FileSystemInfo.FullName).ToArray();
                MinecraftBlockScreen.Instance.Clipboard.SetFileDrop(paths, ClipboardMode.Move);
            }

            private void Copy_Button_RightClick(Control sender, CursorEventArgs e)
            {
                FileBox[] fileBoxes = GetSelectedFileBoxs();
                string[] paths = fileBoxes.Select(s => s.FileSystemInfo.FullName).ToArray();
                MinecraftBlockScreen.Instance.Clipboard.SetFileDrop(paths, ClipboardMode.Copy);
            }

            private void Delete_Button_RightClick(Control sender, CursorEventArgs e)
            {
                Form? form = GetForm();
                FileBox[] fileBoxes = GetSelectedFileBoxs();
                string[] paths = fileBoxes.Select(s => s.FileSystemInfo.FullName).ToArray();

                ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents["System.FileDeleteHandler"];
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, paths, form);
            }

            private void CloseFileMenu_RightClick(Control sender, CursorEventArgs e)
            {
                ChildControls.Remove(FileMeun_ListMenuBox);
            }

            private void ClearSelectedFileBoxs_RightClick(Control sender, CursorEventArgs e)
            {
                ChildControls.ClearSelecteds();
            }

            private void OpenMenu(Point position, ListMenuBox<Control> listMenuBox)
            {
                ArgumentNullException.ThrowIfNull(listMenuBox, nameof(listMenuBox));

                ChildControls.TryAdd(listMenuBox);
                listMenuBox.ClientLocation = new(
                    Math.Min(position.X, ClientSize.Width + OffsetPosition.X - listMenuBox.Width),
                    Math.Min(position.Y, ClientSize.Height + OffsetPosition.Y - listMenuBox.Height));
            }

            private FileBox? GetHoverFileBox(CursorEventArgs e)
            {
                ArgumentNullException.ThrowIfNull(e, nameof(e));

                return ChildControls
                    .GetHovers()
                    .Where(w => GetHoverCursors().Contains(e.CursorContext))
                    .OfType<FileBox>()
                    .FirstOrDefault();
            }

            private FileBox[] GetSelectedFileBoxs()
            {
                return ChildControls
                    .GetSelecteds()
                    .OfType<FileBox>()
                    .ToArray();
            }
        }
    }
}
