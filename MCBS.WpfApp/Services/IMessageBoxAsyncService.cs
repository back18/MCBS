using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MCBS.WpfApp.Services
{
    public interface IMessageBoxAsyncService
    {
        public Task<MessageBoxResult> ShowAsync(string messageBoxText);

        public Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption);

        public Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button);

        public Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);

        public Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult);

        public Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText);

        public Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption);

        public Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button);

        public Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);

        public Task<MessageBoxResult> ShowAsync(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult);
    }
}
