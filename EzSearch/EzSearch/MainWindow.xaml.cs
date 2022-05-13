using System;
using System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace EzSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<GridRowContents> SearchFolders = new List<GridRowContents>();
        public MainWindow()
        {
            InitializeComponent();
            
            //populate data grid initially if configuration text file exists
            PopulateSearchFolderList();
        }

        private void PopulateSearchFolderList()
        {
            List<string> ConfigFileContents = new List<string>();
            string ConfPath = Path.Combine(Directory.GetCurrentDirectory(),
                "FolderConf.dat");
            
            FileStream ConfFileStream = new FileStream(ConfPath, 
                FileMode.OpenOrCreate, 
                FileAccess.Read);
            StreamReader ConfFileReader = new StreamReader(ConfFileStream);
            ConfFileReader.BaseStream.Seek(0, SeekOrigin.Begin);
            
            string? ConfLine = ConfFileReader.ReadLine();
            while(ConfLine != null)
            {
                SearchFolders.Add(
                    new GridRowContents(ConfLine.Split(',')[0], 
                    Convert.ToBoolean(ConfLine.Split(',')[1])));
            }

            ConfFileReader.Close();
        }

        private string OpenDirectoryDialog()
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = folderBrowserDialog.ShowDialog();
            if(result.ToString() != String.Empty)
            {
                return folderBrowserDialog.SelectedPath;
            }
            return string.Empty;
        }

        private bool VerifyBrowsedPath(string path)
        {
            if (path.Equals(string.Empty))
            {
                System.Windows.Forms.MessageBox.Show("Please select a folder to continue",
                    "Oops!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            string Path = OpenDirectoryDialog();
            if (VerifyBrowsedPath(Path))
            {
                FolderNameBox.Text = Path;
            }
        }

        private void AddSearchFolderToConf()
        {
            try
            {
                string Selected = string.Empty;
                foreach(var item in SearchFolders)
                {
                    if(item.FolderName.Equals(FolderNameBox.Text))
                    {
                        Selected = item.Select.ToString();
                        break;
                    }
                }

                string ConfPath = Path.Combine(Directory.GetCurrentDirectory(),
                "FolderConf.dat");

                FileStream ConfFileStream = new FileStream(ConfPath,
                    FileMode.Append,
                    FileAccess.Write);
                StreamWriter ConfFileWriter = new StreamWriter(ConfFileStream);
                ConfFileWriter.WriteLine(FolderNameBox.Text + " , " + Selected);

                ConfFileWriter.Close();
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error occurred while updating folders to search.");
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            bool exists = false;
            foreach(var row in SearchFolders)
            {
                if(row.FolderName.Equals(FolderNameBox.Text))
                {
                    exists = true;
                    break;
                }
            }
            if (exists)
            {
                System.Windows.Forms.MessageBox
                    .Show("The selected folder is already in scanned folders list");
                return;
            }
            else if (FolderNameBox.Text.Equals("Press Browse Button"))
            {
                System.Windows.Forms.MessageBox
                    .Show("You need to select a folder before you can add it");
                return;
            }
            else
            {
                SearchFolders.Add(new GridRowContents(FolderNameBox.Text, false));
            }

            //Open file for write, append contents of SearchFolders to file
            AddSearchFolderToConf();

            FolderNameBox.Text = "Press Browse Button";
            
            SelectedFoldersGrid.Items.Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedFoldersGrid.ItemsSource = SearchFolders;
        } 
    }
}
