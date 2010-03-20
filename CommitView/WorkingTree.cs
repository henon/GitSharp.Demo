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

namespace GitSharp.Demo.CommitView
{
	public class WorkingTree
	{
		public WorkingTree(string path, RepositoryStatus status)
		{
			Path = path;
			RelativePath = "";
			RepositoryStatus = status;
		}

		public RepositoryStatus RepositoryStatus
		{
			get; set;
		}

		public string Path
		{
			get;
			private set;
		}

		protected virtual DirectoryInfo DirectoryInfo
		{
			get
			{
				return new DirectoryInfo(Path);
			}
		}

		public string Name
		{
			get
			{
				return DirectoryInfo.Name;
			}
		}

		public string RelativePath
		{
			get; private set;
		}

		public IEnumerable<WorkingTree> Children
		{
			get {
				return DirectoryInfo.GetDirectories().Select(d => new WorkingTree(d.FullName, RepositoryStatus) {RelativePath = System.IO.Path.Combine(RelativePath, d.Name)}).Concat
					(
						DirectoryInfo.GetFiles().Select(f => new WorkingFile(f.FullName, RepositoryStatus) { RelativePath = System.IO.Path.Combine(RelativePath, f.Name) } as WorkingTree)
					);
			}
		}

		public virtual string Status
		{
			get { return "";  }
		}
	}

	public class WorkingFile : WorkingTree
	{
		public WorkingFile(string path, RepositoryStatus status)
			: base(path, status)
		{
		}


		protected override DirectoryInfo DirectoryInfo
		{
			get
			{
				return new FileInfo(Path).Directory;
			}
		}

		FileInfo FileInfo
		{
			get
			{
				return new FileInfo(Path);
			}
		}

		public string Name
		{
			get
			{
				return FileInfo.Name;
			}
		}

		public IEnumerable<WorkingTree> Children
		{
			get { return new WorkingTree[0]; }
		}

		public override string Status
		{
			get
			{
				if (RepositoryStatus.Added.Contains(RelativePath))
					return "Added";
				if (RepositoryStatus.MergeConflict.Contains(RelativePath))
					return "MergeConflict";
				if (RepositoryStatus.Missing.Contains(RelativePath))
					return "Missing";
				if (RepositoryStatus.Modified.Contains(RelativePath))
					return "Modified";
				if (RepositoryStatus.Removed.Contains(RelativePath))
					return "Removed";
				if (RepositoryStatus.Staged.Contains(RelativePath))
					return "Staged";
				if (RepositoryStatus.Untracked.Contains(RelativePath))
					return "Untracked";
				return "";
			}
		}
	}
}
