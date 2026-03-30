using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MCBS.WpfApp.Services.Implementations
{
    public sealed class FrameNavigationProvider : INavigationProvider
    {
        public FrameNavigationProvider(Frame frame)
        {
            ArgumentNullException.ThrowIfNull(frame, nameof(frame));

            _frame = frame;
        }

        private readonly Frame _frame;

        public NavigationService NavigationService => _frame.NavigationService;
    }
}
