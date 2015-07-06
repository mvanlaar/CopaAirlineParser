using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SQLite;

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
            string filePath = Path.Combine(appDataFolder, "CM.db");
            if (File.Exists(filePath)) {
                string myDir = AppDomain.CurrentDomain.BaseDirectory + "\\data";
                string sqldb = Path.Combine(myDir,"cm.sqlite");
                System.IO.Directory.CreateDirectory(myDir);
                File.Copy(filePath,Path.Combine(myDir,"cm.sqlite"));
                SQLiteConnection m_dbConnection;
                m_dbConnection = new SQLiteConnection("Data Source="+sqldb+";Version=3;");
                m_dbConnection.Open();
                string sql = "select * from flights inner join legs on flights.flightId = legs.flightId where flights.numLegs=1";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    Console.WriteLine("ID: " + reader["flightId"] + "\tFrom: " + reader["dptCity"] + "\tTo: " + reader["arvCity"]);
                }
            }
            else
            {
                Console.WriteLine(filePath);
                Console.ReadKey();
                Webservice.GetLastUpdate()
            }

        }
    }
}
