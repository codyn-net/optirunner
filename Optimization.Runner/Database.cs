using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;

namespace Optimization.Runner
{
	public class Database
	{
		private SqliteConnection d_connection;
		public delegate bool RowCallback(IDataReader reader);
		private string d_filename;

		public Database(string filename)
		{
			d_connection = new SqliteConnection("URI=file:" + filename + ",version=3,busy_timeout=15000");
			d_connection.Open();
			d_filename = filename;
		}
		
		public string Filename
		{
			get
			{
				return d_filename;
			}
		}
		
		public string[] ColumnNames(string table)
		{
			List<string> ret = new List<string>();

			Query("PRAGMA table_info(`" + table + "`)", delegate (IDataReader reader) {
				ret.Add((string)reader[1]);
				return true;
			});
			
			return ret.ToArray();
		}
		
		public bool Query(string s, RowCallback cb, params object[] parameters)
		{
			IDbCommand cmd = d_connection.CreateCommand();
			cmd.CommandText = s;

			for (int idx = 0; idx < parameters.Length; ++idx)
			{
				IDbDataParameter par = cmd.CreateParameter();
				par.ParameterName = "@" + idx;
				par.Value = parameters[idx];

				cmd.Parameters.Add(par);
			}

			bool ret = false;

			if (cb == null)
			{
				ret = cmd.ExecuteNonQuery() > 0;
			}
			else
			{
				IDataReader reader;

				reader = cmd.ExecuteReader();

				while (reader != null && reader.Read())
				{
					ret = cb(reader);

					if (!ret)
					{
						break;
					}
				}

				if (reader != null)
				{
					reader.Close();
					reader = null;
				}
			}

			cmd.Dispose();
			cmd = null;

			return ret;
		}

		public object[] QueryFirst(string s, params object[] parameters)
		{
			object[] ret = null;

			Query(s, delegate (IDataReader reader) {
				ret = new object[reader.FieldCount];

				reader.GetValues(ret);
				return false;
			}, parameters);

			return ret;
		}

		public object QueryValue(string s, params object[] parameters)
		{
			object ret = null;

			Query(s, delegate (IDataReader reader) {
				ret = reader.GetValue(0);
				return false;
			}, parameters);

			return ret;
		}

		public bool Query(string s, params object[] parameters)
		{
			return Query(s, null, parameters);
		}
		
		public void Close()
		{
			d_connection.Close();
		}
	}
}

