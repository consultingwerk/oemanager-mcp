@echo off
rem npx @modelcontextprotocol/inspector dotnet run --project smartframeworkmcpserver --pasoeUrl https://sfrbo.consultingwerkcloud.com:8821/apsv

npx @modelcontextprotocol/inspector bin\Debug\net8.0\win-x64\oemanager-mcp.exe --pasoeUrl https://sfrbo.consultingwerkcloud.com:8821 --appName smartpas_stream --username tomcat --password tomcat

