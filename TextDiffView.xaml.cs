using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitSharp.Demo
{
	public partial class TextDiffView : UserControl
	{
		public TextDiffView()
		{
			InitializeComponent();
			DataContext = this;
		}

		public void Init(Diff diff)
		{
			Diff = diff;
			ListA.ItemsSource = diff.Sections;
			ListB.ItemsSource = diff.Sections;
		}

		public Diff Diff
		{
			get;
			private set;
		}

		internal void Show(Change change)
		{
			if (change == null)
			{
				Clear();
				return;
			}
			var a = (change.ReferenceObject != null ? (change.ReferenceObject as Blob).RawData : new byte[0]);
			var b = (change.ComparedObject != null ? (change.ComparedObject as Blob).RawData : new byte[0]);
			Init(new Diff(a, b));
		}

		private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (sender == m_scrollview_A)
			{
				m_scrollview_B.ScrollToVerticalOffset(m_scrollview_A.VerticalOffset);
				m_scrollview_B.ScrollToHorizontalOffset(m_scrollview_A.HorizontalOffset);
				return;
			}
			if (sender == m_scrollview_B)
			{
				m_scrollview_A.ScrollToVerticalOffset(m_scrollview_B.VerticalOffset);
				m_scrollview_A.ScrollToHorizontalOffset(m_scrollview_B.HorizontalOffset);
				return;
			}
		}

		/// <summary>
		/// Work around a limitation in the framework that does not allow to stretch ListBoxItemTemplates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void StretchDataTemplate(object sender, RoutedEventArgs e)
		{
			// found this method at: http://silverlight.net/forums/p/18918/70469.aspx#70469
			var t = sender as FrameworkElement;
			if (t == null)
				return;
			var p = VisualTreeHelper.GetParent(t) as ContentPresenter;
			if (p == null)
				return;
			p.HorizontalAlignment = HorizontalAlignment.Stretch;
		}


		internal void Clear()
		{
			ListA.ItemsSource = null;
			ListB.ItemsSource = null;
		}
	}

	/// <summary>
	/// Adjust text block sizes to correspond to each other by adding lines
	/// </summary>
	public class BlockTextConverterA : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var section = value as Diff.Section;
			if (section == null)
				return 0;
			var a_lines = section.EndA - section.BeginA;
			var b_lines = section.EndB - section.BeginB;
			var line_difference = Math.Max(a_lines, b_lines) - a_lines;
			var s = new StringBuilder(Regex.Replace(section.TextA, "\r?\n$", ""));
			if (a_lines == 0)
				line_difference -= 1;
			for (var i = 0; i < line_difference; i++)
				s.AppendLine();
			return s.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Adjust text block sizes to correspond to each other by adding lines
	/// </summary>
	public class BlockTextConverterB : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var section = value as Diff.Section;
			if (section == null)
				return 0;
			var a_lines = section.EndA - section.BeginA;
			var b_lines = section.EndB - section.BeginB;
			var line_difference = Math.Max(a_lines, b_lines) - b_lines;
			var s = new StringBuilder(Regex.Replace(section.TextB, "\r?\n$", ""));
			if (b_lines == 0)
				line_difference -= 1;
			for (var i = 0; i < line_difference; i++)
				s.AppendLine();
			return s.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Calculate block background color
	/// </summary>
	public class BlockColorConverterA : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var section = value as Diff.Section;
			if (section == null)
				return Brushes.Pink; // <-- this shouldn't happen anyway
			switch (section.EditWithRespectToA)
			{
				case Diff.EditType.Deleted:
					return Brushes.LightSkyBlue;
				case Diff.EditType.Replaced:
					return Brushes.LightSalmon;
				case Diff.EditType.Inserted:
					return Brushes.DarkGray;
				case Diff.EditType.Unchanged:
				default:
					return Brushes.White;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Calculate block background color
	/// </summary>
	public class BlockColorConverterB : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var section = value as Diff.Section;
			if (section == null)
				return Brushes.Pink; // <-- this shouldn't happen anyway
			switch (section.EditWithRespectToA)
			{
				case Diff.EditType.Deleted:
					return Brushes.DarkGray;
				case Diff.EditType.Replaced:
					return Brushes.LightSalmon;
				case Diff.EditType.Inserted:
					return Brushes.LightGreen;
				case Diff.EditType.Unchanged:
				default:
					return Brushes.White;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}


