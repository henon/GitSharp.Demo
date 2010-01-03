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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GitSharp.Core.RevPlot;

namespace GitSharp.Demo.HistoryGraph
{
    public class PlotRenderer : AbstractPlotRenderer<Brush>
    {
        public void Init(Canvas canvas)
        {
            Canvas = canvas;
        }

        private Dictionary<string, TextBlock> m_texts = new Dictionary<string, TextBlock>();
        private Dictionary<string, Ellipse> m_dots = new Dictionary<string, Ellipse>();

        private Canvas Canvas
        {
            get;
            set;
        }

        public void Update(PlotCommitList list)
        {
            m_line = 0;
            Canvas.Children.Clear();
            m_plot_commits.Clear();
            m_texts.Clear();
            m_dots.Clear();
            foreach (var commit in list)
            {
                paintCommit(commit, LINE_HEIGHT);
            }
            Canvas.Height = (m_line + 1) * LINE_HEIGHT;
        }

        #region Overrides of AbstractPlotRenderer

        private int m_line = 0;

        protected override void paintCommit(PlotCommit commit, int h)
        {
            m_plot_commits.Add( commit);
            base.paintCommit(commit, h);
            m_line++;
        }

        private readonly List<PlotCommit> m_plot_commits = new List<PlotCommit>();
        private const int TEXT_OFFSET = 8;
        private const int LINE_HEIGHT = 20;

        protected override int drawLabel(int x, int y, Core.Ref @ref)
        {
            var child = new TextBlock { Text = @ref.Name, Width = 100, TextTrimming = TextTrimming.CharacterEllipsis, Background = Brushes.CornflowerBlue, Tag = @ref };
            child.SetValue(Canvas.LeftProperty, (double)x);
            child.SetValue(Canvas.TopProperty, (double)y - TEXT_OFFSET + VerticalOffset);
            child.PreviewMouseDown += OnLabelClick;
            Canvas.Children.Add(child);
            return 102; // <--- returning with of label
        }

        private int VerticalOffset
        {
            get { return m_line * LINE_HEIGHT; }
        }

        private PlotCommit CurrentCommit
        {
            get
            {
                return m_plot_commits[m_line];
            }
        }

        protected override Brush laneColor(PlotLane my_lane)
        {
            return Brushes.Black;
        }

        protected override void drawLine(Brush color, int x1, int y1, int x2, int y2, int width)
        {
            var child = new Line { X1 = x1, Y1 = y1 + VerticalOffset, X2 = x2, Y2 = y2 + VerticalOffset, StrokeThickness = width, Stroke = color, Tag = CurrentCommit };
            Canvas.Children.Add(child);
        }

        protected override void drawCommitDot(int x, int y, int w, int h)
        {
            var child = new Ellipse { Width = w, Height = h, Fill = Brushes.Black, Tag = CurrentCommit, ToolTip = CurrentCommit.Name };
            child.SetValue(Canvas.LeftProperty, (double)x);
            child.SetValue(Canvas.TopProperty, (double)y + VerticalOffset);
            child.PreviewMouseDown += OnCommitClick;
            Canvas.Children.Add(child);
            m_dots[CurrentCommit.Name] = child;
        }

        protected override void drawBoundaryDot(int x, int y, int w, int h)
        {
            var child = new Ellipse { Width = w, Height = h, Fill = Brushes.Red, Tag = CurrentCommit };
            child.SetValue(Canvas.LeftProperty, (double)x);
            child.SetValue(Canvas.TopProperty, (double)y + VerticalOffset);
            Canvas.Children.Add(child);
        }

        protected override void drawText(string msg, int x, int y)
        {
            var child = new TextBlock { Text = msg, Tag = CurrentCommit, ToolTip = CurrentCommit.getAuthorIdent().ToString() };
            child.SetValue(Canvas.LeftProperty, (double)x);
            child.SetValue(Canvas.TopProperty, (double)y - TEXT_OFFSET + VerticalOffset);
            child.PreviewMouseDown += OnCommitClick;
            Canvas.Children.Add(child);
            m_texts[CurrentCommit.Name] = child;
        }

        private void OnCommitClick(object sender, MouseButtonEventArgs e)
        {
           if (CommitClicked == null)
               return;
            var plot_commit = (sender as FrameworkElement).Tag as PlotCommit;
            if (plot_commit == null)
                return;
            CommitClicked(plot_commit);
        }

        private void OnLabelClick(object sender, MouseButtonEventArgs e)
        {
            if (LabelClicked == null)
                return;
            var @ref = (sender as FrameworkElement).Tag as Core.Ref;
            if (@ref == null)
                return;
            LabelClicked(@ref);
            
        }


        #endregion

        public event Action<PlotCommit> CommitClicked;
        public event Action<Core.Ref> LabelClicked;


        internal void Select(string hash)
        {
            if (!m_dots.ContainsKey(hash) || ! m_texts.ContainsKey(hash))
                return;
            //m_dots[hash].Fill = Brushes.Silver;
            m_texts[hash].Background = Brushes.Silver;
        }

        internal void Unselect(string hash)
        {
            if (!m_dots.ContainsKey(hash) || !m_texts.ContainsKey(hash))
                return;
            //m_dots[hash].Fill = Brushes.Black;
            m_texts[hash].Background = Brushes.Transparent;
        }
    }
}
