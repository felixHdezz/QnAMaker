using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.QnABot
{
	public class DbConnection
	{
		public static SqlConnection _conn = null;
		public DbConnection() {

			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
			builder.DataSource = "tcp:sc01sqlsrvd02.database.windows.net";
			builder.InitialCatalog = "TecnologiaDigitalDB";
			builder.PersistSecurityInfo = false;
			builder.UserID = "td_admin";
			builder.Password = "Arcacontal1!";
			builder.MultipleActiveResultSets = false;
			builder.Encrypt = true;
			builder.TrustServerCertificate = false;
			builder.ConnectTimeout = 30;

			_conn = new SqlConnection(builder.ConnectionString);
			_conn.Open();
		}

		public SqlDataReader executeQuery(string query)
		{
			using (SqlCommand command = new SqlCommand(query, _conn))
			{
				return command.ExecuteReader();
			}
		}

		public int execQuery(string _strQuery)
		{
			using (SqlCommand _cmd = new SqlCommand(_strQuery, _conn))
			{
				return _cmd.ExecuteNonQuery();
			}
		}
	}
}