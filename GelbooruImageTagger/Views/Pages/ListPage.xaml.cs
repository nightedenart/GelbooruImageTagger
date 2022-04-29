using GelbooruImageTagger.Models;
using GelbooruImageTagger.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GelbooruImageTagger.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListPage : Page
    {
        public ListPage()
        {
            this.InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm && sender is ListView listView)
            {
                vm.SelectedGelbooruImage = (GelbooruImage)listView.SelectedItem;

                if (listView.SelectedItems.Count > 0)
                {
                    vm.AddRangeObservable(vm.SelectedGelbooruImages, listView.SelectedItems.Cast<GelbooruImage>().ToArray(), true);
                }
                else
                {
                    vm.SelectedGelbooruImages.Clear();
                }

                vm.RefreshSelection();
            }
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                if (parent is T correctlyTyped)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            Frame parentFrame = FindVisualParent<Frame>(SettingButton);

            if (parentFrame != null)
            {
                parentFrame.Navigate(typeof(SettingPage));
            }
        }
    }
}
