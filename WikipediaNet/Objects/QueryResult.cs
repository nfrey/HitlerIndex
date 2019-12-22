using System.Collections.Generic;
using RestSharp.Deserializers;

namespace WikipediaNet.Objects
{
    public class Continue
    {
        [DeserializeAs(Name = "blcontinue")]
        public string BlContinue { get; set; }
    }

    public class QueryResult
    {
        [DeserializeAs(Name = "searchinfo")]
        public SearchInfo SearchInfo { get; set; }

        [DeserializeAs(Name = "backlinks")]
        public List<Search> Search { get; set; }

        [DeserializeAs(Name = "servedby")]
        public string ServedBy { get; set; }

        [DeserializeAs(Name = "error")]
        public Error Error { get; set; }

    }
}