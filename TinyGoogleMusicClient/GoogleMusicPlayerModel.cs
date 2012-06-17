using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using GoogleMusicAPI;

namespace TinyGoogleMusicClient
{
    class GoogleMusicPlayerModel
    {
        static GoogleMusicPlayerModel instance = new GoogleMusicPlayerModel ();
        public static GoogleMusicPlayerModel Instance {
            get { return instance; }
        }

        GoogleMusicPlayerModel ()
        {
            songs = new List<GoogleMusicSong> (); // empty by default
            SetupApi ();
        }

        API api = new API ();
        MediaPlayer player;
        bool logged_in;
        List<GoogleMusicSong> songs;

        Controller controller {
            get { return Controller.Instance; }
        }

        public bool IsLoggedIn {
            get { return logged_in; }
        }

        public IList<GoogleMusicSong> Songs {
            get { return songs; }
        }

        void SetupApi ()
        {
            api.OnError += (ex) => controller.Main.ShowApiError (ex);
            api.OnLoginComplete += (o, e) => {
                logged_in = true;
                controller.Main.LoggedIn ();
				songs.Clear ();
                if (controller.Main.TryGetAllSongs (out songs))
					controller.Main.SongsPrepared ();
				else
                    api.GetAllSongs ("");
            };
            api.OnGetAllSongsComplete += (latestSongs) => {
                if (latestSongs == null)
                    throw new ArgumentNullException ("latestSongs");
				songs.Clear ();
                songs.AddRange (latestSongs);
                controller.Main.AllSongsAcquired (songs);
				controller.Main.SongsPrepared ();
            };
            //api.OnCreatePlaylistComplete += api_OnCreatePlaylistComplete;
            //api.OnGetPlaylistsComplete += new API._GetPlaylists(api_OnGetPlaylistsComplete);
            api.OnGetSongURL += (url) => {
                if (player != null) {
                    if (player.IsPlaying)
                        player.Stop ();
                    player.Release ();
                }
                player = controller.Main.CreateMediaPlayer (url.URL);
                player.Start ();
            };
            //api.OnDeletePlaylist += new API._DeletePlaylist(api_OnDeletePlaylist);
            //api.OnGetPlaylistComplete += new API._GetPlaylist(api_OnGetPlaylistComplete);
        }

        public void ProcessDefaultLogin ()
        {
            controller.Main.GetLoginCredential ((user, pwd) => api.Login (user, pwd));
        }

        public void ProcessLoginCommand (string user, string pwd)
        {
            api.Login (user, pwd);
        }

        public void ProcessLogoutCommand ()
        {
            // there is no log out API. Just instantiate new API.
            api = new API ();
            SetupApi ();
        }

        public void UpdateSongs ()
        {
            api.GetAllSongs ("");
        }
    }
}