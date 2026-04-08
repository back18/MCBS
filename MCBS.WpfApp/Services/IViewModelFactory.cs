using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IViewModelFactory<TViewModel>
    {
        public TViewModel Create(string viewModel);
    }
}
