fx_version 'bodacious'
game 'gta5'

author 'You'
version '1.0.0'

fxdk_watch_command 'dotnet' {'watch', '--project', 'Client/DatasetGenerator.Client.csproj', 'publish', '--configuration', 'Release'}
fxdk_watch_command 'dotnet' {'watch', '--project', 'Server/DatasetGenerator.Server.csproj', 'publish', '--configuration', 'Release'}

file 'Client/bin/Release/**/publish/*.dll'

client_script 'Client/bin/Release/**/publish/*.net.dll'
server_scripts{'Server/bin/Release/**/publish/*.net.dll', 'Server/ServerSaveScreenshot.lua'} 
