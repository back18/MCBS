using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms;
using MCBS.Event;
using MCBS.Logging;
using MCBS.UI;
using QuanLib.Core.Event;
using QuanLib.Minecraft.Block;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MCBS.BlockForms.Utility;

namespace MCBS.SystemApplications.PackageManager
{
    public class PackageManagerForm : WindowForm
    {
        public PackageManagerForm(string? path = null)
        {
            if (path != null)
            {
                this.path = path;
            }
        }

        private readonly string path = "";
        private readonly TextBox SearchBox = new TextBox();
        private readonly Button SearchButton = new Button();
        private readonly ScrollablePanel ScrollViewer = new ScrollablePanel();
        private readonly Button RefreshButton = new Button();
        private readonly Button btn = new Button();

        public override void Initialize()
        {
            base.Initialize();

            if (this.path == "")
            {
                ClientPanel.SubControls.Add(SearchButton);
                SearchButton.Text = "搜索";
                SearchButton.ClientLocation = new(2, 2);

                ClientPanel.SubControls.Add(RefreshButton);
                RefreshButton.Text = "刷新";
                RefreshButton.ClientLocation = new(ClientPanel.Width - 2 - RefreshButton.Width, 2);
                RefreshButton.RightClick += RefreshButton_RightClick;

                ClientPanel.SubControls.Add(SearchBox);
                SearchBox.Width = ClientPanel.ClientSize.Width - 2 * 3 - 6 - SearchButton.Width - RefreshButton.Width;
                SearchBox.ClientLocation = new(2 + SearchButton.Width + 6, 2);

                ClientPanel.SubControls.Add(ScrollViewer);
                ScrollViewer.Width = ClientPanel.ClientSize.Width - 2 * 2;
                ScrollViewer.ClientLocation = new(2, 2 * 2 + SearchButton.Height);
                ScrollViewer.Height = ClientPanel.ClientSize.Height - 2 * 3 - SearchButton.Height;

                RefreshListInNewThread();
            }
            else
            {
                try
                {
                    List<object?> objects = PackageManager.GetExternalApp(this.path);
                    List<ApplicationInfo> apps = new List<ApplicationInfo>();
                    foreach (object? obj in objects)
                    {
                        ApplicationInfo? application = obj as ApplicationInfo;
                        if (application != null)
                        {
                            apps.Add(application);
                        }
                    }
                    if (apps.Count > 1)
                    {
                        throw new InvalidOperationException("选定的DLL是一个有效的程序集，但是其程序集结构无法被加载。");
                    }
                    else if (apps.Count == 0)
                    {
                        throw new InvalidOperationException("选定的DLL不是一个有效的程序集，或其不包含一个可被识别的应用程序。");
                    }
                    else
                    {
                        Type? baseType = apps[0].GetType().BaseType;
                        Type? t = null;
                        if (baseType != null)
                        {
                            t = baseType.GetGenericArguments()[0];
                        }
                        if (t != null)
                        {
                            Application? appInst = Activator.CreateInstance(t) as Application;
                            if (appInst != null)
                            {
                                Thread thread = new Thread(() => appInst.Main(new string[0]));
                                thread.Start();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DialogBoxHelper.OpenMessageBox(this, "加载DLL失败", ex.Message, MessageBoxButtons.OK);
                }
            }
        }

        private void RefreshButton_RightClick(Control sender, CursorEventArgs e)
        {
            RefreshListInNewThread();
        }

        public void RefreshListInNewThread()
        {
            Thread thread = new Thread(RefreshOnlinePackageList);
            thread.Start();
        }

        public void RefreshOnlinePackageList()
        {
            ControlCollection<Control> ScrollViewerControl = ScrollViewer.AsControlCollection<Control>()!;
            ScrollViewerControl.Clear();
            int index = 0;
            foreach (PackageManager.PackageList.Package package in PackageManager.GetPKGMList())
            {
                Label name = new Label();
                name.Text = package.name;
                name.Height = 20;
                name.ClientLocation = new(2, 2 + index * 42);

                Label author = new Label();
                author.Text = package.author;
                author.Height = 20;
                author.ClientLocation = new(2, 22 + index * 22);

                Button download = new Button();
                download.ClientLocation = new(ScrollViewer.Width - download.Width - 2 * 2, 22 + index * 22);
                download.Text = "下载";

                ScrollViewerControl.Add(name);
                ScrollViewerControl.Add(author);
                ScrollViewerControl.Add(download);
            }
        }
    }
}
