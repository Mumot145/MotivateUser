using System;
using Gcm.Client;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.Auth;
using Xamarin.Forms;
using System.Linq;
using MotivationUser.Droid;
using Java.Security;

namespace MotivationUser.Droid
{
	[Activity (Label = "TodoAzure.Droid", 
		Icon = "@drawable/icon", 
		MainLauncher = true, 
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
		Theme = "@android:style/Theme.Holo.Light")]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
        static MainActivity instance = null;

        // Return the current activity instance.
        public static MainActivity CurrentActivity
        {
            get
            {
                return instance;
            }

        }

        protected override void OnCreate(Bundle bundle)
        {
            // Set the current instance of MainActivity.
            instance = this;

            base.OnCreate(bundle);

            // Initialize Azure Mobile Apps
            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            // Initialize Xamarin Forms
            global::Xamarin.Forms.Forms.Init(this, bundle);
            IEnumerable<Account> accounts = AccountStore.Create(Forms.Context).FindAccountsForService("Facebook");
            // Initialize the authenticator before loading the app.
            //App.Init((IAuthenticate)this);
            System.Diagnostics.Debug.WriteLine("accounts..." + accounts.FirstOrDefault());
            // Load the main application
            LoadApplication(new App(accounts.FirstOrDefault()));

            
        }
        public void ConnectPush(string _userId)
        {
            PackageInfo info = this.PackageManager.GetPackageInfo("com.motivation.User", PackageInfoFlags.Signatures);

            foreach (Android.Content.PM.Signature signature in info.Signatures)
            {
                MessageDigest md = MessageDigest.GetInstance("MD5");
                md.Update(signature.ToByteArray());

                string keyhash = Convert.ToBase64String(md.Digest());
                Console.WriteLine("KeyHash:", keyhash);
            }
            try
            {
                // Check to ensure everything's set up right
                GcmClient.CheckDevice(this);
                GcmClient.CheckManifest(this);

                // Register for push notifications
                System.Diagnostics.Debug.WriteLine("Registering...");
                GcmClient.Register(this, PushHandlerBroadcastReceiver.SENDER_IDS);
            }
            catch (Java.Net.MalformedURLException)
            {
                CreateAndShowDialog("There was an error creating the client. Verify the URL.", "Error");
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e.Message, "Error");
            }
        }
        private void CreateAndShowDialog(String message, String title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }
    }
}

