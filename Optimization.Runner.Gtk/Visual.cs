using System;
using Optimization;
using System.Threading;

using GGtk = Gtk;

namespace Optimization.Runner.Gtk
{
	public class Visual : Optimization.Visual
	{
		MainWindow d_window;

		bool d_stopped;
		Thread d_loop;
		
		static Visual()
		{
			GGtk.Application.Init();
		}
		
		public Visual(Application application) : base(application)
		{
			d_window = new MainWindow();
			d_window.Present();

			d_window.ButtonStop.Clicked += HandleClicked;
			d_window.DeleteEvent += HandleDeleteEvent;

			d_stopped = false;
			
			d_loop = new Thread(delegate () {
				while (true)
				{
					Thread.Sleep(10);
					Application.WaitHandle.Set();
				}
			});
			
			d_loop.Start();
		}

		void HandleDeleteEvent(object o, GGtk.DeleteEventArgs args)
		{
			if (d_stopped)
			{
				GGtk.Application.Quit();
			}
			else
			{
				d_stopped = true;
				Application.Stop();
			}
			
			args.RetVal = false;
		}

		void HandleClicked(object sender, EventArgs e)
		{
			Application.Stop();
		}
		
		private void Reset()
		{
			d_window.Fill(Application.Job);
		}
		
		protected override void OnError(object source, string message)
		{
			d_window.Log(message, "Red");
			d_window.Expand();
		}
		
		protected override void OnJob(object source, Job job)
		{
			Reset();
		}

		protected override void OnProgress(object source, double progress)
		{
			d_window.Update(Application.Job);
		}

		protected override void OnStatus(object source, string message)
		{
			d_window.Log(message, "Blue");
		}
		
		protected override void OnIterate(object source, System.EventArgs args)
		{
			while (GGtk.Application.EventsPending())
			{
				GGtk.Application.RunIteration(false);
			}
		}

		public override void Run()
		{
			d_loop.Abort();

			if (!d_stopped)
			{
				d_stopped = true;
				d_window.Expand();
				d_window.ButtonStop.Sensitive = false;
				GGtk.Application.Run();
			}
			else
			{
				GGtk.Application.Quit();
			}
		}
	}
}
