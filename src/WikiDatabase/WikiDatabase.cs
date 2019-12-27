using System.Collections.Generic;
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

        public void AddToDelete(int wikiID, string title, int redirect)
        {
            if (title.Contains("'"))
            {
                title = title.Replace("'", "");
            }
            SqlDataAdapter adapter = new SqlDataAdapter();
            string sql = $"insert into dbo.ToDelete (WikiID, Title, Redirect) values ({wikiID}, N'{title}', {redirect})";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            adapter.InsertCommand = command;
            adapter.InsertCommand.ExecuteNonQuery();
        }

        public void RemoveLinksToPreviousPages(int pageId)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            string sql = $"delete from  PreviousLevelPages where WikiID = {pageId}";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            adapter.DeleteCommand = command;
            adapter.DeleteCommand.ExecuteNonQuery();
        }

        public void UpdateArticle(int pageId, int hitlerLevel)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            string sql = $"update Articles set HitlerIndex = {hitlerLevel} where WikiID = {pageId}";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            adapter.UpdateCommand = command;
            adapter.UpdateCommand.ExecuteNonQuery();
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

        public Dictionary<string, int> GetPreviousLevelByTitle(int level)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            string sql = $"select * from dbo.Articles where HitlerIndex = {level} and Title not like N'%:%' order by WikiID";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int val = (int)reader.GetValue(0);
                string title = (string)reader.GetValue(1);
                if (!result.ContainsKey(title))
                {
                    result.Add(title, val);
                }

            }

            reader.Close();
            return result;
        }

        public Dictionary<int, string> GetArticlesFromDb(int level)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            string sql = $"select * from dbo.Articles where HitlerIndex = {level} order by WikiID";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int val = (int) reader.GetValue(0);
                string title = (string) reader.GetValue(1);
                if (!result.ContainsKey(val))
                {
                    result.Add(val, title);
                }

            }

            reader.Close();
            return result;
        }

        public Dictionary<int, string> GetAllArticlesFromDb()
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            string sql = $"select * from dbo.Articles order by WikiID";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int val = (int)reader.GetValue(0);
                string title = (string)reader.GetValue(1);
                if (!result.ContainsKey(val))
                {
                    result.Add(val, title);
                }

            }

            reader.Close();
            return result;
        }

        public Dictionary<int, string> GetMissingArticlesFromDb()
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            //I've used that method for everything, sorry :) 

            //string sql = $"select * from MissingArticles where WikiID > 6022024 and Title NOT like N'%:%' order by WikiID"; 
            //string sql = "select * from MissingArticles where not exists (select * from ToDelete where toDelete.WikiID = MissingArticles.WikiID) and Title not like N'%:%' order by WikiID";
            //string sql = "select * from Articles where not exists (select * from PreviousLevelPages where PreviousLevelPages.WikiID = Articles.WikiID) and HitlerIndex =2";
            string sql = "select * from Articles where HitlerIndex = -2 or HitlerIndex = -1";
            SqlCommand command = new SqlCommand(sql, sqlConnection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int val = (int)reader.GetValue(0);
                string title = (string)reader.GetValue(2);
                if (!result.ContainsKey(val))
                {
                    result.Add(val, title);
                }

            }

            reader.Close();
            return result;
        }


        public void AddMissingArticle(int id, string title, int missing)
        {
            if (title != null && title.Contains("'"))
            {
                title = title.Replace("'", "");
            }

            SqlDataAdapter adapter = new SqlDataAdapter();
            string sql = $"insert into dbo.MissingArticles (WikiID, Title, Missing) values ({id}, N'{title}', {missing})";

            SqlCommand command = new SqlCommand(sql, sqlConnection);
            adapter.InsertCommand = command;
            adapter.InsertCommand.ExecuteNonQuery();
        }
    }
}
