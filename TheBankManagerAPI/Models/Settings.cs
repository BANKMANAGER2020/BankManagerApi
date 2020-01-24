using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBankManagerAPI.Models
{
    public class Settings
    {
        public string getConnectionSettings()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Transactions"].ConnectionString;
            return connectionString;
        }
    }
}
