using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Data.SQLite;
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

                    sql = "CREATE TABLE users (id INT, username VARCHAR(100), password VARCHAR(1000));";//Create a new table in the database for users
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
                try
                {
                    m_dbConnection = new SQLiteConnection("Data Source=userData.sqlite;Version=3;");//Connect and open the database
                    m_dbConnection.Open();
                    Success("Database was opened!");
                }
                catch (Exception e)
                {
                    Error("There was an error: " + e);
                }

            }

            Console.WriteLine("Trying to create activeUsersTable!");
            try
            {
                sql = "CREATE TABLE ActiveUsers (id INT, username VARCHAR(1000), ip VARCHAR(12));";//Create a new table for active users
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
                        try
                        {
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
            while (run == true)
            {
                try
                {
                    Console.WriteLine("Someone can connect to the server again!");
                    Console.Write(">> ");
                    var tcpListener = new TcpListener(ipadress, port);
                    tcpListener.Start();
                    TcpClient client = tcpListener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(threadProc, client);

                }
                catch (Exception e)
                {
                    Error("There was an error: " + e);
                }

            }
        }

        public static void threadProc(object obj)
        {
            var client = (TcpClient)obj;
            Console.WriteLine("Crating network stream with target.");
            Console.Write(">> ");
            var networkStream = client.GetStream();
            Success("Network stream created");
            Console.Write(">> ");
            messaging.conected(networkStream);
            networkStream.Close();
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

/////

namespace ChatAppServer
{
    class messaging
    {
        public static string username;
        public static string ip;
        public static string id;
        public static string sender;
        public static string reciever;
        public static string message;
        public static string tiemstamp;
        public static NetworkStream networkstream;
        public static void conected(NetworkStream networkStream)
        {
            try
            {
                networkstream = networkStream;
                string message;
                var streamreader = new StreamReader(networkStream);
                var streamWriter = new StreamWriter(networkStream);
                message = streamreader.ReadLine().ToLower();
                if (message == "login")
                {
                    if (login(streamreader, streamWriter) != true)
                    {
                        streamWriter.WriteLine("Error incorret username or password.");
                        streamWriter.Flush();
                    }
                    else {
                        activeUsers();
                        understand(streamreader, streamWriter);
                    }
                }
                else if (message == "register")
                {
                    Console.WriteLine("Registering user");
                    register(streamreader, streamWriter);
                }
                else
                {
                    streamWriter.WriteLine("Error not valid command.");
                    streamWriter.Flush();
                }
            }
            catch (Exception e)
            {
                chatApp.Error(e);
            }
        }
        public static void activeUsers()
        {
            string sqlStatement = "INSERT INTO ActiveUsers (id, username, ip) VALUES (@id, @username, @ip)";
            SQLiteCommand command = new SQLiteCommand(sqlStatement, chatApp.m_dbConnection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@ip", ip);
            command.ExecuteNonQuery();
        }

        public static bool login(StreamReader streamReader, StreamWriter streamWriter)
        {
            string line = "l|l";
            bool login = false;
            string password = "l";
            username = streamReader.ReadLine();

            password = streamReader.ReadLine();
            string sqlStatement = "SELECT * FROM users WHERE username = @username AND password = @password;";
            SQLiteCommand command = new SQLiteCommand(sqlStatement, chatApp.m_dbConnection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                line = reader["username"] + "|" + reader["password"] + "|" + reader["id"];
            }
            string[] split = line.Split('|');
            if ((split[0] != "l" && split[1] != "l") && (split[0] != null && split[1] != null))
            {
                login = true;
                id = split[2];
                username = split[0];
            }
            return login;
        }

        public static void register(StreamReader streamreader, StreamWriter streamWriter)
        {
            string line = "l";
            int id = 0;
            Random random = new Random();
            username = streamreader.ReadLine();
            string password = streamreader.ReadLine();
            string sqlStatement = "SELECT * FROM users WHERE username=@username;";
            SQLiteCommand command = new SQLiteCommand(sqlStatement, chatApp.m_dbConnection);
            command.Parameters.AddWithValue("@username", username);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                line = Convert.ToString(reader["username"]);
            }
            if (line == username)
            {
                streamWriter.WriteLine("error username is taken");
                streamWriter.Flush();
                chatApp.Error("Some already has that name");
            }
            else
            {
                sqlStatement = "INSERT INTO users (id, username, password) values (@id, @username,@password);";
                id = random.Next(1, int.MaxValue / 2);
                command = new SQLiteCommand(sqlStatement, chatApp.m_dbConnection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
                streamWriter.Close();
                streamreader.Close();
                chatApp.Success("Someone has added the user: " + username);
            }

        }

        public static void understand(StreamReader streamReader, StreamWriter streamWriter)
        {
            Console.WriteLine(username + " has logged in, determining function...");
            string input = streamReader.ReadLine();
            if (input == "send")
            {
                Console.WriteLine(username + " has logged in to send a message");
                sendMessage(streamReader, streamWriter);
            }
            else if (input == "recieve")
            {
                Console.WriteLine(username + " has logged in to get their message('s)");
                getmessagesFromServer(streamReader, streamWriter);
            }
            else
            {
                streamWriter.WriteLine("Error, incorrect data entered");
                streamWriter.Flush();
                Console.WriteLine("Error, incorrect data entered");

            }
        }

        public static void getmessages()
        {
            var streamreader = new StreamReader(networkstream);
            var streamWriter = new StreamWriter(networkstream);
            getmessagesFromServer(streamreader, streamWriter);
            streamWriter.Close();
            streamreader.Close();
        }

        public static void getmessagesFromServer(StreamReader streamReader, StreamWriter streamWriter)
        {
            IPAddress ipadress = IPAddress.Parse(ip);
            TcpListener sendData = new TcpListener(ipadress, 4001);
            sendData.Start();
            Socket socket = sendData.AcceptSocket();
            streamWriter.WriteLine(sender);
            streamWriter.Flush();
            streamWriter.WriteLine(reciever);
            streamWriter.Flush();
            streamWriter.WriteLine(message);
            streamWriter.Flush();
            streamWriter.WriteLine(tiemstamp);
            streamWriter.Flush();
            socket.Close();
            sendData.Stop();
        }

        public static void sendMessage(StreamReader streamReader, StreamWriter streamWriter)
        {
            sender = streamReader.ReadLine();
            reciever = streamReader.ReadLine();
            message = streamReader.ReadLine();
            tiemstamp = streamReader.ReadLine();
            bool online = false;
            string sqlStatement = "SELECT * FROM ActiveUsers WHERE username=@username;";
            SQLiteCommand command = new SQLiteCommand(sqlStatement, chatApp.m_dbConnection);
            command.Parameters.AddWithValue("@username", reciever);
            command.Parameters.AddWithValue("@username", username);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                online = true;
                string ip = Convert.ToString(reader["ip"]);
            }

            if (online == true)
            {
                Thread thread = new Thread(new ThreadStart(getmessages));
                thread.Start();
            }
        }
    }
}
