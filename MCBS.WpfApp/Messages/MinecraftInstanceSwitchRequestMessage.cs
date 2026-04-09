using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Messages
{
    public class MinecraftInstanceSwitchRequestMessage(string oldInstanceName, string newInstanceName) : RequestMessage<bool>
    {
        public string OldInstanceName { get; init; } = oldInstanceName;

        public string NewInstanceName { get; init; } = newInstanceName;
    }
}
