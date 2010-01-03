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
using System.Linq;
using GitSharp.Core.RevPlot;

namespace GitSharp.Demo.HistoryGraph
{
    public partial class HistoryGraphView
    {
        public event Action<Commit> CommitClicked;

        private PlotRenderer m_plot_renderer;
        private Repository m_repo;
        private PlotWalk m_revwalk;
        private Selection<Commit> m_selection; 

        public HistoryGraphView()
        {
            InitializeComponent();
            m_plot_renderer = new PlotRenderer();
            m_plot_renderer.Init(m_canvas);
            m_plot_renderer.CommitClicked += OnCommitClicked;
            m_plot_renderer.LabelClicked += OnLabelClicked;
            m_selection = Selection<Commit>.ExclusiveSelection(); 
            m_selection.OnSelect = OnSelect;
            m_selection.OnUnselect = OnUnselect;
        }

        public void Update(Repository repo)
        {
            m_repo = repo;
            var list = new PlotCommitList();
            m_revwalk = new PlotWalk(repo);
            m_revwalk.markStart(((Core.Repository)repo).getAllRefsByPeeledObjectId().Keys.Select(id => m_revwalk.parseCommit(id)));
            list.Source(m_revwalk);
            list.fillTo(1000);
            m_plot_renderer.Update(list);
            //var rw = new RevWalk(repo);
            //rw.RevSortStrategy.Add(RevSort.Strategy.COMMIT_TIME_DESC);
            //rw.RevSortStrategy.Add(RevSort.Strategy.TOPO);
            //rw.markStart(((Core.Repository)repo).getAllRefsByPeeledObjectId().Keys.Select(id => rw.parseCommit(id)));
            //m_renderer.Update(rw);
        }

        private void OnSelect(Commit c)
        {
            m_plot_renderer.Select(c.Hash);
        }

        private void OnUnselect(Commit c)
        {
            m_plot_renderer.Unselect(c.Hash);
        }

        private void OnCommitClicked(PlotCommit commit)
        {
            if (CommitClicked==null)
                return;
            var c = new Commit(m_repo, commit.Name);
            m_selection.Update(c);
            CommitClicked(c);
        }

        private void OnLabelClicked(Core.Ref @ref)
        {
            
        }
    }
}
