using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Linq;

namespace PerToDo
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