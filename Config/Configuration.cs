using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace Config
{
    public class Configuration
    {
        static public string DefaultConnection = "";
        static public List<OeManagerConnection> Connections = [];

        static public OeManagerConnection GetConnection(string connectionName)
        {
            if (string.IsNullOrEmpty(connectionName))
            {
                connectionName = DefaultConnection;
            }

            return Connections.FirstOrDefault(c => c.Label == connectionName) ?? Connections[0];
        }
    }
}
