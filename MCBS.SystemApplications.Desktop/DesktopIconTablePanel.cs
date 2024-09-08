using MCBS.Application;
using MCBS.BlockForms;
using MCBS.BlockForms.DialogBox;
using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using MCBS.Events;
using MCBS.SystemApplications.Desktop.DesktopIcons;
using QuanLib.Clipping;
using QuanLib.Core.Events;
using QuanLib.Core.Extensions;
using QuanLib.Core.GenericStructs;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop
{
    public partial class DesktopForm
    {
        public class DesktopIconTablePanel : ScrollablePanel
        {
            public DesktopIconTablePanel(DesktopForm owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                _desktopIconManager = new();
                _menuOpenPosition = Point.Empty;

                Skin.SetAllBackgroundColor(string.Empty);
                BorderWidth = 0;

                DesktopMenu_ListMenuBox = new();
                Refresh_Button = new();
                Paste_Button = new();
                NewBuilt_Button = new();
                EditMode_Switch = new();
                NewBuiltMenu_ListMenuBox = new();
                NewFolder_Button = new();
                NewTextDocument_Button = new();
                IconMeun_ListMenuBox = new();
                Open_Button = new();
                Rename_Button = new();
                Cut_Button = new();
                Copy_Button = new();
                Delete_Button = new();
            }

            private readonly DesktopForm _owner;

            private readonly DesktopIconManager _desktopIconManager;

            private Point _menuOpenPosition;

            private Task? _iconDataSaveTask;

            private Task<string>? _iconDataReadTask;

            private readonly ListMenuBox<Control> DesktopMenu_ListMenuBox;

            private readonly Button Refresh_Button;

            private readonly Button Paste_Button;

            private readonly Button NewBuilt_Button;

            private readonly Switch EditMode_Switch;

            private readonly ListMenuBox<Control> NewBuiltMenu_ListMenuBox;

            private readonly Button NewFolder_Button;

            private readonly Button NewTextDocument_Button;

            private readonly ListMenuBox<Control> IconMeun_ListMenuBox;

            private readonly Button Open_Button;

            private readonly Button Rename_Button;

            private readonly Button Cut_Button;

            private readonly Button Copy_Button;

            private readonly Button Delete_Button;

            public override void Initialize()
            {
                base.Initialize();

                RightClick += CloseDesktopMenu_RightClick;
                RightClick += CloseNewBuiltMenu_RightClick;
                RightClick += CloseIconMenu_RightClick;

                DesktopMenu_ListMenuBox.ClientSize = new(68, 18 * 4 + 5);
                DesktopMenu_ListMenuBox.MaxDisplayPriority = int.MaxValue;
                DesktopMenu_ListMenuBox.DisplayPriority = int.MaxValue - 1;
                DesktopMenu_ListMenuBox.Spacing = 1;
                DesktopMenu_ListMenuBox.FirstHandleRightClick = true;

                Refresh_Button.Text = "刷新";
                Refresh_Button.ClientSize = new(64, 16);
                Refresh_Button.RightClick += Refresh_Button_RightClick;
                Refresh_Button.RightClick += CloseDesktopMenu_RightClick;
                DesktopMenu_ListMenuBox.AddedChildControlAndLayout(Refresh_Button);

                Paste_Button.Text = "粘贴";
                Paste_Button.ClientSize = new(64, 16);
                Paste_Button.RightClick += Paste_Button_RightClick;
                Paste_Button.RightClick += CloseDesktopMenu_RightClick;
                DesktopMenu_ListMenuBox.AddedChildControlAndLayout(Paste_Button);

                NewBuilt_Button.Text = "新建";
                NewBuilt_Button.ClientSize = new(64, 16);
                NewBuilt_Button.RightClick += NewBuilt_Button_RightClick;
                NewBuilt_Button.RightClick += CloseDesktopMenu_RightClick;
                DesktopMenu_ListMenuBox.AddedChildControlAndLayout(NewBuilt_Button);

                EditMode_Switch.Text = "编辑模式";
                EditMode_Switch.ClientSize = new(64, 16);
                EditMode_Switch.RightClick += EditMode_Switch_RightClick;
                EditMode_Switch.RightClick += CloseDesktopMenu_RightClick;
                DesktopMenu_ListMenuBox.AddedChildControlAndLayout(EditMode_Switch);

                NewBuiltMenu_ListMenuBox.ClientSize = new(68, 18 * 2 + 3);
                NewBuiltMenu_ListMenuBox.MaxDisplayPriority = int.MaxValue;
                NewBuiltMenu_ListMenuBox.DisplayPriority = int.MaxValue - 1;
                NewBuiltMenu_ListMenuBox.Spacing = 1;
                NewBuiltMenu_ListMenuBox.FirstHandleRightClick = true;

                NewFolder_Button.Text = "文件夹";
                NewFolder_Button.ClientSize = new(64, 16);
                NewFolder_Button.RightClick += NewFolder_Button_RightClick;
                NewFolder_Button.RightClick += CloseNewBuiltMenu_RightClick;
                NewBuiltMenu_ListMenuBox.AddedChildControlAndLayout(NewFolder_Button);

                NewTextDocument_Button.Text = "文本文档";
                NewTextDocument_Button.ClientSize = new(64, 16);
                NewTextDocument_Button.RightClick += NewTextDocument_Button_RightClick;
                NewTextDocument_Button.RightClick += CloseNewBuiltMenu_RightClick;
                NewBuiltMenu_ListMenuBox.AddedChildControlAndLayout(NewTextDocument_Button);

                IconMeun_ListMenuBox.ClientSize = new(68, 18 * 4 + 5);
                IconMeun_ListMenuBox.MaxDisplayPriority = int.MaxValue;
                IconMeun_ListMenuBox.DisplayPriority = int.MaxValue - 1;
                IconMeun_ListMenuBox.Spacing = 1;
                IconMeun_ListMenuBox.FirstHandleRightClick = true;

                Open_Button.Text = "打开";
                Open_Button.ClientSize = new(64, 16);
                Open_Button.RightClick += Open_Button_RightClick;
                Open_Button.RightClick += CloseIconMenu_RightClick;
                Open_Button.RightClick += ClearSelectedIcons_RightClick;
                IconMeun_ListMenuBox.AddedChildControlAndLayout(Open_Button);

                Rename_Button.Text = "重命名";
                Rename_Button.ClientSize = new(64, 16);
                Rename_Button.RightClick += Rename_Button_RightClick;
                Rename_Button.RightClick += CloseIconMenu_RightClick;
                Rename_Button.RightClick += ClearSelectedIcons_RightClick;
                IconMeun_ListMenuBox.AddedChildControlAndLayout(Rename_Button);

                Cut_Button.Text = "剪切";
                Cut_Button.ClientSize = new(64, 16);
                Cut_Button.RightClick += Cut_Button_RightClick;
                Cut_Button.RightClick += CloseIconMenu_RightClick;
                Cut_Button.RightClick += ClearSelectedIcons_RightClick;
                IconMeun_ListMenuBox.AddedChildControlAndLayout(Cut_Button);

                Copy_Button.Text = "复制";
                Copy_Button.ClientSize = new(64, 16);
                Copy_Button.RightClick += Copy_Button_RightClick;
                Copy_Button.RightClick += CloseIconMenu_RightClick;
                Copy_Button.RightClick += ClearSelectedIcons_RightClick;
                IconMeun_ListMenuBox.AddedChildControlAndLayout(Copy_Button);

                Delete_Button.Text = "删除";
                Delete_Button.ClientSize = new(64, 16);
                Delete_Button.RightClick += Delete_Button_RightClick;
                Delete_Button.RightClick += CloseIconMenu_RightClick;
                Delete_Button.RightClick += ClearSelectedIcons_RightClick;
                IconMeun_ListMenuBox.AddedChildControlAndLayout(Delete_Button);

                _desktopIconManager.MaxSize = new(ClientSize.Width / 24, ClientSize.Height / 24);
                ReadIconData();
            }

            protected override void OnRightClick(Control sender, CursorEventArgs e)
            {
                base.OnRightClick(sender, e);

                DesktopIcon? hoverDesktopIcon = GetHoverDesktopIcon(e);
                if (hoverDesktopIcon is null)
                {
                    DesktopIcon? desktopIcon = e.CursorContext.HoverControls.Keys.OfType<DesktopIcon>().FirstOrDefault();
                    if (desktopIcon is null)
                        return;

                    Point source = ClientPos2IconPos(desktopIcon.ClientLocation);
                    Point destination = ClientPos2IconPos(e.Position);

                    if (destination.X >= _desktopIconManager.MaxSize.Width ||
                        destination.Y >= _desktopIconManager.MaxSize.Height)
                        return;

                    e.CursorContext.HoverControls.TryRemove(desktopIcon, out _);

                    if (desktopIcon.GetForm() is not DesktopForm desktopForm)
                    {
                        _ = DialogBoxHelper.OpenMessageBoxAsync(
                            _owner,
                            "错误",
                            "源图标来自未知的桌面",
                            MessageBoxButtons.OK);
                        return;
                    }

                    if (!ValidateIcon(source, desktopForm, desktopIcon))
                    {
                        _ = DialogBoxHelper.OpenMessageBoxAsync(
                                _owner,
                                "警告",
                                "源图标的状态发生更新，请刷新后重试",
                                MessageBoxButtons.OK);
                        desktopForm.DesktopIconTable_Panel.SynchronizeTable();
                        return;
                    }

                    if (!ValidateIcon(destination, _owner, null))
                    {
                        _ = DialogBoxHelper.OpenMessageBoxAsync(
                                _owner,
                                "警告",
                                "目标图标的状态发生更新，请刷新后重试",
                                MessageBoxButtons.OK);
                        _owner.DesktopIconTable_Panel.SynchronizeTable();
                        return;
                    }

                    if (ChildControls.Contains(desktopIcon))
                    {
                        _desktopIconManager.MoveIcon(source, destination);
                        SynchronizeTable();
                    }
                    else
                    {
                        IconIdentifier iconIdentifier = desktopIcon.GetIconIdentifier();
                        if (iconIdentifier.Type is DesktopAppIcon.ICON_TYPE)
                        {
                            _ = DialogBoxHelper.OpenMessageBoxAsync(
                                _owner,
                                "警告",
                                "无法跨屏移动应用程序图标",
                                MessageBoxButtons.OK);
                            return;
                        }
                        else if (iconIdentifier.Type is DesktopFileIcon.ICON_TYPE or DesktopDirectoryIcon.ICON_TYPE)
                        {
                            string sourceDesktop = desktopForm.DesktopPathManager.DesktopDir;
                            string destinationDesktop = _owner.DesktopPathManager.DesktopDir;
                            ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents["System.FileMoveHandler"];
                            MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [Path.Combine(sourceDesktop, iconIdentifier.Value), destinationDesktop], _owner);
                        }
                    }
                }
                else if (EditMode_Switch.IsSelected)
                {
                    if (e.CursorContext.HoverControls.Keys.OfType<DesktopIcon>().Any())
                        return;

                    hoverDesktopIcon.IsSelected = true;
                    e.CursorContext.HoverControls.TryAdd(hoverDesktopIcon, out _);
                }
                else
                {
                    hoverDesktopIcon.IsSelected = !hoverDesktopIcon.IsSelected;
                }
            }

            protected override void OnDoubleRightClick(Control sender, CursorEventArgs e)
            {
                base.OnDoubleRightClick(sender, e);

                DesktopIcon? hoverDesktopIcon = GetHoverDesktopIcon(e);
                if (hoverDesktopIcon is null)
                {
                    OpenMenu(e.Position, DesktopMenu_ListMenuBox);
                    ChildControls.ClearSelecteds();
                }
                else if (EditMode_Switch.IsSelected)
                {
                    return;
                }
                else if (hoverDesktopIcon.IsSelected)
                {
                    OpenMenu(e.Position, IconMeun_ListMenuBox);
                }
                else
                {
                    hoverDesktopIcon.OpenIcon();
                }
            }

            protected override void OnResize(Control sender, ValueChangedEventArgs<Size> e)
            {
                base.OnResize(sender, e);

                UpdateTableMaxSize();
                PageSize = _desktopIconManager.MaxSize * 24;
            }

            protected override void OnAfterFrame(Control sender, EventArgs e)
            {
                if (_iconDataReadTask is not null && _iconDataReadTask.IsCompleted)
                {
                    string json = _iconDataReadTask.Result;
                    _iconDataReadTask = null;
                    _desktopIconManager.FromJsonUpdate(json);
                }

                if (MinecraftBlockScreen.Instance.SystemTick % 20 == 0)
                {
                    UpdateTable();
                    SaveIconData();
                }
            }

            public void UpdateTable()
            {
                UpdateDesktopIconManager();
                if (IsRequestUpdateTable())
                    SynchronizeTable();
            }

            protected override BlockFrame Drawing()
            {
                BlockFrame baseFrame = base.Drawing();

                if (EditMode_Switch.IsSelected)
                {
                    for (int x = 1; x <= _desktopIconManager.MaxSize.Width; x++)
                        baseFrame.DrawVerticalLine(x * 24, BlockManager.Concrete.LightGray);
                    for (int y = 1; y <= _desktopIconManager.MaxSize.Height; y++)
                        baseFrame.DrawHorizontalLine(y * 24, BlockManager.Concrete.LightGray);
                }

                return baseFrame;
            }

            private void UpdateDesktopIconManager()
            {
                string[] apps = MinecraftBlockScreen.Instance.AppComponents.Values.Where(i => !i.IsBackground).Select(i => i.ID).ToArray();
                string[] dirs, files;
                List<IconIdentifier> iconIdentifiers = [];
                List<Point> miss = [];

                try
                {
                    string desktopDir = _owner.DesktopPathManager.DesktopDir;
                    if (Directory.Exists(desktopDir))
                    {
                        dirs = Directory.GetDirectories(desktopDir);
                        files = Directory.GetFiles(desktopDir);
                    }
                    else
                    {
                        dirs = [];
                        files = [];
                    }
                }
                catch
                {
                    dirs = [];
                    files = [];
                }

                foreach (string value in apps)
                {
                    IconIdentifier iconIdentifier = new(DesktopAppIcon.ICON_TYPE, value);
                    iconIdentifiers.Add(iconIdentifier);
                }

                foreach (string value in dirs)
                {
                    IconIdentifier iconIdentifier = new(DesktopDirectoryIcon.ICON_TYPE, Path.GetFileName(value));
                    iconIdentifiers.Add(iconIdentifier);
                }

                foreach (string value in files)
                {
                    IconIdentifier iconIdentifier = new(DesktopFileIcon.ICON_TYPE, Path.GetFileName(value));
                    iconIdentifiers.Add(iconIdentifier);
                }

                _desktopIconManager.IconsPositionFix();

                foreach (var item in _desktopIconManager.Icons)
                {
                    if (!iconIdentifiers.Contains(item.Value))
                        miss.Add(item.Key);
                }

                foreach (Point position in miss)
                    _desktopIconManager.Icons.Remove(position);

                foreach (IconIdentifier iconIdentifier in iconIdentifiers)
                {
                    if (!_desktopIconManager.Icons.ContainsValue(iconIdentifier))
                    {
                        if (!_desktopIconManager.HasAvailablePosition)
                        {
                            Size maxSize = _desktopIconManager.MaxSize;
                            maxSize.Width++;
                            _desktopIconManager.MaxSize = maxSize;
                        }
                        _desktopIconManager.AppendIcon(iconIdentifier);
                    }
                }

                UpdateTableMaxSize();
            }

            private bool IsRequestUpdateTable()
            {
                DesktopIconManager desktopIconManager = new()
                {
                    MaxSize = _desktopIconManager.MaxSize
                };
                foreach (DesktopIcon desktopIcon in ChildControls.OfType<DesktopIcon>())
                {
                    Point position = ClientPos2IconPos(desktopIcon.ClientLocation);
                    IconIdentifier iconIdentifier = desktopIcon.GetIconIdentifier();

                    if (!desktopIconManager.Icons.TryAdd(position, iconIdentifier))
                    {
                        if (_desktopIconManager.HasAvailablePosition)
                        {
                            Size maxSize = desktopIconManager.MaxSize;
                            maxSize.Width++;
                            desktopIconManager.MaxSize = maxSize;
                        }
                        desktopIconManager.AppendIcon(iconIdentifier);
                    }
                }

                return !DesktopIconManager.Equals(_desktopIconManager.Icons, desktopIconManager.Icons);
            }

            public void SynchronizeTable()
            {
                Dictionary<GenericStruct<Point, IconIdentifier>, DesktopIcon> childControls = [];
                foreach (DesktopIcon desktopIcon in ChildControls.OfType<DesktopIcon>())
                {
                    Point position = ClientPos2IconPos(desktopIcon.ClientLocation);
                    IconIdentifier iconIdentifier = desktopIcon.GetIconIdentifier();
                    childControls.Add(new(position, iconIdentifier), desktopIcon);
                }

                ChildControls.Clear();
                foreach (var item in _desktopIconManager.Icons)
                {
                    GenericStruct<Point, IconIdentifier> iconInfo = new(item.Key, item.Value);
                    if (!childControls.TryGetValue(iconInfo, out var desktopIcon))
                        desktopIcon = DesktopIconFactory.CreateDesktopIcon(_owner.DesktopPathManager.DesktopDir, iconInfo.ItemB);
                    desktopIcon.ClientLocation = IconPos2ClientPos(iconInfo.ItemA);
                    ChildControls.Add(desktopIcon);
                }

                PageSize = _desktopIconManager.MaxSize * 24;
            }

            private void UpdateTableMaxSize()
            {
                Size tableSize = new(_desktopIconManager.Width, _desktopIconManager.Height);
                Size clientSize = new((ClientSize.Width + OffsetPosition.X) / 24, (ClientSize.Height + OffsetPosition.Y) / 24);
                Size maxSize = _desktopIconManager.MaxSize;
                maxSize = new(Math.Max(tableSize.Width, clientSize.Width), Math.Max(tableSize.Height, clientSize.Height));
                maxSize = new(Math.Clamp(tableSize.Width, clientSize.Width, maxSize.Width), Math.Clamp(tableSize.Height, clientSize.Height, maxSize.Height));
                _desktopIconManager.MaxSize = maxSize;
            }

            private void SaveIconData()
            {
                if (_iconDataSaveTask is not null && !_iconDataSaveTask.IsCompleted)
                    return;

                string json = _desktopIconManager.ToJson();
                string path = _owner.DesktopPathManager.IconDataFile;
                string? directory = Path.GetDirectoryName(path);
                if (directory is not null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                _iconDataSaveTask = File.WriteAllTextAsync(path, json, Encoding.UTF8);
            }

            private void ReadIconData()
            {
                _iconDataSaveTask?.Wait();
                _iconDataReadTask?.Wait();

                string path = _owner.DesktopPathManager.IconDataFile;
                if (File.Exists(path))
                    _iconDataReadTask = File.ReadAllTextAsync(path, Encoding.UTF8);
            }

            private DesktopIcon? GetHoverDesktopIcon(CursorEventArgs e)
            {
                ArgumentNullException.ThrowIfNull(e, nameof(e));

                return ChildControls
                    .GetHovers()
                    .Where(w => GetHoverCursors().Contains(e.CursorContext))
                    .OfType<DesktopIcon>()
                    .FirstOrDefault();
            }

            private DesktopIcon[] GetSelectedDesktopIcons()
            {
                return ChildControls
                    .GetSelecteds()
                    .OfType<DesktopIcon>()
                    .ToArray();
            }

            private void OpenMenu(Point position, ListMenuBox<Control> listMenuBox)
            {
                ArgumentNullException.ThrowIfNull(listMenuBox, nameof(listMenuBox));

                _menuOpenPosition = position;

                ChildControls.TryAdd(listMenuBox);
                listMenuBox.ClientLocation = new(
                    Math.Min(position.X, ClientSize.Width + OffsetPosition.X - listMenuBox.Width),
                    Math.Min(position.Y, ClientSize.Height + OffsetPosition.Y - listMenuBox.Height));
            }

            private void Refresh_Button_RightClick(Control sender, CursorEventArgs e)
            {
                UpdateTable();
                _owner.ReloadWallpaper();
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

                ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents[appId];
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, fileDrop.RightAddend(_owner.DesktopPathManager.DesktopDir), _owner);
            }

            private void NewBuilt_Button_RightClick(Control sender, CursorEventArgs e)
            {
                OpenMenu(_menuOpenPosition, NewBuiltMenu_ListMenuBox);
            }

            private void EditMode_Switch_RightClick(Control sender, CursorEventArgs e)
            {

            }

            private void NewFolder_Button_RightClick(Control sender, CursorEventArgs e)
            {
                ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents["System.FileCreateHandler"];
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [Path.Combine(_owner.DesktopPathManager.DesktopDir, "新建文件夹"), "D"], _owner);
            }

            private void NewTextDocument_Button_RightClick(Control sender, CursorEventArgs e)
            {
                ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents["System.FileCreateHandler"];
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [Path.Combine(_owner.DesktopPathManager.DesktopDir, "新建文本文档.txt"), "F"], _owner);
            }

            private void Open_Button_RightClick(Control sender, CursorEventArgs e)
            {
                DesktopIcon[] desktopIcons = GetSelectedDesktopIcons();
                foreach (DesktopIcon desktopIcon in desktopIcons)
                {
                    if (!ValidateIcon(ClientPos2IconPos(desktopIcon.ClientLocation), _owner, desktopIcon))
                        return;

                    desktopIcon.OpenIcon();
                }
            }

            private void Rename_Button_RightClick(Control sender, CursorEventArgs e)
            {
                DesktopIcon[] desktopIcons = GetSelectedDesktopIcons();
                if (desktopIcons.Length != 1)
                {
                    _ = DialogBoxHelper.OpenMessageBoxAsync(
                        _owner,
                        "提醒",
                        "无法重命名多个图标",
                        MessageBoxButtons.OK);
                    return;
                }

                IconIdentifier iconIdentifier = desktopIcons[0].GetIconIdentifier();
                if (iconIdentifier.Type is not DesktopFileIcon.ICON_TYPE and not DesktopDirectoryIcon.ICON_TYPE)
                {
                    _ = DialogBoxHelper.OpenMessageBoxAsync(
                        _owner,
                        "提醒",
                        "只有文件或文件夹允许被重命名",
                        MessageBoxButtons.OK);
                    return;
                }

                ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents["System.FileRenameHandler"];
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [Path.Combine(_owner.DesktopPathManager.DesktopDir, iconIdentifier.Value)], _owner);
            }

            private void Cut_Button_RightClick(Control sender, CursorEventArgs e)
            {
                DesktopIcon[] desktopIcons = GetSelectedDesktopIcons();
                if (desktopIcons.Where(i => i is not DesktopFileIcon and not DesktopDirectoryIcon).Any())
                {
                    _ = DialogBoxHelper.OpenMessageBoxAsync(
                        _owner,
                        "提醒",
                        "只有文件或文件夹允许被剪切",
                        MessageBoxButtons.OK);
                    return;
                }

                string[] paths = desktopIcons.Select(i => Path.Combine(_owner.DesktopPathManager.DesktopDir, i.GetIconIdentifier().Value)).ToArray();
                MinecraftBlockScreen.Instance.Clipboard.SetFileDrop(paths, ClipboardMode.Move);
            }

            private void Copy_Button_RightClick(Control sender, CursorEventArgs e)
            {
                DesktopIcon[] desktopIcons = GetSelectedDesktopIcons();
                if (desktopIcons.Where(i => i is not DesktopFileIcon and not DesktopDirectoryIcon).Any())
                {
                    _ = DialogBoxHelper.OpenMessageBoxAsync(
                        _owner,
                        "提醒",
                        "只有文件或文件夹允许被复制",
                        MessageBoxButtons.OK);
                    return;
                }

                string[] paths = desktopIcons.Select(i => Path.Combine(_owner.DesktopPathManager.DesktopDir, i.GetIconIdentifier().Value)).ToArray();
                MinecraftBlockScreen.Instance.Clipboard.SetFileDrop(paths, ClipboardMode.Copy);
            }

            private void Delete_Button_RightClick(Control sender, CursorEventArgs e)
            {
                DesktopIcon[] desktopIcons = GetSelectedDesktopIcons();
                if (desktopIcons.Where(i => i is not DesktopFileIcon and not DesktopDirectoryIcon).Any())
                {
                    _ = DialogBoxHelper.OpenMessageBoxAsync(
                        _owner,
                        "提醒",
                        "只有文件或文件夹允许被删除",
                        MessageBoxButtons.OK);
                    return;
                }

                string[] paths = desktopIcons.Select(i => Path.Combine(_owner.DesktopPathManager.DesktopDir, i.GetIconIdentifier().Value)).ToArray();
                ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents["System.FileDeleteHandler"];
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, paths, _owner);
            }

            private void CloseDesktopMenu_RightClick(Control sender, CursorEventArgs e)
            {
                ChildControls.Remove(DesktopMenu_ListMenuBox);
            }

            private void CloseNewBuiltMenu_RightClick(Control sender, CursorEventArgs e)
            {
                ChildControls.Remove(NewBuiltMenu_ListMenuBox);
            }

            private void CloseIconMenu_RightClick(Control sender, CursorEventArgs e)
            {
                ChildControls.Remove(IconMeun_ListMenuBox);
            }

            private void ClearSelectedIcons_RightClick(Control sender, CursorEventArgs e)
            {
                ChildControls.ClearSelecteds();
            }

            private static bool ValidateIcon(Point position, DesktopForm desktopForm, DesktopIcon? desktopIcon)
            {
                DesktopIconManager? desktopIconManager = desktopForm?.DesktopIconTable_Panel._desktopIconManager;

                if (desktopIcon is not null &&
                    desktopIconManager is not null &&
                    desktopIconManager.Icons.TryGetValue(position, out var iconIdentifier))
                {
                    return
                        position == ClientPos2IconPos(desktopIcon.ClientLocation) &&
                        iconIdentifier == desktopIcon.GetIconIdentifier();
                }
                else
                {
                    return
                        desktopIcon is null &&
                        (desktopIconManager is null ||
                        !desktopIconManager.Icons.ContainsKey(position));
                }
            }

            private static Point ClientPos2IconPos(Point position)
            {
                return position / 24;
            }

            private static Point IconPos2ClientPos(Point position)
            {
                return position * 24;
            }
        }
    }
}
