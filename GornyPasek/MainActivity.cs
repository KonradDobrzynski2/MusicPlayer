#region Using
using System;
using System.Timers;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Media;
using System.Collections.Generic;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Content.PM;
#endregion

namespace GornyPasek
{
    [Activity(MainLauncher = true, Label = "Music player", ScreenOrientation = ScreenOrientation.Portrait, Icon = "@drawable/icon_aplication", Theme = "@style/MyTheme")]
    public class MainActivity : ActionBarActivity
    {
        #region Declaration
        private Android.Support.V7.Widget.Toolbar navigation_bar;
        enum PlayButtonState { Play, Stop, Pause };
        PlayButtonState position_play = PlayButtonState.Stop;

        bool LoopingButtonState = false;
        bool LoopingAllButtonState = false;
        bool RandomButtonState = false;

        List<int> random_song_list;

        MediaPlayer song = new MediaPlayer();
        int index_actual_song = 0;

        ImageView play;
        ImageView play_left;
        ImageView play_right;
        ImageView random_play;
        ImageView looping_track;

        TextView total_length;
        TextView actual_length;
        TextView remains;
        TextView song_name;
        TextView with_a_folder;
        TextView next_song;

        SeekBar seek_bar;

        Timer timer_main = new Timer();
        #endregion

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            timer_main.Interval = 500;
            timer_main.Elapsed += update_main;

            State.AppState.paths_favorite_file_list = new List<string>();
            State.AppState.name_favorite_file_list = new List<string>();

            random_song_list = new List<int>();

            var my_toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            song_name = FindViewById<TextView>(Resource.Id.song_name);

            // gorny pasek menu
            my_navigation_bar();

            // obsuga przegladarki plikow
            with_a_folder = FindViewById<TextView>(Resource.Id.with_a_folder);

            // tytuł utworu // folder // nastepna piosenka
            with_a_folder.Click += browse_music;
            next_song = FindViewById<TextView>(Resource.Id.next_song);

            // pasek przewijania
            seek_bar = FindViewById<SeekBar>(Resource.Id.slider);           
            seek_bar.StopTrackingTouch += update_actual_length_seekbar;
            
            #region time
            // atualny czas utworu
            actual_length = FindViewById<TextView>(Resource.Id.actual_length);

            // całkowity czas utworu
            total_length = FindViewById<TextView>(Resource.Id.total_length);

            // całkowity czas - utworu - aktualn czas utworu
            remains = FindViewById<TextView>(Resource.Id.remains);
            #endregion
            
            #region Play
            // start utworu
            play = FindViewById<ImageView>(Resource.Id.play);
            play.Click += play_song;
            
            // poprzedni utwor
            play_left = FindViewById<ImageView>(Resource.Id.play_left);
            play_left.Click += play_previous;         

            // nastepny utwor
            play_right = FindViewById<ImageView>(Resource.Id.play_right);
            play_right.Click += play_next;
            #endregion         

            #region Loolpin
            // losowe odtwarzanie utworu
            random_play = FindViewById<ImageView>(Resource.Id.random_play);
            random_play.Click += play_random;

            // powtarzanie utworu
            looping_track = FindViewById<ImageView>(Resource.Id.looping_track);
            looping_track.Click += play_loopin_track;
            #endregion
        }

        #region Method
        private void update_main(object sender, ElapsedEventArgs e)
        {
            update_remains();
            update_seek_bar();
            update_current_position();
            update_duration();

            // odtwarzanie gdy pierwszy utwor sie skończy. Drugi musi się zacząć vv
            if (song.IsPlaying == false && position_play != PlayButtonState.Pause)
            {
                set_next_song();
            }
        }

        #region play
        private void play_song(object sender, EventArgs e)
        {
            if (State.AppState._FullName_file == null)
            {
                Toast.MakeText(this, "You do not select a file", ToastLength.Short).Show();                
            }
            else
            {
                if (position_play == PlayButtonState.Stop)
                {
                    var songCover = Android.Net.Uri.Parse(State.AppState._FullName_file);
                    song = MediaPlayer.Create(this, songCover);

                    play.SetImageResource(Resource.Drawable.icon_pause);
                    //play.SetBackgroundResource(Resource.Drawable.icon_pause);
                    song.Start();

                    position_play = PlayButtonState.Play;

                    timer_main.Start();
                }
                else if (position_play == PlayButtonState.Play)
                {
                    song.Pause();

                    position_play = PlayButtonState.Pause;

                    play.SetImageResource(Resource.Drawable.icon_play);

                    timer_main.Stop();
                }
                else if (position_play == PlayButtonState.Pause)
                {
                    song.Start();

                    play.SetImageResource(Resource.Drawable.icon_pause);

                    position_play = PlayButtonState.Play;

                    timer_main.Start();
                }

                seek_bar.Max = song.Duration / 1000;

                with_a_folder.Text = "Folder: " + State.AppState._Folder_file;
                song_name.Text = State.AppState._Name_file;

                update_index_actial_song();

                if (index_actual_song < State.AppState.name_file_list.Count-1)
                {
                    next_song.Text = "Next song: " + State.AppState.name_file_list[index_actual_song + 1];
                }
                else
                {
                    next_song.Text = "Next song: " + State.AppState.name_file_list[0];
                }
            }          
        }

