# Document
This document is a guidline (not a detailed tutrial) for students to follow along with me on building this app.

## Part 1 - Create the project

Using your prefered IDE (Visual Studio or Xamarin Studio), create new solution and choose **Blank Forms App**, then carry on with the wizard by choosing the targeted platforms and directory location.

## Part 2 - Consume Authentication APIs

1. Create a new folder called **Interfaces**, and then add new interface called **IAuthenticationService.cs** 

```
using System;
using System.Threading.Tasks;
using System.Net;

namespace ToDo
{
	public interface IAuthenticationService
	{
		Task Register(string phoneNumber);

		Task<HttpStatusCode> AuthorizeUser(string phoneNumber, string verificationCode);
	}
}
```

2. Create new folder called **Services**, and then add new class called **AuthenticationService.cs**

> Dont forget to change namespace name according to your project name

```
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ToDo
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
```

3. You will see that there is a missing library, install Newtonsoft package in order to fix this error.

4. Add **Constants** class by creating a new class named *Constants.cs*

```
using System;
namespace ToDo
{
	public static class Constants
	{
		public static string BaseURL = "https://bookmarkerapp.azurewebsites.net/";
	}
}
```

5. Create Singleton class to store the token by adding new folder called *Helpers* add new class in it named **AccountDetailsStore** as shown below

```
using System;
namespace ToDo
{
	public sealed class AccountDetailsStore
	{
		private static readonly AccountDetailsStore instance = new AccountDetailsStore();

		private AccountDetailsStore() { }

		public static AccountDetailsStore Instance
		{
			get
			{
				return instance;
			}
		}

		public string PhoneNumber { get; set; }
		public string Token { get; set; }
	}
}
```

6. Consume these methods by creating a repository or manager by creating new folder called **Repositories** and a class called **AuthenticationRepository** 

```
using System;
using System.Threading.Tasks;

namespace ToDo
{
	public class AuthenticationRepository
	{
		IAuthenticationService authenticationService;

		public AuthenticationRepository(IAuthenticationService service)
		{
			authenticationService = service;
		}

		public Task RegisterAccount(string phoneNumber)
		{
			return authenticationService.Register(phoneNumber);
		}

		public Task<System.Net.HttpStatusCode> AutherizeAccount(string phoneNumber, string verificationCode)
		{
			return authenticationService.AuthorizeUser(phoneNumber, verificationCode);
		}
	}
}
```
