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
using System.Globalization;

namespace CopaAirlineParser
{
    public class Program 
    {
        [Serializable]
        public class CIFLight
        {
            // Auto-implemented properties. 

            public string FromIATA;
            public string ToIATA;
            public DateTime FromDate;
            public DateTime ToDate;
            public Boolean FlightMonday;
            public Boolean FlightTuesday;
            public Boolean FlightWednesday;
            public Boolean FlightThursday;
            public Boolean FlightFriday;
            public Boolean FlightSaterday;
            public Boolean FlightSunday;
            public DateTime DepartTime;
            public DateTime ArrivalTime;
            public String FlightNumber;
            public String FlightAirline;
            public String FlightOperator;
            public String FlightAircraft;
            public Boolean FlightCodeShare;
            public Boolean FlightNextDayArrival;
            public int FlightNextDays;
            public string FlightDuration;
        }
        
        
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
            DateTime ValidFrom = DateTime.MinValue;
            DateTime ValidTo = DateTime.MinValue;
            List<CIFLight> CIFLights = new List<CIFLight> { };
            CultureInfo ci = new CultureInfo("en-US");
            string dateformat = "MM/dd/yyyy h:mm:ss tt";


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
            string sqlupdate = "select * from lastupdated;";
            string sql = "select flights.dptSta, flights.arvSta, datetime(effDate) as effDate, datetime(discDate) as discDate, opMon, opTue, opWed, opThu, opFri, opSat, opSun, cast(dptTimeLocal as TEXT) as dptTimeLocal,  cast(arvTimeLocal as TEXT) as arvTimeLocal, carrier, flightNum, equipCode, codeshare, opCarrier, arvNextDay, legNextDay, flights.flightMinutes from flights inner join legs on flights.flightId = legs.flightId where flights.numLegs=1 and legs.numStops =0";
            SQLiteCommand commandupd = new SQLiteCommand(sqlupdate, m_dbConnection);
            SQLiteDataReader readerupd = commandupd.ExecuteReader();
            while (readerupd.Read())
            {
                ValidFrom = DateTime.Parse(readerupd["UtgValidityStart"].ToString());
                ValidTo = DateTime.Parse(readerupd["UtgValidityEnd"].ToString());
            }

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string TEMP_FromIATA = null;
                string TEMP_ToIATA = null;
                DateTime TEMP_ValidFrom = new DateTime();
                DateTime TEMP_ValidTo = new DateTime();                
                Boolean TEMP_FlightMonday = false;
                Boolean TEMP_FlightTuesday = false;
                Boolean TEMP_FlightWednesday = false;
                Boolean TEMP_FlightThursday = false;
                Boolean TEMP_FlightFriday = false;
                Boolean TEMP_FlightSaterday = false;
                Boolean TEMP_FlightSunday = false;
                DateTime TEMP_DepartTime = new DateTime();
                DateTime TEMP_ArrivalTime = new DateTime();
                Boolean TEMP_FlightCodeShare = false;
                string TEMP_FlightNumber = null;
                string TEMP_Aircraftcode = null;
                TimeSpan TEMP_DurationTime = TimeSpan.MinValue;
                Boolean TEMP_FlightNextDayArrival = false;
                int TEMP_FlightNextDays = 0;
                string TEMP_FlightOperator = null;
                string TEMP_Airline = null;

                TEMP_FromIATA = reader["dptSta"].ToString();
                TEMP_ToIATA = reader["arvSta"].ToString();
                TEMP_ValidFrom = DateTime.Parse(reader["effDate"].ToString());
                TEMP_ValidTo = DateTime.Parse(reader["discDate"].ToString());
                TEMP_FlightMonday = Boolean.Parse(reader["opMon"].ToString());
                TEMP_FlightTuesday = Boolean.Parse(reader["opTue"].ToString());
                TEMP_FlightWednesday = Boolean.Parse(reader["opWed"].ToString());
                TEMP_FlightThursday = Boolean.Parse(reader["opThu"].ToString());
                TEMP_FlightFriday = Boolean.Parse(reader["opFri"].ToString());
                TEMP_FlightSaterday = Boolean.Parse(reader["opSat"].ToString());
                TEMP_FlightSunday = Boolean.Parse(reader["opSun"].ToString());                
                TEMP_DepartTime = DateTime.ParseExact(reader["dptTimeLocal"].ToString(), dateformat, ci);
                TEMP_ArrivalTime = DateTime.ParseExact(reader["arvTimeLocal"].ToString(), dateformat, ci);
                TEMP_FlightNumber = reader["carrier"].ToString() + reader["flightNum"].ToString();
                TEMP_Aircraftcode = reader["equipCode"].ToString();
                if (reader["codeshare"].ToString() == "1")
                {
                    TEMP_FlightOperator = reader["opCarrier"].ToString();
                    TEMP_FlightCodeShare = true;
                }
                else
                {
                    TEMP_FlightOperator = reader["carrier"].ToString();
                    TEMP_FlightCodeShare = false;
                }
                if (reader["arvNextDay"].ToString() == "1")
                {
                    TEMP_FlightNextDayArrival = true;
                }                
                TEMP_Airline = reader["carrier"].ToString();
                TEMP_FlightNextDays = int.Parse(reader["legNextDay"].ToString());
                TEMP_DurationTime = TimeSpan.FromMinutes(double.Parse(reader["flightMinutes"].ToString()));

                CIFLights.Add(new CIFLight
                {
                    FromIATA = TEMP_FromIATA,
                    ToIATA = TEMP_ToIATA,
                    FromDate = TEMP_ValidFrom,
                    ToDate = TEMP_ValidTo,
                    ArrivalTime = TEMP_ArrivalTime,
                    DepartTime = TEMP_DepartTime,
                    FlightAircraft = TEMP_Aircraftcode,
                    FlightAirline = TEMP_Airline,
                    FlightMonday = TEMP_FlightMonday,
                    FlightTuesday = TEMP_FlightTuesday,
                    FlightWednesday = TEMP_FlightWednesday,
                    FlightThursday = TEMP_FlightThursday,
                    FlightFriday = TEMP_FlightFriday,
                    FlightSaterday = TEMP_FlightSaterday,
                    FlightSunday = TEMP_FlightSunday,
                    FlightNumber = TEMP_FlightNumber,
                    FlightOperator = TEMP_FlightOperator,
                    FlightDuration = TEMP_DurationTime.ToString().Replace("-", ""),
                    FlightCodeShare = TEMP_FlightCodeShare,
                    FlightNextDayArrival = TEMP_FlightNextDayArrival,
                    FlightNextDays = TEMP_FlightNextDays
                });
            }
            // Write the list of objects to a file.
            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(CIFLights.GetType());
            string myDirOut = AppDomain.CurrentDomain.BaseDirectory + "\\output";
            System.IO.Directory.CreateDirectory(myDirOut);
            System.IO.StreamWriter file =
               new System.IO.StreamWriter("output\\output.xml");

            writer.Serialize(file, CIFLights);
            file.Close();
            Console.ReadKey();

        }
        
    }
}