        private void play_next(object sender, EventArgs e)
        {
            if (State.AppState._FullName_file == null)
            {
                Toast.MakeText(this, "Choose a file", ToastLength.Short).Show();
            }
            else
            {
                if (LoopingButtonState == true)
                {
                    song.SeekTo(0);

                    return;
                }

                Android.Net.Uri songCover;

                update_index_actial_song();

                timer_main.Stop();

                song.Stop();

                if (RandomButtonState == true)
                {
                    random_play_song();
                    return;   
                }

                // przygotowanie piosenki nastepnej
                else if (index_actual_song < State.AppState.paths_file_list.Count - 1)
                {
                    songCover = Android.Net.Uri.Parse(State.AppState.paths_file_list[index_actual_song + 1]);
                    string next_song = State.AppState.paths_file_list[index_actual_song + 1];
                    next_song = next_song.Remove(0, State.AppState._Path_file.Length);
                    State.AppState._Name_file = next_song;
                    State.AppState._FullName_file = State.AppState._Path_file + next_song;
                }
                else
                {
                    songCover = Android.Net.Uri.Parse(State.AppState.paths_file_list[0]);
                    string next_song = State.AppState.paths_file_list[0];
                    next_song = next_song.Remove(0, State.AppState._Path_file.Length);
                    State.AppState._Name_file = next_song;
                    State.AppState._FullName_file = State.AppState._Path_file + next_song;
                }

                song_name.Text = State.AppState._Name_file;

                if ((index_actual_song + 2) < State.AppState.name_file_list.Count)
                {
                    next_song.Text = "Next song: " + State.AppState.name_file_list[index_actual_song + 2];
                }
                else if ((index_actual_song + 2) == State.AppState.name_file_list.Count)
                {
                    next_song.Text = "Next song: " + State.AppState.name_file_list[0];
                }
                else
                {
                    next_song.Text = "Next song: " + State.AppState.name_file_list[1];
                }

                song = MediaPlayer.Create(this, songCover);

                seek_bar.Max = song.Duration / 1000;

                if (position_play == PlayButtonState.Play)
                {
                    song.Start();
                }

                timer_main.Start();           
            }
        }

        private void play_previous(object sender, EventArgs e)
        {
            if (State.AppState._FullName_file == null)
            {
                Toast.MakeText(this, "Choose a file", ToastLength.Short).Show();
            }
            else
            {
                if (LoopingButtonState == true)
                {
                    song.SeekTo(0);

                    return;
                }

                Android.Net.Uri songCover;

                update_index_actial_song();

                timer_main.Stop();

                song.Stop();

                if (RandomButtonState == true)
                {
                    random_play_song();
                    return;
                }

                else if (index_actual_song > 0)
                {
                    songCover = Android.Net.Uri.Parse(State.AppState.paths_file_list[index_actual_song - 1]);
                    string previous_song = State.AppState.paths_file_list[index_actual_song - 1];
                    previous_song = previous_song.Remove(0, State.AppState._Path_file.Length);
                    State.AppState._Name_file = previous_song;
                    State.AppState._FullName_file = State.AppState._Path_file + previous_song;
                }
                else
                {
                    songCover = Android.Net.Uri.Parse(State.AppState.paths_file_list[State.AppState.paths_file_list.Count - 1]);
                    string previous_song = State.AppState.paths_file_list[State.AppState.paths_file_list.Count - 1];
                    previous_song = previous_song.Remove(0, State.AppState._Path_file.Length);
                    State.AppState._Name_file = previous_song;
                    State.AppState._FullName_file = State.AppState._Path_file + previous_song;
                }

                song_name.Text = State.AppState._Name_file;

                next_song.Text = "Next song: " + State.AppState.name_file_list[index_actual_song];

                song = MediaPlayer.Create(this, songCover);

                seek_bar.Max = song.Duration / 1000;

                if (position_play == PlayButtonState.Play)
                {
                    song.Start();
                }

                timer_main.Start();
            }
        }

