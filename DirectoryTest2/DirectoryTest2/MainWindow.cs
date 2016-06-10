using System;
using System.IO;
using System.Collections.Generic ;
using Gtk;

public class DirectoryOrFile 
{
	private string _name ;
	private string _path ;
	private DirectoryOrFile _parent ;
	private List<DirectoryOrFile> _sub = null ;

	public DirectoryOrFile (string name, string path, DirectoryOrFile parent)
	{
		_name = name;
		_path = path + "\\" + name;
		_parent = parent;
		_sub = new List<DirectoryOrFile>() ;
	}
	public DirectoryOrFile ()
	{
		_name = "" ;
		_path = "" ;
		_parent = null ;
		_sub = new List<DirectoryOrFile>() ;
	}

	public string Name() { return _name; } 
	public string Path() { return _path; } 
	public DirectoryOrFile Parent() { return _parent; } 
	public int NumChildren() { return _sub.Count ; } 
	public DirectoryOrFile Child(int index) { return ((index < 0) || (index >= _sub.Count)) ? null : _sub [index]; } 
	public void AddChild (DirectoryOrFile sub) {_sub.Add (sub);} 
} ;

public partial class MainWindow: Gtk.Window
{
	private bool _bTreeViewInited = false ;

	private Gtk.ListStore _CameraList = null;
	private string _jobDir;
	private string[] _scenes;
	private DirectoryOrFile _jobRoot = null;

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}

	public void Init()
	{
		if (!_bTreeViewInited) 
		{
			Gtk.TreeViewColumn camColumn = new Gtk.TreeViewColumn ();
			camColumn.Title = "Camera Source";
			Gtk.TreeViewColumn fileColumn = new Gtk.TreeViewColumn ();
			fileColumn.Title = "Destination";

			this.DataTransfer_treeview.AppendColumn (camColumn);
			this.DataTransfer_treeview.AppendColumn (fileColumn);

			Gtk.ListStore camDataList = new Gtk.ListStore (typeof(string), typeof(string));
			this.DataTransfer_treeview.Model = camDataList;

			_bTreeViewInited = true;
		}

		this.JobDir_textview.Buffer.Changed += OnJobDirChanged;
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	[GLib.ConnectBefore]
	protected void OnJobDirChanged (object sender, EventArgs e)
	{
		string sendertext = sender.ToString ();
		string thisString = this.JobDir_textview.Buffer.Text;

		bool bDirExists = false;
		try
		{
			if (Directory.Exists (this.JobDir_textview.Buffer.Text)) 
				bDirExists = true ;
			else
				bDirExists = false ;
		} 
		catch (Exception exp)
		{
			Exception foo = exp;
		} ;

		if (bDirExists) 
		{
			_jobDir = this.JobDir_textview.Buffer.Text;
			EnableButtons ();
		} else
			DisableButtons ();
	}

	protected void DisableButtons()
	{
		this.LoadCam_button.Sensitive = false;
		this.CreateScene_button.Sensitive = false;
		this.CreateTake_button.Sensitive = false;
	}

	protected void EnableButtons()
	{
		this.LoadCam_button.Sensitive = true;
		this.CreateScene_button.Sensitive = true;
		this.CreateTake_button.Sensitive = true;
	}

	protected void OnBrowse (object sender, EventArgs e)
	{
		//Open file dialog and choose a file.
		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog("Set the Job directory",
				this, 
				Gtk.FileChooserAction.Save,
				"Cancel",Gtk.ResponseType.Cancel,
				"Open",Gtk.ResponseType.Accept);
		fc.Action = Gtk.FileChooserAction.SelectFolder;
		fc.DoOverwriteConfirmation = true;

		if (fc.Run() == (int)Gtk.ResponseType.Accept) 
		{
			this.JobDir_textview.Buffer.Text = fc.Filename ;
		}
		//Destroy() to close the File Dialog
		fc.Destroy();


		UpdateTreeView ();
	}

	protected void OnLoadCamera (object sender, EventArgs e)
	{
		UpdateTreeView ();
		//throw new NotImplementedException ();
	}

	protected void OnCreateSceneDir (object sender, EventArgs e)
	{
		//Open file dialog and choose a file.
		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog("Set the Job directory",
				this, 
				Gtk.FileChooserAction.Save,
				"Cancel",Gtk.ResponseType.Cancel,
				"Open",Gtk.ResponseType.Accept);
		fc.Action = Gtk.FileChooserAction.SelectFolder;
		fc.DoOverwriteConfirmation = true;
		fc.SetCurrentFolder(this.JobDir_textview.Buffer.Text);

		if (fc.Run() == (int)Gtk.ResponseType.Accept) 
		{
			this.JobDir_textview.Buffer.Text = fc.Filename ;

		}
		//Destroy() to close the File Dialog
		fc.Destroy();
		UpdateTreeView ();
	}

	protected void OnCreateTakeDir (object sender, EventArgs e)
	{
		UpdateTreeView ();
		//throw new NotImplementedException ();
	}

	protected void UpdateTreeView ()
	{
	}
}
