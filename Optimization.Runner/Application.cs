/*
 *  Main.cs - This file is part of optijob
 *
 *  Copyright (C) 2009 - Jesse van den Kieboom
 *
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License as published by the 
 * Free Software Foundation; either version 2.1 of the License, or (at your 
 * option) any later version.
 * 
 * This library is distributed in the hope that it will be useful, but WITHOUT 
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License 
 * for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License 
 * along with this library; if not, write to the Free Software Foundation,
 * Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA 
 */

using System;
using Optimization;
using System.Reflection;
using System.Collections.Generic;

namespace Optimization.Runner
{
	public abstract class Application : Optimization.Application
	{
		string[] d_jobFiles;
		List<string> d_assemblies;
		string d_dataDirectory;
		string d_listOptimizers;
		string d_filename;
	
		public Application(ref string[] args) : base(ref args)
		{
			d_jobFiles = args;
		}
		
		protected abstract Visual CreateVisual();
		
		protected virtual string[] GetJobs()
		{
			return new string[] {};
		}
		
		protected override void Initialize()
		{
			base.Initialize();

			d_assemblies = new List<string>();
			d_listOptimizers = null;
		}
		
		public void Run()
		{
			LoadAssemblies();
			
			if (d_listOptimizers != null)
			{
				if (d_listOptimizers == "")
				{
					ShowOptimizers();
				}
				else
				{
					ShowOptimizers(d_listOptimizers);
				}
				
				Environment.Exit(0);
			}
	
			if (d_jobFiles.Length == 0)
			{
				d_jobFiles = GetJobs();
			}
			
			if (d_jobFiles.Length == 0)
			{
				System.Console.WriteLine("Please provide some jobs to run...");
				Environment.Exit(1);
			}
	
			if (String.IsNullOrEmpty(d_dataDirectory))
			{
				d_dataDirectory = ".";
			}
			
			Visual visual = CreateVisual();
	
			foreach (string filename in d_jobFiles)
			{
				if (!System.IO.File.Exists(filename))
				{
					Error("Job file `{0}' does not exist!", filename);
					continue;
				}

				try
				{
					// Create new job
					Job job;

					if (filename.EndsWith(".db"))
					{
						job = new Job();
						job.LoadFromStorage(filename);
					}
					else
					{
						job = Job.NewFromXml(filename);

						string datafile;

						// Set optimizer storage location
						if (d_filename == null)
						{
							datafile = job.Name + ".db";
						}
						else
						{
							datafile = d_filename;
						}
						
						if (!System.IO.Path.IsPathRooted(datafile))
						{
							datafile = System.IO.Path.Combine(d_dataDirectory, datafile);
						}

						job.Optimizer.Storage.Uri = datafile;
						job.Initialize();
					}
					
					// Run job
					Run(job);
				}
				catch (Exception e)
				{
					System.Console.Error.WriteLine("Could not complete job `{0}': {1}", filename, e);
				}
			}
			
			Stop();
			
			if (visual != null)
			{
				visual.Run();
			}
		}
		
		private void LoadAssemblies()
		{
			foreach (string s in d_assemblies)
			{			
				try
				{
					Assembly.LoadFile(s);
				}
				catch (Exception e)
				{
					System.Console.Error.WriteLine(String.Format("Could not load assembly {0} => {1}", s, e));
					continue;
				}
			}
		}
		private string OptimizerName(Type type)
		{
			string name = Optimizer.GetName(type);
			string ns = type.Namespace;
			
			string prefix = "Optimization.Optimizers.";
			
			if (ns.StartsWith(prefix))
			{
				ns = ns.Substring(prefix.Length);
			}
			
			if (ns == "")
			{
				return name;
			}
			else
			{
				return ns + "." + name;
			}
		}
		
		private void PrintColored(string key, string val)
		{
			System.Console.ForegroundColor = ConsoleColor.Blue;
			System.Console.Write(key + ": ");
			System.Console.ResetColor();
			System.Console.WriteLine(val);
		}
		
		private void ShowOptimizers(string optimizer)
		{
			Optimizer opt = Registry.Create(optimizer);
			
			if (opt == null)
			{
				System.Console.Error.WriteLine("Could not find optimizer: {0}", optimizer);
				return;
			}
			
			Type type = opt.GetType();
			string description = Optimizer.GetDescription(type);
			
			PrintColored("Name", OptimizerName(type));
			PrintColored("Namespace", type.Namespace);
			PrintColored("Description", description != null ? description : "No description available");
			PrintColored("Settings", "");
			System.Console.WriteLine();
		
			foreach (KeyValuePair<string, object> setting in opt.Configuration)
			{
				string desc = opt.Configuration.Description(setting.Key);
				
				if (desc != null)
				{
					System.Console.ForegroundColor = ConsoleColor.DarkGreen;
					System.Console.WriteLine("    [{0}]", desc);
				}
				
				System.Console.ForegroundColor = ConsoleColor.Yellow;
				System.Console.Write("    {0}", setting.Key);
				System.Console.ResetColor();
				System.Console.Write(" = ");
	
				if (setting.Value is Enum)
				{
					string[] names = Enum.GetNames(setting.Value.GetType());
					string name = Enum.GetName(setting.Value.GetType(), setting.Value);
	
					System.Console.Write("(");
					
					for (int i = 0; i < names.Length; ++i)
					{
						if (i != 0)
						{
							System.Console.Write(", ");
						}
						
						if (names[i] == name)
						{
							System.Console.ForegroundColor = ConsoleColor.Green;
						}
						
						System.Console.Write(names[i]);
						System.Console.ResetColor();
					}
					
					System.Console.Write(")");
				}
				else
				{
					System.Console.Write(setting.Value.ToString());
				}
	
				System.Console.WriteLine();
				
				if (desc != null)
				{
					System.Console.WriteLine();
				}
			}
		}
		
		private void ShowOptimizers()
		{
			List<Type> types = new List<Type>(Registry.Optimizers);
			
			types.Sort(delegate (Type a, Type b) { return OptimizerName(a).CompareTo(OptimizerName(b)); });
			
			System.Console.WriteLine("\nAvailable optimizers:\n");
			
			foreach (Type type in types)
			{
				string description = Optimizer.GetDescription(type);
				
				PrintColored(OptimizerName(type), description != null ? description : "No description available");
			}
			
			System.Console.WriteLine();
		}
		
		private void AddAssembly(string s)
		{
			d_assemblies.Add(s);
		}
		
		protected override void AddOptions(NDesk.Options.OptionSet optionSet)
		{
			base.AddOptions(optionSet);
			
			optionSet.Add("o=|optimizers=", "Load additional optimizer assemblies", delegate (string s) { AddAssembly(s); });
			optionSet.Add("d=|datadir=", "Specify directory to store data files", delegate (string s) { d_dataDirectory = s; });
			optionSet.Add("l:|list:", "List available optimizers", delegate (string s) { d_listOptimizers = s != null ? s : ""; });
			optionSet.Add("f=|filename=", "Results database filename", delegate (string s) { d_filename = s; });
		}
	}
}
