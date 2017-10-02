using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Plugin.AudioRecorder;
using Xamarin.Forms;

namespace SpeechAPI
{
    public class SpeechViewModel : INotifyPropertyChanged
    {
		private AudioRecorderService _recorder = new AudioRecorderService();
        private BingSpeechService _bingSpeechService;
        private bool _isRecording;
        private bool _isProcessing;
		private bool _isRecodingOrProcessing;
        private string _Status;
        private string _SpeechText;

		public event PropertyChangedEventHandler PropertyChanged;

		public string Status
		{
			get
			{
				return _Status;
			}
			set
			{
				_Status = value;
				OnPropertyChanged();
			}
		}

		public string SpeechText
		{
			get
			{
				return _SpeechText;
			}
			set
			{
				_SpeechText = value;
				OnPropertyChanged();
			}
		}


		public bool IsRecording
        {
            get {
                return _isRecording; 
            }
            set 
            {
                _isRecording = value;
                OnPropertyChanged();
				OnPropertyChanged("IsNotRecodingOrProcessing");
				OnPropertyChanged("IsRecodingAndNotProcessing");
            }
        }

		public bool IsProcessing
		{
			get
			{
				return _isProcessing;
			}
			set
			{
				_isProcessing = value;
				OnPropertyChanged();
                OnPropertyChanged("IsNotRecodingOrProcessing");
                OnPropertyChanged("IsRecodingAndNotProcessing");
			}
		}

		public bool IsNotRecodingOrProcessing
		{
			get
			{
                return !(_isRecording || _isProcessing);
			}
		}


		public bool IsRecodingAndNotProcessing
		{
			get
			{
				return _isRecording && !_isProcessing;
			}
		}

        public ICommand StartRecordingCommand { protected set; get; }

        public ICommand StopRecordingCommand { protected set; get; }

		async void AudioRecorderService_AudioInputReceived(object sender, string fileName)
		{
            if (fileName == null) 
            {
                Status = "Did not catch that !!!";
            }
            else
            {
                Status = "Processing";
				IsProcessing = true;
				var data = await _bingSpeechService.RecognizeSpeechAsync(fileName);
				SpeechText = data.Lexical;
                Status = $"Confidence : { data.Confidence}";
            }
           
			IsProcessing = false;
            IsRecording = false;

		}

        public SpeechViewModel()
        {

            Status = "Initialzed";
			_bingSpeechService = new BingSpeechService(Device.RuntimePlatform);
            _recorder.AudioInputReceived += AudioRecorderService_AudioInputReceived;

			StartRecordingCommand = new Command(async () =>
			{
				Status = "Initialzed";
                SpeechText = string.Empty;
				IsRecording = true;
	    		await _recorder.StartRecording();
			});

            StopRecordingCommand = new Command(async () =>
            {
                IsRecording = false;
                await _recorder.StopRecording();
            });
        }

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
    }
}
