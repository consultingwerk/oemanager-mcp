using System.Text.Json.Serialization;
 
namespace Config
{
    public class OeManagerConnection
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("applicationname")]
        public string ApplicationName { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("pingurl")]
        public string PingUrl { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string? Label { get; set; }
    }
}
