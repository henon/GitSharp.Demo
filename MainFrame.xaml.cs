/*
 * Copyright (C) 2010, Henon <meinrad.recheis@gmail.com>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the project nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GitSharp.Demo
{
	/// <summary>
	/// Interaction logic for MainFrame.xaml
	/// </summary>
	public partial class MainFrame : Window
	{
		public const string CURRENT_REPOSITORY = "repository";

		public MainFrame()
		{
			InitializeComponent();
			m_url_textbox.Text = UserSettings.GetString(CURRENT_REPOSITORY);
			Loaded += (o, args) => Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => LoadRepository(m_url_textbox.Text)));
		}


		Repository m_repository;

		private void OnLoadRepository(object sender, RoutedEventArgs e)
		{
			LoadRepository(m_url_textbox.Text);
		}

		private void LoadRepository(string url)
		{
			var git_url = Repository.FindRepository(url);
			if (git_url == null || !Repository.IsValid(git_url))
			{
				MessageBox.Show("Given path doesn't seem to refer to a git repository: " + url);
				return;
			}
			var repo = new Repository(git_url);
			m_url_textbox.Text = git_url;
			UserSettings.SetValue(CURRENT_REPOSITORY, git_url);
			var head = repo.Head.Target as Commit;
			Debug.Assert(head != null);
			m_repository = repo;
			foreach (TabItem tab in m_tab_control.Items)
			{
				var repo_view = tab.Content as IRepositoryView;
				if (repo_view==null)
					continue;
				repo_view.Update(m_repository);
			}
		}

		private void OnSelectRepository(object sender, RoutedEventArgs e)
		{
			var dlg = new System.Windows.Forms.FolderBrowserDialog();
			dlg.SelectedPath = Path.GetDirectoryName(UserSettings.GetString(CURRENT_REPOSITORY));
			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				m_url_textbox.Text = dlg.SelectedPath;
				LoadRepository(m_url_textbox.Text);
			}
		}

		private void OnMenuClose(object sender, RoutedEventArgs e)
		{
			this.Close();
			Application.Current.Shutdown();
		}
	}
}
