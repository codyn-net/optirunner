using System;
using System.IO;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Text;

namespace Optimization.Runner
{
	public class InitialPopulation
	{
		class DbSpec
		{
			public string Filename;
			public int Iteration;
			public int Solution;
			
			public DbSpec(string spec)
			{
				string[] parts = spec.Split(':');
				
				Filename = parts[0];
				Iteration = -1;
				Solution = -1;
				
				if (parts.Length > 1)
				{
					Iteration = int.Parse(parts[1]);
				}
				
				if (parts.Length > 2)
				{
					Solution = int.Parse(parts[2]);
				}
			}
		};

		private string[] d_args;
		private Database d_database;
		private Dictionary<string, Database> d_openDatabases;
		private string d_parameterQueryColumns;
		private string d_dataQueryColumns;
		private List<string> d_parameterColumns;
		private List<string> d_dataColumns;
		private uint d_nbest;

		public InitialPopulation(string[] args)
		{
			d_args = args;
			d_openDatabases = new Dictionary<string, Database>();
			d_parameterColumns = new List<string>();
			d_dataColumns = new List<string>();
		}
		
		public Database OpenDatabase(string filename)
		{
			Database ret;
			
			filename = Path.GetFullPath(filename);

			if (!d_openDatabases.TryGetValue(filename, out ret))
			{
				ret = new Database(filename);

				ret.Query("PRAGMA synchronous = OFF;");
				d_openDatabases[filename] = ret;
			}
			
			return ret;
		}
		
		public bool Generate(string outfile, uint nbest)
		{
			d_nbest = nbest;

			if (File.Exists(outfile))
			{
				System.Console.Error.WriteLine("The file `{0}' already exists...", outfile);
				Environment.Exit(1);
			}

			if (d_args.Length == 0)
			{
				System.Console.Error.WriteLine("Please provide input databases");
				Environment.Exit(1);
			}
			
			// Determine parameters from first db
			DbSpec firstSpec = new DbSpec(d_args[0]);
			Database db = OpenDatabase(firstSpec.Filename);

			StringBuilder parameterQuery = new StringBuilder();
			StringBuilder dbcreate = new StringBuilder();

			db.Query("SELECT `name` FROM parameters", delegate (IDataReader reader) {
				string name = (string)reader[0];
				
				d_parameterColumns.Add(name);
				
				if (!String.IsNullOrEmpty(parameterQuery.ToString()))
				{
					parameterQuery.Append(", ");
					dbcreate.Append(", ");
				}

				parameterQuery.AppendFormat("`_p_{0}`", name);
				dbcreate.AppendFormat("`_p_{0}` DOUBLE", name);
				return true;
			});
			
			d_parameterQueryColumns = parameterQuery.ToString();
			
			string[] data = db.ColumnNames("data");
			
			StringBuilder dataQuery = new StringBuilder();
			StringBuilder ddcreate = new StringBuilder();
			
			foreach (string column in data)
			{
				if (!column.StartsWith("_d_"))
				{
					continue;
				}

				if (!String.IsNullOrEmpty(dataQuery.ToString()))
				{
					dataQuery.Append(", ");
					ddcreate.Append(", ");
				}
				
				d_dataColumns.Add(column.Substring(3));
				
				dataQuery.AppendFormat("`{0}`", column);
				ddcreate.AppendFormat("`{0}` TEXT", column);
			}
			
			d_dataQueryColumns = dataQuery.ToString();
			
			d_database = new Database(outfile);
			d_database.Query("CREATE TABLE `initial_population` (`id` INTEGER PRIMARY KEY, " + dbcreate.ToString() + ")");
			d_database.Query("CREATE TABLE `initial_population_data` (`id` INTEGER PRIMARY KEY, " + ddcreate.ToString() + ")");
			
			bool ret = true;

			for (int i = 0; i < d_args.Length; ++i)
			{
				DbSpec spec = new DbSpec(d_args[i]);

				db = OpenDatabase(spec.Filename);
				
				if (!SimilarDatabase(db))
				{
					System.Console.Error.WriteLine("Not all databases have the same parameters");
					ret = false;
					break;
				}

				AddPopulation(db, spec);
			}
			
			CloseAll();
			d_database.Close();
			
			if (!ret)
			{
				File.Delete(d_database.Filename);
			}
			else
			{
				System.Console.WriteLine("Written initial population to `{0}'", outfile);
			}
			
			return ret;
		}

		private List<string> ParameterNames(Database db)
		{
			List<string> ret = new List<string>();

			db.Query("SELECT name FROM parameters", (reader) => {
				ret.Add((string)reader.GetString(0));
				return true;
			});

			return ret;
		}
		
