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
using System.Windows.Shapes;
using Git;

namespace GitSharp.Demo
{
    /// <summary>
    /// Interaction logic for CommitDiff.xaml
    /// </summary>
    public partial class CommitDiff : Window
    {
        public CommitDiff()
        {
            InitializeComponent();
        }

        public void Init(Commit c1, Commit c2)
        {
            m_title.Content = "Differences between commits " + c1.ShortHash + " and " + c2.ShortHash;
            var changes=c1.CompareAgainst(c2);
            m_treediff.ItemsSource = changes;
        }
    }

    public class ChangeColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var change = value as Change;
            if (change == null)
                return Brushes.Black;
            switch (change.ChangeName)
            {
                case "Added":
                    return Brushes.Plum;
                case "Deleted":
                    return Brushes.Red;
                case "Modified":
                    return Brushes.RoyalBlue;
                default:
                    return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
