using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Data.SQLite;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip; 
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Core; 


namespace CopaAirlineParser
{
    class Program
    {
        static void Main(string[] args)
        {
            // Please install http://desktop.innoflightmaps.com/download/cm/cm-EN/index.html
            // And use offline timetable data
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataFolder = appDataFolder + @"\com.innovatallc.FlightMapsDesktop.CM.EN\Local Store\db\";
            string myDir = AppDomain.CurrentDomain.BaseDirectory + "\\data";
            System.IO.Directory.CreateDirectory(myDir);
            string sqldb = Path.Combine(myDir, "cm.sqlite");
            string dbdownload = Path.Combine(myDir, "cm.gz");
            string filePath = Path.Combine(appDataFolder, "CMsafdasdf.db");
            if (File.Exists(filePath)) {                
                File.Copy(filePath,Path.Combine(myDir,"cm.sqlite"));               
            }
            else
            {
                var request = (HttpWebRequest)WebRequest.Create("http://mtk.innovataw3svc.com/MapDataToolKitServices.asmx/GetLastUpdate?");

                var postData = @"_sSearchXML=<GetLastUpdate_Input customerCode=""CM"" customerSubCode="""" productCode=""DskMap8.0"" lang=""EN""/>";
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                string b = WebUtility.HtmlDecode(responseString);

                XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
                xmlDoc.LoadXml(b);
                XmlNodeList xmlnode;
                int i = 0;
                xmlnode = xmlDoc.GetElementsByTagName("dbFile");

                for (i = 0; i <= xmlnode.Count - 1; i++)
                {
                    if (xmlnode[i].Attributes["type"].Value == "mobile")
                    {
                        string downloadurl = xmlnode[i].ChildNodes.Item(0).InnerText.Trim();
                        using (var client = new WebClient())
                        {
                            Console.WriteLine("Downloading {0}", downloadurl);
                            client.DownloadFile(downloadurl, dbdownload);
                            

                        }
                        Console.WriteLine("Extracting {0}", dbdownload);
                        Stream inStream = File.OpenRead(dbdownload);

                        byte[] buffer = new byte[4096];  //more than 4k is a waste
                        using (Stream fs = new FileStream(dbdownload, FileMode.Open, FileAccess.Read))
                        {
                            using (GZipInputStream gzipStream = new GZipInputStream(fs))
                            {
                                using (FileStream fsout = File.Create(sqldb))
                                {
                                    StreamUtils.Copy(gzipStream, fsout, buffer);
                                }
                            }
                        }                        
                    }

                    
                }
            }
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=" + sqldb + ";Version=3;");
            m_dbConnection.Open();
            string sql = "select * from flights inner join legs on flights.flightId = legs.flightId where flights.numLegs=1";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("ID: " + reader["flightId"] + "\tFrom: " + reader["dptCity"] + "\tTo: " + reader["arvCity"]);
            }

        }
        
    }
}
