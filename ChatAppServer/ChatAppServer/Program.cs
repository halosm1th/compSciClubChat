using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using Mono.Data.Sqlite;
using System.Threading;

namespace ChatAppServer
{
    public class chatApp
    {
        //These variables I want to be able to accesss no mater what, so it is helpful to have them global.

        //Networking data
        public static int port = 4000;//The port on which the client connects
        public static string adress = "0.0.0.0";//allow any ipadress to connect
        public static IPAddress ipadress = IPAddress.Parse(adress);
        public static bool run = true;

        //Message data
        public static SqliteConnection m_dbConnection; //The database connection

        //Main method
        static void Main(string[] args)
        {
            string sql;
            SqliteCommand command;
            Console.WriteLine("Starting server 1 !");

            try
            {
                Console.WriteLine("Checking for database.");
                if (!File.Exists("userData.sqlite"))
                {
                    otherStuff.error("No database found! creating a new one!");
                    SqliteConnection.CreateFile("userData.sqlite");//Create and connect to a database;
                    m_dbConnection = new SqliteConnection("Data Source=userData.sqlite;Version=3;");
                    m_dbConnection.Open();

                    sql = "CREATE TABLE users (id INT, username VARCHAR(100), password VARCHAR(1000));";//Create a new table in the database for users
                    command = new SqliteCommand(sql, m_dbConnection);
                    command.ExecuteNonQuery();//Create the table for it
                    otherStuff.Success("Database created!");
                    sql = "CREATE TABLE activeSession (Sessionid VARCAHR(1000), usernameOne VARCHAR(1000), usernameTwo VARCAHR(1000));";
                    command = new SqliteCommand(sql, m_dbConnection);
                    command.ExecuteNonQuery();
                    otherStuff.Success("Active session Table created!");
                }
                else
                {
                    otherStuff.Success("The database was found!");
                    Console.WriteLine("Booting database.");
                    try
                    {
                        m_dbConnection = new SqliteConnection("Data Source=userData.sqlite;Version=3;");//Connect and open the database
                        m_dbConnection.Open();
                        otherStuff.Success("Database was opened!");
                    }
                    catch (Exception e)
                    {
                        otherStuff.error("There was an error: " + e);
                    }
                }
                Console.WriteLine("Trying to create activeUsersTable ");
                try
                {
                    sql = "CREATE TABLE ActiveUsers (id INT, username VARCHAR(1000), ip VARCHAR(12));";//Create a new table for active users
                    command = new SqliteCommand(sql, m_dbConnection);
                    command.ExecuteNonQuery();
                    otherStuff.Success("Active users table created!");
                    
                }
                catch (Exception e)
                {
                    otherStuff.error("There was an error: " + e);
                }

                while (run == true)
                {
                    run = controlConsole.ControlConsole();
                }
                Environment.Exit(1);
            }

            catch (Exception e)
            {
                otherStuff.error("There was an error: " + e);
            }
        }

        
    }
}