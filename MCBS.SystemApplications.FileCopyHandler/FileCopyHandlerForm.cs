using MCBS.BlockForms;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.FileSystem;
using MCBS.BlockForms.FileSystem.IO;
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

namespace MCBS.SystemApplications.FileCopyHandler
{
    public class FileCopyHandlerForm : WindowForm
    {
        public FileCopyHandlerForm(FileCopyHandler fileCopyHandler, Task task, CancellationTokenSource cancellationTokenSource)
        {
            ArgumentNullException.ThrowIfNull(fileCopyHandler, nameof(fileCopyHandler));
            ArgumentNullException.ThrowIfNull(task, nameof(task));
            ArgumentNullException.ThrowIfNull(cancellationTokenSource, nameof(cancellationTokenSource));

            _fileCopyHandler = fileCopyHandler;
            _task = task;
            _cancellationTokenSource = cancellationTokenSource;
            _viewData = GetViewData(fileCopyHandler);
            _progressTimestamp = new(DateTime.MinValue, 0);
            _throughput = new(TimeSpan.Zero, 0);

            FileCount_Label = new();
            FileCount_HorizontalProgressBar = new();
            CurrentProgress_Label = new();
            CurrentProgress_HorizontalProgressBar = new();
            TotalProgress_Label = new();
            TotalProgress_HorizontalProgressBar = new();
            Copying_Label = new();
            Speed_Label = new();
            Cancel_Button = new();
        }

        private static readonly BytesFormatter _bytesFormatter = new(AbbreviationBytesUnitText.Default);

        private ViewData _viewData;

        private ProgressTimestamp _progressTimestamp;

        private Throughput _throughput;

        private readonly FileCopyHandler _fileCopyHandler;

        private readonly Task _task;

        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly Label FileCount_Label;

        private readonly HorizontalProgressBar FileCount_HorizontalProgressBar;

        private readonly Label CurrentProgress_Label;

        private readonly HorizontalProgressBar CurrentProgress_HorizontalProgressBar;

        private readonly Label TotalProgress_Label;

        private readonly HorizontalProgressBar TotalProgress_HorizontalProgressBar;

        private readonly Label Copying_Label;

        private readonly Label Speed_Label;

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

            Home_PagePanel.ChildControls.Add(CurrentProgress_Label);
            CurrentProgress_Label.LayoutDown(Home_PagePanel, FileCount_HorizontalProgressBar, 1);

            Home_PagePanel.ChildControls.Add(CurrentProgress_HorizontalProgressBar);
            CurrentProgress_HorizontalProgressBar.Stretch = Direction.Right;
            CurrentProgress_HorizontalProgressBar.Size = new(Home_PagePanel.ClientSize.Width - 2, 16);
            CurrentProgress_HorizontalProgressBar.LayoutDown(Home_PagePanel, CurrentProgress_Label, 1);
            CurrentProgress_HorizontalProgressBar.Skin.SetAllForegroundColor(BlockManager.Concrete.LightBlue);

            Home_PagePanel.ChildControls.Add(TotalProgress_Label);
            TotalProgress_Label.LayoutDown(Home_PagePanel, CurrentProgress_HorizontalProgressBar, 1);

            Home_PagePanel.ChildControls.Add(TotalProgress_HorizontalProgressBar);
            TotalProgress_HorizontalProgressBar.Stretch = Direction.Right;
            TotalProgress_HorizontalProgressBar.Size = new(Home_PagePanel.ClientSize.Width - 2, 16);
            TotalProgress_HorizontalProgressBar.LayoutDown(Home_PagePanel, TotalProgress_Label, 1);
            TotalProgress_HorizontalProgressBar.Skin.SetAllForegroundColor(BlockManager.Concrete.LightBlue);

            Home_PagePanel.ChildControls.Add(Copying_Label);
            Copying_Label.LayoutDown(Home_PagePanel, TotalProgress_HorizontalProgressBar, 1);

            Home_PagePanel.ChildControls.Add(Speed_Label);
            Speed_Label.LayoutDown(Home_PagePanel, Copying_Label, 1);

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

            ViewData viewData = GetViewData(_fileCopyHandler);
            ProgressTimestamp progressTimestamp = new(DateTime.Now, viewData.TotalFileBytes.Completed);
            _throughput = progressTimestamp - _progressTimestamp;
            _progressTimestamp = progressTimestamp;

            if (_viewData != viewData)
            {
                _viewData = viewData;
                UpdateView();
            }

            if (_viewData.FileCount.Completed >= _viewData.FileCount.Total ||
                _task.IsCompleted ||
                _cancellationTokenSource.IsCancellationRequested)
                CloseForm();
        }

        public override void CloseForm()
        {
            if (_viewData.FileCount.Completed >= _viewData.FileCount.Total ||
                _task.IsCompleted ||
                _cancellationTokenSource.IsCancellationRequested)
            {
                base.CloseForm();
                return;
            }

            _ = DialogBoxHelper.OpenMessageBoxAsync(
                this,
                "警告",
                "文件正在复制中，是否取消？",
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

            CurrentProgress_Label.Text = $"当前进度：{_bytesFormatter.Format(_viewData.CurrentFileBytes.Completed)}/{_bytesFormatter.Format(_viewData.CurrentFileBytes.Total)} ({Math.Round(_viewData.CurrentFileBytes.Progress * 100)}%)";
            CurrentProgress_HorizontalProgressBar.Progress = _viewData.CurrentFileBytes.Progress;

            TotalProgress_Label.Text = $"整体进度：{_bytesFormatter.Format(_viewData.TotalFileBytes.Completed)}/{_bytesFormatter.Format(_viewData.TotalFileBytes.Total)} ({Math.Round(_viewData.TotalFileBytes.Progress * 100)}%)";
            TotalProgress_HorizontalProgressBar.Progress = _viewData.TotalFileBytes.Progress;

            Copying_Label.Text = "正在复制：" + (_fileCopyHandler.CurrentFile?.Source?.Name is string path ? Path.GetFileName(path) : "已完成");
            Speed_Label.Text = $"读写速度：{_bytesFormatter.Format(_throughput.SpeedPerSecond)}/s";
        }

        private static ViewData GetViewData(FileCopyHandler fileCopyHandler)
        {
            ArgumentNullException.ThrowIfNull(fileCopyHandler, nameof(fileCopyHandler));

            return new(
                new(fileCopyHandler.TotalFiles, fileCopyHandler.CompletedFiles),
                new(fileCopyHandler.TotalBytes, fileCopyHandler.CompletedBytes),
                fileCopyHandler.CurrentFile is IOStream fileCopyStream ?
                new(fileCopyStream.Source.Length, fileCopyStream.Destination.Length) : new(0, 0));
        }
    }
}
