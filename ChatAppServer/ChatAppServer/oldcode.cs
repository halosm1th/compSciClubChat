using System;
//These 3 imports are for networking
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Data.SQLite;
/*
 *This is the main part of the chat program. It is where we start the application and get the basics of the network set up. 
 */

namespace ChatAppServer
{
    class Program
    {
        public static int port;//The port on which the client connects
        public static string adress = "0.0.0.0";//allow any ipadress to connect
        public static IPAddress ipadress = IPAddress.Parse(adress);
        public static string sender;
        public static string reciver;
        public static string message;


        /*
         * this is the intro method for the program, we set up some basic information, and then head over to connect.
         */


        static void Main(string[] args)
        {
            SQLiteConnection m_dbConnection;
            SQLiteConnection m_dbConnection2;

            Console.WriteLine("Checking if database exists");
            if (!File.Exists("users.sqlite"))//Check to see if the database exists
            {
                Console.WriteLine("Error, no database found, creating a new one.");
                SQLiteConnection.CreateFile("users.sqlite");//Create the user database

                m_dbConnection = new SQLiteConnection("Data Source=users.sqlite;Version=3;");
                m_dbConnection.Open();//Connect to said database

                string sql = "CREATE TABLE users (id INT, username VARCHAR(100), password VARCHAR(100));";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();//Create the table for it

                Console.WriteLine("Database created");

                Console.WriteLine("Creating active users database");//Crate active users
                SQLiteConnection.CreateFile("activeUsers.sqlite");

                m_dbConnection2 = new SQLiteConnection("Data Source=activeUsers.sqlite;Version=3;");//Connect to it
                m_dbConnection2.Open();

                sql = "CREATE TABLE users (id INT, username VARCHAR(100), ip VARCHAR(12));";//Create a table for it
                command = new SQLiteCommand(sql, m_dbConnection2);
                command.ExecuteNonQuery();


            }
            else
            {
                Console.WriteLine("Database found.");//Open hte database
                m_dbConnection = new SQLiteConnection("Data Source=users.sqlite;Version=3;");
                m_dbConnection.Open();

                m_dbConnection2 = new SQLiteConnection("Data Source=activeUsers.sqlite;Version=3;");
                m_dbConnection2.Open();
            }
            Console.Write("Enter a port to start on: ");//Get the port to run on
            port = Convert.ToInt32(Console.ReadLine());
            while (true)//run forver
            {
                connect(m_dbConnection, m_dbConnection2);
            }
        }

        /*
         * This is used when ever someone connects to the server
         */
        public static void connect(SQLiteConnection db1, SQLiteConnection db2)
        {
            try//Simple try catch block
            {
                var tcpLister = new TcpListener(ipadress, port);//Create a listen for connections
                tcpLister.Start();//Start the listener
                Socket socket = tcpLister.AcceptSocket();//Create anew socket when the listen is connected to
                Console.WriteLine("Connection started");//Write that we have a connection

                var networkStream = new NetworkStream(socket);//The network stream
                string ip = Convert.ToString(socket.RemoteEndPoint);//get the ipadress

                getData(networkStream, ip, db1, db2);//Get and send data

                networkStream.Close();//Close the network stream
                tcpLister.Stop();//Close the listener
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                Console.Clear();
            }
        }


        //this method collects data from the network
        public static void getData(NetworkStream networkStream, string ip, SQLiteConnection db1, SQLiteConnection db2)
        {
            var streamReader = new StreamReader(networkStream);
            var streamWriter = new StreamWriter(networkStream);
            string line = "l|l";
            writeData("it worked!", streamWriter);
            writeData("1", streamWriter);
            string message = streamReader.ReadLine();
            if (message == "1")
            {
                writeData("username: ", streamWriter);
                string username = streamReader.ReadLine();
                writeData("password: ", streamWriter);
                string password = streamReader.ReadLine();
                string sqlCommand = ("SELECT * FROM users WHERE username ='" + username + "' and password ='" + password + "';");
                SQLiteCommand command = new SQLiteCommand(sqlCommand, db1);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    line = reader["username"] + "|" + reader["password"];
                }
                string[] split = line.Split('|');
                if (split[0] != "l" && split[1] != "l")
                {
                    string type = streamReader.ReadLine();

                    streamWriter.WriteLine("0");


                    if (type == "1")
                    {
                        getMessage(streamReader);
                    }
                    else if (type == "2")
                    {
                        sendMessage(streamWriter);
                    }

                }
                else
                {
                    Console.WriteLine("Someone tried to log in as " + username + " But failed!");
                }
            }
            else if (message == "2")
            {
                writeData("0", streamWriter);
                Console.WriteLine("Creating new user");
                Random random = new Random();
                writeData("username: ", streamWriter);
                string username = streamReader.ReadLine();
                Console.WriteLine("Username: " + username);
                writeData("password: ", streamWriter);
                string password = streamReader.ReadLine();
                Console.WriteLine("password: " + password);
                int id = random.Next(0, int.MaxValue / 4);
                Console.WriteLine("ID: " + id);
                string sqlcommand = ("INSERT INTO users (id, username, password) values (" + id + ", '" + username + "','" + password + "');");
                SQLiteCommand command = new SQLiteCommand(sqlcommand, db1);
                command.ExecuteNonQuery();

                Console.WriteLine("insterted data");
            }

            streamReader.Close();
            streamWriter.Close();

        }

        //gets message from client
        public static void getMessage(StreamReader streamReader)
        {

            sender = streamReader.ReadLine();
            reciver = streamReader.ReadLine();
            message = streamReader.ReadLine();
            Console.WriteLine(sender + " sent a message to: " + reciver + " at" + DateTime.Now);

        }

        //sends message to client
        public static void sendMessage(StreamWriter streamWriter)
        {
        }

        public static void writeData(string message, StreamWriter streamWriter)
        {
            streamWriter.WriteLine(message);
            streamWriter.Flush();
        }
    }
}

