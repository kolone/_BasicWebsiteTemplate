using BasicWebsiteTemplate.MemeBLL;
using BasicWebsiteTemplate.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BasicWebsiteTemplate.Controllers
{
    public class MemeController : Controller
    {
        //
        // GET: /Meme/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadImage(string imageData)
        {
             try
            {
            MemeBL memeBL = new MemeBL();
            string filename = memeBL.CreateMeme(imageData);
            return RedirectToAction("Show", "Meme", new { filename = filename });
            }
             catch (Exception ex)
             {
                 //todo: log exception
                 return RedirectToAction("Index");
             }


        }

        public ActionResult Show(string filename)
        {
            MemeBL memeBL = new MemeBL();
            if (memeBL.IsMemeNameExists(filename))
            {
                var model = memeBL.GetMeme(filename);
                return View(model);
            }
            else
            {
                //todo: redirect to 404 page
                return Content("404 !!!");
            }
        }

        public ActionResult All()
        {

            MemeBL memeBL = new MemeBL();
            var model = memeBL.GetAllMemes();
            return View(model);
        }

        public ActionResult Download(MemeViewModel meme)
        {
            try
            {
                MemeBL MemeBL = new MemeBL();
                MemeBL.DownloadMeme(meme);

            }
            catch (Exception ex)
            {
                //todo: log exception
            }
            return PartialView("~/Views/Meme/_MemeDisplay.cshtml", meme);
        }

        public ActionResult Templates()
        {

            MemeBL memeBL = new MemeBL();
            var model = memeBL.GetAllMemesTemplates();
            return PartialView("~/Views/Meme/_MemePicker.cshtml", model);

        }


    }
}
