namespace WikiDatabase
{
    public class Article
    {
        public int WikiId { get; }
        public string Title { get; set; }
        public string CategoryName { get; }
        public int Processed { get; }
        public int LinksCount { get; }
        public int BackLinksCount { get; }
        public int HitlerIndex { get; }

        public Article(int wikiId, string title, string category, int processed, int linksCount, int backLinksCount,
            int hitlerIndex)
        {
            this.WikiId = wikiId;
            this.Title = title;
            this.CategoryName = category;
            this.Processed = processed;
            this.LinksCount = linksCount;
            this.BackLinksCount = backLinksCount;
            this.HitlerIndex = hitlerIndex;
        }
    }
}
