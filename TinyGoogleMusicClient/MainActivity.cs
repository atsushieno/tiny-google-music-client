using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Media;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using GoogleMusicAPI;

[assembly:UsesPermission (Android.Manifest.Permission.Internet)]

namespace TinyGoogleMusicClient
{
    [Activity(Label = "TinyGMusic", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public class MainActivityController
        {
            MainActivity activity;

            public MainActivityController (MainActivity activity)
            {
                this.activity = activity;
            }

            public void GetLoginCredential (Action<string,string> loginCallback)
            {
                activity.GetLoginCredential (loginCallback);
            }

            public MediaPlayer CreateMediaPlayer (string url)
            {
                return MediaPlayer.Create (activity, Android.Net.Uri.Parse (url));
            }

            public bool TryGetAllSongs (out List<GoogleMusicSong> results)
            {
                return activity.TryGetAllSongs (out results);
            }

            // notifications to the UI

            public void ShowApiError (Exception ex)
            {
                activity.ShowApiError (ex);
            }
            
            public void AllSongsAcquired (List<GoogleMusicSong> songs)
            {
                activity.AllSongsAcquired (songs);
            }

			public void SongsPrepared ()
			{
				activity.SongsPrepared ();
			}

            public void LoggedIn ()
            {
                activity.LoggedIn ();
            }

            public void LoggedOut ()
            {
                activity.LoggedOut ();
            }
        }

        GoogleMusicPlayerModel model {
            get { return GoogleMusicPlayerModel.Instance; }
        }

        Controller controller {
            get { return Controller.Instance; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            FindViewById<ImageButton> (Resource.Id.openArtistsButton).Click += (o, e) => StartActivity (new Intent (this, typeof (ArtistsActivity)));
            FindViewById<ImageButton> (Resource.Id.openAlbumsButton).Click += (o, e) => StartActivity (new Intent (this, typeof (AlbumsActivity)));
            FindViewById<ImageButton> (Resource.Id.openListsButton).Click += (o, e) => StartActivity (new Intent (this, typeof (PlayListsActivity)));
            FindViewById<ImageButton> (Resource.Id.loginLogoutSwitchButton).Click += (o, e) => { if (model.IsLoggedIn) UIConfirmLogout (); else UIAskLogon (); };
            FindViewById<ImageButton> (Resource.Id.updateSongsButton).Click += (o, e) => RequestUpdateSongs ();

            controller.Main = new MainActivityController (this);
            model.ProcessDefaultLogin ();
        }

        public void ShowApiError (Exception ex)
        {
            AlertDialog dlg = null;
            var db = new AlertDialog.Builder (this)
                .SetTitle ("Error")
                .SetMessage ("App error: " + ex.Message)
                .SetPositiveButton ("OK", (o, e) => dlg.Hide ());
            this.RunOnUiThread (() => dlg = db.Show ());
        }

        public void GetLoginCredential (Action<string,string> loginCallback)
        {
            var sp = GetSharedPreferences ("tiny_gmusic", FileCreationMode.Private | FileCreationMode.Append);
            var user = sp.GetString ("username", null);
            var pwd = sp.GetString ("password", null);
            if (user == null || pwd == null) {
                //UIAskLogon ();
                AlertDialog dlg = null;
                var db = new AlertDialog.Builder (this)
                    .SetTitle ("Welcome to Tiny GMusic")
                    .SetMessage ("First of all, you have to log in to Google to retrieve song list")
                    .SetPositiveButton ("OK", (o, e) => dlg.Hide ());
                dlg = db.Show ();
            }
            else
                loginCallback (user, pwd);
        }

        public bool TryGetAllSongs (out List<GoogleMusicSong> results)
		{
			results = null;
			try {
				using (var ifs = IsolatedStorageFile.GetUserStoreForApplication ()) {
					if (!ifs.FileExists ("all_songs.lst"))
						return false;
					var fs = ifs.OpenFile ("all_songs.lst", FileMode.Open);
					results = (List<GoogleMusicSong>) new DataContractJsonSerializer (typeof (List<GoogleMusicSong>)).ReadObject (fs);
					return true;
				}
			} catch (Exception ex) {
				Log.Error ("TinyGMusic", ex.ToString ());
				AlertDialog dlg = null;
				var db = new AlertDialog.Builder (this)
					.SetTitle ("Error")
					.SetMessage ("Could not load cached song list. It is ignored")
					.SetPositiveButton ("OK", delegate {
					dlg.Hide ();
				});
				this.RunOnUiThread (() => dlg = db.Show ());
				return false;
			}
        }

        public void AllSongsAcquired (List<GoogleMusicSong> songs)
        {
            var ms = new MemoryStream ();
            var utf8 = System.Text.Encoding.UTF8;
			using (var ifs = IsolatedStorageFile.GetUserStoreForApplication ())
				using (var fs = ifs.CreateFile ("all_songs.lst"))
					new DataContractJsonSerializer (songs.GetType ()).WriteObject (fs, songs.ToArray ());
            this.RunOnUiThread (() => Toast.MakeText (this, "downloaded song list", ToastLength.Short));
        }

		public void SongsPrepared ()
		{
			FindViewById<ImageButton> (Resource.Id.openArtistsButton).Clickable = true;
            FindViewById<ImageButton> (Resource.Id.openAlbumsButton).Clickable = true;
            FindViewById<ImageButton> (Resource.Id.openListsButton).Clickable = true;
		}

        /*
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            menu.Add (0, 0, 0, "Login to Google");
            menu.Add (0, 1, 1, "Logout");
            base.OnCreateContextMenu(menu, v, menuInfo);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            switch (item.ItemId) {
            case 0:
                    UIAskLogon ();
                    break;
            case 1:
                    UIConfirmLogout ();
                    break;
            }
            return base.OnContextItemSelected(item);
        }
        */

        void UIAskLogon ()
        {
            var view = View.Inflate (this, Resource.Layout.LoginDialog, null);
            var db = new AlertDialog.Builder (this)
                .SetTitle ("Login to Google")
                .SetView (view);
            AlertDialog dlg = null;
            var do_login = view.FindViewById<Button> (Resource.Id.loginExecuteButton);
            do_login.Click += delegate {
                var userEntry = view.FindViewById<EditText> (Resource.Id.userNameEntry);
                var pwdEntry = view.FindViewById<EditText> (Resource.Id.passwordEntry);
                var sp = GetSharedPreferences ("tiny_gmusic", FileCreationMode.Private | FileCreationMode.Append);
                var editor = sp.Edit ();
                string user = userEntry.Text;
                string pwd = pwdEntry.Text;
                editor.PutString ("username", user);
                editor.PutString ("password", pwd);
                editor.Commit ();
                model.ProcessLoginCommand (user, pwd);
                dlg.Hide ();
            };
            var do_cancel = view.FindViewById<Button> (Resource.Id.loginCancelButton);
            do_cancel.Click += delegate {
                dlg.Hide ();
            };
            dlg = db.Show ();
        }

        void UIConfirmLogout ()
        {
            var db = new AlertDialog.Builder (this)
                .SetTitle ("Are you sure to log out from Google?")
                .SetPositiveButton ("Yes", (o, e) => model.ProcessLogoutCommand ())
                .SetNegativeButton ("No", (o, e) => {});
            db.Show ();
        }

        void LoggedIn ()
        {
            this.RunOnUiThread (() => {
                Toast.MakeText (this, "logged in", ToastLength.Short);
                FindViewById<TextView> (Resource.Id.loginOrLogoutLabel).Text = "Logout";
                FindViewById<ImageButton> (Resource.Id.updateSongsButton).Clickable = true;
            });
        }

        void LoggedOut ()
        {
            this.RunOnUiThread (() => {
                Toast.MakeText (this, "logged out", ToastLength.Short);
                FindViewById<TextView> (Resource.Id.loginOrLogoutLabel).Text = "Login";
                FindViewById<ImageButton> (Resource.Id.updateSongsButton).Clickable = false;
            });
        }

        void RequestUpdateSongs ()
        {
			FindViewById<ImageButton> (Resource.Id.openArtistsButton).Clickable = false;
            FindViewById<ImageButton> (Resource.Id.openAlbumsButton).Clickable = false;
            FindViewById<ImageButton> (Resource.Id.openListsButton).Clickable = false;
            model.UpdateSongs ();
        }
    }
}

