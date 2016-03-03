using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.SharePoint.Client;
using PagedList;

namespace SharePointApp.Web.Models
{
    public class FilesViewModel
    {
        public List<SharepointFile> FileNames { get; set; }
        public IPagedList<SharepointFile> ListOfFiles { get; set; }
        public string FolderSelection { get; set; }
        public string CurrentSort { get; set; }
        public string NameSortParm { get; set; }
        public string DateSortParm { get; set; }
        public string CurrentFilter { get; set; }        
    }

    public class SharepointFile
    {
        public string Name { get; set; }
        public string Link { get; set; }
    }
}