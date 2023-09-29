﻿using QuanLib.Core.IO;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLib.Core.Event;
using QuanLib.Minecraft.Block;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.SimpleFileSystem;
using MCBS.BlockForms;
using MCBS.Event;
using MCBS.BlockForms.Utility;
using MCBS.Config;
using MCBS.UI;

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

            ClientPanel.PageSize = new(178, 85);
            Size size1 = ClientPanel.ClientSize;
            Size size2 = ClientPanel.ClientSize;
            if (size1.Width < ClientPanel.PageSize.Width)
                size1.Width = ClientPanel.PageSize.Width;
            if (size1.Height < ClientPanel.PageSize.Height)
                size1.Height = ClientPanel.PageSize.Height;
            ClientPanel.ClientSize = size1;

            int spacing = 2;
            int start1 = 2;
            int start2 = ClientPanel.ClientSize.Height - Cancel_Button.Height - 2;

            ClientPanel.ChildControls.Add(Backward_Button);
            Backward_Button.Text = "←";
            Backward_Button.ClientSize = new(16, 16);
            Backward_Button.ClientLocation = ClientPanel.RightLayout(null, spacing, start1);
            Backward_Button.RightClick += Backward_Button_RightClick;

            ClientPanel.ChildControls.Add(Forward_Button);
            Forward_Button.Text = "→";
            Forward_Button.ClientSize = new(16, 16);
            Forward_Button.ClientLocation = ClientPanel.RightLayout(Backward_Button, spacing);
            Forward_Button.RightClick += Forward_Button_RightClick;

            ClientPanel.ChildControls.Add(Path_TextBox);
            Path_TextBox.ClientLocation = ClientPanel.RightLayout(Forward_Button, spacing);
            Path_TextBox.Width = ClientPanel.ClientSize.Width - Backward_Button.Width - Forward_Button.Width - 8;
            Path_TextBox.Stretch = Direction.Right;
            Path_TextBox.TextChanged += Path_TextBox_TextChanged;

            ClientPanel.ChildControls.Add(Cancel_Button);
            Cancel_Button.Text = "取消";
            Cancel_Button.ClientSize = new(32, 16);
            Cancel_Button.ClientLocation = ClientPanel.LeftLayout(null, Cancel_Button, spacing, start2);
            Cancel_Button.Anchor = Direction.Bottom | Direction.Right;
            Cancel_Button.RightClick += Cancel_Button_RightClick;

            ClientPanel.ChildControls.Add(OK_Button);
            OK_Button.Text = "确定";
            OK_Button.ClientSize = new(32, 16);
            OK_Button.ClientLocation = ClientPanel.LeftLayout(Cancel_Button, OK_Button, spacing);
            OK_Button.Anchor = Direction.Bottom | Direction.Right;
            OK_Button.RightClick += OK_Button_RightClick;

            ClientPanel.ChildControls.Add(Clear_Button);
            Clear_Button.Text = "X";
            Clear_Button.ClientLocation = ClientPanel.RightLayout(null, spacing, start2);
            Clear_Button.ClientSize = new(16, 16);
            Clear_Button.Skin.BackgroundBlockID = BlockManager.Concrete.Pink;
            Clear_Button.Skin.BackgroundBlockID_Hover = BlockManager.Concrete.Yellow;
            Clear_Button.Skin.BackgroundBlockID_Selected = BlockManager.Concrete.Red;
            Clear_Button.Anchor = Direction.Bottom | Direction.Left;
            Clear_Button.RightClick += Clear_Button_RightClick;

            ClientPanel.ChildControls.Add(Search_TextBox);
            Search_TextBox.ClientLocation = ClientPanel.RightLayout(Clear_Button, spacing);
            Search_TextBox.Width = ClientPanel.ClientSize.Width - Clear_Button.Width - Cancel_Button.Width - OK_Button.Width - 10;
            Search_TextBox.Anchor = Direction.Bottom | Direction.Left;
            Search_TextBox.Stretch = Direction.Right;
            Search_TextBox.TextChanged += Search_TextBox_TextChanged;

            ClientPanel.ChildControls.Add(SimpleFilesBox);
            SimpleFilesBox.Width = ClientPanel.ClientSize.Width - 4;
            SimpleFilesBox.Height = ClientPanel.ClientSize.Height - Backward_Button.Height - Cancel_Button.Height - 8;
            SimpleFilesBox.ClientLocation = ClientPanel.BottomLayout(Backward_Button, spacing);
            SimpleFilesBox.Stretch = Direction.Bottom | Direction.Right;
            SimpleFilesBox.Skin.SetAllBackgroundBlockID(BlockManager.Concrete.Lime);
            SimpleFilesBox.TextChanged += SimpleFilesBox_TextChanged;
            SimpleFilesBox.OpenFile += SimpleFilesBox_OpenFile;
            SimpleFilesBox.OpeningItemException += SimpleFilesBox_OpeningItemException;

            ClientPanel.ClientSize = size2;
        }

        public override void OnInitCompleted3()
        {
            base.OnInitCompleted3();

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

        private void Path_TextBox_TextChanged(Control sender, TextChangedEventArgs e)
        {
            SimpleFilesBox.Text = e.NewText;
            Search_TextBox.Text = string.Empty;

            if (SR.DefaultFont.GetTotalSize(Path_TextBox.Text).Width > Path_TextBox.ClientSize.Width)
                Path_TextBox.ContentAnchor = AnchorPosition.UpperRight;
            else
                Path_TextBox.ContentAnchor = AnchorPosition.UpperLeft;
        }

        private void Search_TextBox_TextChanged(Control sender, TextChangedEventArgs e)
        {
            SimpleFilesBox.SearchText = e.NewText;

            if (SR.DefaultFont.GetTotalSize(Search_TextBox.Text).Width > Search_TextBox.ClientSize.Width)
                Search_TextBox.ContentAnchor = AnchorPosition.UpperRight;
            else
                Search_TextBox.ContentAnchor = AnchorPosition.UpperLeft;
        }

        private void SimpleFilesBox_TextChanged(Control sender, TextChangedEventArgs e)
        {
            Path_TextBox.Text = SimpleFilesBox.Text;
        }

        private void SimpleFilesBox_OpenFile(SimpleFilesBox sender, FIleInfoEventArgs e)
        {
            FileInfo fileInfo = e.FileInfo;
            string extension = Path.GetExtension(fileInfo.Name).TrimStart('.');
            if (ConfigManager.Registry.TryGetValue(extension, out var id) && MCOS.Instance.ApplicationManager.Items.TryGetValue(id, out var app))
            {
                MCOS.Instance.RunApplication(app, new string[] { fileInfo.FullName }, this);
            }
            else
            {
                SelectApplication(new string[] { fileInfo.FullName });
            }
        }

        private void SimpleFilesBox_OpeningItemException(SimpleFilesBox sender, ExceptionEventArgs e)
        {
            _ = DialogBoxHelper.OpenMessageBoxAsync(this, "警告", $"无法打开文件或文件夹，错误信息：\n{e.Exception.GetType().Name}: {e.Exception.Message}", MessageBoxButtons.OK);
        }

        private void SelectApplication(string[] args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            _ = DialogBoxHelper.OpenApplicationListBoxAsync(this, "请选择应用程序", (appInfo) =>
            {
                if (appInfo is not null)
                {
                    MCOS.Instance.RunApplication(appInfo, args, this);
                }
            });
        }
    }
}
