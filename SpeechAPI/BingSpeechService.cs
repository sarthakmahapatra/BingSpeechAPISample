using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace SpeechAPI
{

	public class BingSpeechService
    {

        private static readonly string authenticationTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0";
        private static readonly string bingSpeechApiKey = "6ee78908dbd742449e6024881c0888e0";
        private static readonly string speechRecognitionEndpoint = "https://speech.platform.bing.com/recognize";
        private static readonly string audioContentType = @"audio/wav; codec=""audio/pcm""; samplerate=16000";

        private string _operatingSystem;
        private string _token;


        public BingSpeechService(string os)
        {
            _operatingSystem = os;
        }

        public async Task<SpeechResult> RecognizeSpeechAsync(string filename)
        {
            if (string.IsNullOrWhiteSpace(_token))
            {
                _token = await FetchTokenAsync(authenticationTokenEndpoint, bingSpeechApiKey);
            }

            // Read audio file to a stream
            var file = await PCLStorage.FileSystem.Current.LocalStorage.GetFileAsync(filename);
            var fileStream = await file.OpenAsync(PCLStorage.FileAccess.Read);

            // Send audio stream to Bing and deserialize the response
            string requestUri = GenerateRequestUri(speechRecognitionEndpoint);
            string accessToken = _token;
            var response = await SendRequestAsync(fileStream, requestUri, accessToken, audioContentType);
            var speechResults = JsonConvert.DeserializeObject<SpeechResults>(response);

            fileStream.Dispose();
            return speechResults.results.FirstOrDefault();
        }

        string GenerateRequestUri(string speechEndpoint)
        {
            string requestUri = speechEndpoint;
            requestUri += @"?scenarios=ulm";                                    // websearch is the other option
            requestUri += @"&appid=a4f08ca2-3eff-4eaf-ac2a-2634a14a0074";       // You must use this ID.
            requestUri += @"&locale=en-US";                                     // Other languages supported.
            requestUri += string.Format("&device.os={0}", _operatingSystem);     // Open field
            requestUri += @"&version=3.0";                                      // Required value
            requestUri += @"&format=json";                                      // Required value
            requestUri += @"&instanceid=2becfc37-fac9-4c40-a6e8-06a13f68944c";  // GUID for device making the request
            requestUri += @"&requestid=" + Guid.NewGuid().ToString();           // GUID for the request
            return requestUri;
        }

        async Task<string> SendRequestAsync(Stream fileStream, string url, string bearerToken, string contentType)
        {
            var content = new StreamContent(fileStream);
            content.Headers.TryAddWithoutValidation("Content-Type", contentType);

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                var response = await httpClient.PostAsync(url, content);

                return await response.Content.ReadAsStringAsync();
            }
        }


        async Task<string> FetchTokenAsync(string fetchUri, string apiKey)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
                UriBuilder uriBuilder = new UriBuilder(fetchUri);
                uriBuilder.Path += "/issueToken";

                var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null);
                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
