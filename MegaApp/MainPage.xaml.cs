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
using MegaApp.Model;
using System.Diagnostics;



namespace MegaApp
{
    
    public sealed partial class MainPage : Page
    {
        CollectionViewSource cvs;
        
        IEnumerable<CategoryGroup> groups;

       
        string CurrectFCategory; // текущая категория файлов указатель на группу)
        int DirectoryCount;      // counter для каталогов

        string megaStorageURL; // 


        public MainPage()
        {
           
            this.InitializeComponent();

            // 1 Preprocess...

            // SANDBOX (ME MEGA channel)
            //megaStorageURL = "https://mega.nz/folder/YdlWiaxD#7qcjO0mtYukRBCuDzoIwGA"; // Sample 2 (created by ME)

            // PRODUCTION (W10M Tg MEGA channel)
            megaStorageURL = "https://mega.nz/#F!SYtigRjB!EhNuflDF9fefSXuolgn0Rw"; 

            // Collect and prepare Mega.nz data...
            Preprocess(megaStorageURL);


            // 2 Form App Groups
            this.NavigationCacheMode = NavigationCacheMode.Required;


            // create transit object mc
            var mc = new SourceData();

            IEnumerable<SourceData> myEnumerable = mc.GetData();
                        
            IEnumerable<CategoryGroup> groups =
                from item in myEnumerable
                group item by item.Category
                    into categoryGroup
                let categoryGroupItems =
                    from item2 in categoryGroup
                    group item2 by item2.Product
                    into productGroup
                    select new ProductGroup(productGroup)
                    {
                        Product = productGroup.Key
                    }
                select new CategoryGroup(categoryGroupItems)
                {
                    Category = categoryGroup.Key
                };

            //var 
            cvs = (CollectionViewSource)Resources["src"];

            cvs.Source = groups.ToList();


           
        }// MainPage



        // Collect and prepare Mega.nz data...
        private void Preprocess(string MegaSharedFolderURL)
        {

            //TEMP
            //filesList.Items.Clear(); // clear it =)
          
            Contact.MegaCount = 0; // counter init 
            
            CurrectFCategory = "<Root>"; // 'parent' group (file category) init  
            DirectoryCount = 0;
            
            //ListView01.Items.Clear(); // clear ListView 
           

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

                    // *** FOLDER object found ***

                    // New Category found?
                    if ( child.Name != CurrectFCategory && child.Size == 0)
                    {
                        // Change category !
                        CurrectFCategory = $"[{DirectoryCount.ToString()}] {child.Name.ToUpper()}";

                        DirectoryCount++; // текущая категория файлов указатель на группу)
                    }


                    //infos = $"{child.Name.ToUpper()}";

                    //SuperWriteLine(infos.PadLeft(infos.Length + level, '\t'));

                    //Contact.MegaFName[Contact.MegaCount] = $"";


                }
                else
                {
                    //*** FILE object found ***

                    //infos = $"{child.Name} [{child.Size} bytes]";

                    //SuperWriteLine(infos.PadLeft(infos.Length + level, '\t'));

                    // Fulfil "Contact" repo
                    Contact.MegaFName[Contact.MegaCount]
                    = $"[{Contact.MegaCount}] {child.Name}";

                    

                    Contact.MegaFCategory[Contact.MegaCount]
                        = $"{CurrectFCategory}";



                    Contact.MegaFKey[Contact.MegaCount]
                        = child.Id;

                    // Store ALL INode object !
                    MegaClient.arNodes[Contact.MegaCount] = child;


                    // show full info 
                    infos = $"{Contact.MegaCount} | {CurrectFCategory.ToUpper()} | {child.Name}";

                    SuperWriteLine(infos);

                    // Increase counter 
                    Contact.MegaCount++;

                }


                

