using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Auth;
using MotivationUser;
using MotivationUser.Droid;


[assembly: ExportRenderer(typeof(LoginPage), typeof(LoginPageRenderer))]
namespace MotivationUser.Droid
{
    public class LoginPageRenderer : PageRenderer
    {
        private string ClientId = "1014195495384668";
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            var activity = this.Context as Activity;

            var auth = new OAuth2Authenticator(
                clientId: ClientId, // your OAuth2 client id
                scope: "", // the scopes for the particular API you're accessing, delimited by "+" symbols
                authorizeUrl: new Uri("https://m.facebook.com/dialog/oauth/"), // the auth URL for the service
                redirectUrl: new Uri("https://www.facebook.com/connect/login_success.html")); // the redirect URL for the service

            auth.Completed += (sender, eventArgs) =>
            {
                if (eventArgs.IsAuthenticated)
                {
                    App.SuccessfulLoginAction.Invoke();
                    // Use eventArgs.Account to do wonderful things
                    //var request = new OAuth2Request("GET", new Uri("https://graph.facebook.com/me?fields=email,first_name,last_name,gender,picture"), null, eventArgs.Account);
                    var request = new OAuth2Request("GET", new Uri("https://graph.facebook.com/me"), null, eventArgs.Account);
                    request.GetResponseAsync().ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            Console.WriteLine("Error: " + t.Exception.InnerException.Message);
                        else
                        {
                            string json = t.Result.GetResponseText();
                            Console.WriteLine("logged in" + json);
                        }
                    });
                    App.SaveToken(eventArgs.Account.Properties["access_token"]);
                    AccountStore.Create(Forms.Context).Save(eventArgs.Account, "Facebook");
                }
                else
                {
                    // The user cancelled
                }
            };
            Console.WriteLine("after fam");
            activity.StartActivity((Intent)auth.GetUI(activity));
        }
    }
}