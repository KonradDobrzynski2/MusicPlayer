using System;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Media;
using System.IO;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Content;
using System.Threading;
using System.Net;

namespace GornyPasek
{
    public class State
    {
        private static State appState;
        public static State AppState
        {
            get
            {
                if (appState == null)
                {
                    appState = new State();
                }
                return appState;
            }
        }
        public string _Extension_file { get; set; }
        public string _FullName_file { get; set; }
        public string _Name_file { get; set; }
        public string _Path_file { get; set; }
        public string _Folder_file { get; set; }
        public List<string> paths_file_list { get; set; }   
        public List<string> name_file_list { get; set; }
        public List<string> paths_favorite_file_list { get; set; }
        public List<string> name_favorite_file_list { get; set; }
    }
}