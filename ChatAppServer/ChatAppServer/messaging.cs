using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using Mono.Data.Sqlite;
using System.Linq;

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

        public static void GetMessage(StreamReader streamReader,StreamWriter streamWriter)
        {
            string chatID;
            string userID;
            string timestamp;
            string message;
            string lastline;
            string[] temp;
            string[] file;
            string dir;
            string[] lostLines;
            int anotherint = 0;
            int tempInt = 0;
            chatID = streamReader.ReadLine();
            userID = streamReader.ReadLine();
            timestamp = streamReader.ReadLine();
            message = streamReader.ReadLine();
            streamWriter.Flush();
            dir  = Directory.GetCurrentDirectory() + @"/" + chatID + ".txt";
            if (!File.Exists(dir))
            {
                streamWriter.WriteLine("Error no file found!");
                streamWriter.Flush();
            }
            else
            {
                lastline = File.ReadLines(dir).Last();
                temp = lastline.Split('|');
                if (temp[2] == timestamp)
                {
                    streamWriter.WriteLine("No new messages");
                    streamWriter.Flush();
                }
                else
                {
                    StreamReader fileReader = new StreamReader(dir);//new file reader
                    file = new string[File.ReadLines(dir).Count()];//Create an array for the file.
                    otherStuff.Success("I made it to the first check!");
                    for (int i = 0; i < file.Length; i++)//load the file into memory
                    {
                        file[i] = fileReader.ReadLine();
                    }
                    fileReader.Close();//stop reading the flie
                    otherStuff.Success("I made it to the second check!");
                    while (file[tempInt] != message)//get the linenumber that we loose messages on.
                    {
                        tempInt++;
                    }
                    lostLines = new string[file.Length - tempInt];
                    otherStuff.Success("I made it to the third check!");
                     for(int i = tempInt; i < file.Length; i++){//load the unused lines into ram
                         otherStuff.Success("loaded: " + i);
                         lostLines[anotherint] = file[i];
                     }
                    streamWriter.WriteLine(lostLines.Length);
                    streamWriter.Flush();
                    Console.WriteLine(lostLines.Length);
                    otherStuff.Success("I made it to the fourth check!");
                     for (int i = 0; i < lostLines.Length; i++)
                     {
                         otherStuff.Success("Sent: " + i);
                         streamWriter.WriteLine(lostLines[i]);
                         streamWriter.Flush();
                     }
                }
            }
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
