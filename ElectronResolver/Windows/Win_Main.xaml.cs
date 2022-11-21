#pragma warning disable 8618
using MisakaCastle.ElectronResolver.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace MisakaCastle.ElectronResolver
{
	/// <summary>
	/// Interaction logic for Win_Main.xaml
	/// </summary>
	public partial class Win_Main : Window
	{
		public static Win_Main Instance { get; private set; }

		public ElectronSearcher Searcher = new ElectronSearcher(out _);

		public Win_Main()
		{
			InitializeComponent();
			Instance = this;
		}

		internal void RefreshList(List<ElectronAppInfo> infoList, bool isInterrupted  = false)
		{
			// TODO
		}

		private void tsmi_StartSearch_Click(object sender, RoutedEventArgs e)
		{
			var search = new Frm_Search();
			lbl_Status.Content = "Searching All Electron-Based apps...";
			search.ShowDialog();
		}
	}
}
