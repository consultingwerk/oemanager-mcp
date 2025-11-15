using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Config;

var builder = Host.CreateApplicationBuilder(args);

var pasoeUrl = builder.Configuration["pasoeUrl"];
List<OeManagerConnection> connections = [];
string? defaultConnection = "";

if (string.IsNullOrWhiteSpace(pasoeUrl))
{
    var configFile = builder.Configuration["configFile"] ?? string.Format("{0}\\Consultingwerk\\oemanager.conf", 
                                                                          Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    
    if (File.Exists(configFile))
    {
        connections = ConnectionReader.ReadConnections(configFile);
    }
    else
    {
        throw new FileNotFoundException("Configuration file not found.", configFile);
    }

    if (connections.Count == 0)
    {
        throw new Exception("No connections found in the configuration file.");
    }

    defaultConnection = builder.Configuration["defaultConnection"] ?? connections[0].Label;

    if (string.IsNullOrWhiteSpace(defaultConnection))
    {
        throw new Exception("No default connection specified.");
    }
}
else 
{
    var appName = builder.Configuration["appName"] ?? "oepas1";
    var username = builder.Configuration["username"] ?? "tomcat";
    var password = builder.Configuration["password"] ?? "tomcat";

    connections.Add(new OeManagerConnection
    {
        Url = pasoeUrl,
        ApplicationName = appName,
        Label = appName,
        Username = username,
        Password = password
    });

    defaultConnection = appName;
}

Configuration.Connections = connections;
Configuration.DefaultConnection = defaultConnection;    

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<OeManagerTools>();

await builder.Build().RunAsync();
