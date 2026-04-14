using MCBS.WpfApp.ViewModels.Downloading;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IMinecraftDownloadViewModelFactory
    {
        public MinecraftDownloadViewModel Create(string gameVersion, string language, string downloadSource);
    }
}
