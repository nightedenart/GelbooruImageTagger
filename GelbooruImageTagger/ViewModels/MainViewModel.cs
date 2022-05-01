using GelbooruImageTagger.Extensions;
using GelbooruImageTagger.Models;
using GelbooruImageTagger.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GelbooruImageTagger.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        #region Internal Fields
        #endregion

        #region Properties

        private bool _isReady = true;
        private ObservableCollection<GelbooruImage> _booruImages = new();

        public bool IsReady
        {
            get => _isReady;
            set => SetField(ref _isReady, value);
        }
        public ObservableCollection<GelbooruImage> BooruImages
        {
            get => _booruImages;
            set => SetField(ref _booruImages, value);
        }

        #endregion

        #region Commands

        private RelayCommand? _addCommand;
        public ICommand AddCommand => _addCommand ??= new RelayCommand( o => OpenFileDialogAndAddItems(), o => IsReady );

        #endregion

        #region Helper Methods

        public static bool IsMD5(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return Regex.IsMatch(input, "^[0-9a-fA-F]{32}$", RegexOptions.Compiled);
        }

        public static int? GetGelbooruImageIDFromFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            if (fileName.StartsWith("gelbooru_") &&
                fileName.Split('_', StringSplitOptions.RemoveEmptyEntries).Length == 2 &&
                int.TryParse(fileName.Split('_')[1], out int id))
            {
                return id;
            }

            return null;
        }

        #endregion

        #region Methods

        public void OpenFileDialogAndAddItems()
        {
            OpenFileDialog dialog = new()
            {
                Filter = "JPEG images|*.jpg;*.jpeg;",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {

                foreach(string path in dialog.FileNames)
                {
                    string displayName = Path.GetFileNameWithoutExtension(path);

                    if (IsMD5(displayName) ||
                        GetGelbooruImageIDFromFileName(displayName) != null)
                    {
                        int? id = GetGelbooruImageIDFromFileName(displayName);

                        GelbooruImage image = new()
                        {
                            Path = path,
                            Id = id,
                        };

                        Bitmap? thumbnail = null;
                        try
                        {
                            thumbnail = WindowsThumbnailProvider.GetThumbnail(path, 256, 256, ThumbnailOptions.None);
                        }
                        finally
                        {
                            if (thumbnail != null)
                            {
                                image.Thumbnail = thumbnail.BitmapToImageSource();
                                thumbnail.Dispose();
                            }
                        }

                        if (BooruImages.Any(image => image.Path == path))
                        {
                            GelbooruImage? existingImage = BooruImages.Where(image => image.Path == path).FirstOrDefault();
                            if (existingImage != null)
                            {
                                image = existingImage;
                            }
                        }
                        else
                        {
                            BooruImages.Add(image);
                        }

                    }
                }
            }
        }

        #endregion

    }
}
