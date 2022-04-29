using GelbooruImageTagger.Utilities;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Xaml.Media;

namespace GelbooruImageTagger.Models
{
    public enum GelbooruImageStatus
    {
        None,
        Success,
        Warning,
        Error,
        PartialSuccess
    }
    public class GelbooruImage : BindableBase
    {

        private StorageFile _file;
        private string _directory;
        private ImageSource _thumbnail;
        private int? _id;
        private ObservableCollection<string> _tags = new ObservableCollection<string>();
        private ObservableCollection<string> _artists = new ObservableCollection<string>();
        private ObservableCollection<string> _characters = new ObservableCollection<string>();
        private ObservableCollection<string> _copyrights = new ObservableCollection<string>();
        private string _source;
        private GelbooruImageStatus _statusLevel = GelbooruImageStatus.None;
        private string _statusMessage = "Ready";

        public StorageFile File
        {
            get => _file;
            set => SetField(ref _file, value);
        }
        public string Directory
        {
            get => _directory;
            set => SetField(ref _directory, value);
        }
        public ImageSource Thumbnail
        {
            get => _thumbnail;
            set => SetField(ref _thumbnail, value);
        }
        public int? Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }
        public ObservableCollection<string> Tags
        {
            get => _tags;
            set => SetField(ref _tags, value);
        }
        public ObservableCollection<string> Artists
        {
            get => _artists;
            set => SetField(ref _artists, value);
        }
        public ObservableCollection<string> Characters
        {
            get => _characters;
            set => SetField(ref _characters, value);
        }
        public ObservableCollection<string> Copyrights
        {
            get => _copyrights;
            set => SetField(ref _copyrights, value);
        }
        public string Source
        {
            get => _source;
            set => SetField(ref _source, value);
        }
        public GelbooruImageStatus StatusLevel
        {
            get => _statusLevel;
            set => SetField(ref _statusLevel, value);
        }
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }
    }
}
