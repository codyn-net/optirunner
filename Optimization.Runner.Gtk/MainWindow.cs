using System;
using Gtk;
using System.Collections.Generic;
using System.Reflection;

namespace Optimization.Runner.Gtk
{
	internal partial class MainWindow : Window
	{
		int d_expandedHeight;
		
		public MainWindow (): base (WindowType.Toplevel)
		{
			Build();
	
			d_expandedHeight = 300;
	
			scrolled_window_details.Mapped += OnDetailsMapped;
			scrolled_window_details.Unmapped += OnDetailsUnmapped;
		}
		
		public void Fill(Optimization.Job job)
		{
			Title = String.Format("{0} - Optimization", job.Name);
			
			label_job.Text = String.Format("{0} [{1}]", job.Name, job.Optimizer.Name);
			label_started.Text = FormatDate(DateTime.Now);
			label_last_update.Text = "";
			
			label_best.Text = "";
			
			text_view_details.Buffer.Clear();
			progressbar_progress.Text = String.Format("0/{0}", job.Optimizer.Configuration.MaxIterations);
			progressbar_progress.Fraction = 0;
		}
		
		private TextTag CreateTag(string spec)
		{
			TextBuffer buffer = text_view_details.Buffer;
			TextTag tag = null;
			Gdk.Color col = new Gdk.Color(0, 0, 0);
			
			if (Gdk.Color.Parse(spec, ref col))
			{
				tag = new TextTag(spec);
				tag.ForegroundGdk = col;
			}
			else if (spec == "bold")
			{
				tag = new TextTag(spec);
				tag.Weight = Pango.Weight.Bold;
			}
			else if (spec == "underline")
			{
				tag = new TextTag(spec);
				tag.Underline = Pango.Underline.Single;
			}
	
			if (tag != null)
			{
				buffer.TagTable.Add(tag);
			}
	
			return tag;
		}
		
		private TextTag[] Tags(string[] tags)
		{
			List<TextTag> ret = new List<TextTag>();
			
			foreach (string part in tags)
			{
				TextTag tag = text_view_details.Buffer.TagTable.Lookup(part);
				
				if (tag != null)
				{
					ret.Add(tag);
				}
				else
				{
					tag = CreateTag(part);
	
					if (tag != null)
					{
						ret.Add(tag);
					}
				}
			}
			
			return ret.ToArray();
		}
	
		private void ScrollEnd()
		{
			Adjustment adj = scrolled_window_details.Vadjustment;
			adj.Value = adj.Upper - adj.PageSize;
		}
	
		private void IdleScrollEnd()
		{
			ScrollEnd();
			GLib.Idle.Add(delegate () { ScrollEnd(); return false;});
		}
		
		public void Log(string str, params string[] tagspec)
		{
			TextBuffer buffer = text_view_details.Buffer;
			TextIter end = buffer.EndIter;
			Adjustment adj = scrolled_window_details.Vadjustment;
	
			bool atend = adj.Value > (adj.Upper - adj.PageSize - 5);
	
			TextTag[] tags = Tags(tagspec);
			buffer.InsertWithTags(ref end, String.Format("{0}\n", str), tags);
			
			if (atend)
			{
				IdleScrollEnd();
			}
		}
		
		public void Update(Optimization.Job job)
		{
			uint iteration = job.Optimizer.CurrentIteration;
			uint maxIterations = job.Optimizer.Configuration.MaxIterations;
			
			// Update progress
			progressbar_progress.Fraction = iteration / (double)maxIterations;
			progressbar_progress.Text = String.Format("{0}/{1}", iteration, maxIterations);
			
			// Update last update
			label_last_update.Text = FormatDate(DateTime.Now);
			
			// Update best
			if (job.Optimizer.Best != null)
			{
				label_best.Text = String.Format("{0:0.###}", job.Optimizer.Best.Fitness.Value);
			}
		}
		
		public void Expand()
		{
			if (!expander_details.Expanded)
			{
				expander_details.Expanded = true;
			}
		}
		
		private string FormatDate(DateTime date)
		{
			return date.ToString("ddd dd MMMM, HH:mm");
		}
	
		private void OnDetailsMapped(object sender, EventArgs args)
		{
			if (d_expandedHeight == 0)
			{
				return;
			}
			
			int width;
			int height;
		
			GetSize(out width, out height);
	
			Resize(width, d_expandedHeight);
			ScrollEnd();
		}
	
		private void OnDetailsUnmapped(object sender, EventArgs args)
		{
			int width;
			int height;
	
			GLib.Idle.Add(delegate () {
				GetSize(out width, out height);
				Resize(width, vbox1.SizeRequest().Height);
				return false;
			});
		}
	
		public Button ButtonStop
		{
			get
			{
				return button_stop;
			}
		}
	}
}