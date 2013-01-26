using BasicWebsiteTemplate.MemeBLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace BasicWebsiteTemplate.ViewModels
{
    public enum MemeFileType
    {
        Template,
        Created
    }

    public abstract class MemeFileDetails
    {
        public MemeFileType MemeFileType { get; set; }
        public string FileName { get; set; }

        public string FileVirtualPath
        {
            get
            {
                if (MemeFileType == ViewModels.MemeFileType.Template)
                {
                    string path = MemeBL.GetTemplatesVirtualPath();
                    return path + FileName;
                }
                else
                {
                    string path = MemeBL.GetStoreVirtualPath();
                    return path + FileName;
                }
            }
        }

        public string FileAbsolutePath
        {
            get
            {
                if (MemeFileType == ViewModels.MemeFileType.Template)
                {
                    string path = MemeBL.GetTemplatesAbsolutePath();
                    return path + FileName;
                }
                else
                {
                    string path = MemeBL.GetStoreAbsolutePath();
                    return path + FileName;
                }
            }
        }



        public string FileNameWithoutExtension
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FileName);
            }
        }

    }

    public class MemeViewModel : MemeFileDetails
    {

        public string ShareUrl { get; set; }
        public string FileUrl { get; set; }
    }


    public class MemeTemplateViewModel : MemeFileDetails
    {

    }
}