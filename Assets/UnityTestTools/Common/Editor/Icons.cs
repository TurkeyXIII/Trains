using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityTest
{
    public static class Icons
    {
        const string k_IconsFolderName = "icons";
        private static readonly string k_IconsFolderPath = String.Format("UnityTestTools{0}Common{0}Editor{0}{1}", Path.DirectorySeparatorChar, k_IconsFolderName);

        private static readonly string k_IconsAssetsPath = "";

        private static Texture2D _FailImg;
        private static Texture2D _IgnoreImg;
        private static Texture2D _RunImg;
        private static Texture2D _RunFailedImg;
        private static Texture2D _RunAllImg;
        private static Texture2D _SuccessImg;
        private static Texture2D _UnknownImg;
        private static Texture2D _InconclusiveImg;
        private static Texture2D _StopwatchImg;
        private static Texture2D _PlusImg;
        private static Texture2D _GearImg;

        private static GUIContent _GUIUnknownImg;
        private static GUIContent _GUIInconclusiveImg;
        private static GUIContent _GUIIgnoreImg;
        private static GUIContent _GUISuccessImg;
        private static GUIContent _GUIFailImg;

        public static Texture2D FailImg { get { if (_FailImg == null) Initialise(); return _FailImg; } }
        public static Texture2D IgnoreImg { get { if (_IgnoreImg == null)Initialise(); return _IgnoreImg; } }
        public static Texture2D RunImg { get { if (_RunImg == null) Initialise(); return _RunImg; } }
        public static Texture2D RunFailedImg { get { if (_RunFailedImg == null) Initialise(); return _RunFailedImg; } }
        public static Texture2D RunAllImg { get { if (_RunAllImg == null) Initialise(); return _RunAllImg; } }
        public static Texture2D SuccessImg { get { if (_SuccessImg == null) Initialise(); return _SuccessImg; } }
        public static Texture2D UnknownImg { get { if (_UnknownImg == null) Initialise(); return _UnknownImg; } }
        public static Texture2D InconclusiveImg { get { if (_InconclusiveImg == null) Initialise(); return _InconclusiveImg; } }
        public static Texture2D StopwatchImg { get { if (_StopwatchImg == null) Initialise(); return _StopwatchImg; } }
        public static Texture2D PlusImg { get { if (_PlusImg == null) Initialise(); return _PlusImg; } }
        public static Texture2D GearImg { get { if (_GearImg == null) Initialise(); return _GearImg; } }

        public static GUIContent GUIUnknownImg { get { if (_GUIUnknownImg == null) Initialise(); return _GUIUnknownImg; } }
        public static GUIContent GUIInconclusiveImg { get { if (_GUIInconclusiveImg == null) Initialise(); return _GUIInconclusiveImg; } }
        public static GUIContent GUIIgnoreImg { get { if (_GUIIgnoreImg == null) Initialise(); return _GUIIgnoreImg; } }
        public static GUIContent GUISuccessImg { get { if (_GUISuccessImg == null) Initialise(); return _GUISuccessImg; } }
        public static GUIContent GUIFailImg { get { if (_GUIFailImg == null) Initialise(); return _GUIFailImg; } }

        static Icons()
        {
            var dirs = Directory.GetDirectories("Assets", k_IconsFolderName, SearchOption.AllDirectories).Where(s => s.EndsWith(k_IconsFolderPath));
            if (dirs.Any())
                k_IconsAssetsPath = dirs.First();
            else
                Debug.LogWarning("The UnityTestTools asset folder path is incorrect. If you relocated the tools please change the path accordingly (Icons.cs).");
            
            /*
            FailImg = LoadTexture("failed.png");
            IgnoreImg = LoadTexture("ignored.png");
            SuccessImg = LoadTexture("passed.png");
            UnknownImg = LoadTexture("normal.png");
            InconclusiveImg = LoadTexture("inconclusive.png");
            StopwatchImg = LoadTexture("stopwatch.png");

            if (EditorGUIUtility.isProSkin)
            {
                RunAllImg = LoadTexture("play-darktheme.png");
                RunImg = LoadTexture("play_selected-darktheme.png");
                RunFailedImg = LoadTexture("rerun-darktheme.png");
                PlusImg = LoadTexture("create-darktheme.png");
                GearImg = LoadTexture("options-darktheme.png");
            }
            else
            {
                RunAllImg = LoadTexture("play-lighttheme.png");
                RunImg = LoadTexture("play_selected-lighttheme.png");
                RunFailedImg = LoadTexture("rerun-lighttheme.png");
                PlusImg = LoadTexture("create-lighttheme.png");
                GearImg = LoadTexture("options-lighttheme.png");
            }
            
            GUIUnknownImg = new GUIContent(UnknownImg);
            GUIInconclusiveImg = new GUIContent(InconclusiveImg);
            GUIIgnoreImg = new GUIContent(IgnoreImg);
            GUISuccessImg = new GUIContent(SuccessImg);
            GUIFailImg = new GUIContent(FailImg);
            */
        }

        private static void Initialise()
        {
            _FailImg = LoadTexture("failed.png");
            _IgnoreImg = LoadTexture("ignored.png");
            _SuccessImg = LoadTexture("passed.png");
            _UnknownImg = LoadTexture("normal.png");
            _InconclusiveImg = LoadTexture("inconclusive.png");
            _StopwatchImg = LoadTexture("stopwatch.png");

            if (EditorGUIUtility.isProSkin)
            {
                _RunAllImg = LoadTexture("play-darktheme.png");
                _RunImg = LoadTexture("play_selected-darktheme.png");
                _RunFailedImg = LoadTexture("rerun-darktheme.png");
                _PlusImg = LoadTexture("create-darktheme.png");
                _GearImg = LoadTexture("options-darktheme.png");
            }
            else
            {
                _RunAllImg = LoadTexture("play-lighttheme.png");
                _RunImg = LoadTexture("play_selected-lighttheme.png");
                _RunFailedImg = LoadTexture("rerun-lighttheme.png");
                _PlusImg = LoadTexture("create-lighttheme.png");
                _GearImg = LoadTexture("options-lighttheme.png");
            }

            _GUIUnknownImg = new GUIContent(UnknownImg);
            _GUIInconclusiveImg = new GUIContent(InconclusiveImg);
            _GUIIgnoreImg = new GUIContent(IgnoreImg);
            _GUISuccessImg = new GUIContent(SuccessImg);
            _GUIFailImg = new GUIContent(FailImg);
        }

        private static Texture2D LoadTexture(string fileName)
        {
            return (Texture2D)Resources.LoadAssetAtPath(k_IconsAssetsPath + Path.DirectorySeparatorChar + fileName, typeof(Texture2D));
        }
    }
}
