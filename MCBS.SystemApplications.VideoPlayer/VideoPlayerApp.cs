using FFMediaToolkit;
using MCBS.BlockForms.DialogBox;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.VideoPlayer
{
    public class VideoPlayerApp : Application
    {
        public const string ID = "VideoPlayer";

        public const string Name = "视频播放器";

        public override object? Main(string[] args)
        {
            try
            {
                FFmpegLoader.LoadFFmpeg();
            }
            catch (Exception ex)
            {
                IForm? initiator = MCOS.Instance.ProcessOf(this)?.Initiator;
                if (initiator is not null)
                    DialogBoxHelper.OpenMessageBox(initiator, "警告", $"因为FFmpeg加载失败，视频播放器无法使用，错误信息：\n{ex.GetType()}: {ex.Message}", MessageBoxButtons.OK);
                return null;
            }

            string? path = null;
            if (args.Length > 0)
                path = args[0];

            RunForm(new VideoPlayerForm(path));
            return null;
        }
    }
}
