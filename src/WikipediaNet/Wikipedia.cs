using RestSharp;
using RestSharp.Serialization.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using WikiDatabase;
using WikipediaNet.Enums;
using WikipediaNet.Misc;
using WikipediaNet.Objects;

namespace WikipediaNet
{
    public class Wikipedia
    {
        private Dictionary<int, string> alreadyInDb = new Dictionary<int, string>();
        private Dictionary<int, string> alreadyInDbNotChanged = new Dictionary<int, string>();
        private static readonly RestClient _client = new RestClient();
        private Format _format;
        private bool copied;
        WikiDatabase.WikiDatabase db = new WikiDatabase.WikiDatabase();

        public Wikipedia(Language language = Language.Russian)
        {
            db.Connect();
            Language = language;
            Format = Format.XML;

            Infos = new List<Info>();
            Namespaces = new List<int>();
            Properties = new List<Property>();
        }

        /// <summary>
        /// Set to true to use HTTPS instead of HTTP.
        /// </summary>
        public bool UseTLS { get; set; }

        /// <summary>
        /// What language to use.
        /// Default: English (en)
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// Gets or sets the format to use.
        /// Note: This currently defaults only to XML - once RestSharp gets DeserializeAs attributes for JSON, I will implement support for JSON as well.
        /// </summary>
        public Format Format
        {
            get => _format;
            private set => _format = value == Format.Default ? Format.XML : value;
        }

        /// <summary>
        /// What metadata to return.
        /// Default: TotalHits, Suggestion
        /// </summary>
        public List<Info> Infos { get; set; }

        /// <summary>
        /// How many total pages to return.
        /// Default: 10
        /// Max: 50
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// The namespace(s) to enumerate.
        /// When the list is empty, it implicitly contains 0, the default namespace to search.
        /// </summary>
        public List<int> Namespaces { get; set; }

        /// <summary>
        /// What propery to include in the results.
        /// Defaults to a combination of snippet, size, word count and timestamp
        /// </summary>
        public List<Property> Properties { get; set; }

        /// <summary>
        /// Gets or sets the place to search.
        /// </summary>
        public What What { get; set; }


        public Dictionary<string, int> GetPreviosLevelByTitle(int level)
        {
            return db.GetPreviousLevelByTitle(level);
        }

        public Dictionary<int, string> GetAllFromDb()
        {
            return db.GetAllArticlesFromDb();
        }

        public Dictionary<int, string> GetMissingArticleFromDb()
        {
            return db.GetMissingArticlesFromDb();
        }

        /// <summary>
        /// Get list of ids and process articles finding if they have predecessors on level N.
        /// </summary>
        /// <param name="levelToCheck">Level to find predecessors on</param>
        /// <param name="allOnLevel">Articles from level N where to find predecessors</param>
        /// <param name="idsToProcess">List of articles ids to process</param>
        public void ProcessArticlesByForwardLinks(int levelToCheck, Dictionary<string, int> allOnLevel, List<int> idsToProcess)
        {
            //https://en.wikipedia.org/w/api.php?action=query&list=backlinks&bltitle=Adolf%20Hitler&bllimit=50
            _client.BaseUrl = new Uri(string.Format(UseTLS ? "https://{0}.wikipedia.org/w/" : "http://{0}.wikipedia.org/w/", Language.GetStringValue()));
            // https://ru.wikipedia.org/w/api.php?action=query&prop=links&pllimit=500&pageids=18

            int deleted = 0;
            int hitlerIndexFound = 0;
            for (int i = 0; i < idsToProcess.Max(); i = i + 1)
            {
                var fiftyIds = idsToProcess.GetRange(i, 1);

                RestRequest request = new RestRequest("api.php", Method.GET);
                //Required
                request.AddParameter("action", "query");
                request.AddParameter("prop", "info|links");
                request.AddParameter("pllimit", "500");
                request.AddParameter("pageids", string.Join("|", fiftyIds));
                request.AddParameter("format", Format.ToString().ToLower());

                //Output
                RestResponse response = (RestResponse)_client.Execute(request);

                XmlAttributeDeserializer deserializer = new XmlAttributeDeserializer();

                //The format that Wikipedia uses
                deserializer.DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
                deserializer.RootElement = "query";
                PageQueryResult results = deserializer.Deserialize<PageQueryResult>(response);

                XmlAttributeDeserializer continueDeserializer = new XmlAttributeDeserializer();
                continueDeserializer.RootElement = "continue";
                Continue continueElement = continueDeserializer.Deserialize<Continue>(response);

                if (continueElement.BlContinue != null)
                {
                    //It could be so, that some articles have more than 500 links, handle it here
                }

                foreach (var page in results.Pages)
                {
                    // if page is redirect, then add it to ToDelete table with Redirect = 1
                    if (page.Redirect != null)
                    {
                        db.AddToDelete(page.PageId, page.Title, 1);
                        deleted++;
                    }
                    else
                    {
                        // Get links and check if there is some article on the level N that page is linked to
                        var exists = page.Links.Where(x => allOnLevel.ContainsKey(x.Title)).ToList();
                        if (exists.Any())
                        {
                            foreach (var e in exists)
                            {
                                // add information about previous level to the table, so it's possible to find links afterwards
                                db.UpdatePreviousLevelData(page.PageId, allOnLevel[e.Title]);
                            }

                            db.UpdateArticle(page.PageId, levelToCheck);
                            hitlerIndexFound++;
                        }
                    }

                }

                Console.WriteLine("Last processed id:" + i + " marked for deletion " + deleted + " found: " + hitlerIndexFound);
            }

        }

