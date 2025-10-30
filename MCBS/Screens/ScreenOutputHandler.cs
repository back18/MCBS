using QuanLib.Minecraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕输出处理
    /// </summary>
    public class ScreenOutputHandler
    {
        public ScreenOutputHandler(ScreenManager owner)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
        }

        private readonly ScreenManager _owner;

        public void HandleOutput()
        {
            List<WorldBlock> blocks = GetBlockUpdateList();
            if (blocks.Count == 0)
                return;

            MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlock(blocks);
        }

        public async Task HandleOutputAsync()
        {
            List<WorldBlock> blocks = GetBlockUpdateList();
            if (blocks.Count == 0)
                return;

            await Task.Yield();
            await MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayBatchSetBlockAsync(blocks);
        }

        private List<WorldBlock> GetBlockUpdateList()
        {
            List<WorldBlock> blocks = [];

            foreach (ScreenContext screenContext in _owner.Items.Values.Where(w => w.StateMachine.CurrentState == ScreenState.Active))
                blocks.AddRange(screenContext.ScreenUpdateHandler.GetBlockUpdateList());

            return blocks;
        }
    }
}
