# OEMANAGER MCP Server

This MCP Server supports basic interaction with an OpenEdge PASOE AppServer. The primary intent is to be able to trim the AppServer agent processes to ensure developers are testing the most recent code while developing.

## MCP Server Configuration

The MCP Server currently only supports the stdio protocol. The oemanager-mcp.exe supports two sets of startup parameters:

--pasoeUrl - The URL to the OEMANAGER, e.g. https://localhost:8811/oemanager 
--appName - The name of the ABL application to monitor, e.g. oepas1
--username - The username to use for authentication, e.g. tomcat 
--password - The password to use for authentication, e.g. tomcat

or 

--defaultConnection - The Label or applicationName of the default connection, e.g. oepas1
--configFile - The path to the configuration file with oemanager connections, see https://marketplace.visualstudio.com/items?itemName=ConsultingwerkApplicationModernizationSolutionsLtd.oemanager

When --configFile is not specified, we're defaulting to %AppData%\Consultingwerk\oemanager.conf

## Sample mcp_config.json

```
{
    "mcpServers": {
        "oemanmager-mcp": {
            "command": "C:\\Path\\oemanager-mcp\\bin\\Debug\\net8.0\\win-x64\\oemanager-mcp.exe",
            "args": [
                "--defaultConnection",
                "smartpas_stream"
            ],
            "disabledTools": [],
            "disabled": false
        }        
    }
}
```


## MCP Server Tool

Show configured connections

```
    [McpServerTool]
    [Description("Returns all configured AppServer or OEMANAGER connections (Label, Url, applicationName)")]
    public string ShowConnections()
```

Returns the status of the AppServer (PASOE) Agents

```
    [McpServerTool]
    [Description("Returns the status of the AppServer (PASOE) Agents")]
    public string ShowAppServerAgentStatus([Description("The Label of the connection to use (optional)")] string connectionName)
```

Terminate or Trim all AppServer (PASOE) Agents

```
    [McpServerTool]
    [Description("Terminates or Trims all AppServer (PASOE) Agents")]
    public string TrimAppServerAgents([Description("The Label of the connection to use (optional)")] string connectionName)
```
