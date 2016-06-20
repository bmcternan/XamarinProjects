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
			//win.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask ;

			win.Init ();
			win.Show ();
			Application.Run ();
		}
	}
}
