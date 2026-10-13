using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using MyUmbracoProject.Models;
using MyUmbracoProject.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace MyUmbracoProject.Controllers
{
    public class NewsController : RenderController
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IPublishedContentQuery _publishedContentQuery;

        public NewsController(
       ILogger<NewsController> logger,
       ICompositeViewEngine compositeViewEngine,
       IUmbracoContextAccessor umbracoContextAccessor, IPublishedContentQuery publishedContentQuery)
       : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _umbracoContextAccessor= umbracoContextAccessor; 
            _publishedContentQuery= publishedContentQuery; ;
        }
        public override IActionResult Index()
        {
            
            var news = CurrentPage;
            NewsService newsService = new NewsService();
            var model = newsService.GetPublishedNews(news);
     
            return CurrentTemplate(model);
        }
    }
}
