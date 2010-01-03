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

namespace GitSharp.Demo.HistoryGraph
{
    /// <summary>
    /// Extremely simplistic selection with different strategy implementations (without interfaces, inheritance, composition and other complications that just get in the way). 
    /// Just supply "select" and "unselect" expressions and have fun.
    ///
    /// Usage: 
    /// 1) Implement OnSelect and OnUnselect to control graphic representation of item selection by setting the actions
    /// 2) Implement IsCtrlDown if you need multi-item-selection toggled by CTRL key
    /// 3) Use one of the preconfigured standard strategies or implement SelectionStrategy for your custom selection mechanics.
    /// 4) Update the selection by calling Update, Select or Unselect
    /// 
    /// Standard implementations are provided:
    /// 
    /// * Standard
    /// * Exclusive
    /// * Toggle
    /// </summary>
    /// <typeparam name="T">Type of the elements of the selection</typeparam>
    public class Selection<T>
    {
        public event Action SelectionChanged;

        /// <summary>
        /// Contains the currently selected items.
        /// </summary>
        public HashSet<T> SelectedItems
        {
            get { return m_selected_items; }
        }
        private readonly HashSet<T> m_selected_items = new HashSet<T>(); 


        #region --> Actions to be supplied by user


        /// <summary>
        /// supply a lambda to select an item
        /// </summary>
        public Action<T> OnSelect;

        /// <summary>
        /// supply a lambda to unselect an item
        /// </summary>
        public Action<T> OnUnselect;

        /// <summary>
        /// supply a function to find out about CTRL key status
        /// </summary>
        public Func<bool> IsCtrlDown;

        /// <summary>
        /// Selection mechanics strategy. Supply the selection update procedure here, or use one of the static standard procedures that have this action preconfigured. 
        /// The calls to Update are forwarded to this delegate.
        /// 
        /// Input: A list of clicked/or otherwise selected items. It is expected that this routine manipulates the SelectedItems accordingly. 
        /// </summary>
        public Action<IEnumerable<T>> SelectionStrategy;


        #endregion

        #region --> Extern selecton manipulation interface


        /// <summary>
        /// Call this to update the selection if the selection status for an item or many items has been changed by the user
        /// </summary>
        public void Update(params T[] items)
        {
            SelectionStrategy(items);
        }

        /// <summary>
        /// Call this to update the selection if the selection status for an item or many items has been changed by the user
        /// </summary>
        public void Update(IEnumerable<T> items)
        {
            SelectionStrategy(items);
        }

        /// <summary>
        /// Empty the selection.
        /// </summary>
        public void Clear()
        {
            foreach (var item in m_selected_items)
                OnUnselect(item);
            m_selected_items.Clear();
            if (SelectionChanged != null) SelectionChanged();
        }

        /// <summary>
        ///  Select the specified item
        /// </summary>
        /// <param name="item"></param>
        public void Select(T item)
        {
            Select(item, true);
        }

        /// <summary>
        ///  Select the specified item and notify about the change by calling SelectionChanged
        /// </summary>
        /// <param name="item"></param>
        public void Select(T item, bool notify)
        {
            m_selected_items.Add(item);
            OnSelect(item);
            if (notify && SelectionChanged != null) SelectionChanged();
        }

        /// <summary>
        ///  Select the specified items
        /// </summary>
        /// <param name="item"></param>
        public void Select(IEnumerable<T> items)
        {
            foreach (var item in items)
                Select(item, false);
            if (SelectionChanged != null) SelectionChanged();
        }

        /// <summary>
        ///  Unselect the specified item
        /// </summary>
        /// <param name="item"></param>
        public void Unselect(T item)
        {
            Unselect(item, true);
        }

        /// <summary>
        ///  Unselect the specified item and notify about the change by calling SelectionChanged
        /// </summary>
        /// <param name="item"></param>
        public void Unselect(T item, bool notify)
        {
            m_selected_items.Remove(item);
            OnUnselect(item);
            if (notify && SelectionChanged != null) SelectionChanged();
        }

        /// <summary>
        ///  Unselect the specified items
        /// </summary>
        /// <param name="item"></param>
        public void UnSelect(IEnumerable<T> items)
        {
            foreach (var item in items)
                Unselect(item, false);
            if (SelectionChanged != null) SelectionChanged();
        }


#endregion

        /// <summary>
        /// Check an item for its selection status
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsSelected(T item)
        {
            return m_selected_items.Contains(item);
        }

        #region --> Preconfigured Selections


        /// <summary>
        /// This selection works like a group of checkbuttons
        /// </summary>
        public static Selection<T> ToggleSelecton()
        {
            var selecton = new Selection<T>();
            selecton.SelectionStrategy = new Action<IEnumerable<T>>(items =>
                {
                    foreach (var item in items)
                    {
                        if (selecton.m_selected_items.Contains(item))
                        {
                            selecton.OnUnselect(item);
                            selecton.m_selected_items.Remove(item);
                        }
                        else
                        {
                            selecton.OnSelect(item);
                            selecton.m_selected_items.Add(item);
                        }
                    }
                    if (selecton.SelectionChanged != null) selecton.SelectionChanged();
                });
            return selecton;
        }

        /// <summary>
        /// This selection works like a group of coupled radiobutton
        /// </summary>
        public static Selection<T> ExclusiveSelection()
        {
            var selection = new Selection<T>();
            selection.SelectionStrategy = new Action<IEnumerable<T>>(items =>
                {
                    selection.Clear();
                    foreach (var item in items)
                    {
                        selection.OnSelect(item);
                        selection.m_selected_items.Add(item);
                    }
                    if (selection.SelectionChanged != null) selection.SelectionChanged();
                });
            return selection;
        }

        /// <summary>
        /// This selection works like file selection in windows explorer
        /// </summary>
        public static Selection<T> StandardSelection()
        {
            var selection = new Selection<T>();
            selection.SelectionStrategy = new Action<IEnumerable<T>>(items =>
                {
                    if (!selection.IsCtrlDown())
                        selection.Clear();
                    foreach (var item in items)
                    {
                        if (selection.m_selected_items.Contains(item))
                        {
                            selection.OnUnselect(item);
                            selection.m_selected_items.Remove(item);
                        }
                        else
                        {
                            selection.OnSelect(item);
                            selection.m_selected_items.Add(item);
                        }
                    }
                    if (selection.SelectionChanged != null) selection.SelectionChanged();
                });
            return selection;
        }


        #endregion
    }
}