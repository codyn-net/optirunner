using System;
using GGtk = Gtk;

namespace Optimization.Runner.Gtk
{
	public class Application : Optimization.Runner.Application
	{
		public Application(ref string[] args) : base(ref args)
		{
		}
		
		protected override void Initialize()
		{
			base.Initialize();
			
			string[] args = new string[] {};

			GGtk.Application.Init("goptirunner", ref args);			
		}
		
		protected override Optimization.Visual CreateVisual()
		{
			return new Optimization.Runner.Gtk.Visual(this);
		}
		
		private GGtk.FileFilter CreateFilter(string name, string pattern)
		{
			GGtk.FileFilter filter = new GGtk.FileFilter();
			filter.Name = name;
			filter.AddPattern(pattern);
			
			return filter;
		}
		
		protected override string[] GetJobs()
		{
			GGtk.Application.Init();
			GGtk.FileChooserDialog dialog = new GGtk.FileChooserDialog("Open jobs - goptirunner", null, GGtk.FileChooserAction.Open, GGtk.Stock.Cancel, GGtk.ResponseType.Cancel, GGtk.Stock.Open, GGtk.ResponseType.Ok);
			
			dialog.AddFilter(CreateFilter("Jobs (*.xml)", "*.xml"));
			dialog.AddFilter(CreateFilter("All files", "*"));
			
			dialog.SelectMultiple = true;
			
			if (dialog.Run() != (int)GGtk.ResponseType.Ok)
			{
				Environment.Exit(1);
			}
			
			string[] ret = dialog.Filenames;
			dialog.Destroy();
			
			return ret;
		}

		public static void Main(string[] args)
		{			
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
			Application application = new Application(ref args);
			application.Run();
		}
	}
}