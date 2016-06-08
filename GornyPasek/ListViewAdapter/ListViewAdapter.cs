#region Using
using Android.App;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
#endregion

namespace GornyPasek
{   
    public class ListViewAdapter : BaseAdapter<string>
    {
        private List<string> list_song;
        private Activity context;

        public ListViewAdapter(Activity context, List<string> items) : base()
        {
            this.context = context;
            this.list_song = items;
        }

        public override int Count
        {
            get { return list_song.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override string this[int position]
        {
            get { return list_song[position]; }
        }    

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;

            if (row == null)
            {
                row = LayoutInflater.FromContext(context).Inflate(Resource.Layout.ElementOfList, null, false);        
            }

            TextView text = row.FindViewById<TextView>(Resource.Id.song_name_list);
            text.Text = list_song[position];

           return row;
        }
    }
}