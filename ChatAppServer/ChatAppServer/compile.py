import os
os.system('echo compiling! To run the server. type go to Debug and type MonochatAppServer.exe')
os.system('dmcs -r:System.Data.dll -r:Mono.Data.Sqlite *.cs -out:bin/Debug/ChatAppServer.exe')
