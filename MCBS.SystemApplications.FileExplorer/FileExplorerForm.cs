using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.SimpleFileSystem;
using MCBS.BlockForms;
using MCBS.BlockForms.Utility;
using MCBS.Config;
using QuanLib.Core.Events;
using MCBS.Events;
using QuanLib.Minecraft.Blocks;
using QuanLib.Game;

namespace MCBS.SystemApplications.FileExplorer
{
    public class FileExplorerForm : WindowForm
    {
        public FileExplorerForm(string rootDirectory, string? path = null)
        {
            _open = path;

            Backward_Button = new();
            Forward_Button = new();
            OK_Button = new();
            Cancel_Button = new();
            Clear_Button = new();
            Path_TextBox = new();
            Search_TextBox = new();
            SimpleFilesBox = new(rootDirectory);
        }

        private readonly string? _open;

        private readonly Button Backward_Button;

        private readonly Button Forward_Button;

        private readonly Button OK_Button;

        private readonly Button Cancel_Button;

        private readonly Button Clear_Button;

        private readonly TextBox Path_TextBox;

        private readonly TextBox Search_TextBox;

        private readonly SimpleFilesBox SimpleFilesBox;

        public override void Initialize()
        {
            base.Initialize();

            Home_PagePanel.PageSize = new(178, 85);
            Size size1 = Home_PagePanel.ClientSize;
            Size size2 = Home_PagePanel.ClientSize;
            if (size1.Width < Home_PagePanel.PageSize.Width)
                size1.Width = Home_PagePanel.PageSize.Width;
            if (size1.Height < Home_PagePanel.PageSize.Height)
                size1.Height = Home_PagePanel.PageSize.Height;
            Home_PagePanel.ClientSize = size1;

            int spacing = 2;
            int start1 = 2;
            int start2 = Home_PagePanel.ClientSize.Height - Cancel_Button.Height - 2;

            Home_PagePanel.ChildControls.Add(Backward_Button);
            Backward_Button.Text = "←";
            Backward_Button.ClientSize = new(16, 16);
            Backward_Button.LayoutRight(Home_PagePanel, start1, spacing);
            Backward_Button.RightClick += Backward_Button_RightClick;

            Home_PagePanel.ChildControls.Add(Forward_Button);
            Forward_Button.Text = "→";
            Forward_Button.ClientSize = new(16, 16);
            Forward_Button.LayoutRight(Home_PagePanel, Backward_Button, spacing);
            Forward_Button.RightClick += Forward_Button_RightClick;

            Home_PagePanel.ChildControls.Add(Path_TextBox);
            Path_TextBox.LayoutRight(Home_PagePanel, Forward_Button, spacing);
            Path_TextBox.Width = Home_PagePanel.ClientSize.Width - Backward_Button.Width - Forward_Button.Width - 8;
            Path_TextBox.Stretch = Direction.Right;
            Path_TextBox.TextChanged += Path_TextBox_TextChanged;

            Home_PagePanel.ChildControls.Add(Cancel_Button);
            Cancel_Button.Text = "取消";
            Cancel_Button.ClientSize = new(32, 16);
            Cancel_Button.LayoutLeft(this, start2, spacing);
            Cancel_Button.Anchor = Direction.Bottom | Direction.Right;
            Cancel_Button.RightClick += Cancel_Button_RightClick;

            Home_PagePanel.ChildControls.Add(OK_Button);
            OK_Button.Text = "确定";
            OK_Button.ClientSize = new(32, 16);
            OK_Button.LayoutLeft(this, Cancel_Button, spacing);
            OK_Button.Anchor = Direction.Bottom | Direction.Right;
            OK_Button.RightClick += OK_Button_RightClick;

            Home_PagePanel.ChildControls.Add(Clear_Button);
            Clear_Button.Text = "X";
            Clear_Button.LayoutRight(Home_PagePanel, start2, spacing);
            Clear_Button.ClientSize = new(16, 16);
            Clear_Button.Skin.SetBackgroundColor(BlockManager.Concrete.Pink, ControlState.None);
            Clear_Button.Skin.SetBackgroundColor(BlockManager.Concrete.Yellow, ControlState.Hover);
            Clear_Button.Skin.SetBackgroundColor(BlockManager.Concrete.Red, ControlState.Selected, ControlState.Hover | ControlState.Selected);
            Clear_Button.Anchor = Direction.Bottom | Direction.Left;
            Clear_Button.RightClick += Clear_Button_RightClick;

            Home_PagePanel.ChildControls.Add(Search_TextBox);
            Search_TextBox.LayoutRight(Home_PagePanel, Clear_Button, spacing);
            Search_TextBox.Width = Home_PagePanel.ClientSize.Width - Clear_Button.Width - Cancel_Button.Width - OK_Button.Width - 10;
            Search_TextBox.Anchor = Direction.Bottom | Direction.Left;
            Search_TextBox.Stretch = Direction.Right;
            Search_TextBox.TextChanged += Search_TextBox_TextChanged;

            Home_PagePanel.ChildControls.Add(SimpleFilesBox);
            SimpleFilesBox.Width = Home_PagePanel.ClientSize.Width - 4;
            SimpleFilesBox.Height = Home_PagePanel.ClientSize.Height - Backward_Button.Height - Cancel_Button.Height - 8;
            SimpleFilesBox.LayoutDown(Home_PagePanel, Backward_Button, spacing);
            SimpleFilesBox.Stretch = Direction.Bottom | Direction.Right;
            SimpleFilesBox.Skin.SetAllBackgroundColor(BlockManager.Concrete.Lime);
            SimpleFilesBox.TextChanged += SimpleFilesBox_TextChanged;
            SimpleFilesBox.OpenFile += SimpleFilesBox_OpenFile;
            SimpleFilesBox.OpeningItemException += SimpleFilesBox_OpeningItemException;

            Home_PagePanel.ClientSize = size2;
        }

