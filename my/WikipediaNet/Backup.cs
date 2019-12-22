﻿//using System;
//using System.Collections.Generic;
//using RestSharp;
//using RestSharp.Deserializers;
//using RestSharp.Serialization.Json;
//using RestSharp.Serialization.Xml;
//using WikipediaNet.Enums;
//using WikipediaNet.Misc;
//using WikipediaNet.Objects;

//namespace WikipediaNet
//{
//    public class Wikipedia
//    {
//        private static readonly RestClient _client = new RestClient();
//        private Format _format;

//        public Wikipedia(Language language = Language.Russian)
//        {
//            Language = language;
//            Format = Format.XML;

//            Infos = new List<Info>();
//            Namespaces = new List<int>();
//            Properties = new List<Property>();
//        }

//        /// <summary>
//        /// Set to true to use HTTPS instead of HTTP.
//        /// </summary>
//        public bool UseTLS { get; set; }

//        /// <summary>
//        /// What language to use.
//        /// Default: English (en)
//        /// </summary>
//        public Language Language { get; set; }

//        /// <summary>
//        /// Gets or sets the format to use.
//        /// Note: This currently defaults only to XML - once RestSharp gets DeserializeAs attributes for JSON, I will implement support for JSON as well.
//        /// </summary>
//        public Format Format
//        {
//            get => _format;
//            private set => _format = value == Format.Default ? Format.XML : value;
//        }

//        /// <summary>
//        /// What metadata to return.
//        /// Default: TotalHits, Suggestion
//        /// </summary>
//        public List<Info> Infos { get; set; }

//        /// <summary>
//        /// How many total pages to return.
//        /// Default: 10
//        /// Max: 50
//        /// </summary>
//        public int Limit { get; set; }

//        /// <summary>
//        /// Use this value to continue paging (return by query).
//        /// Default: 0
//        /// </summary>
//        public int Offset { get; set; }

//        /// <summary>
//        /// The namespace(s) to enumerate.
//        /// When the list is empty, it implicitly contains 0, the default namespace to search.
//        /// </summary>
//        public List<int> Namespaces { get; set; }

//        /// <summary>
//        /// What propery to include in the results.
//        /// Defaults to a combination of snippet, size, word count and timestamp
//        /// </summary>
//        public List<Property> Properties { get; set; }

//        /// <summary>
//        /// Include redirect pages in the search.
//        /// </summary>
//        public bool Redirects { get; set; }

//        /// <summary>
//        /// Gets or sets the place to search.
//        /// </summary>
//        public What What { get; set; }

//        /// <summary>
//        /// Include the hostname that served the request in the results. Unconditionally shown on error.
//        /// </summary>
//        public bool ServedBy { get; set; }

//        /// <summary>
//        /// Request ID to distinguish requests. This will just be output back to you.
//        /// </summary>
//        public string RequestID { get; set; }

//        public QueryResult Search(string query)
//        {
//            //API example: http://en.wikipedia.org/w/api.php?action=query&list=search&srsearch=wikipedia&srprop=timestamp
//            _client.BaseUrl = new Uri(string.Format(UseTLS ? "https://{0}.wikipedia.org/w/" : "http://{0}.wikipedia.org/w/", Language.GetStringValue()));

//            RestRequest request = new RestRequest("api.php", Method.GET);
//            string continueString = "start";
//            QueryResult allResults = new QueryResult() { Search = new List<Search>() };

//            while (!string.IsNullOrEmpty(continueString))
//            {
//                //Required
//                //request.AddParameter("action", "query");
//                //request.AddParameter("list", "search");
//                //request.AddParameter("srsearch", query);
//                //request.AddParameter("format", Format.ToString().ToLower());

//                //https://en.wikipedia.org/w/api.php?action=query&list=backlinks&bltitle=Adolf%20Hitler&bllimit=50


//                //Required
//                request.AddParameter("action", "query");
//                request.AddParameter("list", "backlinks");
//                request.AddParameter("bltitle", "Гитлер, Адольф");
//                request.AddParameter("bllimit", "5000");
//                if (continueString != "start")
//                {
//                    request.AddParameter("blcontinue", continueString);
//                }


//                request.AddParameter("format", Format.ToString().ToLower());

//                //Optional
//                //if (Infos.HasElements())
//                //    request.AddParameter("srinfo", string.Join("|", Infos).ToLower());

//                //if (Limit != 0)
//                //    request.AddParameter("srlimit", Limit);

//                //if (Offset != 0)
//                //    request.AddParameter("sroffset", Offset);

//                //if (Namespaces.HasElements())
//                //    request.AddParameter("srnamespace", string.Join("|", Namespaces).ToLower());

//                //if (Properties.HasElements())
//                //    request.AddParameter("srprop", string.Join("|", Properties).ToLower());

//                //if (Redirects)
//                //    request.AddParameter("srredirects", Redirects.ToString().ToLower());

//                //if (What != What.Title)
//                //    request.AddParameter("srwhat", What.ToString().ToLower());

//                //if (ServedBy)
//                //    request.AddParameter("servedby", ServedBy.ToString().ToLower());

//                //if (!string.IsNullOrEmpty(RequestID))
//                //    request.AddParameter("requestid", RequestID);

//                //Output
//                RestResponse response = (RestResponse)_client.Execute(request);

//                IDeserializer deserializer;

//                //switch (Format)
//                //{
//                //    case Format.JSON:
//                //        JsonDeserializer jsonDeserializer = new JsonDeserializer();

//                //        //The format that Wikipedia uses
//                //        jsonDeserializer.DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
//                //        jsonDeserializer.RootElement = "query";

//                //        deserializer = jsonDeserializer;
//                //        break;
//                //    default:
//                XmlAttributeDeserializer xmlDeserializer = new XmlAttributeDeserializer();

//                //The format that Wikipedia uses
//                xmlDeserializer.DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
//                xmlDeserializer.RootElement = "query";

//                deserializer = xmlDeserializer;
//                //        break;
//                //}

//                //The format that Wikipedia uses
//                //deserializer.DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";

//                //deserializer.RootElement = "query";

//                QueryResult results = deserializer.Deserialize<QueryResult>(response);

//                allResults.Search.AddRange(results.Search);
//                XmlAttributeDeserializer continueDeserializer = new XmlAttributeDeserializer();

//                //The format that Wikipedia uses
//                xmlDeserializer.DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
//                xmlDeserializer.RootElement = "continue";

//                Continue a = continueDeserializer.Deserialize<Continue>(response);

//                continueString = a.BlContinue;

//                //For convinience, we autocreate Uris that point directly to the wiki page.
//                if (results.Search != null)
//                {
//                    foreach (Search search in results.Search)
//                    {
//                        search.Url = UseTLS
//                            ? new Uri("https://" + Language.GetStringValue() + ".wikipedia.org/wiki/" + search.Title)
//                            : new Uri("http://" + Language.GetStringValue() + ".wikipedia.org/wiki/" + search.Title);
//                    }
//                }

//            }


//            return allResults;
//        }
//    }
//}