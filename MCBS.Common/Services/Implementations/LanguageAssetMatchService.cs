using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services.Implementations
{
    public class LanguageAssetMatchService : ILanguageAssetMatchService
    {
        private const char PathSeparator = '/';
        private const char ExtensionSeparator = '.';
        private const string NewAssetNamePrefix = "minecraft/lang/";
        private const string OldAssetNamePrefix = "realms/lang/";
        private const string NewAssetNameSuffix = ".json";
        private const string OldAssetNameSuffix = ".lang";

        public string[] Match(string assetName)
        {
            if (assetName.Contains(PathSeparator))
            {
                if (assetName.Contains(ExtensionSeparator))
                    return [assetName];
                else
                    return [assetName + NewAssetNameSuffix, assetName + OldAssetNameSuffix];
            }
            else
            {
                if (assetName.Contains(ExtensionSeparator))
                    return [NewAssetNamePrefix + assetName, OldAssetNamePrefix + assetName];
                else
                    return
                    [
                        NewAssetNamePrefix + assetName + NewAssetNameSuffix,
                        NewAssetNamePrefix + assetName + OldAssetNameSuffix,
                        OldAssetNamePrefix + assetName + NewAssetNameSuffix,
                        OldAssetNamePrefix + assetName + OldAssetNameSuffix,
                    ];
            }
        }
    }
}