        private void set_next_song()
        {
            RunOnUiThread(() =>
            {
                if (LoopingButtonState == true)
                {
                    song.Start();

                    return;
                }

                

                Android.Net.Uri songCover;

                update_index_actial_song();

                // przygotowanie piosenki nastepnej
                if (index_actual_song < State.AppState.paths_file_list.Count - 1)
                {
                    songCover = Android.Net.Uri.Parse(State.AppState.paths_file_list[index_actual_song + 1]);
                    string next_song = State.AppState.paths_file_list[index_actual_song + 1];
                    next_song = next_song.Remove(0, State.AppState._Path_file.Length);
                    State.AppState._Name_file = next_song;
                    State.AppState._FullName_file = State.AppState._Path_file + next_song;
                }
                else
                {
                    songCover = Android.Net.Uri.Parse(State.AppState.paths_file_list[0]);
                    string next_song = State.AppState.paths_file_list[0];
                    next_song = next_song.Remove(0, State.AppState._Path_file.Length);
                    State.AppState._Name_file = next_song;
                    State.AppState._FullName_file = State.AppState._Path_file + next_song;
                }

                song_name.Text = State.AppState._Name_file;

                if ((index_actual_song + 2) < State.AppState.name_file_list.Count)
                {
                    next_song.Text = "Next song: " + State.AppState.name_file_list[index_actual_song + 2];
                }
                else if ((index_actual_song + 2) == State.AppState.name_file_list.Count)
                {
                    next_song.Text = "Next song: " + State.AppState.name_file_list[0];
                }
                else
                {
                    next_song.Text = "Next song: " + State.AppState.name_file_list[1];
                }

                song = MediaPlayer.Create(this, songCover);

                seek_bar.Max = song.Duration / 1000;

                song.Start();
            }
            );
        }

        private void random_play_song()
        {
            Android.Net.Uri songCover;

            Random random = new Random();

            int rand;

            while (true)
            {
                if (random_song_list.Count == State.AppState.paths_file_list.Count)
                {
                    random_song_list.Clear();
                }

                rand = random.Next(0, State.AppState.paths_file_list.Count);


                if (random_song_list.Contains(rand) == false)
                {
                    random_song_list.Add(rand);
                    break;
                }
            }


            songCover = Android.Net.Uri.Parse(State.AppState.paths_file_list[rand]);
            string random_song = State.AppState.paths_file_list[rand];
            random_song = random_song.Remove(0, State.AppState._Path_file.Length);
            State.AppState._Name_file = random_song;
            State.AppState._FullName_file = State.AppState._Path_file + random_song;

            next_song.Text = "Random";

            song_name.Text = State.AppState._Name_file;

            song = MediaPlayer.Create(this, songCover);

            seek_bar.Max = song.Duration / 1000;

            if (position_play == PlayButtonState.Play)
            {
                song.Start();
            }

            timer_main.Start();
        }
        #endregion

        #region update
        private void update_current_position()
        {
            RunOnUiThread(() =>
            {
                int current_position = song.CurrentPosition / 1000;
                float minutes = current_position / 60;
                float seconds = current_position % 60;
                string current_position_file;

                if (minutes < 10 && seconds < 10)
                {
                    current_position_file = "0" + minutes.ToString() + ":" + "0" + seconds.ToString();
                }
                else if (minutes < 10 && seconds >= 10)
                {
                    current_position_file = "0" + minutes.ToString() + ":" + seconds.ToString();
                }
                else if (seconds < 10 && minutes >= 10)
                {
                    current_position_file = minutes.ToString() + ":" +  "0" + seconds.ToString();
                }
                else
                {
                    current_position_file = minutes.ToString() + ":" + seconds.ToString();
                }

                actual_length.Text = current_position_file;
            }
            ); 
        }

        private void update_duration()
        {
            RunOnUiThread(() =>
            {
                int current_position = song.Duration / 1000;
                int minutes = current_position / 60;
                int seconds = current_position % 60;
                string current_position_file;

                if (minutes < 10 && seconds < 10)
                {
                    current_position_file = "0" + minutes.ToString() + ":" + "0" + seconds.ToString();
                }
                else if (minutes < 10 && seconds >= 10)
                {
                    current_position_file = "0" + minutes.ToString() + ":" + seconds.ToString();
                }
                else if (seconds < 10 && minutes >= 10)
                {
                    current_position_file = minutes.ToString() + ":" + "0" + seconds.ToString();
                }
                else
                {
                    current_position_file = minutes.ToString() + ":" + seconds.ToString();
                }

                total_length.Text = current_position_file;

            }
            );
        }

        private void update_remains()
        {
            RunOnUiThread(() =>
            {
                int current_position = (song.Duration - song.CurrentPosition) / 1000;
                int minutes = current_position / 60;
                int seconds = current_position % 60;
                string current_position_file;

                if (minutes < 10 && seconds < 10)
                {
                    current_position_file = "0" + minutes.ToString() + ":" + "0" + seconds.ToString();
                }
                else if (minutes < 10 && seconds >= 10)
                {
                    current_position_file = "0" + minutes.ToString() + ":" + seconds.ToString();
                }
                else if (seconds < 10 && minutes >= 10)
                {
                    current_position_file = minutes.ToString() + ":" + "0" + seconds.ToString();
                }
                else
                {
                    current_position_file = minutes.ToString() + ":" + seconds.ToString();
                }

                remains.Text = current_position_file;
            }
            );
        }

