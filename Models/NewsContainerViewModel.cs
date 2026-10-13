namespace MyUmbracoProject.Models
{
    public class NewsContainerViewModel
    {
        public string Title { get; set; }

        public string Url { get; set; }

        public IEnumerable<NewsItemViewModel> NewsItems { get; set; }
    
    }
}
