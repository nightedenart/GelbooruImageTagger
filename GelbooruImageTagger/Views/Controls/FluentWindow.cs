using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace GelbooruImageTagger.Views.Controls
{
    public class FluentWindow : Window
    {
        #region Native Methods and Declarations

        [Flags]
        public enum DwmWindowAttribute : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_MICA_EFFECT = 1029
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        [DllImport("dwmapi.dll")]
        static extern bool DwmDefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref IntPtr plResult);
        [DllImport("dwmapi.dll")]
        static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);

        #endregion

        #region Enums
        public enum AppThemeColorMode
        {
            Light,
            Dark,
            System
        }
        #endregion

        #region Internal Fields

        private IntPtr _hwnd;

        #endregion

        #region Dependency Properties and Callbacks
        public AppThemeColorMode AppThemeColor
        {
            get { return (AppThemeColorMode)GetValue(AppThemeColorProperty); }
            set { SetValue(AppThemeColorProperty, value); }
        }
        public static readonly DependencyProperty AppThemeColorProperty =
            DependencyProperty.Register("AppThemeColor", typeof(AppThemeColorMode), typeof(FluentWindow), new UIPropertyMetadata(AppThemeColorMode.System, DependencyPropertyChangedCallback));
        public bool IsDarkMode
        {
            get { return (bool)GetValue(IsDarkModeProperty); }
            private set { SetValue(IsDarkModeProperty, value); }
        }
        public static readonly DependencyProperty IsDarkModeProperty =
            DependencyProperty.Register("IsDarkMode", typeof(bool), typeof(FluentWindow), new UIPropertyMetadata(false));

        public static void DependencyPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FluentWindow window)
                window.RefreshWindow();
        }

        #endregion

        #region Overrides

        protected override void OnSourceInitialized(EventArgs e)
        {
            _hwnd = new WindowInteropHelper(this).Handle;
            RefreshWindow();
            base.OnSourceInitialized(e);
        }

        #endregion

        #region Helper Methods

        #endregion

        #region Methods

        public void RefreshWindow()
        {
            int darkModeValue = 0;

            switch (AppThemeColor)
            {
                case AppThemeColorMode.Light:
                    IsDarkMode = false;
                    break;
                case AppThemeColorMode.Dark:
                    IsDarkMode = true;
                    break;
                case AppThemeColorMode.System:
                    try
                    {
                        using Microsoft.Win32.RegistryKey? key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Microsoft")?.OpenSubKey("Windows")?.OpenSubKey("CurrentVersion")?.OpenSubKey("Themes")?.OpenSubKey("Personalize");
                        if (key != null)
                        {
                            string? keyValue = key.GetValue("AppsUseLightTheme")?.ToString();
                            if (keyValue == "0")
                            {
                                IsDarkMode = true;
                            }
                        }
                    }
                    catch
                    {
                        IsDarkMode = false;
                    }
                    break;
            }

            if (IsDarkMode)
            {
                darkModeValue = 1;
            }

            _ = DwmSetWindowAttribute(_hwnd, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkModeValue, Marshal.SizeOf(typeof(int)));

        }

        #endregion

        static FluentWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FluentWindow),
                new FrameworkPropertyMetadata(typeof(FluentWindow)));
        }

    }
}
