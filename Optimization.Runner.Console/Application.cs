using System;

namespace Optimization.Runner.Console
{
	public class Application : Optimization.Runner.Application
	{
		public Application(ref string[] args) : base(ref args)
		{
		}
		
		protected override Optimization.Visual CreateVisual()
		{
			return new Optimization.Runner.Console.Visual(this);
		}
		
		public static void Main(string[] args)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
			Application application = new Application(ref args);
			application.Run();
		}
	}
}
