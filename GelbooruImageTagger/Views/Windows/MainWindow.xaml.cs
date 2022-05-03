using GelbooruImageTagger.Models;
using GelbooruImageTagger.ViewModels;
using GelbooruImageTagger.Views.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GelbooruImageTagger.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && this.DataContext is MainViewModel vm)
            {
                vm.RefreshSelection(listView.SelectedItems.Cast<GelbooruImage>());
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                ProcessStartInfo startInfo = new()
                {
                    UseShellExecute = true,
                    FileName = e.Uri.AbsoluteUri
                };

                Process.Start(startInfo);
                e.Handled = true;
            }
        }
    }
}
