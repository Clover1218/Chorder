using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Chorder.Clients.Storage {
    public class SQLServerConnectionFactory {

        public SqlConnection CreateConnection()
        {
            return new SqlConnection("Server=localhost;Database=MusicPlayerDB;User Id=sa;Password=123456;TrustServerCertificate=True;");
        }
    }

}
