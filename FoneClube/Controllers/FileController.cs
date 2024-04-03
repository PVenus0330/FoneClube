using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FoneClube.WebAPI.Controllers
{
    public class FileController : Controller
    {
        // GET: File
        public ActionResult Index(string fileName)
        {
            string filePath = $"/temp/pdf/" + fileName;
            var content = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var response = File(content, "application/pdf");

            return response;
        }
    }
}