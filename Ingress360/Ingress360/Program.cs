using System;
using Gtk;

namespace Ingress360
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Init ();
			win.Show ();
			Application.Run ();
		}
	}
}
