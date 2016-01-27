using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    class Program
    {

        public class node
        {
            public int id { get; set; }

            public string name { get; set; }

            public string title { get; set; }
        }

        public class LinkData
        {
            public string source { get; set; }

            public string target { get; set; }

            public double value { get; set; }
        }

        public class WeiboLine
        {
        }

        public class link
        {
            public int source { get; set; }

            public int target { get; set; }

            public double value { get; set; }
        }

        public class simplenode
        {
            public string name { get; set; }
        }

        public class Data
        {
            public simplenode[] nodes { get; set; }

            public link[] links { get; set; }
        }


        static void Main(string[] args)
        {
            //TestFetchWeibo();
            string file = @"D:\Git\TestCode\exp\MediaService\TestWordBreaker\TestWordBreaker\SampleTest.txt";

            for (int i = 0; i < 5; i++)
            {
                var detecter = new HotNewsDetector(100);
                detecter.ReadMessages(file);
                detecter.BreakIntoWindows();
                detecter.Predict(i, 10);
                detecter.WriteResult("SampleResult" + i.ToString());
            }


            //GenerateSankeyJson();
            //ReadWeiBoData();
            //ImportWeiboData();
        }

        public static void TryFillfailedData()
        {
            var path = @"D:\Git\TestCode\Test\TestApplication\TestApplication\bin\Debug\failedsql.txt";
            //ImportWeiboData(path);
            var result = new List<string>();
            using (var sr = new StreamReader(path))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    var cmdText = line
                                    .Replace("`", "")
                                    .Replace("\r\n", "")
                                    .Replace("),(", ")\r\n INSERT INTO new_weibo_news VALUES (");
                    result.Add(cmdText);

                }

                File.WriteAllLines("replacedSql.txt", result);
            }

            var path2 = @"D:\Git\TestCode\Test\TestApplication\TestApplication\bin\Debug\replacedSql.txt";
            ImportWeiboData(path2);
        }

        public static void ImportWeiboData(string path)
        {
            //var path = @"C: \Users\yazha\Downloads\new_weibo_news.sql";
            var connStr = "server=MCDI01;user=admin;password=Passw0rd!;database=LXHDWeibo";
            var lines = new List<string>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }

                using (StreamReader sr = new StreamReader(path))
                {
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //if (line.StartsWith("INSERT"))
                        {
                            var cmd = conn.CreateCommand();
                            try
                            {
                                var cmdText = line
                                    .Replace("`", "")
                                    .Replace("\r\n", "")
                                    .Replace("),(", "); INSERT INTO new_weibo_news VALUES (");
                                cmd.CommandText = cmdText;
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                lines.Add(cmd.CommandText);
                            }
                        }

                        //if (line.StartsWith("DROP") || line.StartsWith("INSERT") || line.StartsWith("CREATE"))
                        //{
                        //    Console.WriteLine(line);
                        //}
                    }
                }
            }
            File.WriteAllLines("failedsql22.txt", lines);
        }

        public static void ReadWeiBoData()
        {
            var path = @"C: \Users\yazha\Downloads\new_weibo_news.sql";
            using (StreamReader sr = new StreamReader(path))
            {
                string line = null;
                int i = 0;
                int fileSize = 10000;
                StreamWriter sw = null;
                int fileIndex = 0;
                while ((line = sr.ReadLine()) != null && i < fileSize)
                {
                    if (sw == null)
                    {
                        sw = new StreamWriter(fileIndex.ToString() + ".txt", true);
                    }

                    sw.WriteLine(line);
                    i++;
                    if (i == fileSize)
                    {
                        fileIndex++;
                        i = 0;
                        sw.Close();
                        sw = null;
                    }
                }
            }
        }

        public static void GenerateSankeyJson()
        {
            var linkList = new List<LinkData>();
            var conn = "Data Source=mcdi01;Initial Catalog=LRTV;Integrated Security=False;Persist Security Info=True;User ID=admin;Password=Passw0rd!;";
            using (var sqlConnection = new SqlConnection(conn))
            {
                if (sqlConnection.State != System.Data.ConnectionState.Open)
                {
                    sqlConnection.Open();
                }

                var cmd = sqlConnection.CreateCommand();
                cmd.CommandText = "select * from test3 where watchtime >= '2016-01-09 22:00:00.000'";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var link = new LinkData
                        {
                            source = reader["sourcename"].ToString(),
                            target = reader["targetname"].ToString(),
                            value = (int)reader["volume"]
                        };
                        linkList.Add(link);
                    }
                }
            }

            var nodelist = linkList.Select(i => i.source).Union(linkList.Select(i => i.target)).Distinct().ToList().OrderBy(i => i);
            int index = 0;
            var dict = new Dictionary<string, int>();
            var data = new Data();
            var nodes = nodelist.Select(i =>
            {
                var n = new node { id = index, name = i, title = i.Substring(0, i.IndexOf('|')) };
                index++;
                dict.Add(n.name, n.id);
                return n;
            }).ToArray();

            data.links = linkList.Select(i => new link { source = dict[i.source], target = dict[i.target], value = i.value }).ToArray();
            data.nodes = nodes.Select(i => new simplenode { name = i.name }).ToArray();
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText("test.txt", json);
        }

        public static void TestFetchWeibo()
        {
            var connStr = "server=MCDI01;user=admin;password=Passw0rd!;database=LXHDWeibo";
            ReadWeboDataFromDatabase(connStr, "data.txt");
        }

        public static void ReadWeboDataFromDatabase(string connStr, string path)
        {
            //var connStr = "server=MCDI01;user=admin;password=Passw0rd!;database=LXHDWeibo";
            var lines = new List<string>();
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "select  weibo_created_at, weibo_text, weibo_id from new_weibo_news order by weibo_created_at";

                    int index = 0;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sw.WriteLine(string.Format("{0},{1}, {2}", reader[0].ToString(), reader[1].ToString(), reader[2].ToString()));
                            index++;
                            if (index % 1000 == 0)
                            {
                                Console.WriteLine(index);
                            }
                        }
                    }
                }
            }
        }

    }
}
