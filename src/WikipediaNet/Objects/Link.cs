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
    public class Link
    {

        [DeserializeAs(Name = "title")]
        public string Title { get; set; }
    }
}