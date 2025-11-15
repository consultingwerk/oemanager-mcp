using Config;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

#pragma warning disable CA1822

internal class OeManagerTools
{
    [McpServerTool]
    [Description("Returns all configured AppServer or OEMANAGER connections (Label, Url, applicationName)")]
    public string ShowConnections()
    {
        var items = new List<object>();

        foreach (var connection in Configuration.Connections)
        {
            items.Add(new
            {
                connection.Label,
                connection.Url,
                connection.ApplicationName
                
            });
        }

        return JsonSerializer.Serialize(items);
    }

    [McpServerTool]
    [Description("Returns the status of the AppServer (PASOE) Agents")]
    public string ShowAppServerAgentStatus([Description("The Label of the connection to use (optional)")] string connectionName)
    {
        var connection = Configuration.GetConnection(connectionName);
        return this.ShowAppServerAgentStatusAsync(connection.Url, connection.ApplicationName, connection.Username, connection.Password).Result;
    }

    protected async Task<string> ShowAppServerAgentStatusAsync(string baseUrl, string appName, string username, string password)
    {
        using var httpClient = new HttpClient();
        
        // Set up basic authentication with username "tomcat" and password "tomcat"
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        
        // Construct the endpoint URL
        var endpoint = $"{baseUrl}/applications/{appName}/agents";
        
        try
        {
            // Make the GET request
            var response = await httpClient.GetAsync(endpoint);
            
            // Ensure the request was successful
            response.EnsureSuccessStatusCode();
            
            // Read and return the response content
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        catch (HttpRequestException ex)
        {
            return $"Error calling API {endpoint}: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error calling API {endpoint}: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Terminates or Trims all AppServer (PASOE) Agents")]
    public string TrimAppServerAgents([Description("The Label of the connection to use (optional)")] string connectionName)
    {
        var connection = Configuration.GetConnection(connectionName);
        return this.TrimAppServerAgentsAsync(connection.Url, connection.ApplicationName, connection.Username, connection.Password).Result;
    }

    protected async Task<string> TrimAppServerAgentsAsync(string baseUrl, string appName, string username, string password)
    {
        using var httpClient = new HttpClient();
        
        // Set up basic authentication
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        
        var results = new List<string>();
        
        try
        {
            // Step 1: Get all agents
            var getEndpoint = $"{baseUrl}/applications/{appName}/agents";
            var getResponse = await httpClient.GetAsync(getEndpoint);
            getResponse.EnsureSuccessStatusCode();
            
            var agentsJson = await getResponse.Content.ReadAsStringAsync();
            results.Add($"Retrieved agents: {agentsJson}");
            
            // Parse the JSON response to extract agent IDs
            using var jsonDoc = JsonDocument.Parse(agentsJson);
            var root = jsonDoc.RootElement;
            
            // Check if the response has an "AgentManager" property with an "agents" array
            if (root.TryGetProperty("result", out var agentManager) &&
                agentManager.TryGetProperty("agents", out var agentsArray))
            {
                // Step 2: Iterate through each agent and terminate it
                foreach (var agent in agentsArray.EnumerateArray())
                {
                    if (agent.TryGetProperty("agentId", out var agentIdElement))
                    {
                        var agentId = agentIdElement.GetString();
                        
                        // Delete the agent with default wait parameters
                        var deleteEndpoint = $"{baseUrl}/applications/{appName}/agents/{agentId}?waitToFinish=120000&waitAfterStop=60000";
                        
                        try
                        {
                            var deleteResponse = await httpClient.DeleteAsync(deleteEndpoint);
                            deleteResponse.EnsureSuccessStatusCode();
                            results.Add($"Successfully terminated agent: {agentId}");
                        }
                        catch (HttpRequestException ex)
                        {
                            results.Add($"Failed to terminate agent {agentId}: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                results.Add("No agents found in the response or unexpected JSON structure");
            }
            
            return string.Join("\n", results);
        }
        catch (HttpRequestException ex)
        {
            return $"Error calling API: {ex.Message}";
        }
        catch (JsonException ex)
        {
            return $"Error parsing JSON response: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Unexpected error: {ex.Message}";
        }
    }

}