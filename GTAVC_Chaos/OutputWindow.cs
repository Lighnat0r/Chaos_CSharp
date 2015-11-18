﻿using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace GTAVC_Chaos
{
    public partial class OutputWindow : Form
    {
        public OutputWindow()
        {
            InitializeComponent();
            Show();
        }

        private void OutputWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("Event OutputWindow_FormClosing fired");

            if (e.CloseReason == CloseReason.UserClosing && Program.game.IsRunning && MessageBox.Show("Are you sure you want to exit the program?", Settings.PROGRAM_NAME, MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void OutputWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.shouldStop = true;
            Debug.WriteLine("Event OutputWindow_FormClosed fired");
        }

        private void OutputWindow_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.SunriseIcon;

            difficultySeedLabel.Text = String.Format("{0} difficulty, seed {1:D" + Settings.SEED_VALID_LENGTH + "}", Settings.difficultyName, Settings.seed);
            StaticEffectsLabel.Text = "Static Effects " + (Settings.staticEffectsEnabled ? "Enabled" : "Disabled");
            PermanentEffectsLabel.Text = "Placeholder";
            TimedEffectLabel.Text = "Timed effect placeholder";
        }

        
    }
}
