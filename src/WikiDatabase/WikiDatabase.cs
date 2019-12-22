using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace WikiDatabase
{
    public class WikiDatabase
    {
        private SqlConnection sqlConnection;
        public void Connect()
        {
            sqlConnection = new SqlConnection
            {
                ConnectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=HitlerIndex;Integrated Security=SSPI;"
            };
            sqlConnection.Open();
        }

        public void AddArticle(Article article)
        {
            if (article.Title.Contains("'"))
            {
                article.Title = article.Title.Replace("'", "");
            }
            SqlDataAdapter adapter = new SqlDataAdapter();
            string sql = $"insert into dbo.Articles (WikiID, Title, CategoryName, Processed, BackLinksCount, LinksCount, HitlerIndex) " +
                $"values ({article.WikiId}, N'{article.Title}', '{article.CategoryName}', 0, {article.BackLinksCount}, {article.LinksCount}, {article.HitlerIndex})";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            adapter.InsertCommand = command;
            adapter.InsertCommand.ExecuteNonQuery();
        }

        public void AddBackLinksToArticle(int backLinks, string title)
        {
            if (title.Contains("'"))
            {
                title = title.Replace("'", "");
            }

            SqlDataAdapter adapter = new SqlDataAdapter();
            string sql = $"update dbo.Articles set BackLinksCount = {backLinks} where Title = N'{title}'";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            adapter.UpdateCommand = command;
            adapter.UpdateCommand.ExecuteNonQuery();
        }

        public void UpdatePreviousLevelData(int wikiID, int previousLevelWikiID)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            string sql = $"insert into dbo.PreviousLevelPages (WikiID, PreviousLevelWikiID) values ({wikiID}, {previousLevelWikiID})";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            adapter.InsertCommand = command;
            adapter.InsertCommand.ExecuteNonQuery();
        }

        public Dictionary<int, bool> GetArticlesFromDb()
        {
            Dictionary<int, bool> result = new Dictionary<int, bool>();
            string sql = $"select * from dbo.Articles where HitlerIndex = 2 order by WikiID";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int val = (int) reader.GetValue(0);
                if (!result.ContainsKey(val))
                {
                    result.Add(val, true);
                }

            }

            return result;
        }
    }
}
