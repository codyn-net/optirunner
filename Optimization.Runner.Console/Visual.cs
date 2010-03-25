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
			
			if (num == 0)
			{
				num = 78;
			}

			string prefix = "Progress:";
			string perc = String.Format("{0:###.00}%", progress * 100);
			
			perc = perc.PadLeft(7);
	
			int len = num - prefix.Length - 12;
			int stars = (int)(len * progress);
			string ss = "";
	
			for (int i = 0; i < len; ++i)
			{
				ss += i < len * progress ? "*" : " ";
			}
			
			System.Console.Write("{0} [{1}] {2} \r", prefix, ss.PadRight(System.Math.Max(ss.Length, len - stars)), perc);
		}
	
		protected override void OnStatus(object source, string message)
		{
			System.Console.Title = "[Status] " + message;
		}
	}
}