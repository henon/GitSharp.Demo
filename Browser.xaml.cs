/*
 * Copyright (C) 2009, Henon <meinrad.recheis@gmail.com>
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
using System.IO;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Threading;


namespace GitSharp.Demo
{

	public partial class Browser
	{
		public const string CURRENT_REPOSITORY = "repository";
		public Browser()
		{
			InitializeComponent();
			//m_commits.SelectionChanged += (o, args) => SelectCommit(m_commits.SelectedItem as Commit);
			//m_branches.SelectionChanged += (o, args) => SelectBranch(m_branches.SelectedItem as Branch);
			//m_refs.SelectionChanged += (o, args) => SelectRef(m_refs.SelectedItem as Ref);
			m_tree.SelectedItemChanged += (o, args) => SelectObject(m_tree.SelectedValue as AbstractObject);
			m_commit_diff.SelectionChanged += change => m_text_diff.Show(change);
			//m_config_tree.SelectedItemChanged += (o, args) => SelectConfiguration(m_config_tree.SelectedItem);
			m_history_graph.CommitClicked += SelectCommit;
			m_url_textbox.Text = UserSettings.GetString(CURRENT_REPOSITORY);
			Loaded += (o, args) => Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => LoadRepository(m_url_textbox.Text)));
		}

		
		Repository m_repository;

		// load
		private void OnLoadRepository(object sender, RoutedEventArgs e)
		{
			var url = m_url_textbox.Text;
			LoadRepository(url);
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
			//var tags = repo.getTags().Values.Select(@ref => repo.MapTag(@ref.Name, @ref.ObjectId));
			//var branches = repo.Branches.Values.Select(@ref => repo.MapCommit(@ref.ObjectId));
			//m_refs.ItemsSource = repo.Refs.Values;
			SelectCommit(head);
			m_history_graph.Update(repo);			
		}

		private void SelectObject(AbstractObject node)
		{
			if (node.IsBlob)
			{
				var blob = node as Leaf;
				var text = blob.Data;
				m_object.Document.Blocks.Clear();
				var p = new Paragraph();
				p.Inlines.Add(text);
				m_object.Document.Blocks.Add(p);
				m_object_title.Content = "Content of " + blob.Path;
			}
			else
			{
				m_object.Document.Blocks.Clear();
			}
		}

		private void SelectBranch(object branch)
		{
			if (branch == null)
				return;
			//DisplayCommit(branch.Commit, "Branch "+branch.Name);
		}

		private void SelectRef(Ref r)
		{
			if (r == null)
				return;
			var obj = r.Target;
			if (obj.IsCommit)
			{
				DisplayCommit(obj as Commit, "Commit history of " + r.Name);
				return;
			}
			else if (obj.IsTag)
			{
				var tag = obj as Tag;
				if (tag.Target == tag) // it sometimes happens to have self referencing tags
				{
					return;
				}
				SelectTag(tag);
				return;
			}
			else if (obj.IsTree)
			{
				// hmm, display somehow
			}
			else if (obj.IsBlob)
			{
				// hmm, display somehow
			}
			else
			{
				Debug.Fail("don't know how to display this object: " + obj.ToString());
			}
		}

		private void SelectTag(Tag tag)
		{
			if (tag == null)
				return;
			if (tag.Target.IsCommit)
				DisplayCommit(tag.Target as Commit, "Commit history of Tag " + tag.Name);
			else
				SelectObject(tag.Target);
		}

		private void OnSelectRepository(object sender, RoutedEventArgs e)
		{
			var dlg = new System.Windows.Forms.FolderBrowserDialog();
			dlg.SelectedPath = Path.GetDirectoryName(UserSettings.GetString(CURRENT_REPOSITORY));
			//dlg.CheckPathExists = true;
			if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				m_url_textbox.Text = dlg.SelectedPath;
				LoadRepository(m_url_textbox.Text);
			}
		}

		//private void SelectConfiguration(object obj)
		//{
		//    if (obj is Entry)
		//    {
		//        var entry = obj as dotGit.Config.Entry;
		//        m_config_name.Content = entry.FullName;
		//        if (entry.Value != null)
		//            m_config_value.Text = entry.Value;
		//    }
		//}

		private void DisplayCommit(Commit commit, string info)
		{
			if (commit == null)
				return;
			//var list = commit.Ancestors.ToList();
			//list.Insert(0, commit);
			//m_commits.ItemsSource = list;
			//m_commits.SelectedIndex = 0;
		}

		private void SelectCommit(Commit commit)
		{
			if (commit == null || commit.Tree == null)
				return;
			m_commit_view.Commit = commit;
			m_tree.ItemsSource = commit.Tree.Children;
			m_tree_title.Content = "Repository tree of Commit " + commit.ShortHash;
			m_commit_diff.Init(commit.Parent, commit);
			//m_text_diff.Clear();
			//m_commit_title.Text = "Commit history for " + info;
			//(m_tree.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem).IsExpanded = true;
		}



		private void OnDiffSelectedCommits(object sender, RoutedEventArgs e)
		{
			//var selection = m_commits.SelectedItems;
			//if (selection.Count < 2)
			//    return;
			//var first_two=selection.Cast<Commit>().Take(2).ToArray();
			//var commit_diff = new CommitDiffView();
			//commit_diff.Init(first_two[0], first_two[1]);
			//commit_diff.ShowDialog();
		}



		private void OnMenuClose(object sender, RoutedEventArgs e)
		{
			this.Close();
		}


		private void OnOpenRepositoryConfiguration(object sender, RoutedEventArgs e)
		{
            Configuration configurationWindow = new Configuration();
            configurationWindow.Init(m_repository);
			configurationWindow.ShowDialog();
		}

		
	}
}
