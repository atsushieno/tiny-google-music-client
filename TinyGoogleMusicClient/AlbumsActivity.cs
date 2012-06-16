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

using String = Java.Lang.String;
using ICharSequence = Java.Lang.ICharSequence;

namespace TinyGoogleMusicClient
{
    [Activity(Label = "Albums")]
    public class AlbumsActivity : Activity
    {
        public class AlbumsActivityController
        {
            AlbumsActivity activity;
            public AlbumsActivityController (AlbumsActivity activity)
            {
                this.activity = activity;
            }

			public void SongsUpdated ()
			{
				activity.UpdateAlbumList ();
			}
        }

        GoogleMusicPlayerModel model {
            get { return GoogleMusicPlayerModel.Instance; }
        }

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.AlbumsView);

			Controller.Instance.Albums = new AlbumsActivityController (this);

			artist = this.Intent.GetStringExtra ("artist");

			var title = FindViewById<TextView> (Resource.Id.albumsViewTitle);
			title.Text = artist; // could be empty

			var listView = FindViewById<ListView> (Resource.Id.albumsViewList);

			listView.ItemClick += (sender, e) => {
				var intent = new Intent (this, typeof (AlbumActivity));
				intent.PutExtra ("album", (ICharSequence) list [e.Position]);
				StartActivity (intent);
			};

			UpdateAlbumList ();
		}

		string artist;
		String [] list;

		public void UpdateAlbumList ()
		{
            list = model.Songs.Where (s => artist != null ? s.Artist == artist : true).Select (s => s.Album).Distinct ().OrderBy (s => s).Select (s => new String (s)).ToArray ();

			var listView = FindViewById<ListView> (Resource.Id.albumsViewList);
            listView.Adapter = new ArrayAdapter<String> (this, Android.Resource.Layout.SimpleListItem1, list);
        }
    }
}
