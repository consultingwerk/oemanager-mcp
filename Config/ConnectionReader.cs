using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
 
namespace Config
{
    public static class ConnectionReader
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static List<OeManagerConnection> ReadConnections(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path must not be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Configuration file not found.", filePath);
            }

            var json = File.ReadAllText(filePath);

            var connections = JsonSerializer.Deserialize<List<OeManagerConnection>>(json, Options);

            if (connections != null)
            {
                foreach (var connection in connections)
                {
                    if (string.IsNullOrWhiteSpace(connection.Label))
                    {
                        connection.Label = connection.ApplicationName;
                    }

                    if (string.IsNullOrWhiteSpace(connection.Password))
                    {
                        connection.Password = connection.Username;
                    }
                }
            }

            return connections ?? [];
        }
    }
}