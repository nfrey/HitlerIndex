using System;
using System.Collections.Generic;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serialization.Json;
using RestSharp.Serialization.Xml;
using WikipediaNet.Enums;
using WikipediaNet.Misc;
using WikipediaNet.Objects;

namespace WikipediaNet
{
    public class Wikipedia
    {
        private static readonly RestClient _client = new RestClient();
        private Format _format;

        public Wikipedia(Language language = Language.Russian)
        {
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
        /// Use this value to continue paging (return by query).
        /// Default: 0
        /// </summary>
        public int Offset { get; set; }

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
        /// Include redirect pages in the search.
        /// </summary>
        public bool Redirects { get; set; }

        /// <summary>
        /// Gets or sets the place to search.
        /// </summary>
        public What What { get; set; }

        /// <summary>
        /// Include the hostname that served the request in the results. Unconditionally shown on error.
        /// </summary>
        public bool ServedBy { get; set; }

        /// <summary>
        /// Request ID to distinguish requests. This will just be output back to you.
        /// </summary>
        public string RequestID { get; set; }

        public QueryResult GetBacklinks(string title = "Гитлер, Адольф")
        {
            //https://en.wikipedia.org/w/api.php?action=query&list=backlinks&bltitle=Adolf%20Hitler&bllimit=50
            _client.BaseUrl = new Uri(string.Format(UseTLS ? "https://{0}.wikipedia.org/w/" : "http://{0}.wikipedia.org/w/", Language.GetStringValue()));


            string continueString = "start";
            QueryResult allResults = new QueryResult() {Search = new List<Search>()};

            while (!string.IsNullOrEmpty(continueString))
            {
                RestRequest request = new RestRequest("api.php", Method.GET);
                //Required
                request.AddParameter("action", "query");
                request.AddParameter("list", "backlinks");
                request.AddParameter("bltitle", title);
                request.AddParameter("bllimit", "5000");
                if (continueString != "start")
                {
                    request.AddParameter("blcontinue", continueString);
                }
                request.AddParameter("format", Format.ToString().ToLower());

                //Output
                RestResponse response = (RestResponse) _client.Execute(request);

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

            return allResults;
        }
    }
}