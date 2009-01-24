﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace GitUI
{
    delegate void DataCallback(string text);
    public partial class FormProcess : Form
    {
        public FormProcess(string process, string arguments, string remote)
        {
            InitializeComponent();

            ProcessString = process;
            ProcessArguments = arguments;
            Remote = remote;

            ShowDialog();
        }

        public FormProcess(string process, string arguments)
        {
            InitializeComponent();

            ProcessString = process;
            ProcessArguments = arguments;

            ShowDialog();
        }

        public FormProcess(string arguments)
        {
            InitializeComponent();

            ProcessString = GitCommands.Settings.GitDir + "git.cmd";
            ProcessArguments = arguments;

            ShowDialog();
        }

        private bool restart = false;
        public string Remote { get; set; }
        public string ProcessString { get; set; }
        public string ProcessArguments { get; set; }
        public Process Process { get; set; }
        private GitCommands.GitCommands gitCommand;

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormProcess_Load(object sender, EventArgs e)
        {
            Start();
        }

        private void Start()
        {
            restart = false;
            Output.Text = "";
            AddOutput(ProcessString + " " + ProcessArguments);

            Plink = GitCommands.GitCommands.Plink();

            ProgressBar.Visible = true;

            outputString = new StringBuilder();

            gitCommand = new GitCommands.GitCommands();
            gitCommand.CollectOutput = false;
            gitCommand.CmdStartProcess(ProcessString, ProcessArguments);

            gitCommand.Exited += new EventHandler(gitCommand_Exited);
            gitCommand.DataReceived += new DataReceivedEventHandler(gitCommand_DataReceived);

            Ok.Enabled = false;
        }

        public bool Plink { get; set; }

        void SetProgress(string text)
        {
            int index = text.IndexOf('%');
            int progressValue;
            if (index > 4 && int.TryParse(text.Substring(index - 3, 3), out progressValue))
            {
                if (ProgressBar.Style != ProgressBarStyle.Blocks)
                    ProgressBar.Style = ProgressBarStyle.Blocks;
                ProgressBar.Value = Math.Min(100, progressValue);
            }
            this.Text = text;
        }

        void AddOutput(string text)
        {
            Output.Text += text + "\n";
        }

        public StringBuilder outputString;

        void Done()
        {
            if (restart)
            {
                Start();
                return;
            }

            AddOutput(outputString.ToString());
            AddOutput("Done");
            ProgressBar.Visible = false;
            Ok.Enabled = true;
            //An error occured!
            if (gitCommand != null && gitCommand.Process != null && gitCommand.Process.ExitCode != 0)
            {
                ErrorImage.Visible = true;
                SuccessImage.Visible = false;
                if (Plink)
                {
                    if (ProcessArguments.Contains("pull") ||
                        ProcessArguments.Contains("push") ||
                        ProcessArguments.Contains("plink") ||
                        ProcessString.Contains("clone") ||
                        ProcessArguments.Contains("clone"))
                    {
                        if (Output.Text.Contains("successfully authenticated"))
                        {
                            SuccessImage.Visible = true;
                            ErrorImage.Visible = false;
                        }

                        if (Output.Text.Contains("FATAL ERROR") && Output.Text.Contains("authentication"))
                        {
                            FormPuttyError puttyError = new FormPuttyError();
                            puttyError.ShowDialog();
                            if (puttyError.RetryProcess)
                            {
                                FormProcess_Load(null, null);
                            }
                        }
                    }
                }
            }
            else
            {
                ErrorImage.Visible = false;
                SuccessImage.Visible = true;
            }
        }

        void gitCommand_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            if (e.Data.Contains("%") || e.Data.StartsWith("remote: Counting objects"))
            {
                if (ProgressBar.InvokeRequired)
                {
                    // It's on a different thread, so use Invoke.
                    DataCallback d = new DataCallback(SetProgress);
                    this.Invoke(d, new object[] { e.Data });
                } else
                {
                    SetProgress(e.Data);
                }
            } else
            {
                /*if (Output.InvokeRequired)
                {
                    // It's on a different thread, so use Invoke.
                    DataCallback d = new DataCallback(AddOutput);
                    this.Invoke(d, new object[] { e.Data });
                } else
                {
                    AddOutput(e.Data);
                }*/
                outputString.Append(e.Data);
                outputString.Append("\n");
            }


            if (Plink)
            {
                if (e.Data.StartsWith("If you trust this host, enter \"y\" to add the key to"))
                {
                    if (MessageBox.Show("The fingerprint of this host is not registered by PuTTY.\nThis causes this process to hang, and that why it is automaticly stopped.\n\nWhen te connection is opened detached from Git and GitExtensions, the host's fingerprint can be registered.\nYou could also manually add the host's fingerprint or run Test Connection from the remotes dialog.\n\nDo you want to register the host's fingerprint and restart the process?", "Host Fingerprint not registered", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        string remoteUrl = GitCommands.GitCommands.GetSetting("remote." + Remote + ".url");

                        if (string.IsNullOrEmpty(remoteUrl))
                            GitCommands.GitCommands.RunRealCmd("cmd.exe", "/k \"\"" + GitCommands.Settings.Plink + "\" " + Remote + "\"");
                        else
                            GitCommands.GitCommands.RunRealCmd("cmd.exe", "/k \"\"" + GitCommands.Settings.Plink + "\" " + remoteUrl + "\"");

                        restart = true;
                    }

                    try
                    {
                        gitCommand.Process.Kill();
                    }
                    catch
                    {
                    }
                }
            }
        }



        void gitCommand_Exited(object sender, EventArgs e)
        {
            if (Ok.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                DoneCallback d = new DoneCallback(Done);
                this.Invoke(d, new object[] {  });
            }
            else
            {
                Done();
            }
        }
    }
}
