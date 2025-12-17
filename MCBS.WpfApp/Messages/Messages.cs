using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Navigation;

namespace MCBS.WpfApp.Messages
{
    public record AppStartupMessage(StartupEventArgs EventArgs);

    public record AppExitMessage(ExitEventArgs EventArgs);

    public record MainWindowClosingMessage(CancelEventArgs EventArgs);

    public record PageNavigatingFromMessage(NavigatingCancelEventArgs EventArgs);
}