        public override void AfterInitialize()
        {
            base.AfterInitialize();

            if (_open is not null)
                Path_TextBox.Text = _open;
            else
                Path_TextBox.Text = SimpleFilesBox.Text;
        }

        private void Backward_Button_RightClick(Control sender, CursorEventArgs e)
        {
            SimpleFilesBox.Backward();
        }

        private void Forward_Button_RightClick(Control sender, CursorEventArgs e)
        {
            SimpleFilesBox.Forward();
        }

        private void OK_Button_RightClick(Control sender, CursorEventArgs e)
        {
            string[] args = SimpleFilesBox.GetSelecteds();
            if (args.Length == 0)
            {
                _ = DialogBoxHelper.OpenMessageBoxAsync(this, "温馨提醒", "请先选择文件或文件夹", MessageBoxButtons.OK);
                return;
            }

            SelectApplication(args);
        }

        private void Cancel_Button_RightClick(Control sender, CursorEventArgs e)
        {

        }

        private void Clear_Button_RightClick(Control sender, CursorEventArgs e)
        {
            Search_TextBox.Text = string.Empty;
        }

        private void Path_TextBox_TextChanged(Control sender, ValueChangedEventArgs<string> e)
        {
            SimpleFilesBox.Text = e.NewValue;
            Search_TextBox.Text = string.Empty;

            if (SR.DefaultFont.GetTotalSize(Path_TextBox.Text).Width > Path_TextBox.ClientSize.Width)
                Path_TextBox.ContentAnchor = AnchorPosition.UpperRight;
            else
                Path_TextBox.ContentAnchor = AnchorPosition.UpperLeft;
        }

        private void Search_TextBox_TextChanged(Control sender, ValueChangedEventArgs<string> e)
        {
            SimpleFilesBox.SearchText = e.NewValue;

            if (SR.DefaultFont.GetTotalSize(Search_TextBox.Text).Width > Search_TextBox.ClientSize.Width)
                Search_TextBox.ContentAnchor = AnchorPosition.UpperRight;
            else
                Search_TextBox.ContentAnchor = AnchorPosition.UpperLeft;
        }

        private void SimpleFilesBox_TextChanged(Control sender, ValueChangedEventArgs<string> e)
        {
            Path_TextBox.Text = SimpleFilesBox.Text;
        }

        private void SimpleFilesBox_OpenFile(SimpleFilesBox sender, EventArgs<FileInfo> e)
        {
            FileInfo fileInfo = e.Argument;
            string extension = Path.GetExtension(fileInfo.Name).TrimStart('.');
            if (ConfigManager.Registry.TryGetValue(extension, out var appID) && MinecraftBlockScreen.Instance.AppComponents.TryGetValue(appID, out var applicationManifest))
            {
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [fileInfo.FullName], this);
            }
            else
            {
                SelectApplication([fileInfo.FullName]);
            }
        }

        private void SimpleFilesBox_OpeningItemException(SimpleFilesBox sender, EventArgs<Exception> e)
        {
            _ = DialogBoxHelper.OpenMessageBoxAsync(this, "警告", $"无法打开文件或文件夹，错误信息：\n{e.Argument.GetType().Name}: {e.Argument.Message}", MessageBoxButtons.OK);
        }

        private void SelectApplication(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            _ = DialogBoxHelper.OpenApplicationListBoxAsync(this, "请选择应用程序", (appInfo) =>
            {
                if (appInfo is not null)
                {
                    MinecraftBlockScreen.Instance.ProcessManager.StartProcess(appInfo, args, this);
                }
            });
        }
    }
}
