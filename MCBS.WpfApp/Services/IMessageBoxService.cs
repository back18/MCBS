using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MCBS.WpfApp.Services
{
    public interface IMessageBoxService
    {
        public MessageBoxResult Show(string messageBoxText);

        public MessageBoxResult Show(string messageBoxText, string caption);

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button);

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult);

        public MessageBoxResult Show(Window owner, string messageBoxText);

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption);

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button);

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult);
    }
}
