using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace SampleApp.Droid
{
	[Activity (Label = "SampleApp", Icon = "@drawable/icon", Theme="@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar; 

			base.OnCreate (bundle);

		    #region ADDED TO SAMPLE TO DEMONSTRATE SimpleAdo.Sqlite

		    //Should be set up before the Xamarin.Forms App is instantiated
		    SampleApp.App.AppDatabasePath = new Services.DatabasePath();
		    SampleApp.App.AppCryptEngine = new Services.SampleAesCryptEngine();

		    #endregion

            global::Xamarin.Forms.Forms.Init (this, bundle);
			LoadApplication (new SampleApp.App ());
		}
	}
}

