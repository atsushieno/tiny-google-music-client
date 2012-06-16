using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using GoogleMusicAPI;

namespace TinyGoogleMusicClient
{
    [Activity(Label = "Album")]
    public class AlbumActivity : Activity
    {

        GoogleMusicPlayerModel model {
            get { return GoogleMusicPlayerModel.Instance; }
        }

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			this.SetContentView (Resource.Layout.AlbumView);

			var album = this.Intent.GetStringExtra ("album");

			var title = FindViewById<TextView> (Resource.Id.albumViewTitle);
			title.Text = album;

			var songs = model.Songs.Where (s => s.Album == album).OrderBy (s => s.Disc).ThenBy (s => s.Track).ToArray ();

			var listView = FindViewById<ListView> (Resource.Id.albumViewSongs);
			var ids = new int [] {
				Resource.Id.songListViewTitle,
				Resource.Id.songListViewArtist,
				Resource.Id.songListViewTime,
			};
			var data = (from s in songs select ToDictionaryItem (s)).ToArray ();

			var adapter = new SimpleAdapter (this, data, Resource.Layout.SongListView,
			                                 new string [] {"title", "artist", "time"}, ids);
			listView.Adapter = adapter;
		}

		IDictionary<string,object> ToDictionaryItem (GoogleMusicSong song)
		{
			var d = new JavaDictionary<string,object> ();
			d ["title"] = song.Title;
			d ["artist"] = song.Artist;
			d ["time"] = TimeSpan.FromMilliseconds (song.Duration);
			return d;
		}
	}
}

