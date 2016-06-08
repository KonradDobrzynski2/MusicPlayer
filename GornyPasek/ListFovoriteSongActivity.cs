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

namespace GornyPasek
{
    [Activity(Label = "Fovorite song list")]
    public class ListFovoriteSongActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.SongList);

            ListView listView = FindViewById<ListView>(Resource.Id.list_song);

            ListViewAdapter adapter = new ListViewAdapter(this, State.AppState.name_favorite_file_list);

            listView.Adapter = adapter;
        }
    }
}