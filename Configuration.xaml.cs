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
using GitSharp;
namespace GitSharp.Demo
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configuration : Window
    {

        public Configuration()
        {
            InitializeComponent();
        }
        public void Init(GitSharp.Repository repository)
        {
            configurationList.ItemsSource = repository.Config;
        }

        private void OnLoadConfiguration(object sender, RoutedEventArgs e)
        {
           //Not Implemented 
        }

        private void OnSaveConfiguration(object sender, RoutedEventArgs e)
        {
            //Not Implemented 
        }
    }
}
