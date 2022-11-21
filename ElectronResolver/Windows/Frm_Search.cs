using MisakaCastle.ElectronResolver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

#pragma warning disable 8618
namespace MisakaCastle.ElectronResolver
{
	public partial class Frm_Search : Form
	{
		public static Frm_Search Instance { get; private set; }

		public Frm_Search()
		{
			InitializeComponent();
			Instance = this;
			Text = I.S["Searching.Title"];
			lbl_Status.Text = I.S["Searching.Information"];
			btn_Cancel.Text = I.S["General.Cancel"];
		}

		private void Frm_Search_Load(object sender, EventArgs e)
		{
			Win_Main.Instance.Searcher.OnSearchFolderChanged += Searcher_OnSearchFolderChanged;
			Win_Main.Instance.Searcher.SearchAsync().Start();
		}

		private void Searcher_OnSearchFolderChanged(object? sender, string e)
		{
			if (e == "Completed")
			{
				Win_Main.Instance.Searcher.OnSearchFolderChanged -= Searcher_OnSearchFolderChanged;
				Win_Main.Instance.RefreshList(Win_Main.Instance.Searcher.ElectronApps);
				Close();
			}
			else
			{
				lbl_Status.Text = string.Format(I.S["Searching.Information"], e);
			}
		}

		private void btn_Cancel_Click(object sender, EventArgs e)
		{
			Win_Main.Instance.Searcher.OnSearchFolderChanged -= Searcher_OnSearchFolderChanged;
			Win_Main.Instance.Searcher.Abort();
			Win_Main.Instance.RefreshList(Win_Main.Instance.Searcher.ElectronApps, true);
			Close();
		}
	}
}
