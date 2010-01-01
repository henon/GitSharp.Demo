using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GitSharp;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;


namespace GitSharp.Demo
{

    public partial class Browser : Window
    {
        public Browser()
        {
            InitializeComponent();
            m_commits.SelectionChanged += (o, args) => SelectCommit(m_commits.SelectedItem as Commit);
            //m_branches.SelectionChanged += (o, args) => SelectBranch(m_branches.SelectedItem as Branch);
            m_refs.SelectionChanged += (o, args) => SelectRef(m_refs.SelectedItem as Ref);
            m_tree.SelectedItemChanged += (o, args) => SelectObject(m_tree.SelectedValue as AbstractObject);
            //m_config_tree.SelectedItemChanged += (o, args) => SelectConfiguration(m_config_tree.SelectedItem);
        }

        Configuration configurationWindow = new Configuration();
        Repository m_repository;

        // load
        private void OnLoadRepository(object sender, RoutedEventArgs e)
        {
            var url = m_url_textbox.Text;
            var repo = new Repository(url);
            var head = repo.Head.Target as Commit;
            Debug.Assert(head is Commit);
            m_repository = repo;
            //var tags = repo.getTags().Values.Select(@ref => repo.MapTag(@ref.Name, @ref.ObjectId));
            //var branches = repo.Branches.Values.Select(@ref => repo.MapCommit(@ref.ObjectId));
            m_refs.ItemsSource = repo.Refs.Values;
            DisplayCommit(head, "HEAD");
            //ReloadConfiguration();
            configurationWindow.Init(m_repository);
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
                m_object_title.Text = "Content of " + blob.Path;
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
                Debug.Fail("don't know how to display this object: "+obj.ToString());
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
            //dlg.CheckPathExists = true;
            if (dlg.ShowDialog() ==  System.Windows.Forms.DialogResult.OK)
            {
                m_url_textbox.Text = dlg.SelectedPath;
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
            var list = commit.Ancestors.ToList();
            list.Insert(0, commit);
            m_commits.ItemsSource = list;
            m_commits.SelectedIndex = 0;
            m_commit_title.Text = "Commit history for " + info;
        }

        private void SelectCommit(Commit commit)
        {
            if (commit == null)
                return;
            m_tree.ItemsSource = commit.Tree.Children;
            m_tree_title.Text = "Repository tree of Commit " + commit.ShortHash;
            //(m_tree.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem).IsExpanded = true;
        }

    

        private void OnDiffSelectedCommits(object sender, RoutedEventArgs e)
        {
            var selection = m_commits.SelectedItems;
            if (selection.Count < 2)
                return;
            var first_two=selection.Cast<Commit>().Take(2).ToArray();
            var commit_diff = new CommitDiff();
            commit_diff.Init(first_two[0], first_two[1]);
            commit_diff.Show();
        }

      

        private void OnClose(object sender, RoutedEventArgs e)
        {
            //Closing application
            this.Close();
        }

        private void OnOpenRepositoryConfiguration(object sender, RoutedEventArgs e)
        {
           
            
            configurationWindow.Show();
        }
    }
}
