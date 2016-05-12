using System;
using Mono.Data.Sqlite;
using System.Threading;

namespace ChatAppServer
{
    class controlConsole
    {
        public static bool ControlConsole()
        {
            Thread thread = new Thread(new ThreadStart(networking.Networking));
            thread.Start();
            string input;
            string sql;
            SqliteCommand command;
            while (chatApp.run == true)
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
                        otherStuff.Success("Messaging down.");
                        Console.WriteLine("Deleting active users");
                        sql = "DROP TABLE ActiveUsers;";
                        command = new SqliteCommand(sql, chatApp.m_dbConnection);
                        command.ExecuteNonQuery();
                        otherStuff.Success("Active users dropped");
                        Console.WriteLine("Shutting database down");
                        chatApp.m_dbConnection.Close();
                        otherStuff.Success("Database Down");
                        Console.WriteLine("Press any key to exit server...");
                        Console.ReadKey();
                        chatApp.run = false;

                    }
                    catch (Exception e)
                    {
                        otherStuff.error("There was an error: " + e);
                    }
                }

            }
            return chatApp.run;
        }
    }
}
