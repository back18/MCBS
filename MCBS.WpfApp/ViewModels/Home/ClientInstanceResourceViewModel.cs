using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using MCBS.WpfApp.Attributes;
using MCBS.WpfApp.Models;
using MCBS.WpfApp.Resources.Strings;
using Newtonsoft.Json;
using QuanLib.IO.Extensions;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Home
{
    [ExcludeFromDI]
    public partial class ClientInstanceResourceViewModel : ObservableObject
    {
        public ClientInstanceResourceViewModel(MinecraftInstanceConfig.Model instance)
        {
            ArgumentNullException.ThrowIfNull(instance, nameof(instance));

            _instance = instance;

            VersionJsonFile = CreateVersionJsonFileModel(null);
            AssetManifestFile = CreateAssetManifestFileModel(null);
            ClientCoreFile = CreateClientCoreFileModel(null);
            ResourcePacks = [];
            LanguageFiles = [];
            SelectedResourcePacks = [];
            SelectedLanguageFile = null;
        }

        private readonly MinecraftInstanceConfig.Model _instance;

        [ObservableProperty]
        public partial FileModel VersionJsonFile { get; set; }

        [ObservableProperty]
        public partial FileModel AssetManifestFile { get; set; }

        [ObservableProperty]
        public partial FileModel ClientCoreFile { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedLanguage))]
        public partial AssetFileModel? SelectedLanguageFile { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedResourcePacks))]
        public partial ObservableCollection<FileInfo> SelectedResourcePacks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<AssetFileModel> LanguageFiles { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<FileInfo> ResourcePacks { get; set; }

        public bool HasSelectedLanguage => SelectedLanguageFile is not null;

        public bool HasSelectedResourcePacks => SelectedResourcePacks.Count > 0;

        [RelayCommand]
        private async Task Refresh()
        {
            if (_instance.IsServer)
                return;

            string gameFolder = Path.GetFullPath(_instance.MinecraftPath);
            string gameVersion = _instance.MinecraftVersion;
            ClientInstanceResource clientInstanceResource = new(gameFolder, gameVersion);
            await clientInstanceResource.Initialize();

            VersionJsonFile = CreateVersionJsonFileModel(CreateFileInfo(clientInstanceResource.VersionJsonFile));
            AssetManifestFile = CreateAssetManifestFileModel(CreateFileInfo(clientInstanceResource.AssetManifestFile));
            ClientCoreFile = CreateClientCoreFileModel(CreateFileInfo(clientInstanceResource.ClientCoreFile));

            string languageName = clientInstanceResource.GetSelectedLanguageName() + ".json";
            string[] resourcePackNames = clientInstanceResource.GetSelectedResourcePacks().ToArray();

            IEnumerable<AssetFileModel> languageFiles = clientInstanceResource.GetLanguageFiles().ToArray();
            IEnumerable<FileInfo> resourcePacks = clientInstanceResource.GetResourcePacks().Select(f => new FileInfo(f)).ToArray();

            SelectedLanguageFile = languageFiles.FirstOrDefault(s => s.AssetName == languageName);
            if (SelectedLanguageFile is not null)
                languageFiles = languageFiles.Where(s => !s.Equals(SelectedLanguageFile));

            SelectedResourcePacks = new(resourcePacks
                .Where(s => Array.IndexOf(resourcePackNames, s.Name) != -1)
                .OrderBy(s => Array.IndexOf(resourcePackNames, s.Name)));
            if (SelectedResourcePacks.Count > 0)
                resourcePacks = resourcePacks.Except(SelectedResourcePacks);

            LanguageFiles = new(languageFiles);
            ResourcePacks = new(resourcePacks);
        }

        private static FileInfo? CreateFileInfo(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            return new FileInfo(filePath);
        }

        private static FileModel CreateVersionJsonFileModel(FileInfo? fileInfo)
        {
            return new FileModel(fileInfo, nameof(Lang.ClientInstanceResource_VersionJson), SegoeFluentIcons.Document);
        }

        private static FileModel CreateAssetManifestFileModel(FileInfo? fileInfo)
        {
            return new FileModel(fileInfo, nameof(Lang.ClientInstanceResource_AssetManifest), SegoeFluentIcons.Document);
        }

        private static FileModel CreateClientCoreFileModel(FileInfo? fileInfo)
        {
            return new FileModel(fileInfo, nameof(Lang.ClientInstanceResource_ClientCore), SegoeFluentIcons.Package);
        }
    }
}
