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
            _semaphore = new(0);
        }

        private readonly ScreenManager _owner;

        private readonly SemaphoreSlim _semaphore;

        public bool IsDelaying => _semaphore.CurrentCount > 0;

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

        public async Task HandleDelayOutputAsync()
        {
            Task delay = WaitSemaphoreAsync();
            List<WorldBlock> blocks = GetBlockUpdateList();

            if (blocks.Count == 0)
                return;

            await Task.Yield();
            await MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.OnewaySender.SendOnewayDelayBatchSetBlockAsync(blocks, delay);
        }

        public void ReleaseSemaphore()
        {
            _semaphore.Release();
        }

        private List<WorldBlock> GetBlockUpdateList()
        {
            List<WorldBlock> blocks = [];

            foreach (ScreenContext screenContext in _owner.Items.Values.Where(w => w.StateMachine.CurrentState == ScreenState.Active))
                blocks.AddRange(screenContext.ScreenUpdateHandler.GetBlockUpdateList());

            return blocks;
        }

        private async Task WaitSemaphoreAsync()
        {
            await _semaphore.WaitAsync();
        }
    }
}
