using Plugin.AudioRecorder;
using Xamarin.Forms;

namespace SpeechAPI
{
    public partial class SpeechAPIPage : ContentPage
    {

        public SpeechAPIPage()
        {
            InitializeComponent();
            BindingContext = new SpeechViewModel();
        }
    }
}
