using GelbooruImageTagger.Extensions;
using GelbooruImageTagger.Models;
using GelbooruImageTagger.Utilities;
using HtmlAgilityPack;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

namespace GelbooruImageTagger.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        #region Internal Fields

        private HttpClient _httpClient = new();

        #endregion

        #region Properties

        private bool _showPreviewPane = true;
        private bool _isReady = true;
        private ObservableCollection<GelbooruImage> _booruImages = new();
        private GelbooruImage? _selectedBooruImage;
        private ObservableCollection<GelbooruImage> _selectedBooruImages = new();
        private ObservableCollection<string> _selectedCommonTags = new();
        private ObservableCollection<string> _selectedArtists = new();
        private ObservableCollection<string> _selectedCopyrights = new();

        public bool ShowPreviewPane
        {
            get => _showPreviewPane;
            set => SetField(ref _showPreviewPane, value);
        }
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
        public GelbooruImage? SelectedBooruImage
        {
            get => _selectedBooruImage;
            set => SetField(ref _selectedBooruImage, value);
        }
        public ObservableCollection<GelbooruImage> SelectedGelbooruImages
        {
            get => _selectedBooruImages;
            set => SetField(ref _selectedBooruImages, value);
        }
        public ObservableCollection<string> SelectedCommonTags
        {
            get => _selectedCommonTags;
            set => SetField(ref _selectedCommonTags, value);
        }
        public ObservableCollection<string> SelectedArtists
        {
            get => _selectedArtists;
            set => SetField(ref _selectedArtists, value);
        }
        public ObservableCollection<string> SelectedCopyrights
        {
            get => _selectedCopyrights;
            set => SetField(ref _selectedCopyrights, value);
        }

        #endregion

        #region Commands

        private RelayCommand? _addCommand;
        private RelayCommand? _clearCommand;
        private RelayCommand? _tagCommand;

        public ICommand AddCommand => _addCommand ??= new RelayCommand( o => OpenFileDialogAndAddItems(), o => IsReady );
        public ICommand ClearCommand => _clearCommand ??= new RelayCommand(o => ClearItems(), o => IsReady && BooruImages.Count > 0);
        public ICommand TagCommand => _tagCommand ??= new RelayCommand(o => TagItems(), o => IsReady && BooruImages.Count > 0);

        #endregion

        #region Helper Methods

        public static bool IsMD5(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return Regex.IsMatch(input, "^[0-9a-fA-F]{32}$", RegexOptions.Compiled);
        }

        public static string? GetGelbooruImageHashFromFileName(string fileName)
        {
            if (fileName.Length == 32 && IsMD5(fileName))
                return fileName;

            if (fileName.StartsWith("sample_"))
            {
                string[] splitPath = fileName.Split('_', StringSplitOptions.RemoveEmptyEntries);
                if (splitPath.Length == 2 &&
                    splitPath[1].Length == 32 &&
                    IsMD5(splitPath[1]))
                {
                    return splitPath[1];
                }
            }

            return null;
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

        public static void AddRangeObservable<T>(ObservableCollection<T> collection, IEnumerable<T> items, bool replace = false)
        {
            if (collection == null || items == null)
                return;

            if (replace)
                collection.Clear();

            foreach(T item in items)
                collection.Add(item);
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

                    if (GetGelbooruImageHashFromFileName(displayName) != null ||
                        GetGelbooruImageIDFromFileName(displayName) != null)
                    {
                        string? hash = GetGelbooruImageHashFromFileName(displayName);
                        int? id = GetGelbooruImageIDFromFileName(displayName);

                        GelbooruImage image = new()
                        {
                            Path = path,
                            Id = id,
                            Hash = hash
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

                        using (ShellFile shellFile = ShellFile.FromFilePath(path))
                        {
                            AddRangeObservable(image.Tags, shellFile.Properties.System.Keywords.Value);
                            AddRangeObservable(image.Artists, shellFile.Properties.System.Author.Value);

                            string? copyright = shellFile.Properties.System.Copyright.Value;
                            if (copyright != null)
                            {
                                AddRangeObservable(image.Copyrights, shellFile.Properties.System.Copyright.Value.Split(";", StringSplitOptions.TrimEntries).ToArray());
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
        public void RefreshSelection(IEnumerable<GelbooruImage> selectedItems)
        {
            if (selectedItems == null)
                return;

            AddRangeObservable(SelectedGelbooruImages, selectedItems, replace: true);

            if (SelectedGelbooruImages.Count > 0)
            {
                string[] artists = SelectedGelbooruImages.SelectMany(x=>x.Artists).Distinct().ToArray();
                string[] copyrights = SelectedGelbooruImages.SelectMany(x => x.Copyrights).Distinct().ToArray();

                List<string[]> tagListCollection = new();

                foreach (GelbooruImage selectedImage in SelectedGelbooruImages)
                    tagListCollection.Add(selectedImage.Tags.ToArray());

                var commonTagsList = tagListCollection
                    .Skip(1)
                    .Aggregate(
                        new HashSet<string>(tagListCollection.First()),
                        (h, e) => { h.IntersectWith(e); return h; }
                    );

                AddRangeObservable(SelectedCommonTags, commonTagsList, replace: true);
                AddRangeObservable(SelectedArtists, artists, replace: true);
                AddRangeObservable(SelectedCopyrights, copyrights, replace: true);
            }
        }
        public void ClearItems()
        {
            BooruImages.Clear();
            SelectedBooruImage = null;
            RefreshSelection(Array.Empty<GelbooruImage>());
        }
        public void TagItems()
        {
            IsReady = false;

            GelbooruImage[] gelbooruImages = BooruImages.ToArray();
            string baseUri = @"https://gelbooru.com";

            Parallel.ForEach(gelbooruImages, async (gelbooruImage, state) =>
            {
                string? response = null;
                if (gelbooruImage.Id != null)
                {
                    string uri = $"{baseUri}/index.php?page=post&s=view&id={gelbooruImage.Id}";
                    response = await _httpClient.GetStringAsync(uri);
                }
                else if (gelbooruImage.Hash != null)
                {
                    string uri = $"{baseUri}/index.php?page=post&s=list&md5={gelbooruImage.Hash}";
                    using HttpRequestMessage message = new(HttpMethod.Head, uri);
                    using HttpResponseMessage? redirectResponse = await _httpClient.SendAsync(message);
                    redirectResponse.EnsureSuccessStatusCode();

                    if (redirectResponse != null &&
                        redirectResponse.RequestMessage != null &&
                        redirectResponse.RequestMessage.RequestUri != null)
                    {
                        Uri requestUri = redirectResponse.RequestMessage.RequestUri;
                        string? id = HttpUtility.ParseQueryString(requestUri.Query).Get("id");

                        if (id != null)
                        {
                            gelbooruImage.Id = int.Parse(id);
                            string redirectUri = $"{baseUri}/index.php?page=post&s=view&id={gelbooruImage.Id}";
                            response = await _httpClient.GetStringAsync(uri);
                        }
                        else
                        {
                            throw new Exception("Did not get redirected to a post ID");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to get response from redirect");
                    }
                }
                
                if (response == null)
                {
                    // unsupported
                }
                else
                {
                    string generalTagXPath = @".//*[@id='tag-list']//li[contains(concat(' ',normalize-space(@class),' '),' tag-type-general ')]/a";
                    string characterTagXPath = @".//*[@id='tag-list']//li[contains(concat(' ',normalize-space(@class),' '),' tag-type-character ')]/a";
                    string artistTagXPath = @".//*[@id='tag-list']//li[contains(concat(' ',normalize-space(@class),' '),' tag-type-artist ')]/a";
                    string copyrightTagXPath = @".//*[@id='tag-list']//li[contains(concat(' ',normalize-space(@class),' '),' tag-type-copyright ')]/a";

                    List<string> mainTags = new();
                    string[]? artistTags = null;
                    string[]? copyrightTags = null;

                    await Task.Run(() =>
                    {
                        HtmlDocument document = new();
                        document.LoadHtml(response);

                        HtmlNodeCollection generalTagNodes = document.DocumentNode.SelectNodes(generalTagXPath);
                        if (generalTagNodes != null)
                            mainTags.AddRange(generalTagNodes
                                .Select(x => HttpUtility.HtmlDecode(x.InnerText))
                                .Where(x => !string.IsNullOrWhiteSpace(x) && x.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                                .ToArray());

                        HtmlNodeCollection characterTagNodes = document.DocumentNode.SelectNodes(characterTagXPath);
                        if (characterTagNodes != null)
                            mainTags.AddRange(characterTagNodes
                                .Select(x => HttpUtility.HtmlDecode(x.InnerText))
                                .Where(x => !string.IsNullOrWhiteSpace(x) && x.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                                .ToArray());

                        mainTags = mainTags.OrderBy(x => x).ToList();

                        HtmlNodeCollection artistTagNodes = document.DocumentNode.SelectNodes(artistTagXPath);
                        if (artistTagNodes != null)
                            artistTags = artistTagNodes
                                .Select(x => HttpUtility.HtmlDecode(x.InnerText))
                                .Where(x => !string.IsNullOrWhiteSpace(x) && x.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                                .ToArray();

                        HtmlNodeCollection copyrightTagNodes = document.DocumentNode.SelectNodes(copyrightTagXPath);
                        if (copyrightTagNodes != null)
                            copyrightTags = copyrightTagNodes
                                .Select(x => HttpUtility.HtmlDecode(x.InnerText))
                                .Where(x => !string.IsNullOrWhiteSpace(x) && x.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                                .ToArray();

                    });

                    AddRangeObservable(gelbooruImage.Tags, mainTags, true);

                    if (artistTags != null)
                        AddRangeObservable(gelbooruImage.Artists, artistTags, true);

                    if (copyrightTags != null)
                        AddRangeObservable(gelbooruImage.Copyrights, copyrightTags, true);

                    using ShellFile shellFile = ShellFile.FromFilePath(gelbooruImage.Path);
                    shellFile.Properties.System.Keywords.Value = gelbooruImage.Tags.ToArray();
                    shellFile.Properties.System.Author.Value = gelbooruImage.Artists.ToArray();
                    shellFile.Properties.System.Copyright.Value = string.Join("; ", gelbooruImage.Copyrights.ToArray());
                }

            });

            IsReady = true;
        }

        #endregion

    }
}