		private bool SimilarDatabase(Database db)
		{
			List<string> pars = new List<string>(ParameterNames(db));
			List<string> orig = new List<string>(d_parameterColumns);

			pars.RemoveAll(delegate (string a) {
				return orig.Remove(a);
			});
			
			return pars.Count == 0 && orig.Count == 0;
		}
		
		private void CloseAll()
		{
			foreach (KeyValuePair<string, Database> pair in d_openDatabases)
			{
				pair.Value.Close();
			}
			
			d_openDatabases.Clear();
		}
		
		private void AddPopulation(Database db, DbSpec spec)
		{
			List<int> iterations = new List<int>();
			List<int> solutions = new List<int>();

			bool stagepso = false;
			bool pso = false;

			db.Query("SELECT name from extensions", (reader) => {
				string name = (string)reader.GetString(0);

				if (name == "StagePSO")
				{
					stagepso = true;
					return false;
				}

				return true;
			});

			db.Query("SELECT name from job", (reader) => {
				string name = (string)reader.GetString(0);

				if (name.ToLower() == "pso")
				{
					pso = true;
					return false;
				}

				return true;
			});

			string q;
			List<object> pars = new List<object>();

			if (spec.Iteration == -1 && spec.Solution == -1)
			{
				if (stagepso)
				{
					q = "SELECT solution.`iteration`, solution.`index` from solution LEFT JOIN data ON (data.`index` = solution.`index` AND data.`iteration` = solution.`iteration`) ORDER BY data.`_d_StagePSO::stage` DESC, fitness DESC";
				}
				else
				{
					q = "SELECT `iteration`, `index` FROM `solution` ORDER BY `fitness` DESC";
				}
			}
			else if (spec.Iteration == -1)
			{
				if (stagepso)
				{
					q = "SELECT solution.`iteration`, solution.`index` from solution LEFT JOIN data ON (data.`index` = solution.`index` AND data.`iteration` = solution.`iteration`) WHERE solution.`index` = @0 ORDER BY data.`_d_StagePSO::stage` DESC, fitness DESC";
				}
				else
				{
					q = "SELECT `iteration`, `index` FROM solution WHERE `index` = @0 ORDER BY `fitness` DESC";
				}

				pars.Add(spec.Solution);
			}
			else if (spec.Solution == -1)
			{
				if (stagepso)
				{
					q = "SELECT solution.`iteration`, solution.`index` from solution LEFT JOIN data ON (data.`index` = solution.`index` AND data.`iteration` = solution.`iteration`) WHERE solution.`iteration` = @0 ORDER BY data.`_d_StagePSO::stage` DESC, fitness DESC";
				}
				else
				{
					q = "SELECT `iteration`, `index` FROM solution WHERE `iteration` = @0 ORDER BY `fitness` DESC";
				}

				pars.Add(spec.Iteration);
			}
			else
			{
				q = "SELECT `iteration`, `index` FROM solution WHERE `iteration` = @0 AND `index` = @1";
				pars.Add(spec.Iteration);
				pars.Add(spec.Solution);
			}

			if (d_nbest == 1)
			{
				q += String.Format(" LIMIT 1");
			}

			int numhad = 0;
			int lastsol = -1;

			db.Query(q, (reader) => {
				int cit = (int)reader.GetInt64(0);
				int csol = (int)reader.GetInt64(1);

				if (!pso || lastsol != csol)
				{
					iterations.Add(cit);
					solutions.Add(csol);

					lastsol = csol;
					++numhad;
				}

				if (numhad >= d_nbest)
				{
					return false;
				}

				return true;
			}, pars.ToArray());

			for (int j = 0; j < iterations.Count; ++j)
			{
				int iteration = iterations[j];
				int solution = solutions[j];

				q = "SELECT " + d_parameterQueryColumns + " FROM `parameter_values` WHERE `iteration` = @0 AND `index` = @1";
				object[] rs = db.QueryFirst(q, iteration, solution);
			
				// Insert this solution in the initial population table
				List<string> vall = new List<string>();
			
				for (int i = 0; i < rs.Length; ++i)
				{
					vall.Add(String.Format("@{0}", i));
				}
			
				string valp = String.Join(", ", vall.ToArray());

				d_database.Query("INSERT INTO `initial_population` (" + d_parameterQueryColumns + ") VALUES (" + valp + ")", rs);

				rs = db.QueryFirst("SELECT " + d_dataQueryColumns + " FROM `data` WHERE `iteration` = @0 AND `index` = @1", iteration, solution);
			
				vall.Clear();
			
				for (int i = 0; i < rs.Length; ++i)
				{
					vall.Add(String.Format("@{0}", i));
				}
			
				valp = String.Join(", ", vall.ToArray());
			
				d_database.Query("INSERT INTO `initial_population_data` (" + d_dataQueryColumns + ") VALUES (" + valp + ")", rs);
			}
		}
	}
}

