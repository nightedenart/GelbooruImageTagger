using GelbooruImageTagger.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GelbooruImageTagger.Models
{
    public enum GelbooruImageStatusLevel
    {
        None,
        Success,
        Warning,
        Error,
        PartialSuccess
    }
    public class GelbooruImage : BindableBase
    {

        private string? _path;
        private ImageSource? _thumbnail;
        private int? _id;
        private string? _hash;
        private ObservableCollection<string> _tags = new();
        private ObservableCollection<string> _artists = new();
        private ObservableCollection<string> _copyrights = new();
        private string? _sourceUri;
        private GelbooruImageStatusLevel _statusLevel;
        private string? _statusMessage = "Ready";

        public string? Path
        {
            get => _path;
            set => SetField(ref _path, value);
        }
        public ImageSource? Thumbnail
        {
            get => _thumbnail;
            set => SetField(ref _thumbnail, value);
        }
        public int? Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }
        public string? Hash
        {
            get => _hash;
            set => SetField(ref _hash, value);
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
        public ObservableCollection<string> Copyrights
        {
            get => _copyrights;
            set => SetField(ref _copyrights, value);
        }
        public string? SourceUri
        {
            get => _sourceUri;
            set => SetField(ref _sourceUri, value);
        }
        public GelbooruImageStatusLevel StatusLevel
        {
            get => _statusLevel;
            set => SetField(ref _statusLevel, value);
        }
        public string? StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }
    }
}
