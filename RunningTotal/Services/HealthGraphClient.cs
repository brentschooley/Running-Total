using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using RunningTotal.DataModel;
using RunningTotal.Model;
using Newtonsoft.Json;
using Windows.Storage;

namespace RunningTotal.Services
{
    public static class HealthGraphClient
    {
        private static HttpClient _client = null;

        public static HttpClient Client
        {
            get {
                if (_client == null)
                {
                    _client = new HttpClient { MaxResponseContentBufferSize = 1000000 };

                    // TODO: Remove the temp authorization code used for dev.
                    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                    isAuthenticated = true;
                }
                return _client; 
            }
        }

        //public FitnessActivityFeed Feed { get; set; }


        private static string clientId = "212a9c331a3b4392bafbfa528005fdfc";
        private static string clientSecret = "12e6cd63dfec4c31b9ea0dcde661d3f7";
        private static string logonUriString = "https://runkeeper.com/apps/authorize";
        private static string redirectUriString = "http://codesnack.com/apps/run-metrics/success";
        private static string tokenRequestUriString = "https://runkeeper.com/apps/token";

        //private static string authToken = "";
        private static string authToken = "bdda5e496db24afb9cda60f7b955fcbe";
        
        private static bool isAuthenticated = false;

        private static User _user = null;

        static async Task AuthenticateAsync()
        {
            var requestString = string.Format("{0}?client_id={1}&response_type=code&redirect_uri={2}", logonUriString, clientId, redirectUriString);

            var requestUri = new Uri(requestString, UriKind.RelativeOrAbsolute);
            var redirectUri = new Uri(redirectUriString, UriKind.RelativeOrAbsolute);

            var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, requestUri, redirectUri);

            if (result.ResponseStatus != WebAuthenticationStatus.Success)
            {
                throw new Exception("Login failed: " + result.ResponseErrorDetail);
            }

            //Fetch Token
            // ...
            var code = ExtractCodeFromResponse(result.ResponseData);
            TokenResponse tokenResponse = null;

            if (!string.IsNullOrEmpty(code))
            {
                //requestString = string.Format("{0}?grant_type=authorization_code&code={1}&client_id={2}&client_secret={3}&redirect_uri={4}", tokenRequestUriString, code, clientId, clientSecret, redirectUriString);
                //requestUri = new Uri(requestString, UriKind.RelativeOrAbsolute);

                HttpResponseMessage response = null;
                var parameters = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string,string>("grant_type", "authorization_code"),
                    new KeyValuePair<string,string>("code", code),
                    new KeyValuePair<string,string>("client_id", clientId),
                    new KeyValuePair<string,string>("client_secret", clientSecret),
                    new KeyValuePair<string,string>("redirect_uri", redirectUriString)
                };

                var content = new FormUrlEncodedContent(parameters);

                response = await Client.PostAsync(new Uri(tokenRequestUriString, UriKind.RelativeOrAbsolute), content);

                response.EnsureSuccessStatusCode();

                
                var stream = await response.Content.ReadAsStreamAsync();
                if (response.Content.Headers.ContentType.MediaType == "application/json")
                {
                    tokenResponse = CreateTokenResponseFromJson(stream);
                }
            }

            authToken = tokenResponse.AccessToken;

            if (!string.IsNullOrEmpty(authToken))
            {
                Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                isAuthenticated = true;
                //Client.BaseAddress = new Uri("http://api.runkeeper.com");
            }
        }

        private static TokenResponse CreateTokenResponseFromJson(System.IO.Stream stream)
        {
            var serializer = new DataContractJsonSerializer(typeof(TokenResponse));
            var tokenResponse = (TokenResponse)serializer.ReadObject(stream);
            return tokenResponse;
        }

        public static async void TestSystem()
        {
            await AuthenticateAsNeeded();
            //var user = await GetUser();
        }

        public static async Task AuthenticateAsNeeded()
        {
            if (!isAuthenticated)
            {
                await AuthenticateAsync();
            }
        }

        public static async Task GetUser()
        {
            await AuthenticateAsNeeded();
            var stream = await Client.GetStreamAsync(new Uri("http://api.runkeeper.com/user", UriKind.RelativeOrAbsolute));

            var serializer = new DataContractJsonSerializer(typeof(User));
            _user = (User)serializer.ReadObject(stream);
        }

        public static async Task<FitnessActivityFeed> GetFitnessActivityFeed()
        {
            //if (_user == null)
            //    await GetUser();

            //var result = await Client.GetStringAsync(new Uri("http://api.runkeeper.com/" + _user.FitnessActivitiesUriString + "?pageSize=100", UriKind.RelativeOrAbsolute));
            var result = await Client.GetStringAsync(new Uri("http://api.runkeeper.com/fitnessActivities?pageSize=500", UriKind.RelativeOrAbsolute));
            var activities = JsonConvert.DeserializeObject<FitnessActivityFeed>(result);
            return activities;
        }

        public static async Task<FitnessActivityFeed> GetFitnessActivityFeedFull()
        {
            //if (_user == null)
            //    await GetUser();
            //var result = await Client.GetStringAsync(new Uri("http://api.runkeeper.com/" + _user.FitnessActivitiesUriString + "?pageSize=100", UriKind.RelativeOrAbsolute));
            var result = await Client.GetStringAsync(new Uri("http://api.runkeeper.com/fitnessActivities?pageSize=73&noEarlierThan=2010-01-01&noLaterThan=2010-12-31", UriKind.RelativeOrAbsolute));
            var feed = JsonConvert.DeserializeObject<FitnessActivityFeed>(result);

            feed.Activities = new List<FitnessActivity>();
            
            foreach (var i in feed.Items)
            {
                var a = await HealthGraphClient.GetFitnessActivity(i.Uri);
                feed.Activities.Add(a);
            }

            return feed;
        }

        public static async Task<FitnessActivityFeed> GetFitnessActivityFeedFromFile()
        {
            var path = @"DataModel\running_data.json";
            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var dataFile = await folder.GetFileAsync(path);
            var s = await FileIO.ReadTextAsync(dataFile);

            var fileFeed = await JsonConvert.DeserializeObjectAsync<FitnessActivityFeed>(s);

            return fileFeed;
        }

        public static async Task<FitnessActivity> GetFitnessActivity(string url)
        {
            if (_user == null)
                await GetUser();

            var result = await Client.GetStringAsync(new Uri("http://api.runkeeper.com/" + url, UriKind.RelativeOrAbsolute));
            return JsonConvert.DeserializeObject<FitnessActivity>(result);
        }

        private static string ExtractCodeFromResponse(string str)
        {
            return str.Split(new string[] { "code=" }, StringSplitOptions.None)[1];
        }

        private static string ExtractTokenFromResponse(string str)
        {
            return str.Split(new string[] { "access_token=", "&expires_in" }, StringSplitOptions.None)[1];
        }
    }

    [DataContract]
    public class TokenResponse
    {
        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }

        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }
    }
}
