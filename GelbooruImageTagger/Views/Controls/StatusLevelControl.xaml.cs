using GelbooruImageTagger.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GelbooruImageTagger.Views.Controls
{
    public sealed partial class StatusLevelControl : UserControl
    {
        public StatusLevelControl()
        {
            this.InitializeComponent();
            this.UpdateLevel();
        }


        public GelbooruImageStatus Level
        {
            get { return (GelbooruImageStatus)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register("Level", typeof(GelbooruImageStatus), typeof(StatusLevelControl), new PropertyMetadata(GelbooruImageStatus.None, LevelPropertyChanged));

        private static void LevelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatusLevelControl control)
            {
                control.UpdateLevel();
            }
        }

        public SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }

        private void UpdateLevel()
        {
            switch (Level)
            {
                case GelbooruImageStatus.None:

                    this.LevelBorder.BorderThickness = new Thickness(1);
                    this.LevelBorder.BorderBrush = GetSolidColorBrush("#FF64748B");
                    this.LevelBorder.Background = GetSolidColorBrush("#00000000");
                    break;
                case GelbooruImageStatus.Success:
                    this.LevelBorder.BorderThickness = new Thickness(0);
                    this.LevelBorder.Background = GetSolidColorBrush("#FF10B981");
                    break;
                case GelbooruImageStatus.Warning:
                    this.LevelBorder.BorderThickness = new Thickness(0);
                    this.LevelBorder.Background = GetSolidColorBrush("#FFEAB308");
                    break;
                case GelbooruImageStatus.Error:
                    this.LevelBorder.BorderThickness = new Thickness(0);
                    this.LevelBorder.Background = GetSolidColorBrush("#FFF43F5E");
                    break;
                case GelbooruImageStatus.PartialSuccess:
                    this.LevelBorder.BorderThickness = new Thickness(1);
                    this.LevelBorder.BorderBrush = GetSolidColorBrush("#FF10B981");
                    this.LevelBorder.Background = GetSolidColorBrush("#00000000");
                    break;
            }
        }

    }
}
