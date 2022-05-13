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
                    new GridRowContents(ConfLine.Split("||,||")[0].Trim(), 
                    Convert.ToBoolean(ConfLine.Split("||,||")[1].Trim())));
                ConfLine = ConfFileReader.ReadLine();
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
                ConfFileWriter.WriteLine(FolderNameBox.Text + "||,||" + Selected);

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
            try
            {
                SelectedFoldersGrid.ItemsSource = SearchFolders;
            }
            catch
            {

            }
            
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int i = SelectedFoldersGrid.SelectedIndex;
            SearchFolders[i].Select = true;

            //Note to self - Commit both the cell edit and row edit
            //Without both commits, the edit transaction is still in progress
            //and throws an exception when trying to call Refresh()
            //This selection change is not made persistent because selection is only
            //required to make in-memory changes. Storing it in a file is pointless.
            //On the other hand, if the selected rows are deleted,
            //then the file should be updated. This can be handled in the delete button.
            SelectedFoldersGrid.CommitEdit();
            SelectedFoldersGrid.CommitEdit();

            SelectedFoldersGrid.Items.Refresh();
            System.Windows.Forms.MessageBox.Show(SearchFolders[i].FolderName);
        }

        private List<string> GetRowsToDelete()
        {
            List<string> RowsToDelete = new List<string>();
            foreach(var row in SearchFolders)
            {
                if (row.Select)
                {
                    RowsToDelete.Add(row.FolderName + "||,||" + false.ToString());
                }
            }
            return RowsToDelete;
        }

        

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            List<string> RowsToDelete = GetRowsToDelete();

            if(RowsToDelete.Count > 0)
            {
                DeleteRows(RowsToDelete);
                UpdateFolderList(RowsToDelete);

                SelectedFoldersGrid.Items.Refresh();
            }
        }

        private void UpdateFolderList(List<string> rowsToDelete)
        {
            List<int> Indices = new List<int>();
            foreach(var folder in SearchFolders)
            {
                if (folder.Select == true)
                {
                    Indices.Add(SearchFolders.IndexOf(folder));
                }
            }

            foreach(int index in Indices)
            {
                SearchFolders.RemoveAt(index);
            }
        }

        private void DeleteRows(List<string> RowsToDelete)
        {
            List<string> ConfigFileContents = new List<string>();
            string ConfPath = Path.Combine(Directory.GetCurrentDirectory(),
                "FolderConf.dat");

            FileStream ConfFileStream = new FileStream(ConfPath,
                FileMode.Open,
                FileAccess.Read);
            StreamReader ConfFileReader = new StreamReader(ConfFileStream);
            ConfFileReader.BaseStream.Seek(0, SeekOrigin.Begin);


            string TempConfPath = Path.Combine(Directory.GetCurrentDirectory(),
                "FolderConf.dat.temp");

            FileStream TempFileStream = new FileStream(TempConfPath,
                FileMode.Create,
                FileAccess.Write);
            StreamWriter ConfFileWriter = new StreamWriter(TempFileStream);


            string? ConfLine = ConfFileReader.ReadLine();
            while (ConfLine != null)
            {
                foreach (string row in RowsToDelete)
                {
                    if (!ConfLine.Equals(row))
                    {
                        ConfFileWriter.WriteLine(ConfLine);
                    }
                }
                ConfLine = ConfFileReader.ReadLine();
            }

            ConfFileReader.Close();
            ConfFileWriter.Close();

            File.Delete(ConfPath);
            File.Move(TempConfPath, ConfPath);
        }
    }
}
