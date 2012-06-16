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
    class Controller
    {
        static Controller instance;
        public static Controller Instance {
            get { return instance = instance ?? new Controller (); }
        }
        
        Controller ()
        {
        }

        public MainActivity.MainActivityController Main { get; internal set; }
        public AlbumsActivity.AlbumsActivityController Albums { get; internal set; }
        public ArtistsActivity.ArtistsActivityController Artists { get; internal set; }
    }
}