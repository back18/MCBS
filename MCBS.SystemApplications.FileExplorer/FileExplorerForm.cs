using MCBS.BlockForms;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.FileSystem;
using MCBS.BlockForms.Utility;
using MCBS.Config;
using MCBS.Events;
using QuanLib.Core.Events;
using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileExplorer
{
    public class FileExplorerForm : WindowForm
    {
        public FileExplorerForm(string? open)
        {
            _open = open;

            Backward_Button = new();
            Forward_Button = new();
            Menu_Button = new();
            Path_TextBox = new();
            FileBrowser_Box = new();
        }

        private readonly string? _open;

        private readonly Button Backward_Button;

        private readonly Button Forward_Button;

        private readonly Button Menu_Button;

        private readonly TextBox Path_TextBox;

        private readonly FileBrowserBox FileBrowser_Box;

        public override void Initialize()
        {
            base.Initialize();

            Home_PagePanel.ChildControls.Add(Backward_Button);
            Backward_Button.Text = "←";
            Backward_Button.ClientSize = new(16, 16);
            Backward_Button.LayoutRight(Home_PagePanel, 1, 1);
            Backward_Button.RightClick += Backward_Button_RightClick;

            Home_PagePanel.ChildControls.Add(Forward_Button);
            Forward_Button.Text = "→";
            Forward_Button.ClientSize = new(16, 16);
            Forward_Button.LayoutRight(Home_PagePanel, Backward_Button, 1);
            Forward_Button.RightClick += Forward_Button_RightClick;

            Home_PagePanel.ChildControls.Add(Menu_Button);
            Menu_Button.ClientSize = new(16, 16);
            Menu_Button.RequestDrawTransparencyTexture = false;
            Menu_Button.LayoutRight(Home_PagePanel, Forward_Button, 1);
            Menu_Button.Skin.SetAllBackgroundTexture(TextureManager.Instance["Menu"]);
            Menu_Button.RightClick += Menu_Button_RightClick;

            Home_PagePanel.ChildControls.Add(Path_TextBox);
            Path_TextBox.Width = Home_PagePanel.ClientSize.Width - Backward_Button.Width - Forward_Button.Width - Menu_Button.Width - 5;
            Path_TextBox.LayoutRight(Home_PagePanel, Menu_Button, 1);
            Path_TextBox.Stretch = Direction.Right;
            Path_TextBox.TextChanged += Path_TextBox_TextChanged;

            Home_PagePanel.ChildControls.Add(FileBrowser_Box);
            FileBrowser_Box.Size = new(Home_PagePanel.ClientSize.Width - 2, Home_PagePanel.ClientSize.Height - Path_TextBox.Height - 3);
            FileBrowser_Box.LayoutDown(Home_PagePanel, Backward_Button, 1);
            FileBrowser_Box.Stretch = Direction.Bottom | Direction.Right;
            FileBrowser_Box.TextChanged += FileBrowser_Box_TextChanged;
            FileBrowser_Box.OpenFile += FileBrowser_Box_OpenFile;
            FileBrowser_Box.OpenDirectory += FileBrowser_Box_OpenDirectory;
        }

        public override void AfterInitialize()
        {
            base.AfterInitialize();

            if (_open is not null)
                Path_TextBox.Text = _open;
            else
                Path_TextBox.Text = FileBrowser_Box.Text;
        }

        private void Backward_Button_RightClick(Control sender, CursorEventArgs e)
        {
            FileBrowser_Box.Backward();
        }

        private void Forward_Button_RightClick(Control sender, CursorEventArgs e)
        {
            FileBrowser_Box.Forward();
        }

        private void Menu_Button_RightClick(Control sender, CursorEventArgs e)
        {
            if (FileBrowser_Box.ActivePage == FileBrowser_Box.FileList_Page)
                FileBrowser_Box.ShowMenu();
            else if (FileBrowser_Box.ActivePage == FileBrowser_Box.Menu_Page)
                FileBrowser_Box.ShowFileList();
        }

        private void Path_TextBox_TextChanged(Control sender, ValueChangedEventArgs<string> e)
        {
            string path = e.NewValue;
            FileBrowser_Box.Text = path;

            if (SR.DefaultFont.GetTotalSize(path).Width > Path_TextBox.ClientSize.Width)
                Path_TextBox.ContentAnchor = AnchorPosition.UpperRight;
            else
                Path_TextBox.ContentAnchor = AnchorPosition.UpperLeft;
        }

        private void FileBrowser_Box_TextChanged(Control sender, ValueChangedEventArgs<string> e)
        {
            string path = e.NewValue;
            Path_TextBox.Text = path;
        }

        private void FileBrowser_Box_OpenFile(FileBrowserBox sender, EventArgs<FileInfo> e)
        {
            FileInfo fileInfo = e.Argument;
            if (!fileInfo.Exists)
                return;

            string extension = fileInfo.Extension.TrimStart('.');
            if (ConfigManager.Registry.TryGetValue(extension, out var appId) &&
                MinecraftBlockScreen.Instance.AppComponents.TryGetValue(appId, out var applicationManifest))
            {
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [fileInfo.FullName], GetForm());
            }
            else
            {
                SelectApplication(fileInfo.FullName);
            }
        }

        private void FileBrowser_Box_OpenDirectory(FileBrowserBox sender, EventArgs<DirectoryInfo> e)
        {
            DirectoryInfo directoryInfo = e.Argument;
            if (!directoryInfo.Exists)
                return;

            if (MinecraftBlockScreen.Instance.AppComponents.TryGetValue("System.FileExplorer", out var applicationManifest))
            {
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [directoryInfo.FullName], GetForm());
            }
            else
            {
                SelectApplication(directoryInfo.FullName);
            }
        }

        private void SelectApplication(params string[] args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            Form? form = GetForm();
            if (form is null)
                return;

            _ = DialogBoxHelper.OpenApplicationListBoxAsync(form, "请选择应用程序", (applicationManifest) =>
            {
                if (applicationManifest is not null)
                    MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, args, form);
            });
        }
    }
}
