using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MCBS.BlockForms;
using QuanLib.Game;
using QuanLib.Minecraft.Blocks;
using MCBS.Screens;
using QuanLib.IO.Extensions;
using MCBS.Events;
using MCBS.SystemApplications.Desktop.DesktopIcons;
using MCBS.BlockForms.DialogBox;
using System.ComponentModel;
using QuanLib.Core.Events;
using System.Xml.Linq;

namespace MCBS.SystemApplications.Desktop
{
    public class DesktopForm : Form
    {
        public DesktopForm()
        {
            AllowDrag = false;
            AllowStretch = false;
            DisplayPriority = int.MinValue;
            MaxDisplayPriority = int.MinValue + 1;
            BorderWidth = 0;

            _menuOpenPosition = Point.Empty;

            IconTable_ScrollablePanel = new();
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

            _desktopIconTable = new();
        }

        private static readonly HashSet<string> _wallpaperExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".webp"];

        private Point _menuOpenPosition;

        private Task? _iconDataSaveTask;

        private readonly DesktopIconTable _desktopIconTable;

        private readonly ScrollablePanel IconTable_ScrollablePanel;

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

            Skin.SetAllBackgroundColor(BlockManager.StainedGlass.Gray);

            ChildControls.Add(IconTable_ScrollablePanel);
            IconTable_ScrollablePanel.BorderWidth = 0;
            IconTable_ScrollablePanel.ClientSize = ClientSize;
            IconTable_ScrollablePanel.Stretch = Direction.Bottom | Direction.Right;
            IconTable_ScrollablePanel.Skin.SetAllBackgroundColor(string.Empty);
            IconTable_ScrollablePanel.RightClick += IconTable_ScrollablePanel_RightClick;
            IconTable_ScrollablePanel.RightClick += CloseDesktopMenu_RightClick;
            IconTable_ScrollablePanel.RightClick += CloseNewBuiltMenu_RightClick;
            IconTable_ScrollablePanel.RightClick += CloseIconMenu_RightClick;
            IconTable_ScrollablePanel.DoubleRightClick += IconTable_ScrollablePanel_DoubleRightClick;
            IconTable_ScrollablePanel.Resize += IconTable_ScrollablePanel_Resize;

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

