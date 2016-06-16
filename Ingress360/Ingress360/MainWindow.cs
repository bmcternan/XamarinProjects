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
	private DOF_Type _dof_type ;
	private int _size ;
	private DateTime _modified ;

	public enum DOF_Type { UNDEF, DIR, FILE } ;

	public DirectoryOrFile (string name, string path, DirectoryOrFile parent, DOF_Type nodeType)
	{
		_name = name;
		_path = path + "\\" + name;
		_parent = parent;
		_sub = new List<DirectoryOrFile>() ;
		_dof_type = nodeType;
	}
	public void Set (string name, string path, DirectoryOrFile parent, DOF_Type nodeType)
	{
		_name = name;
		_path = path + "\\" + name;
		_parent = parent;
		_sub = new List<DirectoryOrFile>() ;
		_dof_type = nodeType;
	}
	public DirectoryOrFile ()
	{
		_name = "" ;
		_path = "" ;
		_parent = null ;
		_sub = new List<DirectoryOrFile>() ;
		_dof_type = DOF_Type.UNDEF;
	}

	public string Name() { return _name; } 
	public string Path() { return _path; } 
	public DirectoryOrFile Parent() { return _parent; } 
	public int NumChildren() { return _sub.Count ; } 
	public DirectoryOrFile Child(int index) { return ((index < 0) || (index >= _sub.Count)) ? null : _sub [index]; } 
	public void AddChild (DirectoryOrFile sub) {_sub.Add (sub);} 
	public DOF_Type Type () { return _dof_type;	}
} ;

public partial class MainWindow: Gtk.Window
{
	private bool _bTreeViewInited = false ;

