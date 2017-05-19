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
## Part 3 - Add Sign-in pages

1. Create a new folder called **Pages** and then create your first page **Forms ContentPage Xaml** 

**XAML**
```
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms" xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" x:Class="ToDo.SignIn">
	<ContentPage.Content>
		<StackLayout Orientation="Vertical" VerticalOptions="CenterAndExpand" Padding="20,0,20,0">
			<Label HorizontalOptions="Center" HorizontalTextAlignment="Center" Text="Select your country and enter your phone number in order to sign in">
			</Label>
			<Picker VerticalOptions="CenterAndExpand" SelectedIndexChanged="CountryPicker_SelectedIndexChanged" x:Name="countryPicker" ItemDisplayBinding="{Binding name}" Title="Select a country...">
			</Picker>
			<Entry x:Name="phoneNumberEntry" Keyboard="Telephone" Placeholder="Enter your phone">
			</Entry>
			<Button Text="Sign in" x:Name="signInButton">
			</Button>
			<ActivityIndicator x:Name="pageActivityIndicator">
			</ActivityIndicator>
			<Label HorizontalOptions="Center" HorizontalTextAlignment="Start" Text="You will receive an SMS with verification code.">
			</Label>
		</StackLayout>
	</ContentPage.Content>
</ContentPage>

```

**CS**
```
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Linq;

namespace ToDo
{
	public partial class SignIn : ContentPage
	{
		AuthenticationRepository autheticationRepo = new AuthenticationRepository(new AuthenticationService());

		bool isLoading
		{
			set
			{
				pageActivityIndicator.IsVisible = value;
				pageActivityIndicator.IsRunning = value;
				signInButton.IsVisible = !value;
				this.IsBusy = value;
			}
		}

		public SignIn()
		{
			InitializeComponent();
			Title = "Sign in";

			configureCountryPicker();
			configureSignInButton();
			pageActivityIndicator.BindingContext = this;
		}

		private void configureCountryPicker()
		{
			countryPicker.ItemsSource = getCountries();
			countryPicker.SelectedIndexChanged += CountryPicker_SelectedIndexChanged;
		}

		private void configureSignInButton()
		{
			signInButton.Clicked += SignInButton_Clicked;
		}

		private List<CountryCode> getCountries()
		{
			var assembly = typeof(SignIn).GetTypeInfo().Assembly;
			Stream stream = assembly.GetManifestResourceStream("PerToDo.Data.Countries.json");

			List<CountryCode> countryCodes;

			using (var reader = new System.IO.StreamReader(stream))
			{
				var json = reader.ReadToEnd();
				countryCodes = JsonConvert.DeserializeObject<List<CountryCode>>(json);
			}
			return countryCodes;
		}

		private bool validateForm()
		{
			if (countryPicker.SelectedIndex == -1) return false;
			if (string.IsNullOrWhiteSpace(phoneNumberEntry.Text)) return false;
			return true;
		}

		void CountryPicker_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (countryPicker.SelectedIndex == -1) { return; }
			var selectCountry = (CountryCode)countryPicker.SelectedItem;
			phoneNumberEntry.Text = selectCountry.dial_code;
		}

		async void SignInButton_Clicked(object sender, EventArgs e)
		{
			if (!validateForm()) { await DisplayAlert("Validation", "Please fill missing field(s)", "Ok"); return; };
			isLoading = true;
			var phoneNumber = phoneNumberEntry.Text.Trim();
			await autheticationRepo.RegisterAccount(phoneNumber);
			await Navigation.PushAsync(new VerificationPage(phoneNumber));
			isLoading = false;
		}
	}
}	
```
2. Add the country list JSON file and make sure to make it as embedded resource.
Just create a new folder named **Data** add the json file with **.json** extension
> This file can be found in this repo

3. Add new folder and name it **Models** inside it just put a poco class named **CountryCode**

```
using System;
namespace ToDo
{
	public class CountryCode
	{
		public string name { get; set; }
		public string dial_code { get; set; }
		public string code { get; set; }
	}
}
```

4. Create VerificationPage called **VerificationPage** as follows:

**XAML** 
```
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms" xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" x:Class="ToDo.VerificationPage">
	<ContentPage.Content>
		<StackLayout Orientation="Vertical" VerticalOptions="CenterAndExpand" Padding="20,0,20,0">
			<Label Text="Enter verification code">
			</Label>
			<Entry Keyboard="Numeric" x:Name="verificationEntry" Placeholder="e.g. 1234">
			</Entry>
			<Button x:Name="signInButton" Text="Sign in">
			</Button>
			<ActivityIndicator x:Name="pageActivityIndicator">
			</ActivityIndicator>
		</StackLayout>
	</ContentPage.Content>
</ContentPage>
```

**CS**
```
using System;
using System.Net;

using Xamarin.Forms;

namespace ToDo
{
    public partial class VerificationPage : ContentPage
    {
        AuthenticationRepository autheticationRepo = new AuthenticationRepository(new AuthenticationService());
        string phoneNumber;

        bool isLoading
        {
            set
            {
                pageActivityIndicator.IsVisible = value;
                pageActivityIndicator.IsRunning = value;
                signInButton.IsVisible = !value;
                this.IsBusy = value;
            }
        }

        public VerificationPage(string phoneNumber)
        {
            InitializeComponent();

            this.phoneNumber = phoneNumber;
            Title = "Verify yourself";
            configureSignInButton();
        }

        private void configureSignInButton()
        {
            signInButton.Clicked += SignInButton_Clicked;
        }

        async void SignInButton_Clicked(object sender, EventArgs e)
        {
            var verificationCode = verificationEntry.Text.Trim();
            isLoading = true;
            var isSuccess = await autheticationRepo.AutherizeAccount(phoneNumber, verificationCode);
            if (isSuccess == HttpStatusCode.OK)
            {
                await Navigation.PushAsync(new MainPage());
            }
            else {
                await DisplayAlert("Ops...", "Something went wrong, please try again later", "Ok");
            }
            isLoading = false;
        }
    }
}

```
5. In order to start the project we need to set SignIn page as our start page so we go to **App.xaml.cs** and change the line of code to
```
MainPage = new NavigationPage(new SignIn());
```