        private void update_seek_bar()
        {
            RunOnUiThread(() =>
            {
                seek_bar.Progress = song.CurrentPosition/1000;               
            }
            );
        }

        private void update_index_actial_song()
        {
            for (int i = 0; i < State.AppState.paths_file_list.Count; i++)
            {
                if (State.AppState.paths_file_list[i].Contains(State.AppState._Name_file))
                {
                    index_actual_song = i;

                    break;
                }
            }
        }

        private void update_actual_length_seekbar(object sender, EventArgs e)
        {
            song.SeekTo((seek_bar.Progress * 1000));
        }
        #endregion
        
        #region loop
        private void play_loopin_track(object sender, EventArgs e)
        {
            if (State.AppState._FullName_file == null)
            {
                Toast.MakeText(this, "Give me the file...", ToastLength.Short).Show();
            }
            else
            {
                if (LoopingButtonState == false)
                {
                    looping_track.SetImageResource(Resource.Drawable.icon_looping_track_on);
                    LoopingButtonState = true;
                   
                }
                else
                {
                    looping_track.SetImageResource(Resource.Drawable.icon_looping_track_off);
                    LoopingButtonState = false;
                }
            }
        }

        private void play_random(object sender, EventArgs e)
        {
            if (State.AppState._FullName_file == null)
            {
                Toast.MakeText(this, "File... Give me the file :)", ToastLength.Short).Show();
            }
            else
            {
                if (RandomButtonState == false)
                {
                    random_play.SetImageResource(Resource.Drawable.icon_random_play_on);
                    RandomButtonState = true;
                }
                else
                {
                    random_play.SetImageResource(Resource.Drawable.icon_random_play_off);
                    RandomButtonState = false;
                }
            }
        }
        #endregion

        private void my_navigation_bar()
        {
            navigation_bar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(navigation_bar);
            SupportActionBar.Title = "Music player";
        }

        private void browse_music(object sender, EventArgs e)
        {
            StartActivity(typeof(BrowseFilesActivity));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.action_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.a_list_of_songs_in_the_folder)
            {
                if (State.AppState._FullName_file == null)
                {
                    Toast.MakeText(this, "This list is empty because you did not choose any folder with songs", ToastLength.Short).Show();
                }
                else
                {
                    StartActivity(typeof(ListSongActivity));
                }
            }
            else if (item.ItemId == Resource.Id.information_about_aplication)
            {
                StartActivity(typeof(InfoAplicationActivity));
            }
            else if (item.ItemId == Resource.Id.add_to_favorites)
            {
                if (State.AppState._FullName_file == null)
                {
                    Toast.MakeText(this, "Ehh ... what do you want to add?", ToastLength.Short).Show();
                }
                else
                {
                    State.AppState.paths_favorite_file_list.Add(State.AppState._FullName_file);

                    State.AppState.name_favorite_file_list.Add(State.AppState._FullName_file.Remove(0, State.AppState._Path_file.Length));
                }
            }
            else if (item.ItemId == Resource.Id.a_list_of_songs_in_the_favorites)
            {
                if (State.AppState._FullName_file == null)
                {
                    Toast.MakeText(this, "This list is empty :(", ToastLength.Short).Show();
                    StartActivity(typeof(ListFovoriteSongActivity));
                }
                else
                {
                    StartActivity(typeof(ListFovoriteSongActivity));
                }
            }
            else if (item.ItemId == Resource.Id.play_A_favorites_song)
            {
                if (State.AppState._FullName_file == null)
                {
                    Toast.MakeText(this, "TList of favorite songs is empty :(", ToastLength.Short).Show();
                }
                else
                {
                    State.AppState.paths_file_list = State.AppState.paths_favorite_file_list;

                    State.AppState.name_file_list = State.AppState.name_favorite_file_list;

                    State.AppState._Name_file = State.AppState.name_favorite_file_list[0];

                    if (position_play == PlayButtonState.Play)
                    {
                        song.Stop();
                        play.SetImageResource(Resource.Drawable.icon_play);
                    }

                    var songCover = Android.Net.Uri.Parse(State.AppState.paths_file_list[0]);
                    song = MediaPlayer.Create(this, songCover);
                    play.SetImageResource(Resource.Drawable.icon_pause);
                    song.Start();
                    position_play = PlayButtonState.Play;

                    song_name.Text = State.AppState._Name_file;
                    next_song.Text = State.AppState.name_favorite_file_list[1];
                }
            }

            return base.OnOptionsItemSelected(item);
        }
        #endregion
    }
}