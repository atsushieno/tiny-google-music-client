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

namespace no_music_no_desire
{
    [Activity(Label = "Albums")]
    public class AlbumsActivity : ListActivity
    {
        public class AlbumsActivityController
        {
            AlbumsActivity activity;
            public AlbumsActivityController (AlbumsActivity activity)
            {
                this.activity = activity;
            }
        }

        GoogleMusicPlayerModel model {
            get { return GoogleMusicPlayerModel.Instance; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here

            Controller.Instance.Albums = new AlbumsActivityController (this);

            var list = model.Songs.Select (s => s.Artist).Distinct ().Concat (new string [] {"--select--"}).ToArray ();
            var adapter = new ArrayAdapter<string> (this, Resource.Id.textOnlyLayoutText, list);
            this.ListAdapter = adapter;
        }
    }
}