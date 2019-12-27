using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp.Deserializers;

namespace WikipediaNet.Objects
{
    /// <summary>
    /// 
    /// </summary>
    public class Page
    {
        [DeserializeAs(Name = "pageid")]
        public int PageId { get; set; }

        [DeserializeAs(Name = "missing")]
        public string Missing { get; set; }

        [DeserializeAs(Name = "title")]
        public string Title { get; set; }

        [DeserializeAs(Name = "links")]
        public List<Link> Links { get; set; }

        [DeserializeAs(Name = "linkshere")]
        public List<Link> LinksHere { get; set; }

        [DeserializeAs(Name = "redirect")]
        public string Redirect { get; set; }
    }
}
