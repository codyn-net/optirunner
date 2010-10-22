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
			System.Console.Error.WriteLine("\x1b[31m[Error] {0}\x1b[0m", message);
		}

		protected override void OnWarning(object source, string message)
		{
			System.Console.Error.WriteLine("\x1b[33m[Warning] {0}\x1b[0m", message);
		}
		
		protected override void OnMessage(object source, string message)
		{
			System.Console.Error.WriteLine("\x1b[32m[Message] {0}\x1b[0m", message);
		}
	
		protected override void OnJob(object source, Optimization.Job job)
		{
			System.Console.WriteLine("\x1b[34m[Started new job {0} => {1}]\x1b[0m", job.Name, job.Optimizer.Name);
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
