using BasicWebsiteTemplate.ViewModels;
using Common.MemeBLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace BasicWebsiteTemplate.MemeBLL
{
    public class MemeBL
    {
        #region Utils

        private string StoreImage(string imageData)
        {

            string path = GetStoreAbsolutePath();
            string extension = Constants.IMAGE_EXTENSION;
            CreatePathIfNotExist(path);

            imageData = imageData.Replace("data:image/jpeg;base64,", "");

            //filename by with datetime
            //string filename = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "- ").Replace(":", "");

            //filename by guid
            //string filename = Guid.NewGuid().ToString();

            //filename by random string
            string filename = GenerateMemeUniqueName();// Guid.NewGuid().ToString();


            //full path and filename with extension to store
            string fileNameWitPath = path + filename + extension;
            using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
            {

                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    byte[] data = Convert.FromBase64String(imageData);
                    bw.Write(data);
                    bw.Close();
                }
            }
            //returning just the filename without path or extension
            return filename;
        }

        private int GetRandomNumber()
        {
            int maxNumber = 53;
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            int seed = (b[0] & 0x7f) << 24 | b[1] << 16 | b[2] << 8 | b[3];
            System.Random r = new System.Random(seed);
            return r.Next(1, maxNumber);
        }

        private string GetRandomString()
        {
            int length = 4;
            string[] array = new string[54]
	            {
		            "0","2","3","4","5","6","8","9",
		            "a","b","c","d","e","f","g","h","j","k","m","n","p","q","r","s","t","u","v","w","x","y","z",
		            "A","B","C","D","E","F","G","H","J","K","L","M","N","P","R","S","T","U","V","W","X","Y","Z"
	            };
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < length; i++) sb.Append(array[GetRandomNumber()]);
            return sb.ToString();
        }

        #endregion

        #region Get Paths And Folders

        public static string GetStoreAbsolutePath()
        {
            string rootPath = System.Configuration.ConfigurationManager.AppSettings[Constants.KEY_CONFIG_STORE_IMAGES_ABSOLUTE_PATH];
            string date = DateTime.Now.ToShortDateString().Replace("/", "-").Replace(" ", "- ").Replace(":", "");
            return rootPath;// +date + "\\";
        }
        public static string GetStoreVirtualPath()
        {
            string virtualPath = System.Configuration.ConfigurationManager.AppSettings[Constants.KEY_CONFIG_STORE_IMAGES_VIRTUAL_PATH];
            return virtualPath;
        }
        public static string GetTemplatesAbsolutePath()
        {
            string rootPath = System.Configuration.ConfigurationManager.AppSettings[Constants.KEY_CONFIG_TEMPLATE_IMAGES_ABSOLUTE_PATH];
            return rootPath;
        }
        public static string GetTemplatesVirtualPath()
        {
            string virtualPath = System.Configuration.ConfigurationManager.AppSettings[Constants.KEY_CONFIG_TEMPLATE_IMAGES_VIRTUAL_PATH];
            return virtualPath;
        }
        public void CreatePathIfNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        #endregion

        #region Meme Create

        public List<string> GetAllMemesCreatedFilesCached()
        {
            List<string> filenames = null;

            if (!CacheHelper.Get(Constants.KEY_CACHE_CREATED_MEMES_FILES, out filenames))
            {

                filenames = GetAllMemesCreatedFiles();
                CacheHelper.Add(filenames, Constants.KEY_CACHE_CREATED_MEMES_FILES);
            }

            return filenames;
        }

        private List<string> GetAllMemesCreatedFiles()
        {
            List<string> filenames = null;
            try
            {
                string path = GetStoreAbsolutePath();

                filenames = Directory.EnumerateFiles(path, "*" + Constants.IMAGE_EXTENSION + "*", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension).ToList();

            }
            catch (Exception ex)
            {

            }
            return filenames;
        }

        public string CreateMeme(string imageData)
        {
            string filename  = StoreImage(imageData);
            CacheHelper.Clear(Constants.KEY_CACHE_CREATED_MEMES_FILES);
            return filename;
        }
        public int CountMemes()
        {
            string path = GetStoreAbsolutePath();
            int count = 0;
            try
            {
                var files = GetAllMemesCreatedFilesCached();
                count = files.Count();
            }
            catch (Exception ex)
            {

            }

            return count;

        }

        public bool IsMemeNameExists(string name)
        {
            bool isExists = true;

            try
            {
                isExists = File.Exists(GetStoreAbsolutePath() + name + Constants.IMAGE_EXTENSION);
            }
            catch (Exception ex)
            {

            }
            return isExists;

        }

        public string GenerateMemeUniqueName()
        {
            int i = CountMemes();

            string name = GetRandomString();
            while (IsMemeNameExists(name))
            {
                name = GetRandomString();
            }
            return name;

        }

        public MemeViewModel GetMeme(string filename)
        {
            HttpContextWrapper context = new HttpContextWrapper(System.Web.HttpContext.Current);
            var request = context.Request;
            var url = request.Url.ToString();
            var uri = new Uri(HttpContext.Current.Request.Url.AbsoluteUri).GetLeftPart(UriPartial.Authority);

            if (!url.Contains(filename))
            {
                url = url.Replace("All", "Show") + "/" + filename;
            }

            MemeViewModel meme = new MemeViewModel
            {
                MemeFileType = ViewModels.MemeFileType.Created,
                FileName = filename + Constants.IMAGE_EXTENSION,
                ShareUrl = url,
                FileUrl = uri + MemeBL.GetStoreVirtualPath() + filename + Constants.IMAGE_EXTENSION
            };
            return meme;
        }

        public List<MemeViewModel> GetAllMemes()
        {
            List<MemeViewModel> lstMemes = new List<MemeViewModel>();

            var memeFiles = GetAllMemesCreatedFilesCached();
            for (int i = 0; i < memeFiles.Count(); i++)
            {
                var meme = GetMeme(memeFiles[i]);
                lstMemes.Add(meme);
            }

            return lstMemes;

        }

        public void DownloadMeme(MemeViewModel meme)
        {
            //Create a stream for the file
            Stream stream = null;

            //This controls how many bytes to read at a time and send to the client
            int bytesToRead = 10000;

            // Buffer to read bytes in chunk size specified above
            byte[] buffer = new Byte[bytesToRead];

            // The number of bytes read
            try
            {
                //Create a WebRequest to get the file
                HttpWebRequest fileReq = (HttpWebRequest)HttpWebRequest.Create(meme.FileUrl);

                //Create a response for this request
                HttpWebResponse fileResp = (HttpWebResponse)fileReq.GetResponse();

                if (fileReq.ContentLength > 0)
                    fileResp.ContentLength = fileReq.ContentLength;

                //Get the Stream returned from the response
                stream = fileResp.GetResponseStream();

                // prepare the response to the client. resp is the client Response
                var resp = HttpContext.Current.Response;

                //Indicate the type of data being sent
                resp.ContentType = "application/octet-stream";

                //Name the file 
                resp.AddHeader("Content-Disposition", "attachment; filename=\"" + meme.FileName + "\"");
                resp.AddHeader("Content-Length", fileResp.ContentLength.ToString());

                int length;
                do
                {
                    // Verify that the client is connected.
                    if (resp.IsClientConnected)
                    {
                        // Read data into the buffer.
                        length = stream.Read(buffer, 0, bytesToRead);

                        // and write it out to the response's output stream
                        resp.OutputStream.Write(buffer, 0, length);

                        // Flush the data
                        resp.Flush();

                        //Clear the buffer
                        buffer = new Byte[bytesToRead];
                    }
                    else
                    {
                        // cancel the download if client has disconnected
                        length = -1;
                    }
                } while (length > 0); //Repeat until no data is read
            }
            finally
            {
                if (stream != null)
                {
                    //Close the input stream
                    stream.Close();
                }
            }
        }

        #endregion

        #region Meme Templates

        public List<string> GetAllMemesTemplateFilesCached()
        {
            List<string> filenames = null;

            if (!CacheHelper.Get(Constants.KEY_CACHE_TEMPLATES_MEMES_FILES, out filenames))
            {

                filenames = GetAllMemesTemplateFiles();
                CacheHelper.Add(filenames, Constants.KEY_CACHE_TEMPLATES_MEMES_FILES);
            }
           
            return filenames;
        }

        private List<string> GetAllMemesTemplateFiles()
        {
            List<string> filenames = null;
            try
            {
                string path = GetTemplatesAbsolutePath();

                filenames = Directory.EnumerateFiles(path, "*" + Constants.IMAGE_EXTENSION + "*", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension).ToList();

            }
            catch (Exception ex)
            {

            }
            return filenames;
        }

        public MemeTemplateViewModel GetMemeTemplate(string filename)
        {
            MemeTemplateViewModel memeTemplate = new MemeTemplateViewModel
            {
                MemeFileType = ViewModels.MemeFileType.Template,
                FileName = filename + Constants.IMAGE_EXTENSION
            };
            return memeTemplate;
        }

        public List<MemeTemplateViewModel> GetAllMemesTemplates()
        {

            List<MemeTemplateViewModel> lstMemes = new List<MemeTemplateViewModel>();

            var memeTemplateFiles = GetAllMemesTemplateFilesCached();
            for (int i = 0; i < memeTemplateFiles.Count(); i++)
            {
                var meme = GetMemeTemplate(memeTemplateFiles[i]);
                lstMemes.Add(meme);
            }

            return lstMemes;

        }


        #endregion
    }
}