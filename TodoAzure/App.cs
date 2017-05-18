using System;
using System.Diagnostics;
using Xamarin.Auth;
using Xamarin.Forms;

namespace TodoAzure
{
	public class App : Application
	{
        static string _Token;
        private string aToken = "";
        static NavigationPage _navPage;
        public App (Account _account)
		{
            // The root page of your application
            if (_account != null)
                aToken = _account.Properties["access_token"];

            if(aToken == "")
            {
                MainPage = _navPage = new NavigationPage(new LoginPage());
            } else
            {
                MainPage = new TodoList ();
            }
            
		}
        public static bool IsLoggedIn
        {
            get { return !string.IsNullOrWhiteSpace(_Token); }
        }

        public static string Token
        {
            get { return _Token; }
        }

        public static void SaveToken(string token)
        {
            Debug.WriteLine("setting " + token);
            _Token = token;
            Debug.WriteLine("setting _Token" + _Token);
        }

        public static Action SuccessfulLoginAction
        {
            get
            {
                return new Action(() => {
                    Debug.WriteLine("logged in");
                    _navPage.Navigation.PopModalAsync();


                });
            }
        }
        protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

