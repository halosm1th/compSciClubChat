import os

os.system('dmcs -r:System.Data.dll -r:Mono.Data.Sqlite *.cs -out:bin/Debug/ChatAppServer.exe')
