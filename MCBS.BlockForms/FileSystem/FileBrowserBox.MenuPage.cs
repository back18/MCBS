using MCBS.Application;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Clipping;
using QuanLib.Core.Extensions;
using QuanLib.Game;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public partial class FileBrowserBox
    {
        public class MenuPage : PagePanel
        {
            public MenuPage(FileBrowserBox owner, string pageKey = "Menu") : base(pageKey)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;

                DirectoryMenu_Label = new();
                DirectoryMenu_Panel = new();
                Refresh_Button = new();
                Paste_Button = new();
                NewFolder_Button = new();
                NewTextDocument_Button = new();
                SortOrderMenu_Label = new();
                SortOrderMenu_Panel = new();
                Sort_ComboButton = new();
                SortDirection_ComboButton = new();
            }

            private readonly FileBrowserBox _owner;

            private readonly Label DirectoryMenu_Label;

            private readonly Panel<Control> DirectoryMenu_Panel;

            private readonly Button Refresh_Button;

            private readonly Button Paste_Button;

            private readonly Button NewFolder_Button;

            private readonly Button NewTextDocument_Button;

            private readonly Label SortOrderMenu_Label;

            private readonly Panel<Control> SortOrderMenu_Panel;

            private readonly ComboButton<FileListSort> Sort_ComboButton;

            private readonly ComboButton<ListSortDirection> SortDirection_ComboButton;

            public override void Initialize()
            {
                base.Initialize();

                ChildControls.Add(DirectoryMenu_Label);
                DirectoryMenu_Label.Text = "在当前目录";
                DirectoryMenu_Label.ClientLocation = new(1, 1);

                ChildControls.Add(DirectoryMenu_Panel);
                DirectoryMenu_Panel.Stretch = Direction.Right;
                DirectoryMenu_Panel.ClientSize = new(ClientSize.Width - 4, 16 * 4 + 3);
                DirectoryMenu_Panel.LayoutDown(this, DirectoryMenu_Label, 1);
                DirectoryMenu_Panel.Skin.SetAllBackgroundColor(DirectoryMenu_Panel.Skin.BorderColor);

                DirectoryMenu_Panel.ChildControls.Add(Refresh_Button);
                Refresh_Button.Text = "刷新";
                Refresh_Button.Stretch = Direction.Right;
                Refresh_Button.BorderWidth = 0;
                Refresh_Button.ClientSize = new(DirectoryMenu_Panel.ClientSize.Width, 16);
                Refresh_Button.ClientLocation = Point.Empty;
                Refresh_Button.RightClick += Refresh_Button_RightClick;
                Refresh_Button.RightClick += ShowFileList_RightClick;

                DirectoryMenu_Panel.ChildControls.Add(Paste_Button);
                Paste_Button.Text = "粘贴";
                Paste_Button.Stretch = Direction.Right;
                Paste_Button.BorderWidth = 0;
                Paste_Button.ClientSize = new(DirectoryMenu_Panel.ClientSize.Width, 16);
                Paste_Button.LayoutDown(DirectoryMenu_Panel, Refresh_Button, 1);
                Paste_Button.RightClick += Paste_Button_RightClick;
                Paste_Button.RightClick += ShowFileList_RightClick;

                DirectoryMenu_Panel.ChildControls.Add(NewFolder_Button);
                NewFolder_Button.Text = "新建文件夹";
                NewFolder_Button.Stretch = Direction.Right;
                NewFolder_Button.BorderWidth = 0;
                NewFolder_Button.ClientSize = new(DirectoryMenu_Panel.ClientSize.Width, 16);
                NewFolder_Button.LayoutDown(DirectoryMenu_Panel, Paste_Button, 1);
                NewFolder_Button.RightClick += NewFolder_Button_RightClick;
                NewFolder_Button.RightClick += ShowFileList_RightClick;

                DirectoryMenu_Panel.ChildControls.Add(NewTextDocument_Button);
                NewTextDocument_Button.Text = "新建文本文档";
                NewTextDocument_Button.Stretch = Direction.Right;
                NewTextDocument_Button.BorderWidth = 0;
                NewTextDocument_Button.ClientSize = new(DirectoryMenu_Panel.ClientSize.Width, 16);
                NewTextDocument_Button.LayoutDown(DirectoryMenu_Panel, NewFolder_Button, 1);
                NewTextDocument_Button.RightClick += NewTextDocument_Button_RightClick;
                NewTextDocument_Button.RightClick += ShowFileList_RightClick;

                ChildControls.Add(SortOrderMenu_Label);
                SortOrderMenu_Label.Text = "排序方式";
                SortOrderMenu_Label.LayoutDown(this, DirectoryMenu_Panel, 1);

                ChildControls.Add(SortOrderMenu_Panel);
                SortOrderMenu_Panel.Stretch = Direction.Right;
                SortOrderMenu_Panel.ClientSize = new(ClientSize.Width - 4, 16 * 2 + 1);
                SortOrderMenu_Panel.LayoutDown(this, SortOrderMenu_Label, 1);
                SortOrderMenu_Panel.Skin.SetAllBackgroundColor(SortOrderMenu_Panel.Skin.BorderColor);

                SortOrderMenu_Panel.ChildControls.Add(Sort_ComboButton);
                Sort_ComboButton.Title = "依据";
                Sort_ComboButton.Stretch = Direction.Right;
                Sort_ComboButton.BorderWidth = 0;
                Sort_ComboButton.ClientSize = new(SortOrderMenu_Panel.ClientSize.Width, 16);
                Sort_ComboButton.ClientLocation = Point.Empty;
                Sort_ComboButton.RightClick += Sort_ComboButton_RightClick;
                Sort_ComboButton.Items.AddRenge(Enum.GetValues<FileListSort>());
                Sort_ComboButton.Items.SelectedItem = FileListSort.FileName;
                Sort_ComboButton.Items.ItemToStringFunc = (item) =>
                {
                    return item switch
                    {
                        FileListSort.FileName => "文件名",
                        FileListSort.FileSize => "大小",
                        FileListSort.WriteTime => "修改日期",
                        _ => throw new InvalidEnumArgumentException(nameof(item), (int)item, typeof(FileListSort))
                    };
                };

                SortOrderMenu_Panel.ChildControls.Add(SortDirection_ComboButton);
                SortDirection_ComboButton.Title = "方向";
                SortDirection_ComboButton.Stretch = Direction.Right;
                SortDirection_ComboButton.BorderWidth = 0;
                SortDirection_ComboButton.ClientSize = new(SortOrderMenu_Panel.ClientSize.Width, 16);
                SortDirection_ComboButton.LayoutDown(this, Sort_ComboButton, 1);
                SortDirection_ComboButton.RightClick += SortDirection_ComboButton_RightClick;
                SortDirection_ComboButton.Items.AddRenge(Enum.GetValues<ListSortDirection>());
                SortDirection_ComboButton.Items.SelectedItem = ListSortDirection.Ascending;
                SortDirection_ComboButton.Items.ItemToStringFunc = (item) =>
                {
                    return item switch
                    {
                        ListSortDirection.Ascending => "递增",
                        ListSortDirection.Descending => "递减",
                        _ => throw new InvalidEnumArgumentException(nameof(item), (int)item, typeof(ListSortDirection))
                    };
                };

                PageSize = new(ClientSize.Width, SortOrderMenu_Panel.BottomLocation + 2);
            }

            private void Refresh_Button_RightClick(Control sender, CursorEventArgs e)
            {
                if (string.IsNullOrEmpty(_owner.Text) && OperatingSystem.IsWindows())
                    _owner.DriveList_Page.UpdateList();
                else
                    _owner.FileList_Page.UpdateList(true);
            }

            private void Paste_Button_RightClick(Control sender, CursorEventArgs e)
            {
                string[]? fileDrop = MinecraftBlockScreen.Instance.Clipboard.GetFileDrop();
                if (fileDrop is null)
                    return;

                string appId;
                switch (MinecraftBlockScreen.Instance.Clipboard.GetClipboardMode())
                {
                    case ClipboardMode.Copy:
                        appId = "System.FileCopyHandler";
                        break;
                    case ClipboardMode.Move:
                        appId = "System.FileMoveHandler";
                        break;
                    default:
                        return;
                }

                Form? form = GetForm();
                string path = _owner.Text;
                ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents[appId];
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, fileDrop.RightAddend(path), form);
            }

            private void NewFolder_Button_RightClick(Control sender, CursorEventArgs e)
            {
                Form? form = GetForm();
                string path = _owner.Text;
                ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents["System.FileCreateHandler"];

                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [Path.Combine(path, "新建文件夹"), "D"], form);
            }

            private void NewTextDocument_Button_RightClick(Control sender, CursorEventArgs e)
            {
                Form? form = GetForm();
                string path = _owner.Text;
                ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents["System.FileCreateHandler"];

                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [Path.Combine(path, "新建文本文档.txt"), "F"], form);
            }

            private void Sort_ComboButton_RightClick(Control sender, CursorEventArgs e)
            {
                _owner.FileListSort = Sort_ComboButton.Items.SelectedItem;

                switch (Sort_ComboButton.Items.SelectedItem)
                {
                    case FileListSort.FileName:
                        _owner.FileListSortDirection = SortDirection_ComboButton.Items.SelectedItem = ListSortDirection.Ascending;
                        break;
                    case FileListSort.FileSize:
                    case FileListSort.WriteTime:
                        _owner.FileListSortDirection = SortDirection_ComboButton.Items.SelectedItem = ListSortDirection.Descending;
                        break;
                }
            }

            private void SortDirection_ComboButton_RightClick(Control sender, CursorEventArgs e)
            {
                _owner.FileListSortDirection = SortDirection_ComboButton.Items.SelectedItem;
            }

            private void ShowFileList_RightClick(Control sender, CursorEventArgs e)
            {
                _owner.ShowFileList();
            }
        }
    }
}
