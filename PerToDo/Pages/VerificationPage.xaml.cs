using System;
using System.Net;

using Xamarin.Forms;

namespace PerToDo
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
