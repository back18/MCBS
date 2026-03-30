using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iNKORE.UI.WPF.Modern.Controls;
using MCBS.WpfApp.Models;
using MCBS.WpfApp.Resources.Strings;
using MCBS.WpfApp.Services;
using Newtonsoft.Json.Linq;
using QuanLib.Core;
using QuanLib.DataAnnotations;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Windows;

namespace MCBS.WpfApp.ViewModels.DialogBoxs
{
    public partial class CreateInstanceViewModel : ObservableValidator, IDialogViewModel
    {
        public CreateInstanceViewModel(
            IInstanceListStorage instanceListStorage,
            IMessageBoxService messageBoxService,
            IFolderDialogService folderDialogService)
        {
            ArgumentNullException.ThrowIfNull(instanceListStorage, nameof(instanceListStorage));
            ArgumentNullException.ThrowIfNull(messageBoxService, nameof(messageBoxService));
            ArgumentNullException.ThrowIfNull(folderDialogService, nameof(folderDialogService));

            _instanceListStorage = instanceListStorage;
            _messageBoxService = messageBoxService;
            _folderDialogService = folderDialogService;

            InstanceName = string.Empty;
            MinecraftVersion = string.Empty;
            MinecraftPath = string.Empty;
            IsServer = false;
            ServerAddress = "localhost";
            ServerPort = 25565;
        }

        private readonly IInstanceListStorage _instanceListStorage;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IFolderDialogService _folderDialogService;

        public ContentDialogResult DialogResult { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        [FileName]
        public partial string InstanceName { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string MinecraftVersion { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        [DirectoryExists]
        public partial string MinecraftPath { get; set; }

        [ObservableProperty]
        public partial bool IsServer { get; set; }

        [ObservableProperty]
        [RequiredIf(nameof(IsServer), CompareOperator.Equal, true, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
        public partial string ServerAddress { get; set; }

        [ObservableProperty]
        [RequiredIf(nameof(IsServer), CompareOperator.Equal, true, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
        public partial ushort ServerPort { get; set; }

        [RelayCommand]
        public void Create(ContentDialogButtonClickEventArgs eventArgs)
        {
            ArgumentNullException.ThrowIfNull(eventArgs, nameof(eventArgs));

            ValidateAllProperties();
            if (HasErrors)
            {
                var errors = GetErrors();
                string errorMessage = string.Join(Environment.NewLine, errors.Select(s => s.ErrorMessage));
                _messageBoxService.Show(
                    string.Format(Lang.MessageBox_Warn_InvalidProperties, errorMessage),
                    Lang.MessageBox_Caption_UnableCreateInstance,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                eventArgs.Cancel = true;
            }

            try
            {
                MinecraftInstanceIndex instanceIndex = _instanceListStorage.GetInstanceIndex();
                if (instanceIndex.InstanceList.Contains(InstanceName))
                {
                    _messageBoxService.Show(
                        string.Format(Lang.MessageBox_Warn_InstanceNameExists, InstanceName),
                        Lang.MessageBox_Caption_UnableCreateInstance,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    eventArgs.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.Show(
                        string.Format(Lang.MessageBox_Error_UnableAccessInstanceList, ObjectFormatter.Format(ex)),
                        Lang.MessageBox_Caption_UnableCreateInstance,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                eventArgs.Cancel = true;
            }
        }

        [RelayCommand]
        public async Task SelectInstance()
        {
            SingleselectDialogResult dialogResult = _folderDialogService.ShowSingleselectDialog(Lang.CreateInstanceDialog_SelectFolder_Title, Environment.CurrentDirectory);
            if (!dialogResult.IsSuccess)
                return;

            MinecraftPath = dialogResult.SelectedItem;
            if (!IsServer && (string.IsNullOrEmpty(InstanceName) || string.IsNullOrEmpty(MinecraftVersion)))
            {
                string? versionJsonFile = await QueryVersionJsonFilePath(MinecraftPath, MinecraftVersion);
                if (string.IsNullOrEmpty(versionJsonFile))
                    return;

                VersionJson? versionJson = await TryLoadVersionJson(versionJsonFile);
                if (versionJson is null)
                    return;

                VersionJsonPatch? versionPatch = versionJson.GetGamePatch();
                if (versionPatch is null)
                    return;

                if (string.IsNullOrEmpty(InstanceName))
                    InstanceName = versionJson.Id;

                if (string.IsNullOrEmpty(MinecraftVersion))
                    MinecraftVersion = versionPatch.Version;
            }
        }

        public MinecraftInstanceConfig.Model CreateConfigModel()
        {
            var model = MinecraftInstanceConfig.Model.CreateDefault();
            model.InstanceName = InstanceName;
            model.MinecraftVersion = MinecraftVersion;
            model.MinecraftPath = MinecraftPath;

            if (IsServer)
            {
                model.IsServer = true;
                model.ServerAddress = ServerAddress;
                model.ServerPort = ServerPort;
            }

            return model;
        }

        private static async Task<string?> QueryVersionJsonFilePath(string minecraftPath, string? minecraftVersion = null)
        {
            if (!Directory.Exists(minecraftPath))
                return null;

            List<string> versionJsonFiles = [];
            string? baseFolder = Path.GetDirectoryName(minecraftPath);
            if (Directory.Exists(baseFolder) && Path.GetFileName(baseFolder) == "versions")
            {
                string jsonFile = Path.Combine(minecraftPath, Path.GetFileName(minecraftPath) + ".json");
                if (File.Exists(jsonFile))
                {
                    if (string.IsNullOrEmpty(minecraftVersion))
                        return jsonFile;
                    else
                        versionJsonFiles.Add(jsonFile);
                }
            }

            string versionIsolationFolder = Path.Combine(minecraftPath, "versions");
            if (Directory.Exists(versionIsolationFolder))
            {
                string[] versionFolders = Directory.GetDirectories(versionIsolationFolder);
                foreach (string versionFolder in versionFolders)
                {
                    string jsonFile = Path.Combine(versionFolder, Path.GetFileName(versionFolder) + ".json");
                    if (File.Exists(jsonFile))
                        versionJsonFiles.Add(jsonFile);
                }
            }

            if (versionJsonFiles.Count == 0)
                return null;

            if (string.IsNullOrEmpty(minecraftVersion))
                return versionJsonFiles[0];

            foreach (string versionJsonFile in versionJsonFiles)
            {
                VersionJson? versionJson = await TryLoadVersionJson(versionJsonFile);
                VersionJsonPatch? versionPatch = versionJson?.GetGamePatch();
                if (versionPatch is null)
                    continue;

                if (versionPatch.Version == minecraftVersion)
                    return versionJsonFile;
            }

            return null;
        }

        private static async Task<VersionJson?> TryLoadVersionJson(string versionJsonFile)
        {
            try
            {
                string json = await File.ReadAllTextAsync(versionJsonFile, Encoding.UTF8);
                JObject jobj = JObject.Parse(json);
                return new VersionJson(jobj);
            }
            catch
            {
                return null;
            }
        }
    }
}
