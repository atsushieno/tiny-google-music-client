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

namespace no_music_no_desire
{
    [Activity(Label = "Artists")]
    public class ArtistsActivity : ListActivity
    {
        public class ArtistsActivityController
        {
            ArtistsActivity activity;
            public ArtistsActivityController (ArtistsActivity activity)
            {
                this.activity = activity;
            }

			public void SongsUpdated ()
			{
				activity.UpdateArtistsList ();
			}
        }

        GoogleMusicPlayerModel model {
            get { return GoogleMusicPlayerModel.Instance; }
        }

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here

			Controller.Instance.Artists = new ArtistsActivityController (this);

			this.ListView.ItemClick += (sender, e) => {
				var intent = new Intent (this, typeof (AlbumsActivity));
				intent.PutExtra ("artist", (ICharSequence) list [e.Position]);
				StartActivity (intent);
			};

			UpdateArtistsList ();
		}

		String [] list;

		public void UpdateArtistsList ()
		{
            list = model.Songs.Select (s => s.Artist).Distinct ().OrderBy (s => s).Select (s => new String (s)).ToArray ();

            var adapter = new ArrayAdapter<String> (this, Android.Resource.Layout.SimpleListItem1, list);
            this.ListAdapter = adapter;
        }
    }
}
