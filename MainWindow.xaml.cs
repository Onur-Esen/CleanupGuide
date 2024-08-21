using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CleanupGuide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string UserTempDescription = "Temporary files for the current user";
        string UserTempCaution = "Some files might be in use and give skip error";
        string AdminTempDescription = "Temporary files for the user admin";
        string AdminTempCaution = "Some files might be in use and give skip error";
        string RevitCacheDescription = "Collaboration cache is a temporary storage for changes made by users as they edit a project.";
        string RevitCacheCaution = "Loading the model next time might take time";
        string RevitPacDescription = "Pac cache is a temporary storage used when Revit opens a project.";
        string RevitPacCaution = "Loading the model next time might take time";

        public MainWindow()
        {
            InitializeComponent();

            LangJp();
        }

        private void LangJp()
        {
            UserTempDescription = "現在のユーザーの一時ファイル";
            UserTempCaution = "一部のファイルが使用中であるため、スキップ警告が発生する可能性があります";
            AdminTempDescription = "ユーザーadminの一時ファイル";
            AdminTempCaution = "一部のファイルが使用中であるため、スキップ警告が発生する可能性があります";
            RevitCacheDescription = "コラボレーションキャッシュは、ユーザーがプロジェクトを編集する際に行った変更を一時的に保存するための領域です。";
            RevitCacheCaution = "次回モデルを開く時に時間がかかります。";
            RevitPacDescription = "Revitがプロジェクトを開く際に使用される一時的なファイルを保存するためのキャッシュです。";
            RevitPacCaution = "次回モデルを開く時に時間がかかります。";

            TextBlockAutodeskUninstall.Text = "オートデスク製品はアンインストールできます。2022以降では、Windows コントロール パネルを使用することをお勧めします。";
            TextBlockDiskCleanup.Text = "不要なファイルを削除するのに非常に便利なツールです。システム ファイルのクリーンアップもお勧めします。";

            ButtonAutodeskUninstall.Content = "開く";
            ButtonDiskCleanup.Content = "開く";
        }

        private class MyListViewItem
        {
            public string Name { get; set; }

            public string Path { get; set; }

            public int Count { get; set; }

            public double Size { get; set; }

            public string SizeText { get; set; }

            public BitmapImage Image { get; set; }

            public string Description { get; set; }

            public string Caution { get; set; }

            public MyListViewItem(string name, string path, string uri, string description, string caution = "")
            {
                Name = name;
                Path = path;
                Count = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
                Size = DirectorySize(new DirectoryInfo(path));
                Description = description;
                Image = new BitmapImage(new Uri(uri));
                Caution = caution;

                double size = Math.Round(Size / 1024 / 1024);
                if (size >= 1024)
                    SizeText = Math.Round(size / 1024) + " GB";
                else
                    SizeText = size + " MB";
            }
        }

        private class MyListViewDiskItem
        {
            public string Disk { get; set; }

            public string Storage { get; set; }

            public double FreeSpace { get; set; }

            public double TotalSpace { get; set; }

            public double Progress { get; set; }

            public BitmapImage Image { get; set;}

            public MyListViewDiskItem(string disk, long freeSpace, long totalSpace)
            {
                Disk = disk;
                FreeSpace = freeSpace / 1024 / 1024 / 1024;
                TotalSpace = totalSpace / 1024 / 1024 / 1024;
                Storage = FreeSpace + " GB / " + TotalSpace + " GB"; ;
                Progress = (TotalSpace - FreeSpace) / TotalSpace * 100;
            }
        }

        private List<MyListViewItem> GetFolderList()
        {
            List<MyListViewItem> items = new List<MyListViewItem>();

            string user = Environment.UserName;
            string uriFolder = "pack://application:,,,/CleanupGuide;component/Resources/WindowsFolder32.png";
            string uriRevit = "pack://application:,,,/CleanupGuide;component/Resources/RevitIcon.png";

            try
            {
                string userTemp = "C:\\Users\\" + user + "\\AppData\\Local\\Temp";
                items.Add(new MyListViewItem("User Temp", userTemp, uriFolder, UserTempDescription, UserTempCaution));
            }
            catch (Exception) { }

            try
            {
                string adminTemp = "C:\\Users\\useradmin\\AppData\\Local\\Temp";
                items.Add(new MyListViewItem("Admin Temp", adminTemp, uriFolder, AdminTempDescription, AdminTempCaution));
            }
            catch (Exception) { }

            try
            {
                string revitPac = "C:\\Users\\" + user + "\\AppData\\Local\\Autodesk\\Revit\\PacCache";
                items.Add(new MyListViewItem("Revit Pac Cache", revitPac, uriRevit, RevitPacDescription, RevitPacCaution));
            }
            catch (Exception) { }

            try
            {
                string[] versions = ["2017","2019", "2020", "2021", "2022", "2023", "2024"];
                foreach (string version in versions)
                {
                    string revitCache = $"C:\\Users\\{user}\\AppData\\Local\\Autodesk\\Revit\\Autodesk Revit {version}\\CollaborationCache";
                    if(Directory.Exists(revitCache))
                        items.Add(new MyListViewItem($"Revit {version} Cache", revitCache, uriRevit, RevitCacheDescription, RevitCacheCaution));
                }
            }
            catch (Exception) { }



            return items;
        }

        private List<MyListViewDiskItem> GetDiskList()
        {
            List<MyListViewDiskItem> items = new List<MyListViewDiskItem>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                MyListViewDiskItem item = new MyListViewDiskItem(drive.VolumeLabel, drive.AvailableFreeSpace, drive.TotalSize);

                var systemRoot = Path.GetPathRoot(Environment.SystemDirectory).ToString();

                if (drive.RootDirectory.Name == systemRoot)
                    item.Image = new BitmapImage(new Uri("pack://application:,,,/CleanupGuide;component/Resources/DriveWindows64.png"));
                else
                    item.Image = new BitmapImage(new Uri("pack://application:,,,/CleanupGuide;component/Resources/DrivePersonal64.png"));

                items.Add(item);
            }

            return items;
        }

        private static double DirectorySize(DirectoryInfo dir)
        {
            double size = 0;

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                size += file.Length;
            }

            // Add subdirectory sizes.
            DirectoryInfo[] subDirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs)
            {
                size += DirectorySize(subDir);
            }
            return size;
        }

        private void ButtonFolder_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string path = button.Tag as string;

            string localFilePath = path;
            Process.Start("explorer.exe", localFilePath);
        }

        #region Installed Products
        // https://stackoverflow.com/questions/3526449/how-to-get-a-list-of-installed-software-products

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern Int32 MsiGetProductInfo(string product, string property,
        [Out] StringBuilder valueBuf, ref Int32 len);

        [DllImport("msi.dll", SetLastError = true)]
        static extern int MsiEnumProducts(int iProductIndex,
            StringBuilder lpProductBuf);

        static List<string> GetUninstallList()
        {
            List<string> items = new List<string>();

            StringBuilder sbProductCode = new StringBuilder(39);
            int iIdx = 0;
            while (0 == MsiEnumProducts(iIdx++, sbProductCode))
            {
                Int32 productNameLen = 512;
                StringBuilder sbProductName = new StringBuilder(productNameLen);

                MsiGetProductInfo(sbProductCode.ToString(),
                    "ProductName", sbProductName, ref productNameLen);

                if (sbProductName.ToString().Contains("Autodesk"))
                {
                    Int32 installDirLen = 1024;
                    StringBuilder sbInstallDir = new StringBuilder(installDirLen);

                    MsiGetProductInfo(sbProductCode.ToString(),
                        "InstallLocation", sbInstallDir, ref installDirLen);

                    Console.WriteLine("ProductName {0}: {1}",
                        sbProductName, sbInstallDir);

                    items.Add(sbProductName.ToString());
                }
            }

            items = items.OrderBy(x => x).ToList();
            return items;
        }

        #endregion

        private void ButtonAutodeskUninstall_Click(object sender, RoutedEventArgs e)
        {
            string path = "C:\\Program Files (x86)\\Common Files\\Autodesk Shared\\Uninstall Tool\\R1\\UninstallTool.exe"; ;

            var psi = new ProcessStartInfo(path)
            {
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        private void ButtonDiskCleanup_Click(object sender, RoutedEventArgs e)
        {
            string path = "C:\\WINDOWS\\system32\\cleanmgr.exe";
            Process.Start(path);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                ListViewDisk.ItemsSource = GetDiskList();

            }));

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                ListViewMain.ItemsSource = GetFolderList();
            }));
        }
    }
}