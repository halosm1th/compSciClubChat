using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Data.SQLite;

namespace ChatAppServer
{
    class networking
    {
        public static void Networking()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 4000);
            TcpClient tcpClient = default(TcpClient);
            tcpListener.Start();
            int counter = 0;
            while (chatApp.run == true)
            {
                counter++;
                try
                {
                    tcpClient = tcpListener.AcceptTcpClient();
                    handleClient client = new handleClient();
                    Console.WriteLine("someone connected @ " + ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString());
                    Console.Write(">> ");
                    client.startClient(tcpClient, counter);
                }
                catch (Exception e)
                {
                    otherStuff.error("There was an error: " + e);
                }
            }
        }
    }

    public class handleClient{
        TcpClient client;
        public static string username;
        public static string id;
        private static string ip;
        public void startClient(TcpClient clientSocket, int counter)
        {
            Console.WriteLine("Starting new thread! Client number: " + counter);
            Console.Write(">> ");
            
            this.client = clientSocket;
            ip = ((IPEndPoint)this.client.Client.RemoteEndPoint).Address.ToString();
            Thread chatThread = new Thread(networkStuff);
            chatThread.Start();
        }
        public void networkStuff()
        {
            try {
                var networkStream = client.GetStream();
                var streamReader = new StreamReader(networkStream);
                var streamWriter = new StreamWriter(networkStream);
                string input = streamReader.ReadLine();
                if (input == "login")
                {
                    Console.WriteLine("Someone has chosen to log in");
                    Console.Write(">> ");
                    streamWriter.WriteLine("You have chosen login!");
                    streamWriter.Flush();
                    if (login(streamReader, streamWriter) != true)
                    {
                        streamWriter.WriteLine("Error incorret username or password.");
                        streamWriter.Flush();
                    }
                    else {
                        activeUsers();
                        understand(streamReader, streamWriter);
                    }

                }
                else if (input == "register")
                {
                    Console.WriteLine("Someone has chosen to register!");
                    Console.Write(">> ");
                    streamWriter.WriteLine("You have chosen Register!");
                    streamWriter.Flush();
                    register(streamReader, streamWriter);
                }
                else
                {
                    streamWriter.WriteLine("error, that is not valid!");
                    streamWriter.Flush();
                }
            }
            catch(Exception e)
            {
                otherStuff.error("There was an error: " + e);
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
            bool loginPerson = false;
            try {
                string line = "l|l";
                
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
                    loginPerson = true;
                    id = split[2];
                    username = split[0];
                }
                else
                {
                    otherStuff.error("The user failed to login");
                }
            }

            catch(Exception e)
            {
                otherStuff.error("There was an error:" + e);
            }
            return loginPerson;
        }

        public static void understand(StreamReader streamReader, StreamWriter streamWriter)
        {
            Console.WriteLine(username + " has logged in, determining function...");
            Console.Write(">> ");
            string input = streamReader.ReadLine();
            if (input == "send")
            {
                Console.WriteLine(username + " is sending a message");
                Console.Write(">> ");
            }
            else if (input == "recieve")
            {
                Console.WriteLine(username + " logged in to get their message('s)");
                Console.WriteLine(">> ");
            }
            else
            {
                streamWriter.WriteLine("Error, incorrect data entered");
                streamWriter.Flush();
                Console.WriteLine("Error, incorrect data entered");
                Console.Write(">> ");
            }
        }

        public static void register(StreamReader streamreader, StreamWriter streamWriter)
        {
            string line = "l";
            int id = 0;
            Random random = new Random();
            username = streamreader.ReadLine();
            Console.WriteLine("The new users name is: " + username);
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
                otherStuff.error("Some already has that name");
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
                otherStuff.Success("Someone has added the user: " + username);
            }

        }

    }
}
