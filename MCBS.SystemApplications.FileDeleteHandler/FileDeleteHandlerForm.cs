using MCBS.BlockForms;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using QuanLib.Game;
using QuanLib.Minecraft.Blocks;
using QuanLib.TextFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileDeleteHandler
{
    public class FileDeleteHandlerForm : WindowForm
    {
        public FileDeleteHandlerForm(FileDeleteHandler fileDeleteHandler, CancellationTokenSource cancellationTokenSource)
        {
            ArgumentNullException.ThrowIfNull(fileDeleteHandler, nameof(fileDeleteHandler));
            ArgumentNullException.ThrowIfNull(cancellationTokenSource, nameof(cancellationTokenSource));

            _fileDeleteHandler = fileDeleteHandler;
            _cancellationTokenSource = cancellationTokenSource;

            FileCount_Label = new();
            FileCount_HorizontalProgressBar = new();
            Deleting_Label = new();
            Cancel_Button = new();
        }

        private static readonly BytesFormatter _bytesFormatter = new(AbbreviationBytesUnitText.Default);

        private ViewData _viewData;

        private readonly FileDeleteHandler _fileDeleteHandler;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly Label FileCount_Label;

        private readonly HorizontalProgressBar FileCount_HorizontalProgressBar;

        private readonly Label Deleting_Label;

        private readonly Button Cancel_Button;

        public override void Initialize()
        {
            base.Initialize();

            Home_PagePanel.ChildControls.Add(FileCount_Label);
            FileCount_Label.ClientLocation = new(1, 1);

            Home_PagePanel.ChildControls.Add(FileCount_HorizontalProgressBar);
            FileCount_HorizontalProgressBar.Stretch = Direction.Right;
            FileCount_HorizontalProgressBar.Size = new(Home_PagePanel.ClientSize.Width - 2, 16);
            FileCount_HorizontalProgressBar.LayoutDown(Home_PagePanel, FileCount_Label, 1);
            FileCount_HorizontalProgressBar.Skin.SetAllForegroundColor(BlockManager.Concrete.LightBlue);

            Home_PagePanel.ChildControls.Add(Deleting_Label);
            Deleting_Label.LayoutDown(Home_PagePanel, FileCount_HorizontalProgressBar, 1);

            Home_PagePanel.ChildControls.Add(Cancel_Button);
            Cancel_Button.Text = "取消";
            Cancel_Button.Anchor = Direction.Bottom | Direction.Right;
            Cancel_Button.ClientLocation = new(Home_PagePanel.ClientSize.Width - Cancel_Button.Width - 1, Home_PagePanel.ClientSize.Height - Cancel_Button.Height - 1);
            Cancel_Button.RightClick += Cancel_Button_RightClick;

            UpdateView();
        }

        protected override void OnAfterFrame(Control sender, EventArgs e)
        {
            base.OnAfterFrame(sender, e);

            ViewData viewData = GetViewData(_fileDeleteHandler);

            if (_viewData != viewData)
            {
                _viewData = viewData;
                UpdateView();
            }

            if (_viewData.FileCount.Completed == _viewData.FileCount.Total || _cancellationTokenSource.IsCancellationRequested)
                CloseForm();
        }

        public override void CloseForm()
        {
            if (_viewData.FileCount.Completed == _viewData.FileCount.Total || _cancellationTokenSource.IsCancellationRequested)
            {
                base.CloseForm();
                return;
            }

            _ = DialogBoxHelper.OpenMessageBoxAsync(
                this,
                "警告",
                "文件正在删除中，是否取消？",
                MessageBoxButtons.Yes | MessageBoxButtons.No,
                (result) =>
                {
                    if (result == MessageBoxButtons.Yes)
                        base.CloseForm();
                });
        }

        private void Cancel_Button_RightClick(Control sender, CursorEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private void UpdateView()
        {
            FileCount_Label.Text = $"文件计数：{_viewData.FileCount.Completed}/{_viewData.FileCount.Total} ({Math.Round(_viewData.FileCount.Progress * 100)}%)";
            FileCount_HorizontalProgressBar.Progress = _viewData.FileCount.Progress;
            Deleting_Label.Text = "正在删除：" + _fileDeleteHandler.CurrentFile is string path ? Path.GetFileName(path) : "已完成";
        }

        private static ViewData GetViewData(FileDeleteHandler fileDeleteHandler)
        {
            ArgumentNullException.ThrowIfNull(fileDeleteHandler, nameof(fileDeleteHandler));

            return new(new(fileDeleteHandler.TotalFiles, fileDeleteHandler.CompletedFiles));
        }
    }
}
