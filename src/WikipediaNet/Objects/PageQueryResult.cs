using System.Collections.Generic;
using RestSharp.Deserializers;

namespace WikipediaNet.Objects
{
    public class PageQueryResult
    {
        [DeserializeAs(Name = "pages")]
        public List<Page> Pages { get; set; }

        [DeserializeAs(Name = "error")]
        public Error Error { get; set; }

    }
}