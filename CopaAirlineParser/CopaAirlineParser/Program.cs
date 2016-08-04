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
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json.Linq;
using CsvHelper;
using System.IO.Compression;

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

        public class IATAAirport
        {
            public string stop_id;
            public string stop_name;
            public string stop_desc;
            public string stop_lat;
            public string stop_lon;
            public string zone_id;
            public string stop_url;
        }

        public class AirlinesDef
        {
            // Auto-implemented properties.  
            public string Name { get; set; }
            public string IATA { get; set; }
            public string DisplayName { get; set; }
            public string WebsiteUrl { get; set; }
        }

        static List<AirlinesDef> _Airlines = new List<AirlinesDef>
        {
            new AirlinesDef { IATA = "DA", Name="AEROLINEA DE ANTIOQUIA S.A.", DisplayName="ADA",WebsiteUrl="https://www.ada-aero.com/" },
            new AirlinesDef { IATA = "EF", Name="EASYFLY S.A", DisplayName="Easyfly",WebsiteUrl="http://www.easyfly.com.co" },
            new AirlinesDef { IATA = "2K", Name="AEROGAL", DisplayName="Avianca Ecuador",WebsiteUrl="http://www.avianca.com" },
            new AirlinesDef { IATA = "9H", Name="DUTCH ANTILLES EXPRESS SUCURSAL COLOMBIA", DisplayName="Dutch Antilles Express",WebsiteUrl="https://nl.wikipedia.org/wiki/Dutch_Antilles_Express" },
            new AirlinesDef { IATA = "AR", Name="AEROLINEAS ARGENTINAS", DisplayName="Aerolíneas Argentinas",WebsiteUrl="http://www.aerolineas.com.ar/" },
            new AirlinesDef { IATA = "AM", Name="AEROMEXICO SUCURSAL COLOMBIA", DisplayName="Aeroméxico",WebsiteUrl="http://www.aeromexico.com/" },
            new AirlinesDef { IATA = "P5", Name="AEROREPUBLICA", DisplayName="Copa Airlines",WebsiteUrl="http://www.copa.com" },
            new AirlinesDef { IATA = "AC", Name="Air Canada", DisplayName="Air Canada",WebsiteUrl="http://www.aircanada.com" },
            new AirlinesDef { IATA = "AF", Name="AIR FRANCE", DisplayName="Air France",WebsiteUrl="http://www.airfrance.com" },
            new AirlinesDef { IATA = "4C", Name="AIRES", DisplayName="LATAM Colombia",WebsiteUrl="http://www.latam.com/" },
            new AirlinesDef { IATA = "AA", Name="AMERICAN", DisplayName="American Airlines",WebsiteUrl="http://www.aa.com" },
            new AirlinesDef { IATA = "AV", Name="Avianca", DisplayName="Avianca",WebsiteUrl="http://www.avianca.com" },
            new AirlinesDef { IATA = "V0", Name="CONVIASA", DisplayName="Conviasa",WebsiteUrl="http://www.conviasa.aero/" },
            new AirlinesDef { IATA = "CM", Name="COPA", DisplayName="Copa Airlines",WebsiteUrl="http://www.copaair.com/" },
            new AirlinesDef { IATA = "CU", Name="CUBANA", DisplayName="Cubana de Aviación",WebsiteUrl="http://www.cubana.cu/home/?lang=en" },
            new AirlinesDef { IATA = "DL", Name="DELTA", DisplayName="Delta",WebsiteUrl="http://www.delta.com" },
            new AirlinesDef { IATA = "4O", Name="INTERJET", DisplayName="Interjet",WebsiteUrl="http://www.interjet.com/" },
            new AirlinesDef { IATA = "5Z", Name="FAST COLOMBIA SAS", DisplayName="ViVaColombia",WebsiteUrl="http://www.vivacolombia.co/" },
            new AirlinesDef { IATA = "IB", Name="IBERIA", DisplayName="Iberia",WebsiteUrl="http://www.iberia.com" },
            new AirlinesDef { IATA = "B6", Name="JETBLUE AIRWAYS CORPORATION", DisplayName="Jetblue",WebsiteUrl="http://www.jetblue.com" },
            new AirlinesDef { IATA = "LR", Name="LACSA", DisplayName="Avianca Costa Rica",WebsiteUrl="http://www.avianca.com" },
            new AirlinesDef { IATA = "LA", Name="LAN AIRLINES S.A.", DisplayName="LAN Airlines",WebsiteUrl="http://www.lan.com/" },
            new AirlinesDef { IATA = "LP", Name="LAN PERU", DisplayName="LAN Airlines",WebsiteUrl="http://www.lan.com/" },
            new AirlinesDef { IATA = "LH", Name="Lufthansa", DisplayName="Lufthansa",WebsiteUrl="http://www.lufthansa.com" },
            new AirlinesDef { IATA = "9R", Name="SERVICIO AEREO A TERRITORIOS NACIONALES SATENA", DisplayName="Satena",WebsiteUrl="http://www.satena.com/" },
            new AirlinesDef { IATA = "NK", Name="SPIRIT AIRLINES", DisplayName="Spirit",WebsiteUrl="http://www.spirit.com" },
            new AirlinesDef { IATA = "TA", Name="TACA INTERNATIONAL", DisplayName="TACA Airlines",WebsiteUrl="http://www.taca.com/" },
            new AirlinesDef { IATA = "EQ", Name="TAME", DisplayName="TAME",WebsiteUrl="http://www.tame.com.ec/" },
            new AirlinesDef { IATA = "3P", Name="TIARA", DisplayName="Tiara Air Aruba",WebsiteUrl="http://www.tiara-air.com/" },
            new AirlinesDef { IATA = "T0", Name="TRANS AMERICAN AIR LINES S.A. SUCURSAL COL.", DisplayName="Trans American Airlines",WebsiteUrl="http://www.avianca.com/" },
            new AirlinesDef { IATA = "UA", Name="United Airlines", DisplayName="United",WebsiteUrl="http://www.united.com" },
            new AirlinesDef { IATA = "4C", Name="LATAM AIRLINES GROUP S.A SUCURSAL COLOMBIA", DisplayName="LATAM",WebsiteUrl="http://www.latam.com/" },
            new AirlinesDef { IATA = "TP", Name="TAP PORTUGAL SUCURSAL COLOMBIA", DisplayName="TAP",WebsiteUrl="http://www.flytap.com" },
            new AirlinesDef { IATA = "7P", Name="AIR PANAMA", DisplayName="Air Panama",WebsiteUrl="http://www.airpanama.com/" },
            new AirlinesDef { IATA = "O6", Name="OCEANAIR", DisplayName="Avianca Brazil",WebsiteUrl="http://www.avianca.com" },
            new AirlinesDef { IATA = "8I", Name="INSELAIR ARUBA", DisplayName="Insel Air Aruba",WebsiteUrl="http://www.fly-inselair.com/"},
            new AirlinesDef { IATA = "7I", Name="INSEL AIR", DisplayName="Insel Air",WebsiteUrl="http://www.fly-inselair.com/"},
            new AirlinesDef { IATA = "TK", Name="Turkish Airlines", DisplayName="Turkish Airlines",WebsiteUrl="http://www.turkishairlines.com"},
            new AirlinesDef { IATA = "UX", Name="AIR EUROPA", DisplayName="Air Europe",WebsiteUrl="http://www.aireurope.com"},
            new AirlinesDef { IATA = "9V", Name="AVIOR AIRLINES,C.A.", DisplayName="Avior Airlines",WebsiteUrl="http://www.avior.com.ve/"},
            new AirlinesDef { IATA = "KL", Name="KLM", DisplayName="KLM",WebsiteUrl="http://www.klm.nl"},
            new AirlinesDef { IATA = "JJ", Name="TAM", DisplayName="TAM Linhas Aéreas",WebsiteUrl="http://www.latam.com/"}
        };


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
            string filePath = Path.Combine(appDataFolder, "CM.db");
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

            //Console.WriteLine("Insert into Database...");
            //for (int i = 0; i < CIFLights.Count; i++) // Loop through List with for)
            //{
            //    using (SqlConnection connection = new SqlConnection("Server=(local);Database=CI-Import;Trusted_Connection=True;"))
            //    {
            //        using (SqlCommand command3 = new SqlCommand())
            //        {
            //            command3.Connection = connection;            // <== lacking
            //            command3.CommandType = CommandType.StoredProcedure;
            //            command3.CommandText = "InsertFlight";
            //            command3.Parameters.Add(new SqlParameter("@FlightSource", 8));
            //            command3.Parameters.Add(new SqlParameter("@FromIATA", CIFLights[i].FromIATA));
            //            command3.Parameters.Add(new SqlParameter("@ToIATA", CIFLights[i].ToIATA));
            //            command3.Parameters.Add(new SqlParameter("@FromDate", CIFLights[i].FromDate));
            //            command3.Parameters.Add(new SqlParameter("@ToDate", CIFLights[i].ToDate));
            //            command3.Parameters.Add(new SqlParameter("@FlightMonday", CIFLights[i].FlightMonday));
            //            command3.Parameters.Add(new SqlParameter("@FlightTuesday", CIFLights[i].FlightTuesday));
            //            command3.Parameters.Add(new SqlParameter("@FlightWednesday", CIFLights[i].FlightWednesday));
            //            command3.Parameters.Add(new SqlParameter("@FlightThursday", CIFLights[i].FlightThursday));
            //            command3.Parameters.Add(new SqlParameter("@FlightFriday", CIFLights[i].FlightFriday));
            //            command3.Parameters.Add(new SqlParameter("@FlightSaterday", CIFLights[i].FlightSaterday));
            //            command3.Parameters.Add(new SqlParameter("@FlightSunday", CIFLights[i].FlightSunday));
            //            command3.Parameters.Add(new SqlParameter("@DepartTime", CIFLights[i].DepartTime));
            //            command3.Parameters.Add(new SqlParameter("@ArrivalTime", CIFLights[i].ArrivalTime));
            //            command3.Parameters.Add(new SqlParameter("@FlightNumber", CIFLights[i].FlightNumber));
            //            command3.Parameters.Add(new SqlParameter("@FlightAirline", CIFLights[i].FlightAirline));
            //            command3.Parameters.Add(new SqlParameter("@FlightOperator", CIFLights[i].FlightOperator));
            //            command3.Parameters.Add(new SqlParameter("@FlightAircraft", CIFLights[i].FlightAircraft));
            //            command3.Parameters.Add(new SqlParameter("@FlightCodeShare", CIFLights[i].FlightCodeShare));
            //            command3.Parameters.Add(new SqlParameter("@FlightNextDayArrival", CIFLights[i].FlightNextDayArrival));
            //            command3.Parameters.Add(new SqlParameter("@FlightDuration", CIFLights[i].FlightDuration));
            //            command3.Parameters.Add(new SqlParameter("@FlightNextDays", CIFLights[i].FlightNextDays));
            //            command3.Parameters.Add(new SqlParameter("@FlightNonStop", true));
            //            command3.Parameters.Add(new SqlParameter("@FlightVia", DBNull.Value));
            //            foreach (SqlParameter parameter in command3.Parameters)
            //            {
            //                if (parameter.Value == null)
            //                {
            //                    parameter.Value = DBNull.Value;
            //                }
            //            }


            //            try
            //            {
            //                connection.Open();
            //                int recordsAffected = command3.ExecuteNonQuery();
            //            }

            //            finally
            //            {
            //                connection.Close();
            //            }
            //        }
            //    }

            //}


            Console.WriteLine("Reading IATA Airports....");
            string IATAAirportsFile = AppDomain.CurrentDomain.BaseDirectory + "IATAAirports.json";
            JArray o1 = JArray.Parse(File.ReadAllText(IATAAirportsFile));
            IList<IATAAirport> TempIATAAirports = o1.ToObject<IList<IATAAirport>>();
            var IATAAirports = TempIATAAirports as List<IATAAirport>;



            string gtfsDir = AppDomain.CurrentDomain.BaseDirectory + "\\gtfs";
            System.IO.Directory.CreateDirectory(gtfsDir);

            Console.WriteLine("Creating GTFS Files...");

            Console.WriteLine("Creating GTFS File agency.txt...");
            using (var gtfsagency = new StreamWriter(@"gtfs\\agency.txt"))
            {
                var csv = new CsvWriter(gtfsagency);
                csv.Configuration.Delimiter = ",";
                csv.Configuration.Encoding = Encoding.UTF8;
                csv.Configuration.TrimFields = true;
                // header 
                csv.WriteField("agency_id");
                csv.WriteField("agency_name");
                csv.WriteField("agency_url");
                csv.WriteField("agency_timezone");
                csv.WriteField("agency_lang");
                csv.WriteField("agency_phone");
                csv.WriteField("agency_fare_url");
                csv.WriteField("agency_email");
                csv.NextRecord();

                var airlines = CIFLights.Select(m => new { m.FlightAirline }).Distinct().ToList();

                for (int i = 0; i < airlines.Count; i++) // Loop through List with for)
                {
                    var item4 = _Airlines.Find(q => q.IATA == airlines[i].FlightAirline);
                    string TEMP_Name = item4.DisplayName;
                    string TEMP_Url = item4.WebsiteUrl;
                    string TEMP_IATA = item4.IATA;
                    csv.WriteField(TEMP_IATA);
                    csv.WriteField(TEMP_Name);
                    csv.WriteField(TEMP_Url);
                    csv.WriteField("America/Bogota");
                    csv.WriteField("ES");
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.NextRecord();
                }

                //csv.WriteField("AV");
                //csv.WriteField("Avianca");
                //csv.WriteField("http://www.avianca.com");
                //csv.WriteField("America/Bogota");
                //csv.WriteField("ES");
                //csv.WriteField("");
                //csv.WriteField("");
                //csv.WriteField("");
                //csv.NextRecord();
            }

            Console.WriteLine("Creating GTFS File routes.txt ...");

            using (var gtfsroutes = new StreamWriter(@"gtfs\\routes.txt"))
            {
                // Route record


                var csvroutes = new CsvWriter(gtfsroutes);
                csvroutes.Configuration.Delimiter = ",";
                csvroutes.Configuration.Encoding = Encoding.UTF8;
                csvroutes.Configuration.TrimFields = true;
                // header 
                csvroutes.WriteField("route_id");
                csvroutes.WriteField("agency_id");
                csvroutes.WriteField("route_short_name");
                csvroutes.WriteField("route_long_name");
                csvroutes.WriteField("route_desc");
                csvroutes.WriteField("route_type");
                csvroutes.WriteField("route_url");
                csvroutes.WriteField("route_color");
                csvroutes.WriteField("route_text_color");
                csvroutes.NextRecord();

                var routes = CIFLights.Select(m => new { m.FromIATA, m.ToIATA, m.FlightAirline }).Distinct().ToList();

                for (int i = 0; i < routes.Count; i++) // Loop through List with for)
                {

                    //var item4 = _Airlines.Find(q => q.IATA == routes[i].FlightAirline);
                    //string TEMP_Name = item4.DisplayName;
                    //string TEMP_Url = item4.WebsiteUrl;
                    //string TEMP_IATA = item4.IATA;

                    var FromAirportInfo = IATAAirports.Find(q => q.stop_id == routes[i].FromIATA);
                    var ToAirportInfo = IATAAirports.Find(q => q.stop_id == routes[i].ToIATA);

                    csvroutes.WriteField(routes[i].FromIATA + routes[i].ToIATA + routes[i].FlightAirline);
                    csvroutes.WriteField(routes[i].FlightAirline);
                    csvroutes.WriteField(routes[i].FromIATA + routes[i].ToIATA);
                    csvroutes.WriteField(FromAirportInfo.stop_name + " - " + ToAirportInfo.stop_name);
                    csvroutes.WriteField(""); // routes[i].FlightAircraft + ";" + CIFLights[i].FlightAirline + ";" + CIFLights[i].FlightOperator + ";" + CIFLights[i].FlightCodeShare
                    csvroutes.WriteField(1102);
                    csvroutes.WriteField("");
                    csvroutes.WriteField("");
                    csvroutes.WriteField("");
                    csvroutes.NextRecord();
                }
            }

            // stops.txt

            List<string> agencyairportsiata =
             CIFLights.SelectMany(m => new string[] { m.FromIATA, m.ToIATA })
                     .Distinct()
                     .ToList();

            using (var gtfsstops = new StreamWriter(@"gtfs\\stops.txt"))
            {
                // Route record
                var csvstops = new CsvWriter(gtfsstops);
                csvstops.Configuration.Delimiter = ",";
                csvstops.Configuration.Encoding = Encoding.UTF8;
                csvstops.Configuration.TrimFields = true;
                // header                                 
                csvstops.WriteField("stop_id");
                csvstops.WriteField("stop_name");
                csvstops.WriteField("stop_desc");
                csvstops.WriteField("stop_lat");
                csvstops.WriteField("stop_lon");
                csvstops.WriteField("zone_id");
                csvstops.WriteField("stop_url");
                csvstops.NextRecord();

                for (int i = 0; i < agencyairportsiata.Count; i++) // Loop through List with for)
                {
                    //int result1 = IATAAirports.FindIndex(T => T.stop_id == 9458)
                    var airportinfo = IATAAirports.Find(q => q.stop_id == agencyairportsiata[i]);
                    csvstops.WriteField(airportinfo.stop_id);
                    csvstops.WriteField(airportinfo.stop_name);
                    csvstops.WriteField(airportinfo.stop_desc);
                    csvstops.WriteField(airportinfo.stop_lat);
                    csvstops.WriteField(airportinfo.stop_lon);
                    csvstops.WriteField(airportinfo.zone_id);
                    csvstops.WriteField(airportinfo.stop_url);
                    csvstops.NextRecord();
                }
            }


            Console.WriteLine("Creating GTFS File trips.txt, stop_times.txt, calendar.txt ...");

            using (var gtfscalendar = new StreamWriter(@"gtfs\\calendar.txt"))
            {
                using (var gtfstrips = new StreamWriter(@"gtfs\\trips.txt"))
                {
                    using (var gtfsstoptimes = new StreamWriter(@"gtfs\\stop_times.txt"))
                    {
                        // Headers 
                        var csvstoptimes = new CsvWriter(gtfsstoptimes);
                        csvstoptimes.Configuration.Delimiter = ",";
                        csvstoptimes.Configuration.Encoding = Encoding.UTF8;
                        csvstoptimes.Configuration.TrimFields = true;
                        // header 
                        csvstoptimes.WriteField("trip_id");
                        csvstoptimes.WriteField("arrival_time");
                        csvstoptimes.WriteField("departure_time");
                        csvstoptimes.WriteField("stop_id");
                        csvstoptimes.WriteField("stop_sequence");
                        csvstoptimes.WriteField("stop_headsign");
                        csvstoptimes.WriteField("pickup_type");
                        csvstoptimes.WriteField("drop_off_type");
                        csvstoptimes.WriteField("shape_dist_traveled");
                        csvstoptimes.WriteField("timepoint");
                        csvstoptimes.NextRecord();

                        var csvtrips = new CsvWriter(gtfstrips);
                        csvtrips.Configuration.Delimiter = ",";
                        csvtrips.Configuration.Encoding = Encoding.UTF8;
                        csvtrips.Configuration.TrimFields = true;
                        // header 
                        csvtrips.WriteField("route_id");
                        csvtrips.WriteField("service_id");
                        csvtrips.WriteField("trip_id");
                        csvtrips.WriteField("trip_headsign");
                        csvtrips.WriteField("trip_short_name");
                        csvtrips.WriteField("direction_id");
                        csvtrips.WriteField("block_id");
                        csvtrips.WriteField("shape_id");
                        csvtrips.WriteField("wheelchair_accessible");
                        csvtrips.WriteField("bikes_allowed ");
                        csvtrips.NextRecord();

                        var csvcalendar = new CsvWriter(gtfscalendar);
                        csvcalendar.Configuration.Delimiter = ",";
                        csvcalendar.Configuration.Encoding = Encoding.UTF8;
                        csvcalendar.Configuration.TrimFields = true;
                        // header 
                        csvcalendar.WriteField("service_id");
                        csvcalendar.WriteField("monday");
                        csvcalendar.WriteField("tuesday");
                        csvcalendar.WriteField("wednesday");
                        csvcalendar.WriteField("thursday");
                        csvcalendar.WriteField("friday");
                        csvcalendar.WriteField("saturday");
                        csvcalendar.WriteField("sunday");
                        csvcalendar.WriteField("start_date");
                        csvcalendar.WriteField("end_date");
                        csvcalendar.NextRecord();

                        //1101 International Air Service
                        //1102 Domestic Air Service
                        //1103 Intercontinental Air Service
                        //1104 Domestic Scheduled Air Service


                        for (int i = 0; i < CIFLights.Count; i++) // Loop through List with for)
                        {

                            // Calender

                            csvcalendar.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightMonday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightTuesday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightWednesday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightThursday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightFriday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightSaterday));
                            csvcalendar.WriteField(Convert.ToInt32(CIFLights[i].FlightSunday));
                            csvcalendar.WriteField(String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate));
                            csvcalendar.WriteField(String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate));
                            csvcalendar.NextRecord();

                            // Trips

                            //var item4 = _Airlines.Find(q => q.Name == CIFLights[i].FlightAirline);
                            //string TEMP_IATA = item4.IATA;

                            var FromAirportInfo = IATAAirports.Find(q => q.stop_id == CIFLights[i].FromIATA);
                            var ToAirportInfo = IATAAirports.Find(q => q.stop_id == CIFLights[i].ToIATA);

                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightAirline);
                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate));
                            csvtrips.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate));
                            csvtrips.WriteField(ToAirportInfo.stop_name);
                            csvtrips.WriteField(CIFLights[i].FlightNumber);
                            csvtrips.WriteField("");
                            csvtrips.WriteField("");
                            csvtrips.WriteField("");
                            csvtrips.WriteField("1");
                            csvtrips.WriteField("");
                            csvtrips.NextRecord();

                            // Depart Record
                            csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate));
                            csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].DepartTime));
                            csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].DepartTime));
                            csvstoptimes.WriteField(CIFLights[i].FromIATA);
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("");
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("0");
                            csvstoptimes.WriteField("");
                            csvstoptimes.WriteField("");
                            csvstoptimes.NextRecord();
                            // Arrival Record
                            if (!CIFLights[i].FlightNextDayArrival)
                            {
                                csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate));
                                csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].ArrivalTime));
                                csvstoptimes.WriteField(String.Format("{0:HH:mm:ss}", CIFLights[i].ArrivalTime));
                                csvstoptimes.WriteField(CIFLights[i].ToIATA);
                                csvstoptimes.WriteField("2");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("");
                                csvstoptimes.NextRecord();
                            }
                            else
                            {
                                //add 24 hour for the gtfs time
                                int hour = CIFLights[i].ArrivalTime.Hour;
                                hour = hour + 24;
                                int minute = CIFLights[i].ArrivalTime.Minute;
                                string strminute = minute.ToString();
                                if (strminute.Length == 1) { strminute = "0" + strminute; }
                                csvstoptimes.WriteField(CIFLights[i].FromIATA + CIFLights[i].ToIATA + CIFLights[i].FlightNumber.Replace(" ", "") + String.Format("{0:yyyyMMdd}", CIFLights[i].FromDate) + String.Format("{0:yyyyMMdd}", CIFLights[i].ToDate));
                                csvstoptimes.WriteField(hour + ":" + strminute + ":00");
                                csvstoptimes.WriteField(hour + ":" + strminute + ":00");
                                csvstoptimes.WriteField(CIFLights[i].ToIATA);
                                csvstoptimes.WriteField("2");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("0");
                                csvstoptimes.WriteField("");
                                csvstoptimes.WriteField("");
                                csvstoptimes.NextRecord();
                            }
                        }
                    }
                }
            }

            // Create Zip File
            string startPath = gtfsDir;
            string zipPath = myDir + "\\CM.zip";
            if (File.Exists(zipPath)) { File.Delete(zipPath); }
            System.IO.Compression.ZipFile.CreateFromDirectory(startPath, zipPath, CompressionLevel.Fastest, false);
        }
        
    }
}