            UpdateDesktopIconTable();
            ReloadWallpaper();
        }

        private void IconTable_ScrollablePanel_Resize(Control sender, ValueChangedEventArgs<Size> e)
        {
            Size iconTableSize = new Size(_desktopIconTable.Width, _desktopIconTable.Height) * 24;
            IconTable_ScrollablePanel.PageSize = new(
                Math.Max(IconTable_ScrollablePanel.ClientSize.Width, iconTableSize.Width),
                Math.Max(IconTable_ScrollablePanel.ClientSize.Height, iconTableSize.Height));
        }

        protected override void OnBeforeFrame(Control sender, EventArgs e)
        {
            if (MinecraftBlockScreen.Instance.SystemTick % 20 == 0)
                SaveIconData();
        }

        private void IconTable_ScrollablePanel_RightClick(Control sender, CursorEventArgs e)
        {
            Point iconPos = ClientPos2IconPos(e.Position);
            if (!_desktopIconTable.TryGetIcon(iconPos, out var desktopIcon))
            {
                IconTable_ScrollablePanel.ChildControls.ClearSelecteds();

                DesktopIcon? hoverIcon = e.CursorContext.HoverControls.Keys.OfType<DesktopIcon>().FirstOrDefault();
                if (hoverIcon is not null)
                {
                    if (IconTable_ScrollablePanel.ChildControls.Contains(hoverIcon))
                    {
                        e.CursorContext.HoverControls.TryRemove(hoverIcon, out _);
                        Point hoverIconPos = ClientPos2IconPos(hoverIcon.ClientLocation);
                        _desktopIconTable.MoveIcon(hoverIconPos, iconPos);
                    }
                    else
                    {
                        IconIdentifier hoverIconIdentifier = hoverIcon.GetIconIdentifier();
                        Func<string, bool> existsHandler;
                        Action<string, string> moveHandler;

                        switch (hoverIconIdentifier.Type)
                        {
                            case DesktopFileIcon.ICON_TYPE:
                                existsHandler = File.Exists;
                                moveHandler = File.Move;
                                break;
                            case DesktopDirectoryIcon.ICON_TYPE:
                                existsHandler = Directory.Exists;
                                moveHandler = Directory.Move;
                                break;
                            case DesktopAppIcon.ICON_TYPE:
                                e.CursorContext.HoverControls.TryRemove(hoverIcon, out _);
                                _ = DialogBoxHelper.OpenMessageBoxAsync(this,
                                    "警告",
                                    $"无法跨屏移动内置应用程序\"{hoverIconIdentifier.Value}\"",
                                    MessageBoxButtons.OK);
                                return;
                            default:
                                throw new InvalidEnumArgumentException();
                        }

                        if (hoverIcon.GetForm() is not DesktopForm desktopForm)
                            return;

                        string sourceDesktopPath = desktopForm.GetScreenDataDirectory().CombineDirectory("Desktop").FullName;
                        string destDesktopPath = GetScreenDataDirectory().CombineDirectory("Desktop").FullName;
                        string sourcePath = Path.Combine(sourceDesktopPath, hoverIconIdentifier.Value);
                        string destPath = Path.Combine(destDesktopPath, hoverIconIdentifier.Value);

                        if (existsHandler.Invoke(destPath))
                        {
                            e.CursorContext.HoverControls.TryRemove(hoverIcon, out _);
                            _ = DialogBoxHelper.OpenMessageBoxAsync(this,
                                "警告",
                                $"目标桌面已存在同名文件或目录\"{hoverIconIdentifier.Value}\"",
                                MessageBoxButtons.OK);
                            return;
                        }

                        if (!Directory.Exists(destDesktopPath))
                            Directory.CreateDirectory(destDesktopPath);

                        try
                        {
                            moveHandler.Invoke(sourcePath, destPath);
                        }
                        catch (Exception ex)
                        {
                            e.CursorContext.HoverControls.TryRemove(hoverIcon, out _);
                            _ = DialogBoxHelper.OpenMessageBoxAsync(this,
                                "警告",
                                $"图标移动失败，错误信息：\n{ex.GetType().Name}: {ex.Message}",
                                MessageBoxButtons.OK);
                            return;
                        }

                        Point hoverIconPos = ClientPos2IconPos(hoverIcon.ClientLocation);

                        e.CursorContext.HoverControls.TryRemove(hoverIcon, out _);
                        desktopForm._desktopIconTable.RemoveIcon(hoverIconPos);
                        desktopForm.IconTable_ScrollablePanel.ChildControls.Remove(hoverIcon);
                        _desktopIconTable.CreateIcon(iconPos, destDesktopPath, hoverIconIdentifier);
                        IconTable_ScrollablePanel.ChildControls.Add(_desktopIconTable.GetIcon(iconPos));
                    }
                }
            }
            else if (EditMode_Switch.IsSelected)
            {
                if (e.CursorContext.HoverControls.Keys.OfType<DesktopIcon>().Any())
                    return;

                desktopIcon.IsSelected = true;
                e.CursorContext.HoverControls.TryAdd(desktopIcon, out _);
            }
            else
            {
                desktopIcon.IsSelected = !desktopIcon.IsSelected;
            }
        }

        private void IconTable_ScrollablePanel_DoubleRightClick(Control sender, CursorEventArgs e)
        {
            if (!_desktopIconTable.TryGetIcon(ClientPos2IconPos(e.Position), out var desktopIcon))
            {
                OpenMenu(e.Position, DesktopMenu_ListMenuBox);
                IconTable_ScrollablePanel.ChildControls.ClearSelecteds();
            }
            else if (EditMode_Switch.IsSelected)
            {
                return;
            }
            else if (desktopIcon.IsSelected)
            {
                OpenMenu(e.Position, IconMeun_ListMenuBox);
            }
            else
            {
                if (ValidateIcon(desktopIcon))
                    desktopIcon.OpenIcon();
            }
        }

        private void Refresh_Button_RightClick(Control sender, CursorEventArgs e)
        {
            UpdateDesktopIconTable();
            ReloadWallpaper();
        }

        private void Paste_Button_RightClick(Control sender, CursorEventArgs e)
        {

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
            Point iconPos = ClientPos2IconPos(_menuOpenPosition);
            if (_desktopIconTable.ContainsIcon(iconPos))
                return;

            string desktopPath = GetScreenDataDirectory().CombineDirectory("Desktop").FullName;
            string folderName = GetNextNewFolderName();
            string folderPath = Path.Combine(desktopPath, folderName);

            Directory.CreateDirectory(folderPath);
            DesktopDirectoryIcon desktopDirectoryIcon = new(folderPath);
            _desktopIconTable.CreateIcon(iconPos, desktopDirectoryIcon);
            IconTable_ScrollablePanel.ChildControls.Add(desktopDirectoryIcon);
        }

        private void NewTextDocument_Button_RightClick(Control sender, CursorEventArgs e)
        {
            Point iconPos = ClientPos2IconPos(_menuOpenPosition);
            if (_desktopIconTable.ContainsIcon(iconPos))
                return;

            string desktopPath = GetScreenDataDirectory().CombineDirectory("Desktop").FullName;
            string fileName = GetNextNewTextDocumentName();
            string filePath = Path.Combine(desktopPath, fileName);

            if (!Directory.Exists(desktopPath))
                Directory.CreateDirectory(desktopPath);

            File.Create(filePath);
            DesktopFileIcon desktopFileIcon = new(filePath);
            _desktopIconTable.CreateIcon(iconPos, desktopFileIcon);
            IconTable_ScrollablePanel.ChildControls.Add(desktopFileIcon);
        }

        private void Open_Button_RightClick(Control sender, CursorEventArgs e)
        {
            foreach (DesktopIcon desktopIcon in GetSelectedIcon())
            {
                if (ValidateIcon(desktopIcon))
                    desktopIcon.OpenIcon();
            }
        }

        private void Rename_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DesktopIcon[] selectedIcons = GetSelectedIcon();
            if (selectedIcons.Length != 1)
            {
                _ = DialogBoxHelper.OpenMessageBoxAsync(this,
                    "温馨提醒",
                    "无法同时重命名多个图标",
                    MessageBoxButtons.OK);
                return;
            }

            DesktopIcon desktopIcon = selectedIcons[0];
            IconIdentifier iconIdentifier = desktopIcon.GetIconIdentifier();
            Action<string, string> renameHandler;

            switch (iconIdentifier.Type)
            {
                case DesktopFileIcon.ICON_TYPE:
                    renameHandler = File.Move;
                    break;
                case DesktopDirectoryIcon.ICON_TYPE:
                    renameHandler = Directory.Move;
                    break;
                case DesktopAppIcon.ICON_TYPE:
                    _ = DialogBoxHelper.OpenMessageBoxAsync(this,
                        "警告",
                        $"无法重命名内置应用程序\"{iconIdentifier.Value}\"",
                        MessageBoxButtons.OK);
                    return;
                default:
                    throw new InvalidEnumArgumentException();
            }

            _ = DialogBoxHelper.OpenTextInputBoxAsync(this, "输入名称", (name) =>
            {
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    if (name.Contains(c))
                    {
                        DialogBoxHelper.OpenMessageBox(this,
                            "警告",
                            $"名称包含非法字符\'{c}\'",
                            MessageBoxButtons.OK);
                        return;
                    }
                }

                string desktopPath = GetScreenDataDirectory().CombineDirectory("Desktop").FullName;
                string sourcePath = Path.Combine(desktopPath, iconIdentifier.Value);
                string destPath = Path.Combine(desktopPath, name);
                bool retry = true;

                while (retry)
                {
                    try
                    {
                        renameHandler.Invoke(sourcePath, destPath);
                        break;
                    }
                    catch (Exception ex)
                    {
                        DialogBoxHelper.OpenMessageBox(this,
                            "警告",
                            $"重命名失败，错误信息：\n{ex.GetType().Name}: {ex.Message}",
                            MessageBoxButtons.Retry | MessageBoxButtons.Cancel,
                            (result) => retry = result == MessageBoxButtons.Retry);
                    }
                }

                if (!retry)
                    return;

                MinecraftBlockScreen.Instance.Submit(() =>
                {
                    Point iconPosition = ClientPos2IconPos(desktopIcon.ClientLocation);

                    _desktopIconTable.RemoveIcon(iconPosition);
                    IconTable_ScrollablePanel.ChildControls.Remove(desktopIcon);

                    _desktopIconTable.CreateIcon(iconPosition, desktopPath, new(iconIdentifier.Type, name));
                    IconTable_ScrollablePanel.ChildControls.Add(_desktopIconTable.GetIcon(iconPosition));
                });
            });
        }

        private void Cut_Button_RightClick(Control sender, CursorEventArgs e)
        {

        }

        private void Copy_Button_RightClick(Control sender, CursorEventArgs e)
        {

        }

        private void Delete_Button_RightClick(Control sender, CursorEventArgs e)
        {
            DesktopIcon[] selectedIcons = GetSelectedIcon();
            Task.Run(() =>
            {
                foreach (DesktopIcon desktopIcon in selectedIcons)
                {
                    if (!ValidateIcon(desktopIcon))
                        continue;

                    IconIdentifier iconIdentifier = desktopIcon.GetIconIdentifier();
                    Action<string> deleteHandler;

                    switch (iconIdentifier.Type)
                    {
                        case DesktopFileIcon.ICON_TYPE:
                            deleteHandler = File.Delete;
                            break;
                        case DesktopDirectoryIcon.ICON_TYPE:
                            deleteHandler = (path) => Directory.Delete(path, true);
                            break;
                        case DesktopAppIcon.ICON_TYPE:
                            DialogBoxHelper.OpenMessageBox(this,
                                "警告",
                                $"无法删除内置应用程序\"{iconIdentifier.Value}\"",
                                MessageBoxButtons.OK);
                            continue;
                        default:
                            throw new InvalidEnumArgumentException();
                    }

                    string desktopPath = GetScreenDataDirectory().CombineDirectory("Desktop").FullName;
                    string name = iconIdentifier.Value;
                    string path = Path.Combine(desktopPath, name);
                    bool retry = true;

                    while (retry)
                    {
                        try
                        {
                            deleteHandler.Invoke(path);
                            break;
                        }
                        catch (Exception ex)
                        {
                            DialogBoxHelper.OpenMessageBox(this,
                                "警告",
                                $"图标无法删除，错误信息：\n{ex.GetType().Name}: {ex.Message}",
                                MessageBoxButtons.Retry | MessageBoxButtons.Cancel,
                                (result) => retry = result == MessageBoxButtons.Retry);
                        }
                    }

                    if (!retry)
                        continue;

                    MinecraftBlockScreen.Instance.Submit(() =>
                    {
                        _desktopIconTable.RemoveIcon(ClientPos2IconPos(desktopIcon.ClientLocation));
                        IconTable_ScrollablePanel.ChildControls.Remove(desktopIcon);
                    });
                }
            });
        }

        private void CloseDesktopMenu_RightClick(Control sender, CursorEventArgs e)
        {
            IconTable_ScrollablePanel.ChildControls.Remove(DesktopMenu_ListMenuBox);
        }

        private void CloseNewBuiltMenu_RightClick(Control sender, CursorEventArgs e)
        {
            IconTable_ScrollablePanel.ChildControls.Remove(NewBuiltMenu_ListMenuBox);
        }

        private void CloseIconMenu_RightClick(Control sender, CursorEventArgs e)
        {
            IconTable_ScrollablePanel.ChildControls.Remove(IconMeun_ListMenuBox);
        }

        private void ClearSelectedIcons_RightClick(Control sender, CursorEventArgs e)
        {
            IconTable_ScrollablePanel.ChildControls.ClearSelecteds();
        }

        [Obsolete("暂时无法设置壁纸", true)]
        public void SetAsWallpaper(Image<Rgba32> image)
        {
            throw new NotSupportedException();
        }

        private void UpdateDesktopIconTable()
        {
            if (_desktopIconTable.IconCount > 0)
                SaveIconData();

            _desktopIconTable.ClearIcons();

            string screenDataDirectory = GetScreenDataDirectory().FullName;
            string desktopDirectory = Path.Combine(screenDataDirectory, "Desktop");
            Dictionary<Point, IconIdentifier> icons = IconTableReader.ReadIconTable(screenDataDirectory, ClientSize / 24);

            foreach (var item in icons)
                _desktopIconTable.CreateIcon(item.Key, desktopDirectory, item.Value);

            int width = _desktopIconTable.Width;
            int height = _desktopIconTable.Height;
            IconTable_ScrollablePanel.PageSize = new(Math.Max(ClientSize.Width, width * 24), Math.Max(ClientSize.Height, height * 24));
            IconTable_ScrollablePanel.ChildControls.Clear();
            IconTable_ScrollablePanel.OffsetPosition = Point.Empty;
            IconTable_ScrollablePanel.RefreshHorizontalScrollBar();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (_desktopIconTable.TryGetIcon(new(x, y), out var desktopIcon))
                        IconTable_ScrollablePanel.ChildControls.Add(desktopIcon);
                }
        }

        private void ReloadWallpaper()
        {
            string? wallpaperPath = GetWallpaperPath();
            if (wallpaperPath is not null)
            {
                try
                {
                    Image<Rgba32> wallpaper = Image.Load<Rgba32>(wallpaperPath);
                    Skin.SetAllBackgroundTexture(wallpaper);
                    return;
                }
                catch
                {

                }
            }

            Skin.SetAllBackgroundTexture(null);
        }

        private string? GetWallpaperPath()
        {
            DirectoryInfo wallpapersDirectory = GetScreenDataDirectory().CombineDirectory("Wallpapers");
            if (!wallpapersDirectory.Exists)
                return null;

            string[] files = wallpapersDirectory.GetFilePaths();
            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);
                if (name == "Wallpaper" && _wallpaperExtensions.Contains(extension))
                    return file;
            }

            return null;
        }

        private void SaveIconData()
        {
            DirectoryInfo screenDataDirectory = GetScreenDataDirectory();
            screenDataDirectory.CreateIfNotExists();
            string savePath = screenDataDirectory.CombineFile("IconData.json").FullName;
            _iconDataSaveTask?.Wait();
            _iconDataSaveTask = File.WriteAllTextAsync(savePath, _desktopIconTable.ToJson(), Encoding.UTF8);
        }

        private void OpenMenu(Point position, ListMenuBox<Control> listMenuBox)
        {
            ArgumentNullException.ThrowIfNull(listMenuBox, nameof(listMenuBox));

            _menuOpenPosition = position;

            IconTable_ScrollablePanel.ChildControls.TryAdd(listMenuBox);
            listMenuBox.ClientLocation = new(Math.Clamp(position.X, 0, IconTable_ScrollablePanel.PageSize.Width - listMenuBox.Width), Math.Clamp(position.Y, 0, IconTable_ScrollablePanel.PageSize.Height - listMenuBox.Height));
        }

        private bool ValidateIcon(DesktopIcon desktopIcon)
        {
            ArgumentNullException.ThrowIfNull(desktopIcon, nameof(desktopIcon));

            string desktopPath = GetScreenDataDirectory().CombineDirectory("Desktop").FullName;
            IconIdentifier iconIdentifier = desktopIcon.GetIconIdentifier();

            switch (iconIdentifier.Type)
            {
                case DesktopFileIcon.ICON_TYPE:
                    if (!File.Exists(Path.Combine(desktopPath, iconIdentifier.Value)))
                    {
                        _ = DialogBoxHelper.OpenMessageBoxAsync(this,
                            "警告",
                            $"文件\"{iconIdentifier.Value}\"已被删除，请刷新桌面",
                            MessageBoxButtons.OK);
                        return false;
                    }
                    break;
                case DesktopDirectoryIcon.ICON_TYPE:
                    if (!Directory.Exists(Path.Combine(desktopPath, iconIdentifier.Value)))
                    {
                        _ = DialogBoxHelper.OpenMessageBoxAsync(this,
                            "警告",
                            $"目录\"{iconIdentifier.Value}\"已被删除，请刷新桌面",
                            MessageBoxButtons.OK);
                        return false;
                    }
                    break;
            }

            return true;
        }

        private DesktopIcon[] GetSelectedIcon()
        {
            List<DesktopIcon> result = [];
            foreach (Control control in IconTable_ScrollablePanel.ChildControls)
            {
                if (control.IsSelected && control is DesktopIcon desktopIcon)
                    result.Add(desktopIcon);
            }
            return result.ToArray();
        }

        private DirectoryInfo GetScreenDataDirectory()
        {
            ScreenContext? screenContext = GetScreenContext() ?? throw new InvalidOperationException("无法获取屏幕上下文");
            return McbsPathManager.MCBS_Applications.CombineDirectory(DesktopApp.ID, "ScreenData", screenContext.GUID.ToString());
        }

        private string GetNextNewFolderName()
        {
            string desktopPath = GetScreenDataDirectory().CombineDirectory("Desktop").FullName;
            for (int i = 1; ; i++)
            {
                string folderName = $"新建文件夹({i})";
                string folderPath = Path.Combine(desktopPath, folderName);
                if (!Directory.Exists(folderPath))
                    return folderName;
            }
        }

        private string GetNextNewTextDocumentName()
        {
            string desktopPath = GetScreenDataDirectory().CombineDirectory("Desktop").FullName;
            for (int i = 1; ; i++)
            {
                string fileName = $"新建文本文档({i}).txt";
                string filePath = Path.Combine(desktopPath, fileName);
                if (!File.Exists(filePath))
                    return fileName;
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
