using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Data.SQLite;
using System.Threading;

namespace ChatAppServer
{
    class messaging
    {
        public static string username;

        public static string id;

        public static void conected(NetworkStream networkStream)
        {
            string message;
            var streamreader = new StreamReader(networkStream);
            var streamWriter = new StreamWriter(networkStream);
            message = streamreader.ReadLine();
            if (message == "login")
            {
                if (login(streamreader, streamWriter) != true)
                {
                    streamWriter.WriteLine("Error incorret username or password.");
                }
                understand(networkStream);
            }
            else if (message == "register")
            {
                register(streamreader, streamWriter);
            }
            else
            {
                streamWriter.WriteLine("Error not valid command.");
            }
        }

        public static bool login(StreamReader streamreader,StreamWriter streamWriter)
        {
            string line = "l|l";
            bool login = false;
            string password;
            username = streamreader.ReadLine();
            password = streamreader.ReadLine();
            string sqlStatement = "SELECT * FROM users WHERE username=@username AND password=@password;";
            SQLiteCommand command = new SQLiteCommand(sqlStatement, chatApp.m_dbConnection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                line = reader["username"] + "|" + reader["password"] + "|"+reader["id"];
            }
            string[] split = line.Split('|');
            if (split[0] != "l" && split[1] != "l")
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
                line = Convert.ToString (reader["username"]);
            }
            if (line == username)
            {
                streamWriter.WriteLine("error username is taken");
            }
            else
            {
                sqlStatement = "INSTERT INTO users (id, username, password) values (@username,@password,@id);";
                id = random.Next(1, int.MaxValue);
                command = new SQLiteCommand(sqlStatement, chatApp.m_dbConnection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();

            }
            
        }

        public static void understand(NetworkStream networkStream)
        {

        }

        public static void getmessages(NetworkStream networkStream)
        {
            var streamreader = new StreamReader(networkStream);
            var streamWriter = new StreamWriter(networkStream);

        }

        public static void sendMessage(NetworkStream networkStream)
        {

        }
    }
}