                // Если нашли "потомков" класса "Папка", повторяем маневры... 
                if (child.Type == NodeType.Directory)
                {
                    ProcessAllSubNodes(nodes, child, level + 1);
                }
            }
        }//ProcessAllSubNodes


        // Add new item into ListView
        void SuperWriteLine(string s)
        {
            //filesList.Items.Add(s);
        }


        // GroupBox Manual update handle (when click)
        private void GroupBoxManualUpdate_Click(object sender, RoutedEventArgs e)
        {
          
            // ------------------------------------------------------------------

            // SANDBOX (ME MEGA channel)
            //megaStorageURL = "https://mega.nz/folder/YdlWiaxD#7qcjO0mtYukRBCuDzoIwGA"; 

            // PRODUCTION (W10M Tg MEGA channel)
            megaStorageURL = "https://mega.nz/#F!SYtigRjB!EhNuflDF9fefSXuolgn0Rw";

            // Collect and prepare Mega.nz data...
            Preprocess(megaStorageURL);

            //-------------------------------------------------------------------

            // RnD: Manual forming of some app groups
            this.NavigationCacheMode = NavigationCacheMode.Required;

         
            //IEnumerable<CategoryGroup> 

            // create transit object mc
            var mc = new SourceData();
                       
            IEnumerable<SourceData> myEnumerable = mc.GetData();


            groups =
                from item in myEnumerable
                group item by item.Category
                    into categoryGroup
                let categoryGroupItems =
                    from item2 in categoryGroup
                    group item2 by item2.Product
                    into productGroup
                    select new ProductGroup(productGroup)
                    {
                        Product = productGroup.Key
                    }
                select new CategoryGroup(categoryGroupItems)
                {
                    Category = categoryGroup.Key
                };

            
            cvs = (CollectionViewSource)Resources["src"];

            cvs.Source = groups.ToList();

            
        }

        

        
        // GroupBox Item taped 
        private async void GroupBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //var idx = 0; // 

            // Init status...
            //filesList.Text = $"";

            //Temp: "Click / Tap " handle with Diagnostics output (message box only)
            ListView l1 = sender as ListView;
            int idx = l1.SelectedIndex;

            // FoolishProof *******************
            //1
            if (idx < 0 || idx > 10000) return;

            //2
            //if (isBusy) return;

            //3 

            // Modernize it! =)
            if (MegaClient.arNodes[idx].Type.ToString() != "File") return;
            // ********************************

            /*
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
            */

            /*
            bool Doubling = false;
            bool FirstFound = false;
            bool SecondFound = false;

            // find index key...
            for(var i = 0; i < 10000; i++)
            {
                if (MegaClient.arNodes[i].Id == Contact.MegaFKey[sel_idx])
                {
                    idx = i;

                    if (!FirstFound)
                    {
                        FirstFound = true;

                    }

                    if (FirstFound)
                    {
                        if (!SecondFound)
                        {
                            SecondFound = true;
                            Doubling = true;
                        }
                    }
                }
            }
            */

            ContentDialog SimpleDialog = new ContentDialog()
            {
                Title = "Operation Request",
                Content = "Do you want to download (into Image folder) & launch file \n" +
               MegaClient.arNodes[idx].Name+
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

            if (result == ContentDialogResult.Primary)
            {
                // Change status...
                //filesList.Text = "Downloading...";
            }
            else 
            {
                //if (result == ContentDialogResult.Secondary)
                

                // Change status to empty
               //filesList.Text = "";
                return;
            }

            // Phase 2

            //"MEGA login"
            MegaClient.client.LoginAnonymous();

            

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
                    //filesList.Text = $"File downloaded from Mega.nz into Images folder.\n";
                    //filesList.Text += $"{ffile.Name}\n";
                    var props = await ffile.GetBasicPropertiesAsync();
                    //filesList.Text += $"Date modified: {props.DateModified} \n";
                    //filesList.Text += $"Size: {props.Size} \n\n";

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

            

        }// GroupBox_Taped end


    }// Page partial class end 


} // namespace MegaApp end
