using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzSearch
{
    public class GridRowContents
    {
        public string FolderName { get; set; }
        
        public bool Select { get; set; }

        public GridRowContents(string folderName, bool isSelected)
        {
            FolderName = folderName;
            Select = isSelected;
        }
    }
}
