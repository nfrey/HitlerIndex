using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using WikipediaNet.Enums;
using WikipediaNet.Objects;

namespace WikipediaNet.Examples
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            //Default language is English
            Wikipedia wikipedia = new Wikipedia();

            //Use HTTPS instead of HTTP
            wikipedia.UseTLS = true;

            //We would like 5 results
            wikipedia.Limit = 5;

            //We would like to search inside the articles
            wikipedia.What = What.Text;

            // var previous = wikipedia.GetPreviousLevel();
            //var allDb = wikipedia.GetAllFromDb();

            var getPrevLevel = wikipedia.GetPreviosLevelByTitle(4);
            //var getLevel = wikipedia.GetPreviousLevel(4);
            var a = wikipedia.GetMissingArticleFromDb();

            //wikipedia.FixLevel4(getPrevLevel, getLevel.Keys.ToList());
            wikipedia.FindArticlesWithoutBacklinks(a.Keys.ToList());

            // Level to check has to be +1 to one used in GetPreviousLevelByTitle
            wikipedia.ProcessArticlesByForwardLinks(5, getPrevLevel, a.Keys.ToList());
            
            //wikipedia.FindMissingArticles(allDb);
           
            Console.WriteLine();
        }
    }
}
