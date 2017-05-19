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
## Part 4 - Consume operations APIs

1. In the Interfaces folder add new Interface called **IToDoService**

```
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ToDo
{
	public interface IToDoService
	{
		Task<List<ToDoItem>> GetToDoList();

		Task<HttpStatusCode> DeleteToDoItem(int itemId);

		Task<Uri> AddNewToDoItem(ToDoItem item);

		Task<ToDoItem> UpdateToDoItem(int id, ToDoItem item);
	}
}
```

2. Add a new model called **ToDoItem**

```
using System;
namespace ToDo
{
	public class ToDoItem
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Notes { get; set; }
		public bool Done { get; set; }
	}
}
```

3. in the services folder add ToDoService

```
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ToDo
{
	public class ToDoService : IToDoService
	{
		//I recommend to use 
		//https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client

		HttpClient client;
		//IUserDetailsStore storeService;
		readonly string token;
		readonly Uri baseUri = new Uri(Constants.BaseURL);

		public ToDoService()
		{
			//storeService = DependencyService.Get<IUserDetailsStore>();
			token = AccountDetailsStore.Instance.Token;
			client = new HttpClient();
			client.MaxResponseContentBufferSize = 256000;
		}

		public async Task<HttpStatusCode> DeleteToDoItem(int id)
		{
			var uri = new Uri(baseUri, string.Format("api/todo/{0}", id));

			client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));

			var response = await client.DeleteAsync(uri);

			return response.StatusCode;
		}

		public async Task<Uri> AddNewToDoItem(ToDoItem item)
		{
			var uri = new Uri(baseUri, "api/todo/");

			client.DefaultRequestHeaders.Clear();
			client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));

			var json = JsonConvert.SerializeObject(item);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await client.PostAsync(uri, content);

			response.EnsureSuccessStatusCode();

			// Return 	the URI of the created resource.
			return response.Headers.Location;
		}

		public async Task<ToDoItem> UpdateToDoItem(int id, ToDoItem item)
		{
			var uri = new Uri(baseUri, string.Format("api/todo/{0}", id));

			client.DefaultRequestHeaders.Clear();
			client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));

			var json = JsonConvert.SerializeObject(item);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await client.PutAsync(uri, content);

			// Deserialize the updated product from the response body.
			var responseContent = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<ToDoItem>(responseContent);
		}

		public async Task<List<ToDoItem>> GetToDoList()
		{
			var uri = new Uri(baseUri, "api/todo/");

			client.DefaultRequestHeaders.Clear();
			client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));

			var response = await client.GetAsync(uri);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<List<ToDoItem>>(content);
			}
			return new List<ToDoItem>();
		}
	}
}
```

4. Add new Repository file called **ToDoRepository**

```
using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ToDo
{
	public class ToDoRepository
	{
		readonly IToDoService todoService;

		public Task<List<ToDoItem>> GetToDoList()
		{
			return todoService.GetToDoList();
		}

		public ToDoRepository(IToDoService todoService)
		{
			this.todoService = todoService;
		}

		public Task<HttpStatusCode> DeleteToDoItem(int itemId)
		{
			return todoService.DeleteToDoItem(itemId);
		}

		public Task<Uri> AddNewToDoItem(ToDoItem item)
		{
			return todoService.AddNewToDoItem(item);
		}

		public Task<ToDoItem> UpdateToDoItem(int id, ToDoItem item)
		{
			return todoService.UpdateToDoItem(id, item);
		}
	}
}
```

## Part 5 - List and details pages

1. Create a new page in the Pages folder called **MainPage**

