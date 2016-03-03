using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SharePointApp.Web.Models;
using ClientOM = Microsoft.SharePoint.Client;


namespace SharePointApp.Web.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly string _serverPath = ConfigurationManager.AppSettings["ServerPath"];
        private readonly string _username = ConfigurationManager.AppSettings["AccountUsername"];
        private readonly string _password = ConfigurationManager.AppSettings["AccountPassword"];
        private const int PageSize = 10;
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {            
            var model = new FilesViewModel();
            model.CurrentSort = sortOrder;
            model.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            model.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            model.CurrentFilter = searchString;


            model.FileNames = new List<SharepointFile>();
            ClientOM.ClientContext clientContext = new ClientOM.ClientContext(_serverPath);

            var password = new SecureString();
            foreach (char c in _password)
            {
                password.AppendChar(c);
            }
            clientContext.Credentials = new ClientOM.SharePointOnlineCredentials(_username, password);


            ClientOM.List sharedDocumentsList = clientContext.Web.Lists.GetByTitle("Documents");
            ClientOM.CamlQuery camlQuery = new ClientOM.CamlQuery();
            var position = new ClientOM.ListItemCollectionPosition();


            var iSkip = 10;
            position.PagingInfo = string.Format("Paged=TRUE&p_ID={0}", iSkip);
            camlQuery.ListItemCollectionPosition = position;


            camlQuery.ViewXml =
                @"<View Scope='RecursiveAll'>
                <Query>
                  <Where>
                    <And>
                    <Eq>
                      <FieldRef Name='CustomerName'/>
                      <Value Type='Text'>" + searchString + @"</Value>
                    </Eq>
                    <Eq>
                      <FieldRef Name='ContentType' /><Value Type='Text'>Folder</Value>
                    </Eq>
                    </And>
                  </Where>   
                  <OrderBy Override='True'>
                      <FieldRef Name='ID' />
                  </OrderBy>
                </Query>
                <ViewFields>
                    <FieldRef Name='Name' />
                </ViewFields>
                <RowLimit Paged='True'>500</RowLimit>
              </View>";


            ClientOM.ListItemCollection listItems = sharedDocumentsList.GetItems(camlQuery);
            clientContext.Load(sharedDocumentsList);
            clientContext.Load(listItems);
            clientContext.ExecuteQuery();

            if (listItems.Count > 0)
            {

                foreach (var item in listItems)
                {
                    var sharepointFile = new SharepointFile()
                    {
                        Name = item["FileLeafRef"].ToString(),
                        Link = item["FileRef"].ToString()
                    };
                    model.FileNames.Add(sharepointFile);
                }

            }


            int pageNumber = (page ?? 1);
            model.ListOfFiles = model.FileNames.ToPagedList(pageNumber, PageSize);
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, string folderSelection)
        {
            if (file != null && file.ContentLength > 0)
            {
                ClientOM.ClientContext clientContext = new ClientOM.ClientContext(_serverPath);
                var password = new SecureString();
                foreach (char c in _password)
                {
                    password.AppendChar(c);
                }
                clientContext.Credentials = new ClientOM.SharePointOnlineCredentials(_username, password);


                //using (FileStream fileStream = new FileStream("NewDocument.docx", FileMode.Open))
                //{
                //    ClientOM.File.SaveBinaryDirect(clientContext, "/Shared Documents/NewDocument.docx", fileStream, true);
                //}

                MemoryStream target = new MemoryStream();
                file.InputStream.CopyTo(target);
                byte[] byteArray = target.ToArray();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, (int)byteArray.Length);

                    string fileUrl = string.Format("{0}/{1}",folderSelection,Path.GetFileName(file.FileName));
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    ClientOM.File.SaveBinaryDirect(clientContext, fileUrl, memoryStream, true);
                }

            }
            // redirect back to the index action to show the form once again
            return RedirectToAction("Index");
        }
        
        public ViewResult About(string sortOrder, string currentFilter, string searchString, int? page)
        {            
            var model = new FilesViewModel();
            model.CurrentSort = sortOrder;
            model.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            model.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            model.CurrentFilter = searchString;

            
            model.FileNames = new List<SharepointFile>();
            ClientOM.ClientContext clientContext = new ClientOM.ClientContext(_serverPath);

            var password = new SecureString();
            foreach (char c in _password)
            {
                password.AppendChar(c);
            }
            clientContext.Credentials = new ClientOM.SharePointOnlineCredentials(_username, password);


            ClientOM.List sharedDocumentsList = clientContext.Web.Lists.GetByTitle("Documents");
            ClientOM.CamlQuery camlQuery = new ClientOM.CamlQuery();
            var position = new ClientOM.ListItemCollectionPosition();


            var iSkip = 10;
            position.PagingInfo = string.Format("Paged=TRUE&p_ID={0}", iSkip);
            camlQuery.ListItemCollectionPosition = position;


            camlQuery.ViewXml =
                @"<View Scope='RecursiveAll'>
                <Query>
                  <Where>
                    <Eq>
                      <FieldRef Name='CustomerName'/>
                      <Value Type='Text'>" + searchString + @"</Value>
                    </Eq>
                  </Where>   
                  <OrderBy Override='True'>
                      <FieldRef Name='ID' />
                  </OrderBy>
                </Query>
                <ViewFields>
                    <FieldRef Name='Name' />
                </ViewFields>
                <RowLimit Paged='True'>500</RowLimit>
              </View>";


            ClientOM.ListItemCollection listItems = sharedDocumentsList.GetItems(camlQuery);
            clientContext.Load(sharedDocumentsList);
            clientContext.Load(listItems);
            clientContext.ExecuteQuery();

            if (listItems.Count > 0)
            {

                foreach (var item in listItems)
                {
                    var sharepointFile = new SharepointFile()
                    {
                        Name = item["FileLeafRef"].ToString(),
                        Link = string.Format("{0}{1}", _serverPath, item["FileRef"])
                    };
                    model.FileNames.Add(sharepointFile);
                }
                
            }
           
            
            int pageNumber = (page ?? 1);
            model.ListOfFiles = model.FileNames.ToPagedList(pageNumber, PageSize);
            return View(model);
        }
    }
}