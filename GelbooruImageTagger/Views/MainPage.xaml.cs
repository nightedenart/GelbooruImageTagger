using GelbooruImageTagger.Views;
using GelbooruImageTagger.Views.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GelbooruImageTagger.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            Window.Current.SetTitleBar(AppTitleBarRegion);

            ContentFrame.Navigate(typeof(ListPage));
            ContentFrame.Navigated += ContentFrame_Navigated;

            BackButton.Opacity = 0;
            CaptionPanel.RenderTransform = new TranslateTransform() { X = -32 };
        }

        public void RefreshNavigationState()
        {
            Storyboard backButtonStoryboard = new Storyboard();
            DoubleAnimation backButtonOpacityAnimation = new DoubleAnimation();
            backButtonOpacityAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(83));
            Storyboard.SetTarget(backButtonOpacityAnimation, BackButton);
            Storyboard.SetTargetProperty(backButtonOpacityAnimation, "Opacity");
            backButtonStoryboard.Children.Add(backButtonOpacityAnimation);

            CaptionPanel.RenderTransform = new TranslateTransform();
            Storyboard captionStoryboard = new Storyboard();
            DoubleAnimation captionAnimation = new DoubleAnimation();
            captionAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(333));
            captionAnimation.EasingFunction = new QuinticEase() { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTarget(captionAnimation, CaptionPanel);
            Storyboard.SetTargetProperty(captionAnimation, "(StackPanel.RenderTransform).(TranslateTransform.X)");
            captionStoryboard.Children.Add(captionAnimation);

            if (ContentFrame.CurrentSourcePageType == typeof(SettingPage))
            {
                BackButton.IsEnabled = true;

                backButtonOpacityAnimation.From = 0;
                backButtonOpacityAnimation.To = 1;
                backButtonStoryboard.Begin();

                captionAnimation.From = -32;
                captionAnimation.To = 0;
                captionStoryboard.Begin();

            }
            else
            {
                BackButton.IsEnabled = false;

                backButtonOpacityAnimation.From = 1;
                backButtonOpacityAnimation.To = 0;
                backButtonStoryboard.Begin();

                captionAnimation.From = 0;
                captionAnimation.To = -32;
                captionStoryboard.Begin();
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            RefreshNavigationState();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CurrentSourcePageType == typeof(SettingPage))
            {
                ContentFrame.GoBack();
            }
        }
    }
}
