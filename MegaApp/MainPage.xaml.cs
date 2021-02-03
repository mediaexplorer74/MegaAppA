// TApp1 (Test App for MegaLib) 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CG.Web.MegaApiClient;
using Windows.Storage;
using Windows.UI.Popups;
using System.Text;
using Megabox.Model;
using System.Diagnostics;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace MegaApp
{

    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //private MainPage rootPage = MainPage.Current;

        
        public MainPage()
        {
            this.InitializeComponent();

            
            //TEST 0
            //string megaStorage = "https://mega.nz/#F!e1ogxQ7T!ee4Q_ocD1bSLmNeg9B6kBw"; // Sample 1 (created by someone)

            //TEST 1
            // string megaStorage = "https://mega.nz/folder/YdlWiaxD#7qcjO0mtYukRBCuDzoIwGA"; // Sample 2 (created by ME)

            //TEST 2
            string megaStorage = "https://mega.nz/#F!SYtigRjB!EhNuflDF9fefSXuolgn0Rw"; // Prod (W10M)

            // Collect and prepare Mega.nz data...
            Preprocess(megaStorage);

        }


        // Add new item into ListView
        void SuperWriteLine(string s)
        {
            ListViewItem lvi = new ListViewItem();

            lvi.Content = s;
            //lvi.ImageIndex = 0; // установка картинки для файла
            // добавляем элемент в ListView
            ListView01.Items.Add(lvi);
        }


       
        // Button01 Press handler (!!!MY Test zone!!!)
        private void Button01_Click(object sender, RoutedEventArgs e)
        {
            
            string megaStorage = "https://mega.nz/folder/YdlWiaxD#7qcjO0mtYukRBCuDzoIwGA"; 

            // Collect and prepare Mega.nz data...
            Preprocess(megaStorage);
        }


        // Collect and prepare Mega.nz data...
        private void Preprocess(string MegaSharedFolderURL)
        { 
            Contact.MegaCount = 0; // counter init 

            ListView01.Items.Clear(); // clear ListView 

                       
            // "no login"
            MegaClient.client.LoginAnonymous(); // "Users only" =)

            // GetNodes retrieves all files/folders metadata from Mega
            // so this method can be time consuming

            //IEnumerable<INode> nodes = MegaClient.client.GetNodes();

           
            // Reconstruct URI
            Uri folderLink = new Uri(MegaSharedFolderURL);

            // Get nodes (folders and files in level 0)
            MegaClient.nodes = MegaClient.client.GetNodesFromLink(folderLink);

            // Get parent node 
            MegaClient.parent = MegaClient.nodes.Single(n => n.Type == NodeType.Root);

            // Get sub-nodes (folders and files in level 1, 2, etc...)
            ProcessAllSubNodes(MegaClient.nodes, MegaClient.parent);

            // Logout
            MegaClient.client.Logout();
        }

        // Process All Sub Nodes (Level 1, 2, etc...)
        void ProcessAllSubNodes(IEnumerable<INode> nodes, INode parent, int level = 0)
        {
            string infos; 
            IEnumerable<INode> children = nodes.Where(x => x.ParentId == parent.Id);

            foreach (INode child in children)
            {
                //string infos = $"- {child.Name} - {child.Size} bytes - {child.CreationDate}";

                if (child.Type.ToString() == "Directory")
                {
                    infos = $"{child.Name.ToUpper()}";
                }
                else
                {
                    infos = $"{child.Name} [{child.Size} bytes]";
                }


                //SuperWriteLine(infos.PadLeft(infos.Length + level, '\t'));
                SuperWriteLine(infos);

                // Temp 

                Contact.MegaFName[Contact.MegaCount] = child.Name;
                Contact.MegaFKey[Contact.MegaCount] = child.Id;

                // Store ALL INode object !
                MegaClient.arNodes[Contact.MegaCount] = child;

                // Increase counter 
                Contact.MegaCount++;

                // Если нашли "потомков" класса "Папка", повторяем маневры... 
                if (child.Type == NodeType.Directory)
                {
                    ProcessAllSubNodes(nodes, child, level + 1);
                }
            }
        }//ProcessAllSubNodes



        // ListView01 Select Changed (Click handler)
        private async void Click_ListView01SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            // Phase 1 

            //Temp: "Click / Tap " handle with Diagnostics output (message box only)
            ListView l1 = sender as ListView;
            int idx = l1.SelectedIndex;



            if (MegaClient.arNodes[idx].Type.ToString() != "File") return;

            ContentDialog SimpleDialog = new ContentDialog()
            {
                Title = "Operation Request",
                Content = "Download into Image folder & launch \n" +
                MegaClient.arNodes[idx].Name +
                " \n" +
                "[file size: " +
                MegaClient.arNodes[idx].Size.ToString() + " bytes\n"
                + "mod. date: " +
                MegaClient.arNodes[idx].ModificationDate +
                "] ?"
                ,
                PrimaryButtonText = "ОК",
                SecondaryButtonText = "Cancel"
            };

            ContentDialogResult result = await SimpleDialog.ShowAsync();

            //if (result == ContentDialogResult.Primary)
            //{
            //    header.Text = "Файл удален";
            //}
            //else 
            if (result == ContentDialogResult.Secondary)
            {
                return;//header.Text = "Отмена действия";
            }

            // Phase 2

            //"MEGA login"
            MegaClient.client.LoginAnonymous();

            // Show startpoint...
            filesList.Text = "Downloading...";

            // Скачиваем файл и получаем полный путь к нему...
            string FullLSPath = MegaClient.client.DownloadFile
            (
                MegaClient.arNodes[idx],
                MegaClient.arNodes[idx].Name
            );

            //"MEGA logout"
            MegaClient.client.Logout();


            // -----------------

            StorageFolder folder = ApplicationData.Current.LocalFolder;//KnownFolders.VideosLibrary;
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();

            // Пробегаемся по всем файлам в хранилище... ищем скачанный 
            // TODO: че-то придумать с избавлением от цикла!!!
            foreach (StorageFile ffile in files)
            {
                if (ffile.Path == FullLSPath)
                {
                    // Show stop-point...
                    filesList.Text = $"File downloaded from Mega.nz into Images folder.\n";
                    filesList.Text += $"{ffile.Name}\n";
                    var props = await ffile.GetBasicPropertiesAsync();
                    filesList.Text += $"Date modified: {props.DateModified} \n";
                    filesList.Text += $"Size: {props.Size} \n\n";

                    // ++++++++++++++++++++

                    StorageFolder fLibrary = await KnownFolders.GetFolderForUserAsync(null, KnownFolderId.PicturesLibrary);
                    StorageFile fileCopy = await ffile.CopyAsync(fLibrary, MegaClient.arNodes[idx].Name, NameCollisionOption.ReplaceExisting);
                }
            }


            // Phase 3 - Launch result !

            // Path to the file in the app package to launch
            string appxFile = MegaClient.arNodes[idx].Name;


            // определяем локальное хранилище
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            // подправляем путь
            string FullPath = localFolder.Path + "\\" + appxFile;

            //string FullLSPath = client.DownloadFile(fileLink, node.Name);

            var appFile = await localFolder.GetFileAsync(appxFile);


            if (appFile != null)
            {
                // Launch the retrieved file
                var success = await Windows.System.Launcher.LaunchFileAsync(appFile);

                if (success)
                {
                    // File launched (TODO: some msg)
                }
                else
                {
                    // File launch failed (TODO: some msg)
                }
            }
            else
            {
                // Could not find file (TODO: some msg)
            }

        }// click handle end

        // Experimental : TODO
        private void ItemClickHandle(object sender, ItemClickEventArgs e)
        {
            
        }//
    }
}
