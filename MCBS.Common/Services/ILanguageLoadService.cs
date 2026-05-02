using QuanLib.Minecraft.Resource;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface ILanguageLoadService
    {
        public LanguageManager LoadLanguage(ResourceEntryManager resourceEntryManager, string language);
    }
}
