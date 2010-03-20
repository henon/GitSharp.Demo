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
using GitSharp.Demo.CommitView;


namespace GitSharp.Demo
{

	public partial class BrowserView : IRepositoryView
	{
		public BrowserView()
		{
			InitializeComponent();
			m_tree.SelectedItemChanged += (o, args) => SelectObject(m_tree.SelectedValue as AbstractObject);
			m_commit_diff.SelectionChanged += change => m_text_diff.Show(change);
			m_history_graph.CommitClicked += SelectCommit;
		}

		public Repository Repository { get; private set; }

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

		private void SelectCommit(Commit commit)
		{
			if (commit == null || commit.Tree == null)
				return;
			m_commit_view.Commit = commit;
			m_tree.ItemsSource = commit.Tree.Children;
			m_tree_title.Content = "Repository tree of Commit " + commit.ShortHash;
			m_commit_diff.Init(commit.Parent, commit);
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

		#region IRepositoryView Members

		public void Update(Repository repository)
		{
			Repository = repository;
			SelectCommit(Repository.Head.CurrentCommit);
			m_history_graph.Update(Repository);
		}

		#endregion
	}
}
