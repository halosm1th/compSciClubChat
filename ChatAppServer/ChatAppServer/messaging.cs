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
        public static string LASTLINE;
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
                    if (LASTLINE != message)
                    {
                        streamWriter.WriteLine("new messages");
                        streamWriter.Flush();
                        StreamReader fileReader = new StreamReader(dir);//new file reader
                        file = new string[File.ReadLines(dir).Count()];//Create an array for the file.
                        for (int i = 0; i < file.Length; i++)//load the file into memory
                        {
                            file[i] = fileReader.ReadLine();
                        }
                        fileReader.Close();//stop reading the flie
                        while (file[tempInt] != message)//get the linenumber that we loose messages on.
                        {
                            tempInt++;
                            if(tempInt == file.Length)
                            {
                                tempInt--;
                                file[tempInt] = message;
                            }
                        }
                        lostLines = new string[file.Length - tempInt];
                        for (int i = tempInt; i < file.Length; i++)
                        {//load the unused lines into ram
                            lostLines[anotherint] = file[i];
                            anotherint++;
                        }
                        streamWriter.WriteLine(lostLines.Length);
                        streamWriter.Flush();
                        Console.WriteLine(lostLines.Length);
                        for (int i = 0; i < lostLines.Length; i++)
                        {
                            streamWriter.WriteLine(lostLines[i]);
                            streamWriter.Flush();
                        }
                        LASTLINE = message;

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
            File.WriteAllText(Directory.GetCurrentDirectory()+@"/"+id+".txt","");

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

        public static void checkMessages(StreamReader streamReader, StreamWriter streamWriter, string username)
        {
            string[] chatsTemp = new string[1000];
            string[] chat;
            int i = 0;
            string command = "SELECT * FROM activeSession WHERE usernameOne = @username OR usernameTwo = @username";
            SqliteCommand sqlitecommand = new SqliteCommand(command, chatApp.m_dbConnection);
            sqlitecommand.Parameters.AddWithValue("@username", username);
            SqliteDataReader reader = sqlitecommand.ExecuteReader();
            while (reader.Read())
            {
                chatsTemp[i] = Convert.ToString (reader["Sessionid"] + "|"+reader["usernameOne"]+"|"+reader["usernameTwo"]);
                i++;
            }
            chat = new string[chatsTemp.Count(x => x != null)];
            for(int b = 0; b  < chat.Length; b++)
            {
                chat[b] = chatsTemp[b];
            }

            streamWriter.WriteLine(chat.Length);
            streamWriter.Flush();
            for (int b = 0; b < chat.Length; b++)
            {
                streamWriter.WriteLine(chat[b]);
                streamWriter.Flush();
            }
        }
    }
}
