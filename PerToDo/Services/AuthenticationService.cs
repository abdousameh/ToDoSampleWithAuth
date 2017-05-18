using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace PerToDo
{
    public class AuthenticationService : IAuthenticationService
    {
        HttpClient client;
        readonly Uri baseUri = new Uri(Constants.BaseURL);

        public AuthenticationService()
        {
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
        }

        public async Task Register(string phoneNumber)
        {
            string purgedPhoneNumber = phoneNumber;

            var uri = new Uri(baseUri, "api/account/register");

            try
            {
                if (phoneNumber.Contains("+")) { purgedPhoneNumber = phoneNumber.Replace("+", ""); }
                var json = string.Concat("{\"username\": ", purgedPhoneNumber, "}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine(@"successfully sent.");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"ERROR {0}", ex.Message);
            }
        }

        public async Task<HttpStatusCode> AuthorizeUser(string phoneNumber, string verificationCode)
        {
            string purgedPhoneNumber = phoneNumber;
            var uri = new Uri(baseUri, "token");

            if (phoneNumber.Contains("+")) { purgedPhoneNumber = phoneNumber.Replace("+", ""); }
            var postBody = new Dictionary<string, string>()
                {
                    {"username", purgedPhoneNumber},
                    {"password", verificationCode},
                    {"grant_type", "password"}
                };

            var content = new FormUrlEncodedContent(postBody);

            var response = await client.PostAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                //var storeService = DependencyService.Get<IUserDetailsStore>();
                var result = await response.Content.ReadAsStringAsync();
                var jsonData = (JObject)JsonConvert.DeserializeObject(result);

                var token = jsonData["access_token"].Value<string>();
                //Token should be saved in key chain instead of singleton class
                //But for demo purposes, ill put it here
                AccountDetailsStore.Instance.Token = token;
                //storeService.SaveCredentials(phoneNumber, token);
            }
            return response.StatusCode;
        }
    }
}