using MyUmbracoProject.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MyUmbracoProject.Services
{
    public class NewsService
    {
        public IEnumerable<NewsContainerViewModel> GetPublishedNews(
     IPublishedContent newsRoot)
        {
            var now = DateTime.Now;

            return newsRoot
                .Children()
                
                .Select(container => new NewsContainerViewModel
                {
                    Title = container.Name,
                    Url = container.Url(),

                    NewsItems = container
                        .Children()
                        .Where(news =>
                        {
                            var publishDate =
                                news.Value<DateTime?>("publishDate");

                            var expiryDate =
                                news.Value<DateTime?>("expiryDate");

                            return publishDate.HasValue &&
                                   publishDate.Value <= now &&
                                   (!expiryDate.HasValue ||
                                    expiryDate.Value >= now);
                        })
                        .Select(news => new NewsItemViewModel
                        {
                            Title = news.Value<string>("title"),
                            Brief = news.Value<string>("brief"),
                            ImageUrl =
                                news.Value<IPublishedContent>("image")?.Url(),
                            PublishDate =
                                news.Value<DateTime?>("publishDate"),
                            Url = news.Url()
                        })
                        .ToList()
                })
                .ToList();
        }
    }

}