        /// <summary>
        /// Get all articles between ID 1 and N where N is about 8.500.000. There could be missing articles, redirects and real arcticles
        /// We're interested in the last group.
        /// </summary>
        /// <param name="allInDb">All existing articles in db, key is ID, string is title.</param>
        public void FindMissingArticles(Dictionary<int, string> allInDb)
        {
            alreadyInDb = new Dictionary<int, string>();
            alreadyInDbNotChanged = new Dictionary<int, string>();
            foreach (var p in allInDb)
            {
                alreadyInDb.Add(p.Key, p.Value);
                alreadyInDbNotChanged.Add(p.Key, p.Value);
            }

            _client.BaseUrl = new Uri(string.Format(UseTLS ? "https://{0}.wikipedia.org/w/" : "http://{0}.wikipedia.org/w/", Language.GetStringValue()));

            List<int> ids = new List<int>();
            for (int i = 0; i < allInDb.Keys.Max(); i++)
            {
                if (allInDb.ContainsKey(i))
                {
                    continue;
                }
                else
                {
                    ids.Add(i);
                }

                if (ids.Count < 50)
                {
                    continue;
                }
                else
                {
                    RestRequest request = new RestRequest("api.php", Method.GET);
                    //Required
                    request.AddParameter("action", "query");
                    request.AddParameter("prop", "info");
                    request.AddParameter("pageids", string.Join("|", ids));
                    request.AddParameter("format", Format.ToString().ToLower());

                    //Output
                    RestResponse response = (RestResponse)_client.Execute(request);

                    XmlAttributeDeserializer backLinksDeserializer = new XmlAttributeDeserializer();

                    //The format that Wikipedia uses
                    backLinksDeserializer.DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
                    backLinksDeserializer.RootElement = "query";
                    PageQueryResult results = backLinksDeserializer.Deserialize<PageQueryResult>(response);
                    foreach (var page in results.Pages)
                    {
                        if (page.Title != null)
                        {
                            // Add missing articles to the special table to process it afterwards
                            db.AddMissingArticle(page.PageId, page.Title, page.Title == null ? 1 : 0);
                        }
                    }

                    ids.Clear();
                    Console.WriteLine($"Last processed id: {i}");
                }
            }

        }

        /// <summary>
        /// Find articles without backlinks => without links to Hitler.
        /// </summary>
        /// <param name="idsToProcess">Ids to process</param>
        public void FindArticlesWithoutBacklinks(List<int> idsToProcess)
        {

            //https://en.wikipedia.org/w/api.php?action=query&list=backlinks&bltitle=Adolf%20Hitler&bllimit=50
            _client.BaseUrl = new Uri(string.Format(UseTLS ? "https://{0}.wikipedia.org/w/" : "http://{0}.wikipedia.org/w/", Language.GetStringValue()));
            // https://ru.wikipedia.org/w/api.php?action=query&prop=links&pllimit=500&pageids=18

            int deleted = 0;
            // Here instead of doing i+1 you can do i+50, but then there is a risk, that some links will not be taken by API
            // In order to be sure, that links are taken, it's needed to do i+1 and then check for BlContinue. 
            // But for quick filtering i+50 will work
            for (int i = 0; i < idsToProcess.Max(); i = i + 1)
            {
                var fiftyIds = idsToProcess.GetRange(i, 1);

                //https://ru.wikipedia.org/w/api.php?action=query&prop=info|linkshere&lhlimit=10&pageids=20029
                RestRequest request = new RestRequest("api.php", Method.GET);
                //Required
                request.AddParameter("action", "query");
                request.AddParameter("prop", "info|links");
                request.AddParameter("pllimit", "500");
                request.AddParameter("pageids", string.Join("|", fiftyIds));
                request.AddParameter("format", Format.ToString().ToLower());

                //Output
                RestResponse response = (RestResponse)_client.Execute(request);

                XmlAttributeDeserializer backLinksDeserializer = new XmlAttributeDeserializer();

                //The format that Wikipedia uses
                backLinksDeserializer.DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
                backLinksDeserializer.RootElement = "query";
                PageQueryResult results = backLinksDeserializer.Deserialize<PageQueryResult>(response);

                XmlAttributeDeserializer continueDeserializer = new XmlAttributeDeserializer();
                continueDeserializer.RootElement = "continue";
                Continue continueElement = continueDeserializer.Deserialize<Continue>(response);

                if (continueElement.BlContinue != null)
                {
                    //You can handle articles with more links here.
                }

                foreach (var page in results.Pages.Where(x => x.Title != null))
                {
                    if (page.Redirect != null)
                    {
                        db.AddToDelete(page.PageId, page.Title, 1);
                        deleted++;
                        continue;
                    }

                    if (page.Links.All(x => x.Title.Contains(":")))
                    {
                        deleted++;
                        db.AddToDelete(page.PageId, page.Title, 0);
                        db.AddArticle(new Article(page.PageId, page.Title, "", 0, 0, 0, -3));
                    }
                }

                Console.WriteLine("Last processed id:" + i + " marked for deletion " + deleted);
            }
        }

