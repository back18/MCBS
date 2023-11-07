using FFMediaToolkit;
using MCBS.Application;
using MCBS.BlockForms.DialogBox;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.VideoPlayer
{
    public class VideoPlayerApp : IProgram
    {
        public const string ID = "System.VideoPlayer";

        public const string Name = "视频播放器";

        public int Main(string[] args)
        {
            try
            {
                FFmpegLoader.LoadFFmpeg();
            }
            catch (Exception ex)
            {
                IForm? initiator = MCOS.Instance.ProcessContextOf(this)?.Initiator;
                if (initiator is not null)
                    DialogBoxHelper.OpenMessageBox(initiator, "警告", $"由于FFmpeg加载失败，因此视频播放器无法使用，错误信息：\n{ex.GetType()}: {ex.Message}", MessageBoxButtons.OK);
                return -1;
            }

            string? path = null;
            if (args.Length > 0)
                path = args[0];

            this.RunForm(new VideoPlayerForm(path));
            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