**XAML**
```
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms" xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" x:Class="ToDo.MainPage">
	<ContentPage.Content>
		<ListView x:Name="todoListView" ItemSelected="todoListView_ItemSelected">
			<ListView.ItemTemplate>
				<DataTemplate>
					<ViewCell>
						<ViewCell.ContextActions>
							<MenuItem Text="Delete" IsDestructive="True" Clicked="OnTaskDelete" CommandParameter="{Binding Id}" />
						</ViewCell.ContextActions>
						<StackLayout Padding="20,0,0,0" HorizontalOptions="StartAndExpand" Orientation="Horizontal">
							<Label Text="{Binding Name}" VerticalTextAlignment="Center" />
						</StackLayout>
					</ViewCell>
				</DataTemplate>
			</ListView.ItemTemplate>
			<ListView.Footer>
				<Button x:Name="AddTaskButton" Text="Add task"></Button>
			</ListView.Footer>
		</ListView>
	</ContentPage.Content>
</ContentPage>
```
**CS**
```
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ToDo
{
	public partial class MainPage : ContentPage
	{
		ToDoRepository todoRepo = new ToDoRepository(new ToDoService());

		public MainPage()
		{
			InitializeComponent();

			configurePage();
		}

		private void configurePage()
		{
			Title = "To do list";
			AddTaskButton.Clicked += AddTaskButton_Clicked;
			NavigationPage.SetHasBackButton(this, false);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			var _ = configureTodoList();
		}

		private async Task configureTodoList()
		{
			todoListView.ItemsSource = await todoRepo.GetToDoList();
		}

		void AddTaskButton_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ItemDetailsPage(new ToDoItem()));
		}

		void todoListView_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
		{
			var todoItem = e.SelectedItem as ToDoItem;
            var itemDetailsPage = new ItemDetailsPage(todoItem)
            {
                BindingContext = todoItem
            };
            Navigation.PushAsync(itemDetailsPage);
		}

        public async void OnTaskDelete(object sender, EventArgs e)
		{
			var menuItem = ((MenuItem)sender);
            var taskId = (Int32)menuItem.CommandParameter;
            var isConfirmed = await DisplayAlert("Delete Confirmation", "Are you sure you want to delete this task?" ,"Confirm", "Cancel");

            if (isConfirmed) {
                await todoRepo.DeleteToDoItem(taskId);
                await configureTodoList();
            }
		}
	}
}
```

2. Create Details page called **ItemDetailsPage**

**XAML**
```
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms" xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" x:Class="ToDo.ItemDetailsPage">
	<ContentPage.Content>
		<StackLayout Orientation="Vertical" VerticalOptions="StartAndExpand" Padding="20,0,20,0">
			<Label Text="Name">
			</Label>
			<Entry x:Name="nameEntry" Keyboard="Default" Text="{Binding Path=Name}" Placeholder="task name">
			</Entry>
			<Label Text="Notes">
			</Label>
			<Editor x:Name="notesEditor" HeightRequest="90" Keyboard="Default" Text="{Binding Path=Notes}">
			</Editor>
			<Label Text="Done">
			</Label>
			<Switch x:Name="doneSwitch" IsToggled="{Binding Path=Done}">
			</Switch>
			<Button Text="Save" x:Name="saveItemButton" VerticalOptions="CenterAndExpand" Clicked="SaveItemButton_Clicked">
			</Button>
		</StackLayout>
	</ContentPage.Content>
</ContentPage>
```

**CS**
```
using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ToDo
{
	public partial class ItemDetailsPage : ContentPage
	{
		ToDoItem toDoItem;
		ToDoRepository toDoRepo = new ToDoRepository(new ToDoService());

		public ItemDetailsPage(ToDoItem item)
		{
			InitializeComponent();
			this.toDoItem = item;
			Title = "Details";
		}

		async void SaveItemButton_Clicked(object sender, System.EventArgs e)
		{
			toDoItem.Name = nameEntry.Text.Trim();
			toDoItem.Notes = notesEditor.Text.Trim();
			toDoItem.Done = doneSwitch.IsToggled;

			if (toDoItem.Id == 0)
			{
				await toDoRepo.AddNewToDoItem(toDoItem);
			}
			else
			{
				await toDoRepo.UpdateToDoItem(toDoItem.Id, toDoItem);
			}
			await Navigation.PopAsync(true);
		}
	}
}
```
