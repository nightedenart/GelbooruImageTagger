using GelbooruImageTagger.Models;
using GelbooruImageTagger.Utilities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace GelbooruImageTagger.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Structs

        public struct BooruResponse
        {
            public string[] Artists;
            public string[] Characters;
            public string[] Copyrights;
            public string[] Tags;
            public string Source;
        }

        #endregion

        #region Internal Fields

        private bool _isLoading = true;
        private HttpClient _httpClient = new HttpClient();
        private string _baseUri = @"https://gelbooru.com";

        #endregion

        #region Properties

        private bool _isTagging = false;
        private bool _isPreviewOpen = false;
        private bool _skipTaggedImages = false;
        private ObservableCollection<GelbooruImage> _gelbooruImages = new ObservableCollection<GelbooruImage>();
        private GelbooruImage _selectedGelbooruImage;
        private ObservableCollection<GelbooruImage> _selectedGelbooruImages = new ObservableCollection<GelbooruImage>();

        private ObservableCollection<string> _selectedCharacters = new ObservableCollection<string>();
        private ObservableCollection<string> _selectedCopyrights = new ObservableCollection<string>();
        private ObservableCollection<string> _selectedArtists = new ObservableCollection<string>();
        private ObservableCollection<string> _selectedTags = new ObservableCollection<string>();

        public bool IsTagging
        {
            get => _isTagging;
            set => SetField(ref _isTagging, value);
        }
        public bool IsPreviewOpen
        {
            get => _isPreviewOpen;
            set => SetField(ref _isPreviewOpen, value);
        }
        public bool SkipTaggedImages
        {
            get => _skipTaggedImages;
            set
            {
                SetField(ref _skipTaggedImages, value);
                if (!_isLoading)
                {
                    SaveSetting("SkipTaggedImages", value);
                }
            }
        }
        public ObservableCollection<GelbooruImage> GelbooruImages
        {
            get => _gelbooruImages;
            set => SetField(ref _gelbooruImages, value);
        }

        public GelbooruImage SelectedGelbooruImage
        {
            get => _selectedGelbooruImage;
            set => SetField(ref _selectedGelbooruImage, value);
        }

        public ObservableCollection<GelbooruImage> SelectedGelbooruImages
        {
            get => _selectedGelbooruImages;
            set => SetField(ref _selectedGelbooruImages, value);
        }

        public ObservableCollection<string> SelectedCharacters
        {
            get => _selectedCharacters;
            set => SetField(ref _selectedCharacters, value);
        }

        public ObservableCollection<string> SelectedTags
        {
            get => _selectedTags;
            set => SetField(ref _selectedTags, value);
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

        private RelayCommand _addCommand;
        private RelayCommand _clearCommand;
        private RelayCommand _tagCommand;
        public RelayCommand AddCommand
        {
            get
            {
                return _addCommand ?? (_addCommand = new RelayCommand(() => _ = OpenFileDialogAndAddItems(), () => !_isTagging));
            }
        }
        public RelayCommand ClearCommand
        {
            get
            {
                return _clearCommand ?? (_clearCommand = new RelayCommand(() => ClearItems(), () => GelbooruImages.Count > 0 && !_isTagging));
            }
        }
        public RelayCommand TagCommand
        {
            get
            {
                return _tagCommand ?? (_tagCommand = new RelayCommand(() => _ = TagItems(), () => GelbooruImages.Count > 0 && !_isTagging));
            }
        }

        #endregion

        #region Utility Methods

        public void AddRangeObservable<T>(ObservableCollection<T> observable, IEnumerable<T> items, bool clear = false)
        {
            if (clear)
                observable.Clear();

            foreach (T item in items)
                observable.Add(item);
        }

        #endregion

        #region Methods

        public void RefreshCommandCanExecutes()
        {
            ClearCommand.RaiseCanExecuteChanged();
            TagCommand.RaiseCanExecuteChanged();
        }

        public async Task OpenFileDialogAndAddItems()
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads
            };

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();

            foreach (StorageFile file in files)
            {
                string path = file.Path;

                bool alreadyExists = GelbooruImages.Any(image => image.File.Path == path);
                if (!alreadyExists)
                {
                    string displayName = file.DisplayName; // file name without extension
                    int booruImageId = -1;

                    // check if file name is hash or sample_hash or gelbooru_imageId
                    if (displayName.Length == 32 && !displayName.Contains('.') && !displayName.Contains('-') ||
                        displayName.StartsWith("sample_") && displayName.Split('_').Length == 2 && displayName.Split('_')[1].Length == 32 ||
                        displayName.StartsWith("gelbooru_") && displayName.Split('_').Length == 2 && int.TryParse(displayName.Split('_')[1], out booruImageId))
                    {

                        StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);
                        BitmapImage bitmapThumbnail = new BitmapImage();
                        bitmapThumbnail.SetSource(thumbnail);

                        ImageProperties imgProperties = await file.Properties.GetImagePropertiesAsync();
                        DocumentProperties docProperties = await file.Properties.GetDocumentPropertiesAsync();

                        string source = docProperties.Comment;

                        GelbooruImage image = new GelbooruImage()
                        {
                            File = file,
                            Directory = Path.GetDirectoryName(file.Path),
                            Thumbnail = bitmapThumbnail,
                            Source = source
                        };

                        if (booruImageId > 0)
                        {
                            image.Id = booruImageId;
                        }

                        AddRangeObservable(image.Tags, imgProperties.Keywords);
                        AddRangeObservable(image.Artists, docProperties.Author);

                        #region Get Copyright Property
                        try
                        {
                            var copyrightProps = await file.Properties.RetrievePropertiesAsync(new string[1] { "System.Copyright" });

                            foreach (var prop in copyrightProps)
                            {
                                if (prop.Key == "System.Copyright")
                                {
                                    if (prop.Value != null && !string.IsNullOrEmpty(prop.Value.ToString()))
                                    {
                                        AddRangeObservable(image.Copyrights, prop.Value.ToString()?.Split("; ", StringSplitOptions.None));
                                    }
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            Debug.WriteLine("Failed to get copyright props for {0}", path);
                        }
                        #endregion

                        if (booruImageId > 0)
                            image.Id = booruImageId;

                        GelbooruImages.Add(image);
                    }

                }

            }

            RefreshCommandCanExecutes();
            RefreshSelection();
        }

        public void ClearItems()
        {
            GelbooruImages.Clear();
            RefreshCommandCanExecutes();
        }

        public async Task TagItems()
        {
            IsTagging = true;
            RefreshCommandCanExecutes();

            ParallelOptions parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 10
            };

            foreach (var image in GelbooruImages)
            {
                if (image.Tags.Count > 0 && SkipTaggedImages)
                {
                    image.StatusLevel = GelbooruImageStatus.PartialSuccess;
                    image.StatusMessage = "Skipped";
                }
            }

            GelbooruImage[] nonSkippedImages = GelbooruImages.Where(image => image.StatusMessage != "Skipped").ToArray();

            List<Task<Tuple<GelbooruImage, BooruResponse>>> imagesToProcess = new List<Task<Tuple<GelbooruImage, BooruResponse>>>();
            foreach(var booruImage in nonSkippedImages)
            {
                imagesToProcess.Add(CreateImageProcessTask(booruImage));
            }

            var imageResponses = await Task.WhenAll(imagesToProcess);

            foreach (var imageResponse in imageResponses)
            {
                GelbooruImage booruImage = imageResponse.Item1;
                BooruResponse tagList = imageResponse.Item2;

                try
                {
                    List<KeyValuePair<string, object>> propertiesToSave = new List<KeyValuePair<string, object>>();

                    if (tagList.Tags.Length > 0)
                    {
                        propertiesToSave.Add(new KeyValuePair<string, object>("System.Keywords", tagList.Tags));
                        AddRangeObservable(booruImage.Tags, tagList.Tags, true);
                    }

                    if (tagList.Artists.Length > 0)
                    {
                        propertiesToSave.Add(new KeyValuePair<string, object>("System.Author", tagList.Artists));
                        AddRangeObservable(booruImage.Artists, tagList.Artists, true);
                    }

                    if (tagList.Copyrights.Length > 0)
                    {
                        propertiesToSave.Add(new KeyValuePair<string, object>("System.Copyright", string.Join("; ", tagList.Copyrights)));
                        AddRangeObservable(booruImage.Copyrights, tagList.Copyrights, true);
                    }

                    if (!string.IsNullOrWhiteSpace(tagList.Source))
                    {
                        propertiesToSave.Add(new KeyValuePair<string, object>("System.Comment", tagList.Source));
                        booruImage.Source = tagList.Source;
                    }

                    await booruImage.File.Properties.SavePropertiesAsync(propertiesToSave);

                    booruImage.StatusLevel = GelbooruImageStatus.Success;
                    booruImage.StatusMessage = "Tagged";
                }
                catch
                {
                    booruImage.StatusLevel = GelbooruImageStatus.Error;
                    booruImage.StatusMessage = "Failed";
                }
            }

            IsTagging = false;
            RefreshCommandCanExecutes();
        }

        public async Task<Tuple<GelbooruImage, BooruResponse>> CreateImageProcessTask(GelbooruImage booruImage)
        {
            if (booruImage.Id != null)
            {
                int id = (int)booruImage.Id;
                string uri = _baseUri + "/index.php?page=post&s=view&id=" + id;
                var response = await GetResponseByPostId(uri, id);
                var tagList = await ParseResponse(response);
                return new Tuple<GelbooruImage, BooruResponse>(booruImage, tagList);
            }
            else if (booruImage.File.DisplayName.Length == 32 ||
                booruImage.File.DisplayName.StartsWith("sample_") && booruImage.File.DisplayName.Split('_').Length == 2)
            {
                string fileName = booruImage.File.DisplayName;
                string md5 = booruImage.File.DisplayName;

                if (fileName.StartsWith("sample_"))
                {
                    md5 = fileName.Split('_', StringSplitOptions.RemoveEmptyEntries)[1];
                }

                string uri = _baseUri + "/index.php?page=post&s=list&md5=" + md5;
                string response = await GetResponseByPostMD5(uri, md5);
                var tagList = await ParseResponse(response);
                return new Tuple<GelbooruImage, BooruResponse>(booruImage, tagList);
            }

            return null;
        }

        public async Task<BooruResponse> ParseResponse(string response)
        {
            BooruResponse tagList = new BooruResponse();

            string generalTagXPath = @".//*[@id='tag-list']//li[contains(concat(' ',normalize-space(@class),' '),' tag-type-general ')]/a";
            string characterTagXPath = @".//*[@id='tag-list']//li[contains(concat(' ',normalize-space(@class),' '),' tag-type-character ')]/a";
            string artistTagXPath = @".//*[@id='tag-list']//li[contains(concat(' ',normalize-space(@class),' '),' tag-type-artist ')]/a";
            string copyrightTagXPath = @".//*[@id='tag-list']//li[contains(concat(' ',normalize-space(@class),' '),' tag-type-copyright ')]/a";

            string genericTagListNodeXPath = @".//*[@id='tag-list']/li";

            List<string> mainTags = new List<string>();
            string[] artistTags = Array.Empty<string>();
            string[] copyrightTags = Array.Empty<string>();
            string source = "";

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response);

            await Task.Run(() =>
            {
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

                HtmlNodeCollection artistTagNodes = document.DocumentNode.SelectNodes(artistTagXPath);
                if (artistTagNodes != null)
                {
                    artistTags = artistTagNodes.Select(x => HttpUtility.HtmlDecode(x.InnerText))
                    .Where(x => !string.IsNullOrWhiteSpace(x) && x.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                    .ToArray();
                }

                HtmlNodeCollection copyrightTagNodes = document.DocumentNode.SelectNodes(copyrightTagXPath);
                if (copyrightTagNodes != null)
                    copyrightTags = copyrightTagNodes
                        .Select(x => HttpUtility.HtmlDecode(x.InnerText))
                        .Where(x => !string.IsNullOrWhiteSpace(x) && x.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                        .ToArray();

                mainTags = mainTags.OrderBy(x => x).ToList();

                HtmlNodeCollection miscNodes = document.DocumentNode.SelectNodes(genericTagListNodeXPath);
                if (miscNodes != null)
                {
                    Parallel.ForEach(miscNodes, (miscNode, state) =>
                    {
                        if (miscNode.InnerText.StartsWith("Source: "))
                        {
                            HtmlNode anchorTag = miscNode.SelectSingleNode(@".//a");
                            if (anchorTag != null)
                            {
                                string href = anchorTag.Attributes["href"].Value;
                                if (!string.IsNullOrWhiteSpace(href))
                                {
                                    source = href;
                                }
                            }
                            state.Break();
                        }
                    });
                }

            });

            tagList.Tags = mainTags.ToArray();
            tagList.Artists = artistTags;
            tagList.Copyrights = copyrightTags;
            tagList.Source = source;

            return tagList;
        }

        public async Task<string> GetResponseByPostId(string uri, int? id)
        {
            if (string.IsNullOrWhiteSpace(uri) || id == null)
                return null;

            var response = await _httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null;
        }

        public async Task<string> GetResponseByPostMD5(string uri, string md5)
        {
            if (string.IsNullOrWhiteSpace(uri) || string.IsNullOrWhiteSpace(md5))
                return null;

            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Head, uri))
            {
                using (HttpResponseMessage redirectResponse = await _httpClient.SendAsync(message))
                {
                    redirectResponse.EnsureSuccessStatusCode();

                    if (redirectResponse != null &&
                        redirectResponse.RequestMessage != null &&
                        redirectResponse.RequestMessage.RequestUri != null)
                    {
                        Uri redirectedUri = redirectResponse.RequestMessage.RequestUri;
                        string id = HttpUtility.ParseQueryString(redirectedUri.Query).Get("id");

                        if (id != null)
                        {
                            string finalUri = _baseUri + "/index.php?page=post&s=view&id=" + id;
                            return await GetResponseByPostId(finalUri, int.Parse(id));
                        }
                        else
                        {
                            throw new Exception("Failed to get ID from redirect");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to get response from redirect");
                    }
                }
            }
        }

        public void RefreshSelection()
        {
            if (SelectedGelbooruImages.Count > 0)
            {
                List<string[]> tagListCollection = new List<string[]>();

                foreach (GelbooruImage selectedImage in SelectedGelbooruImages)
                {
                    tagListCollection.Add(selectedImage.Tags.ToArray());
                }

                if (tagListCollection.Count > 0)
                {
                    var intersection = tagListCollection
                        .Skip(1)
                        .Aggregate(
                            new HashSet<string>(tagListCollection.First()),
                            (h, e) => { h.IntersectWith(e); return h; }
                        );
                    AddRangeObservable(SelectedTags, intersection, true);
                }
                else
                {
                    SelectedTags.Clear();
                }

                AddRangeObservable(SelectedCopyrights, SelectedGelbooruImages.SelectMany(x => x.Copyrights).Distinct(), true);
                AddRangeObservable(SelectedArtists, SelectedGelbooruImages.SelectMany(x => x.Artists).Distinct(), true);

            } else
            {
                SelectedCopyrights.Clear();
                SelectedArtists.Clear();
                SelectedTags.Clear();
            }

        }

        public void SaveSetting(string key, object value)
        {
            if (key == null)
                return;

            try
            {
                ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
                roamingSettings.Values[key] = value;
            }
            catch
            {
                Debug.WriteLine("Failed to save '{0}' with: {1}", key, value);
            }
        }

        public object LoadSetting(string key)
        {
            try
            {
                ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
                return roamingSettings.Values[key];
            }
            catch
            {
                Debug.WriteLine("Failed to get '{0}'.", key);
            }
            return null;
        }

        #endregion

        public MainViewModel()
        {
            // block saving any settings while we are loading
            _isLoading = true;

            if (LoadSetting("SkipTaggedImages") is bool skipTaggedImages)
            {
                SkipTaggedImages = skipTaggedImages;
            }

            _isLoading = false;
        }

    }
}
