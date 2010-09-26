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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GitSharp.Demo.HistoryGraph
{
    /// <summary>
    /// UserControl that lists Commits and provides selection
    /// </summary>
    public partial class HistoryGraphView : IRepositoryView
    {
        public event Action<Commit> CommitClicked;

        private Repository m_repo;
        private PlotWalk m_revwalk;

        public HistoryGraphView()
        {
            InitializeComponent();
        }

        public void Update(Repository repo)
        {
            m_repo = repo;
            var list = new PlotCommitList();
            m_revwalk = new PlotWalk(repo);
            m_revwalk.markStart(((Core.Repository)repo).getAllRefsByPeeledObjectId().Keys.Select(id => m_revwalk.parseCommit(id)));
            list.Source(m_revwalk);
            list.fillTo(1000);
            lstCommits.ItemsSource = list;
        }

        private void lstCommits_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PlotCommit commit = lstCommits.SelectedItem as PlotCommit;
            if (CommitClicked == null || commit == null)
                return;
            var c = new Commit(m_repo, commit.Name);
            CommitClicked(c);
        }
    } // END CLASS: HistoryGraphView

    
    /// <summary>
    /// FrameworkElement that renders a PlotCommit
    /// </summary>
    public class PlotCommitElement : FrameworkElement //Would it be better to ext TextBlock
    {
        public static DependencyProperty CurrentCommitProperty;
        public PlotCommit CurrentCommit
        {
            get { return (PlotCommit)GetValue(CurrentCommitProperty); }
            set { SetValue(CurrentCommitProperty, value); }
        }

        private DrawingContextPlotRender _Render;

        public PlotCommitElement()
        {
            const int LineHeight = 20;
            // At moment only grabs text styling at creation, could grab dynamically I suppose
            Typeface CurTp = new Typeface((FontFamily)GetValue(TextBlock.FontFamilyProperty),
                (FontStyle)GetValue(TextBlock.FontStyleProperty), (FontWeight)GetValue(TextBlock.FontWeightProperty),
                (FontStretch)GetValue(TextBlock.FontStretchProperty));
            this.Height = LineHeight;
            _Render = new DrawingContextPlotRender(LineHeight, CurTp);
        }
        static PlotCommitElement()
        {
            CurrentCommitProperty = DependencyProperty.Register("CurrentCommit", typeof(PlotCommit),
                typeof(PlotCommitElement), new PropertyMetadata(null, new PropertyChangedCallback(OnCurrentCommitChanged)));
        }

        private static void OnCurrentCommitChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PlotCommitElement that = sender as PlotCommitElement;
            if (that != null)
            {
                that.ToolTip = that.CurrentCommit.getAuthorIdent().ToString();
                that.InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            // Does this force a 2nd draw? Would it be more efficient if fixed size?
            this.Width = _Render.DrawPlotCommit(CurrentCommit, dc);
        }
    } // END CLASS: PlotCommitElement

    
    /// <summary>
    /// PlotRenderer that draws to a DrawingContext
    /// </summary>
    public class DrawingContextPlotRender : AbstractPlotRenderer<Brush>
    {
        public int Height;              // Overall Height of the Block we are drawing to
        public Typeface CurTypeface;    // For text drawing - Typeface
        public double FontSize;         // For text drawing - Size
        public double LabelMaxWidth;    // For label drawing - limits the width
        public int LabelMargin;         // For label drawing - trailing space after label
        private DrawingContext _DC;
        private double _MaxX;

        public DrawingContextPlotRender()
        {
            LabelMaxWidth = 100;
            LabelMargin = 2;
        }
        public DrawingContextPlotRender(int H, Typeface Tp)
            : this()
        {
            Height = H;
            FontSize = H / 2;   // assume font is half the height
            CurTypeface = Tp;
        }

        /// <summary>
        /// Draws the given PlotCommit to given DrawingContext using class's parameters
        /// </summary>
        /// <param name="Cmt">PlotCommit to render</param>
        /// <param name="CurDC">DrawingContext to use</param>
        /// <returns>Maximum width used</returns>
        public double DrawPlotCommit(PlotCommit Cmt, DrawingContext CurDC)
        {
            if (Cmt == null) return 0;
            _DC = CurDC;    // setup private variables for render process
            _MaxX = 0;
            paintCommit(Cmt, Height);
            _DC = null;     // DC likely won't be valid after this, don't hang on to it
            return _MaxX;
        }

        #region Overrides of AbstractPlotRenderer

        protected override int drawLabel(int x, int y, Core.Ref @ref)
        {
            FormattedText Tx = new FormattedText(@ref.Name, System.Globalization.CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, CurTypeface, FontSize, Brushes.White);
            Tx.MaxTextWidth = LabelMaxWidth;    //limit width of label
            Tx.MaxTextHeight = Height;
            // need to draw color background
            Point TxOrg = new Point(x, y - FontSize / 2); // given y is center, need top
            Point TxEnd = new Point(TxOrg.X + Tx.Width, TxOrg.Y + Tx.Height);
            _DC.DrawRectangle(Brushes.CornflowerBlue, null, new Rect(TxOrg, TxEnd));
            _DC.DrawText(Tx, TxOrg);
            if (_MaxX < TxEnd.X) _MaxX = TxEnd.X; //push out max X
            return (int)Math.Ceiling(Tx.Width) + LabelMargin;
        }

        protected override void drawText(string msg, int x, int y)
        {
            FormattedText Tx = new FormattedText(msg, System.Globalization.CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, CurTypeface, FontSize, Brushes.White);
            Tx.MaxTextHeight = Height;
            double Xend = Tx.Width + x;
            _DC.DrawText(Tx, new Point(x, y - FontSize / 2)); // given y is center, need top
            if (_MaxX < Xend) _MaxX = Xend; //push out max X
        }

        protected override Brush laneColor(PlotLane my_lane)
        {
            return Brushes.DarkCyan;
        }

        protected override void drawLine(Brush color, int x1, int y1, int x2, int y2, int width)
        {
            Pen MyPen = new Pen(color, width);
            Point P0 = new Point(x1, y1);
            Point P1 = new Point(x2, y2);
            _DC.DrawLine(MyPen, P0, P1);
        }

        protected override void drawCommitDot(int x, int y, int w, int h)
        {
            double Rx = w / 2;  // convert width/height to radius
            double Ry = h / 2;
            _DC.DrawEllipse(Brushes.DarkCyan, null, new Point(x + Rx, y + Ry), Rx, Ry);
        }

        protected override void drawBoundaryDot(int x, int y, int w, int h)
        {
            double Rx = w / 2;  // convert width/height to radius
            double Ry = h / 2;
            _DC.DrawEllipse(Brushes.Red, null, new Point(x + Rx, y + Ry), Rx, Ry);
        }

        #endregion

    } // END CLASS: DrawingContextPlotRender

} // END NAMESPACE
