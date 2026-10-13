using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using MyUmbracoProject.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace MyUmbracoProject.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly IContentService _contentService;
        public ContactUsController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, IContentService contentService) 
        {
            _contentService = contentService;
        }
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("ContactUs");
        }
        [HttpPost("Submit")]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(ContactFormViewModel model)
        {
            if (!ModelState.IsValid) { return View("ContactUs", model); }
            SaveToUmbraco(model);
            return RedirectToAction("Success"); 
        }

   private void SaveToUmbraco(ContactFormViewModel model)
        {
            var parent = _contentService
       .GetRootContent()
       .FirstOrDefault(x => x.ContentType.Alias == "contactUsRequests");

            if (parent == null)
            {
                throw new Exception("ContactUsSubmissions parent was not found.");
            }

            var content = _contentService.Create(
                $"{model.FirstName} - {model.Subject}",
                parent.Id,
                "contactUs"
            );

            content.SetValue("firstName", model.FirstName);
            content.SetValue("email", model.Email);
            content.SetValue("subject", model.Subject);
            content.SetValue("message", model.Message);

            var result = _contentService.Save(content);
            
        }

        [HttpGet("Success")]
        public IActionResult Success()
        {
            return View("Success");
        }
    }
}
