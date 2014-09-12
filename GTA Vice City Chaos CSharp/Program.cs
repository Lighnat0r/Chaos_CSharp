﻿using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace GTAVC_Chaos
{
    static class Program
    {


        public const int SEED_VALID_LENGTH = 4;
        public const float PROGRAM_VERSION = 1.21f;

        static public WelcomeWindow welcomeWindow;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // So that the decimal separator is always a period and stuff like that we set the cultureinfo 
            // to en-UK if it isn't already.
            if (Thread.CurrentThread.CurrentCulture.Name != "en-UK")
            {
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-UK");
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
            }

            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitWelcomeWindow();

        }

        static void InitWelcomeWindow()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            welcomeWindow = new WelcomeWindow();
            Application.Run(welcomeWindow);
        }

        static void OnApplicationExit(object Sender, EventArgs e)
        {
            Debug.WriteLine("Event OnApplicationExit fired");
        }
    }
}