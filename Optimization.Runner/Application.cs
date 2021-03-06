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
		uint d_repeat;
		string d_initialPopulation;
		uint d_nbest = 1;
	
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
			d_repeat = 1;
		}
		
		public void Run()
		{
			LoadAssemblies();
			
			if (d_listOptimizers != null)
			{
				if (d_listOptimizers == "")
				{
					ShowTypes();
				}
				else
				{
					ShowType(d_listOptimizers);
				}
				
				Environment.Exit(0);
			}
			
			if (d_initialPopulation != null)
			{
				InitialPopulation ip = new InitialPopulation(d_jobFiles);
				
				if (ip.Generate(d_initialPopulation, d_nbest))
				{
					Environment.Exit(0);
				}
				else
				{
					Environment.Exit(1);
				}
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
				
				for (uint i = 0; i < d_repeat; ++i)
				{
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
						System.Console.Error.WriteLine("Could not complete job `{0}': {1}", filename, e.GetBaseException().Message);
						System.Console.Error.WriteLine("  Trace:");
						System.Console.Error.WriteLine("  ======");
						System.Console.Error.WriteLine("  {0}", e.GetBaseException().StackTrace.Replace("\n", "\n  "));
					}
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
		
		private string TypeName(Type type, string name, string prefix)
		{
			string ns = type.Namespace;
			prefix += ".";
			
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

		private string OptimizerName(Type type)
		{
			return TypeName(type, Optimizer.GetName(type), "Optimization.Optimizers");
		}
		
		private string ExtensionName(Type type)
		{
			return TypeName(type, Extension.GetName(type), "Optimization.Optimizers.Extensions");
		}
		
		private void PrintColored(string key, string val)
		{
			System.Console.ForegroundColor = ConsoleColor.Blue;
			System.Console.Write(key + ": ");
			System.Console.ResetColor();
			System.Console.WriteLine(val);
		}
		
		private void ShowType(string name)
		{
			Optimizer opt = null;
			
			try
			{
				opt = Registry.Create(name);
			}
			catch
			{
			}
			
			if (opt != null)
			{
				ShowOptimizer(opt);
				return;
			}

			Extension ext = Extension.Create(name);
				
			if (ext != null)
			{
				ShowExtension(ext);
				return;
			}

			System.Console.Error.WriteLine("Could not find optimizer or extension `{0}'", name);
		}
		
		private void ShowSettings(Settings settings)
		{
			PrintColored("Settings", "");
			System.Console.WriteLine();
		
			foreach (KeyValuePair<string, object> setting in settings)
			{
				string desc = settings.Description(setting.Key);
				
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
					if (setting.Value == null)
					{
						System.Console.Write("null");
					}
					else
					{
						System.Console.Write(setting.Value.ToString());
					}
				}
	
				System.Console.WriteLine();
				
				if (desc != null)
				{
					System.Console.WriteLine();
				}
			}
		}
		
		private void ShowOptimizer(Optimizer opt)
		{
			Type type = opt.GetType();
			string description = opt.Description;
			
			PrintColored("Name", OptimizerName(type));
			PrintColored("Namespace", type.Namespace);
			PrintColored("Description", !String.IsNullOrEmpty(description) ? description : "No description available");
			
			ShowSettings(opt.Configuration);
		}
		
		private string[] ExtensionAppliesTo(Extension ext)
		{
			Type[] applies = ext.AppliesTo;
			string[] ret = new string[applies.Length];
			
			for (int i = 0; i < applies.Length; ++i)
			{
				ret[i] = OptimizerName(applies[i]);
			}
			
			return ret;
		}
		
		private void ShowExtension(Extension ext)
		{
			Type type = ext.GetType();
			string description = ext.Description;

			PrintColored("Name", ExtensionName(type));
			PrintColored("Namespace", type.Namespace);
			PrintColored("Description", !String.IsNullOrEmpty(description) ? description : "No description available");
			PrintColored("Extends", String.Join(",", ExtensionAppliesTo(ext)));
			
			ShowSettings(ext.Configuration);
		}
		
		private void ShowTypes()
		{
			List<Type> types = new List<Type>(Registry.Optimizers);
			
			types.Sort(delegate (Type a, Type b) {
				return OptimizerName(a).CompareTo(OptimizerName(b)); });
			
			System.Console.WriteLine("\nAvailable optimizers:\n");
			
			foreach (Type type in types)
			{
				string description = Optimizer.GetDescription(type);
				
				PrintColored(OptimizerName(type), !String.IsNullOrEmpty(description) ? description : "No description available");
			}
			
			System.Console.WriteLine();
			
			System.Console.WriteLine("\nAvailable extensions:\n");
			
			types = new List<Type>(Extension.Extensions);
			types.Sort(delegate (Type a, Type b) {
				return ExtensionName(a).CompareTo(ExtensionName(b)); });

			foreach (Type type in Extension.Extensions)
			{
				string description = Extension.GetDescription(type);
				
				PrintColored(ExtensionName(type), !String.IsNullOrEmpty(description) ? description : "No description available");
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
			
			optionSet.Add("o=|optimizers=", "Load additional optimizer assemblies", delegate (string s) {
				AddAssembly(s); });
			optionSet.Add("d=|datadir=", "Specify directory to store data files", delegate (string s) {
				d_dataDirectory = s; });
			optionSet.Add("l:|list:", "List available optimizers and extensions", delegate (string s) {
				d_listOptimizers = s != null ? s : ""; });
			optionSet.Add("f=|filename=", "Results database filename", delegate (string s) {
				d_filename = s; });
			optionSet.Add("r=|repeat=", "Repeat job N times", delegate (string s) {
				d_repeat = UInt32.Parse(s); });
			optionSet.Add("g=|generate-initial-population=", "Generate initial population into the specified file. Additional command line arguments are databases from which the initial population is drawn. Specific iteration/solution can be specified using 'filename.db[:iteration-id[:solution-id]]'.", delegate (string s) {
				d_initialPopulation = s; });
			optionSet.Add("n=|select-n-best=", "Select N-best solutions to generate initial population from.", delegate (string s) {
				d_nbest = UInt32.Parse(s); });
		}
	}
}
