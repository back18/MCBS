using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface ILanguageAssetMatchService
    {
        public string[] Match(string assetName);
    }
}
