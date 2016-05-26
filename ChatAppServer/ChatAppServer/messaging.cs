using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using Mono.Data.Sqlite;

namespace ChatAppServer
{
    public class Messages
    {
        public static void SendMessage(StreamReader streamReader, StreamWriter streamWriter)
        {
            
            string input;
            string sessionID = "";
            input = streamReader.ReadLine();
            if(input == "new")
            {

                newSession( streamReader,streamWriter);
            }
            else if (input == "old")
            {
                sessionID = streamReader.ReadLine();
                input = streamReader.ReadLine();
		
                File.AppendAllText(Directory.GetCurrentDirectory()+@"/"+sessionID+".txt", input+Environment.NewLine);
            }
        }

        public static void newSession(StreamReader streamReader, StreamWriter streamWriter)
        {
            Random random = new Random();
            char temp;
            int runtime;
            string username;
            string username2;

            string sessionID = "";
            bool sessionIDUnique = false;
            while (sessionIDUnique != true)
            {
                runtime = random.Next(5, 60);
                for (int i = 0; i < runtime; i++)
                {
                    temp = Convert.ToChar(random.Next(65, 90));
                    sessionID += temp;
                }
                sessionIDUnique = checkSQL(sessionID);
            }
            username = streamReader.ReadLine();
            username2 = streamReader.ReadLine();
            registerSession(sessionID, username, username2);
            streamWriter.WriteLine("Your session id is: " + sessionID);
            streamWriter.Flush();
            Console.Write("New chat crated between: " + username + " and " + username2);
           
        }

        public static void GetMessage()
        {

        }

        public static void registerSession(string id, string username, string username2)
        {
            string command = "INSERT INTO activeSession (Sessionid,usernameOne, usernameTwo) VALUES (@id, @username, @username2);";
            SqliteCommand sqlcommand = new SqliteCommand(command, ChatAppServer.chatApp.m_dbConnection);
            sqlcommand.Parameters.AddWithValue("@id", id);
            sqlcommand.Parameters.AddWithValue("@username",username);
            sqlcommand.Parameters.AddWithValue("@username2", username2);
            sqlcommand.ExecuteNonQuery();
	    Console.WriteLine("The directory is: " + Directory.GetCurrentDirectory());
            File.Create(Directory.GetCurrentDirectory()+@"/"+id+".txt");

        }

        public static bool checkSQL(string data)
        {
            bool run = true;
            string command = "SELECT * FROM activeSession WHERE Sessionid = @sessionId";
            SqliteCommand sql = new SqliteCommand(command, ChatAppServer.chatApp.m_dbConnection);
            sql.Parameters.AddWithValue("@sessionId", data);
            SqliteDataReader reader = sql.ExecuteReader();
            while (reader.Read())
            {
                run = false;
            }

            return run;
        }
    }
}