        /// <summary>
        /// Find list of articles, which have a link to the entire article
        /// </summary>
        /// <param name="level">Level to add found articles to</param>
        /// <param name="title">Title of the article to find backlinks to</param>
        /// <param name="pageId">page ID of the article to find backlinks to</param>
        /// <param name="allInDb">All articles in db, key - pageID, value - title</param>
        /// <returns></returns>
        public QueryResult FindBacklinks(int level, string title, int pageId, Dictionary<int, string> allInDb)
        {
            if (!copied)
            {
                alreadyInDb = new Dictionary<int, string>();
                alreadyInDbNotChanged = new Dictionary<int, string>();
                foreach (var p in allInDb)
                {
                    alreadyInDb.Add(p.Key, p.Value);
                    alreadyInDbNotChanged.Add(p.Key, p.Value);
                }

                copied = true;
            }

            //https://en.wikipedia.org/w/api.php?action=query&list=backlinks&bltitle=Adolf%20Hitler&bllimit=50
            _client.BaseUrl = new Uri(string.Format(UseTLS ? "https://{0}.wikipedia.org/w/" : "http://{0}.wikipedia.org/w/", Language.GetStringValue()));


            string continueString = "start";
            QueryResult allResults = new QueryResult() { Search = new List<Search>() };

            while (!string.IsNullOrEmpty(continueString))
            {
                RestRequest request = new RestRequest("api.php", Method.GET);
                //Required
                request.AddParameter("action", "query");
                request.AddParameter("list", "backlinks");
                request.AddParameter("bltitle", title);
                request.AddParameter("bllimit", "5000");
                request.AddParameter("prop", "info");

                if (continueString != "start")
                {
                    request.AddParameter("blcontinue", continueString);
                }
                request.AddParameter("format", Format.ToString().ToLower());

                //Output
                RestResponse response = (RestResponse)_client.Execute(request);

                XmlAttributeDeserializer backLinksDeserializer = new XmlAttributeDeserializer();

                //The format that Wikipedia uses
                backLinksDeserializer.DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
                backLinksDeserializer.RootElement = "query";
                QueryResult results = backLinksDeserializer.Deserialize<QueryResult>(response);


                allResults.Search.AddRange(results.Search);
                XmlAttributeDeserializer continueDeserializer = new XmlAttributeDeserializer();
                continueDeserializer.RootElement = "continue";
                Continue continueElement = continueDeserializer.Deserialize<Continue>(response);

                continueString = continueElement.BlContinue;

                //For convinience, we autocreate Uris that point directly to the wiki page.
                if (results.Search != null)
                {
                    foreach (Search search in results.Search)
                    {
                        search.Url = UseTLS
                            ? new Uri("https://" + Language.GetStringValue() + ".wikipedia.org/wiki/" + search.Title)
                            : new Uri("http://" + Language.GetStringValue() + ".wikipedia.org/wiki/" + search.Title);
                    }
                }

            }

            var resultsToSave = allResults.Search;

            foreach (var resultToSave in resultsToSave)
            {
                if (!alreadyInDb.ContainsKey(resultToSave.PageId))
                {
                    Article articleToSave = new Article(resultToSave.PageId, resultToSave.Title, "", 0, 0, 0, level);
                    db.AddArticle(articleToSave);
                    alreadyInDb.Add(resultToSave.PageId, resultToSave.Title);
                }

                if (allInDb != null && !alreadyInDbNotChanged.ContainsKey(resultToSave.PageId))
                    db.UpdatePreviousLevelData(resultToSave.PageId, pageId);
            }

            if (allInDb != null)
            {
                db.AddBackLinksToArticle(resultsToSave.Count, title);
            }

            return allResults;
        }
    }
}