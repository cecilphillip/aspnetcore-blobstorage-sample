using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SimpleWebGallery.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleWebGallery.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlobStorageService _storageService;

        public HomeController(BlobStorageService storageService)
        {
            this._storageService = storageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<string> urls = await _storageService.RetrieveImageBlobUrls();
            return View(urls);
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile fileUpload)
        {
            if (fileUpload?.Length > 0 && _storageService.IsImage(fileUpload))
            {
                await _storageService.UploadImageToBlobStorage(fileUpload);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
