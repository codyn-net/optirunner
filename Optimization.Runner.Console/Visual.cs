using System;
using Optimization;

namespace Optimization.Runner.Console
{
	public class Visual : Optimization.Visual
	{
		public Visual(Application application) : base(application)
		{
		}
		
		public override void Run()
		{
			System.Console.WriteLine();
		}
		
		protected override void OnError(object source, string message)
		{
			System.Console.ForegroundColor = ConsoleColor.Red;
			System.Console.Error.WriteLine("[Error] " + message);
			System.Console.ResetColor();
		}
	
		protected override void OnJob(object source, Optimization.Job job)
		{
			System.Console.ForegroundColor = ConsoleColor.Blue;
			System.Console.WriteLine("[Started new job {0} => {1}]", job.Name, job.Optimizer.Name);
			System.Console.ResetColor();
		}
	
		protected override void OnProgress(object source, double progress)
		{
			int num = System.Console.WindowWidth;
			string prefix = "Progress:";
			string perc = String.Format("{0:###.##}%", progress * 100);
	
			int len = num - prefix.Length - perc.Length - 4;
			int stars = (int)(len * progress);
			string ss = "";
	
			for (int i = 0; i < len; ++i)
			{
				ss += i < len * progress ? "*" : " ";
			}
			
			System.Console.Write("{0} [{1}] {2}\r", prefix, ss.PadRight(len - stars), perc);
		}
	
		protected override void OnStatus(object source, string message)
		{
			System.Console.Title = "[Status] " + message;
		}
	}
}