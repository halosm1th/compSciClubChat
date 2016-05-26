using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Mono.Data.Sqlite;

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
            Thread thread = new Thread(reciveData);
            while (chatApp.run == true)
            {
                counter++;
                try
                {
                    if (!thread.IsAlive)
                    {
                        thread.Start();
                    }
                    
                    tcpClient = tcpListener.AcceptTcpClient();
                    
                    Console.WriteLine("test");
                    
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
        public static void reciveData()
        {
            int counter = 0;
            handleClient client = new handleClient();
            TcpListener tcpListener2 = new TcpListener(IPAddress.Any, 4001);
            TcpClient tcpClient2 = default(TcpClient);
            tcpListener2.Start();
            while (true) {
                counter++;
                tcpClient2 = tcpListener2.AcceptTcpClient();
                client.startClient(tcpClient2, counter);
                Console.WriteLine("someone connected @ " + ((IPEndPoint)tcpClient2.Client.RemoteEndPoint).Address.ToString());
                Console.Write(">> ");
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
                    
                    streamWriter.Flush();
                    if (login(streamReader, streamWriter) != true)
                    {
                        streamWriter.WriteLine("Error incorret username or password.");
                        streamWriter.Flush();
                    }
                    else {
                        activeUsers();

                        understand(streamReader, streamWriter);
                        streamWriter.Close();
                        streamReader.Close();
                        networkStream.Close();
                       
                    }

                }
                else if (input == "register")
                {
                    Console.WriteLine("Someone has chosen to register!");
                    Console.Write(">> ");
                    
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
            SqliteCommand command = new SqliteCommand(sqlStatement, chatApp.m_dbConnection);
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
                SqliteCommand command = new SqliteCommand(sqlStatement, chatApp.m_dbConnection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                SqliteDataReader reader = command.ExecuteReader();
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
                    Messages.SendMessage(streamReader, streamWriter);
                }
                else if (input == "recieve")
                {
                    Console.WriteLine(username + " logged in to get their message('s)");
                    Console.WriteLine(">> ");
                    Messages.GetMessage(streamReader, streamWriter);
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
            try {
                string line = "l";
                int id = 0;
                Random random = new Random();
                username = streamreader.ReadLine();
                Console.WriteLine("The new users name is: " + username);
                string password = streamreader.ReadLine();
                string sqlStatement = "SELECT * FROM users WHERE username=@username;";
                SqliteCommand command = new SqliteCommand(sqlStatement, chatApp.m_dbConnection);
                command.Parameters.AddWithValue("@username", username);
                SqliteDataReader reader = command.ExecuteReader();
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
                    sqlStatement = "SELECT * FROM users WHERE id=@id;";
                    line = "1";
                    id = random.Next(1000, int.MaxValue);
                    command = new SqliteCommand(sqlStatement, chatApp.m_dbConnection);
                    command.Parameters.AddWithValue("@id", id);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        try {
                            line = Convert.ToString(reader["id"]);
                        }catch(Exception e)
                        {
                            Console.WriteLine("There was an error on" + reader["id"] + " with id: " +e);
                        }
                    }
                    while (id == Convert.ToInt32(line))
                    {
                        id = random.Next(1000, int.MaxValue);
                        command = new SqliteCommand(sqlStatement, chatApp.m_dbConnection);
                        command.Parameters.AddWithValue("@id", id);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            line = Convert.ToString(reader["id"]);
                        }
                    }

                    sqlStatement = "INSERT INTO users (id, username, password) values (@id, @username,@password);";
                    command = new SqliteCommand(sqlStatement, chatApp.m_dbConnection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                    streamWriter.Close();
                    streamreader.Close();
                    otherStuff.Success("Someone has added the user: " + username + " with the id: " + id);
                    Console.WriteLine(">> ");
                }
            }catch(Exception e)
            {
                otherStuff.error(Convert.ToString (e));
            }
        }

    }
}
