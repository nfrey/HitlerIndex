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

            QueryResult results = wikipedia.GetBacklinks("Гитлер, Адольф", 0, null);

            QueryResult secondLevelResults = new QueryResult() {Search = new List<Search>()};
            int i = 1;
            foreach (var result in results.Search)
            {
                Console.WriteLine($"Checking {i} out of 2730 pages. Title: {result.Title}");
                var backlinks = wikipedia.GetBacklinks(result.Title, result.PageId, results);
                secondLevelResults.Search.AddRange(backlinks.Search);
                Console.WriteLine($"Backlinks from the page: {backlinks.Search.Count}");
                Console.WriteLine($"Total links after search: {secondLevelResults.Search.Count}");
                i++;
            }

            var a = File.OpenWrite("C:\\Private\\GitlerScript\\HitlerScript.txt");
            StreamWriter writer = new StreamWriter(a);

            List<string> distinctList = secondLevelResults.Search.Select(x => x.Title).Distinct().ToList();
            List<string> withoutFirstLevel = distinctList.Where(x => results.Search.All(y => y.Title != x)).ToList();

            var c = withoutFirstLevel.Count;
            foreach (Search s in results.Search)
            {
                writer.WriteLine($"{s.Title}  {s.Url}");
                //Console.WriteLine(s.Title);
            }

            writer.Close();
            Console.WriteLine();
            Console.WriteLine();

        }
    }
}