	private int _numCameras = 0;
	private string _jobName;
	private Gtk.ListStore _CameraList = null;
	private string _jobDir;
	private string[] _scenes;
	private DirectoryOrFile _jobRoot = null;	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}

	public void Init()
	{
		if (!_bTreeViewInited) 
		{
			//Gtk.TreeViewColumn camColumn = new Gtk.TreeViewColumn ();
			//camColumn.Title = "Camera Source";
			//Gtk.TreeViewColumn fileColumn = new Gtk.TreeViewColumn ();
			//fileColumn.Title = "Destination";

			//this.DataTransfer_treeview.AppendColumn (camColumn);
			//this.DataTransfer_treeview.AppendColumn (fileColumn);

			//Gtk.ListStore camDataList = new Gtk.ListStore (typeof(string), typeof(string));
			//this.DataTransfer_treeview.Model = camDataList;


			TreeViewColumn col = new TreeViewColumn ( );
			CellRenderer colr = new CellRendererText ( );
			col.Title = "Target File";
			col.PackStart (colr, true);
			col.AddAttribute (colr, "text", 0);
			this.DataTransfer_treeview.AppendColumn (col);

			// the above can be written more concisely as
			// tv.AppendColumn ("Column 1", new CellRendererText ( ), 
			//                  "text", 0);

			TreeStore store = new TreeStore (typeof (string));
			this.DataTransfer_treeview.Model = store;

			//TreeIter iter = new TreeIter ( );
			//for (int i = 0; i < 4; i++) 
			//{
			//	iter = store.AppendValues ("Point " + i.ToString ( ));
			//	for (int j = i-1; j >= 0; j--) 
			//	{
			//		TreeIter iter2 = new TreeIter ( );
			//		iter2 = store.AppendValues (iter, "Visited " + j.ToString ( ));
			//		for (int k = 0; k < 2; k++) 
			//		{
			//			store.AppendValues (iter2, "Next " + k.ToString ( ));
			//		}
			//	}
			//}


			_bTreeViewInited = true;
		}

		this.JobDir_textview.Buffer.Changed += OnJobDirChanged;
		this.NumCameras_textview.Buffer.Changed += OnNumCamerasChanged;
		this.JobName_textview.Buffer.Changed += OnJobNameChanged;
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void TraverseDirTree (DirectoryOrFile node, int level, ref TreeStore store, TreeIter iter)
	{
		TreeIter thisNodeIter = new TreeIter ( );

		if (iter.Equals (TreeIter.Zero))
			thisNodeIter = store.AppendValues (node.Name());
		else
			thisNodeIter = store.AppendValues (iter, node.Name());

		for (int i = 0; i < level; i++)
			Console.Write ("   ");
		Console.WriteLine ("{0} = {1} ({2})", node.Name(), node.Path(), node.Type() == DirectoryOrFile.DOF_Type.DIR ? "D" : (node.Type() == DirectoryOrFile.DOF_Type.FILE ? "F" : "U"));

		for (int i = 0; i < node.NumChildren (); i++) 
		{
			TraverseDirTree (node.Child (i), level + 1, ref store, thisNodeIter);
		}
	}



	protected void LoadDirTree (ref DirectoryOrFile node, string path, DirectoryOrFile parent, DirectoryOrFile.DOF_Type dofType)
	{
		if (node == null)
			return;

		string nodeName = path.Substring(path.LastIndexOf("\\") + 1) ;

		node.Set(nodeName, path, parent, dofType);

		if (dofType == DirectoryOrFile.DOF_Type.DIR) {
			//scan for directories
			try {
				List<string> dirList = new List<string> (Directory.EnumerateDirectories (path));
				foreach (var dir in dirList) {
					string foo = dir.Substring (dir.LastIndexOf ("\\") + 1);
					Console.WriteLine ("{0}", dir.Substring (dir.LastIndexOf ("\\") + 1));

					DirectoryOrFile sub = new DirectoryOrFile ();
					LoadDirTree (ref sub, dir, node, DirectoryOrFile.DOF_Type.DIR);

					node.AddChild (sub);
				}
				int num = dirList.Count;
				Console.WriteLine ("{0} directories found.", dirList.Count);
			} catch (UnauthorizedAccessException UAEx) {
				Console.WriteLine (UAEx.Message);
			} catch (PathTooLongException PathEx) {
				Console.WriteLine (PathEx.Message);
			}

			//scan for files
			try {
				List<string> fileList = new List<string> (Directory.EnumerateFiles (path));
				foreach (var fname in fileList) {
					string foo = fname.Substring (fname.LastIndexOf ("\\") + 1);
					Console.WriteLine ("{0}", fname.Substring (fname.LastIndexOf ("\\") + 1));
					DirectoryOrFile sub = new DirectoryOrFile ();
					LoadDirTree (ref sub, fname, node, DirectoryOrFile.DOF_Type.FILE);

					node.AddChild (sub);
				}
				int num = fileList.Count;
				Console.WriteLine ("{0} directories found.", fileList.Count);
			} catch (UnauthorizedAccessException UAEx) {
				Console.WriteLine (UAEx.Message);
			} catch (PathTooLongException PathEx) {
				Console.WriteLine (PathEx.Message);
			}
		}
	}

	[GLib.ConnectBefore]
	protected void OnJobNameChanged (object sender, EventArgs e)
	{
		_jobName = this.JobName_textview.Buffer.Text;
	}

	[GLib.ConnectBefore]
	protected void OnNumCamerasChanged (object sender, EventArgs e)
	{
		this.NumCameras_textview.Buffer.Changed -= OnNumCamerasChanged;

		string thisString = this.NumCameras_textview.Buffer.Text;
		string numString = "";

		for (int i = 0; i < thisString.Length; i++) 
		{
			if ((thisString [i] >= '0') && (thisString [i] <= '9'))
				numString += thisString.Substring (i, 1);
		}
		this.NumCameras_textview.Buffer.Text = numString;

		this.NumCameras_textview.Buffer.Changed += OnNumCamerasChanged;
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
			_jobRoot = new DirectoryOrFile ();
			LoadDirTree (ref _jobRoot, _jobDir, null, DirectoryOrFile.DOF_Type.DIR);
			EnableButtons ();

			TreeStore store = new TreeStore (typeof (string));
			this.DataTransfer_treeview.Model = store;
			TraverseDirTree (_jobRoot, 0, ref store, TreeIter.Zero);
			this.DataTransfer_treeview.ExpandAll ();

		} else
			DisableButtons ();
	}

	protected bool FormEntriesAreValid ()
	{
		return true;
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

	protected void UpdateTreeView ()
	{
	}


	protected void OnDataTransferTreviewRowCollapsed (object o, RowCollapsedArgs args)
	{
		this.DataTransfer_treeview.ExpandAll ();
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

	protected void OnLoadCamera (object sender, EventArgs e)
	{
		UpdateTreeView ();
	}

	protected void OnCreateTakeDir (object sender, EventArgs e)
	{
		UpdateTreeView ();
	}
}
