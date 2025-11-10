using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

internal class OeManagerTools
{
    [McpServerTool]
    [Description("Returns the status of the AppServer (PASOE) Agents")]
    public string ShowAppServerAgentStatus()
    {
        return this.ShowAppServerAgentStatusAsync(Configuration.PasoeUrl, Configuration.AppName, Configuration.Username, Configuration.Password).Result;
    }

    protected async Task<string> ShowAppServerAgentStatusAsync(string baseUrl, string appName, string username, string password)
    {
        using var httpClient = new HttpClient();
        
        // Set up basic authentication with username "tomcat" and password "tomcat"
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        
        // Construct the endpoint URL
        var endpoint = $"{baseUrl}/oemanager/applications/{appName}/agents";
        
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
            return $"Error calling API: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Unexpected error: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Terminates all AppServer (PASOE) Agents")]
    public string TrimAppServerAgents()
    {
        return this.TrimAppServerAgentsAsync(Configuration.PasoeUrl, Configuration.AppName, Configuration.Username, Configuration.Password).Result;
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
            var getEndpoint = $"{baseUrl}/oemanager/applications/{appName}/agents";
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
                        var deleteEndpoint = $"{baseUrl}/oemanager/applications/{appName}/agents/{agentId}?waitToFinish=120000&waitAfterStop=60000";
                        
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