using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Data.SQLite;
using System.Threading;

namespace ChatAppServer
{
    public class Program
    {
        //These variables I want to be able to accesss no mater what, so it is helpful to have them global.

        //Networking data
        public static int port = 4000;//The port on which the client connects
        public static string adress = "0.0.0.0";//allow any ipadress to connect
        public static IPAddress ipadress = IPAddress.Parse(adress);
        public static bool run = true;

        //Message data
        public static string sender;
        public static string reciver;
        public static string message;
        public static SQLiteConnection m_dbConnection; //The database connection

        //Main method
        static void Main(string[] args)
        {
            
            string sql;
            SQLiteCommand command;

            Console.WriteLine("Starting server!");//Declare that the sever is started
            if (!File.Exists("userData.sqlite"))//If the database exists
            {
                Error("Error: no database file found! Generating new database.");
                try
                {
                    SQLiteConnection.CreateFile("userData.sqlite");//Create and connect to a database;
                    m_dbConnection = new SQLiteConnection("Data Source=userData.sqlite;Version=3;");
                    m_dbConnection.Open();

                    sql = "CREATE TABLE users (id INT, username VARCHAR(100), password VARCHAR(100));";//Create a new table in the database for users
                    command = new SQLiteCommand(sql, m_dbConnection);
                    command.ExecuteNonQuery();//Create the table for it
                    Success("Database created!");
                }
                catch (Exception e)
                {
                    Error("There was an error: " + e);
                }
            }
            else//If it does exist, do this
            {
                Success("The database was found!");
                Console.WriteLine("Booting database.");
                try {
                    m_dbConnection = new SQLiteConnection("Data Source=userData.sqlite;Version=3;");//Connect and open the database
                    m_dbConnection.Open();
                    Success("Database was opened!");
                }
                catch(Exception e)
                {
                    Error("There was an error: " + e);
                }
                
            }
            
            Console.WriteLine("Trying to create activeUsersTable!");
            try {
                sql = "CREATE TABLE ActiveUsers (id INT, username VARCHAR(1000), password VARCHAR(1000));";//Create a new table for active users
                command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                Success("Active users table created!");
            }
            catch (Exception e)
            {
                Error("There was an error: " + e);
            }

            while (run == true)
            {
                run = ControlConsole();
            }
            Environment.Exit(1);
        }

        public static bool ControlConsole()
        {
            Thread thread = new Thread(new ThreadStart(networking));
            thread.Start();
            string input;
            string sql;
            SQLiteCommand command;
            while (run == true)
            {
                if (!thread.IsAlive)
                {
                    thread.Start();
                }
                Console.Write(">> ");
                input = Console.ReadLine();
                if (input == "exit")
                {

                    Console.WriteLine("Shutting down server!");
                    try
                    {
                        Console.WriteLine("Shutting down messaging!");
                        try {
                            thread.Suspend();
                        }
                        catch
                        {

                        }
                        //thread.Abort();
                        Success("Messaging down.");
                        Console.WriteLine("Deleting active users");
                        sql = "DROP TABLE ActiveUsers;";
                        command = new SQLiteCommand(sql, m_dbConnection);
                        command.ExecuteNonQuery();
                        Success("Active users dropped");
                        Console.WriteLine("Shutting database down");
                        m_dbConnection.Close();
                        Success("Database Down");
                        Console.WriteLine("Press any key to exit server...");
                        Console.ReadKey();
                        run = false;

                    }
                    catch (Exception e)
                    {
                        Error("There was an error: " + e);
                    }
                }

            }
            return run;
        }

        public static void networking()
        {
            while (run == true) {
                try
                {
                    Console.WriteLine("Someone can connect to the server again!");
                    Console.Write(">> ");
                    var tcpListener = new TcpListener(ipadress, port);
                    tcpListener.Start();
                    Socket socket = tcpListener.AcceptSocket();
                    Success("Someone has conencted! Their ip is: " + socket.RemoteEndPoint);
                    Console.WriteLine("Crating network stream with target.");
                    Console.Write(">> ");
                    var networkStream = new NetworkStream(socket);
                    Success("Network stream created");
                    Console.Write(">> ");
                    networkStream.Close();
                    tcpListener.Stop();
                }
                catch (Exception e)
                {
                    Error("There was an error: " + e);
                }
        }
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}