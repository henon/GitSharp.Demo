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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace GitSharp.Demo.CommitView
{
	/// <summary>
	/// Interaction logic for StatusWindow.xaml
	/// </summary>
	public partial class StatusView : IRepositoryView
	{
		public StatusView()
		{
			InitializeComponent();
			m_status_list.SelectionChanged += OnStatusViewSelectionChanged;
		}

		public void Update(Repository repo)
		{
			Repository = repo;
			m_status_list.Update(repo);
		}

		private void OnStatusViewSelectionChanged(IEnumerable<PathStatus> status_paths)
		{
			var status_path = status_paths.FirstOrDefault();
			if (status_path == null)
				return;
			byte[] a = new byte[0];
			var a_path = Path.Combine(Repository.WorkingDirectory, status_path.Path);
			if (new FileInfo(a_path).Exists)
			{
				if (Diff.IsBinary(a_path) == true)
					a = Encoding.ASCII.GetBytes("Binary content\nFile size: " + new FileInfo(a_path).Length);
				else
					a = File.ReadAllBytes(a_path);
			}
			byte[] b = new byte[0];
			var blob = Repository.Index[status_path.Path];
			if (blob != null)
				if (Diff.IsBinary(blob.RawData) == true)
					b = Encoding.ASCII.GetBytes("Binary content\nFile size: " + blob.RawData.Length);
				else
					b = blob.RawData;
			m_diff_view.Init(new Diff(a, b));
		}

		public Repository Repository
		{
			get;
			private set;
		}

		private void OnCommitButtonClick(object sender, RoutedEventArgs e)
		{
			m_status_list.StartCommitDialog();
		}
	}
}
