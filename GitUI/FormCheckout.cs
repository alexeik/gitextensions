﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormCheckout : GitExtensionsForm
    {
        public FormCheckout()
        {
            InitializeComponent(); Translate();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                if (RevisionGrid.GetRevisions().Count != 1)
                {
                    MessageBox.Show("Select 1 revision to checkout.", "Checkout");
                    return;
                }

                string command = "checkout \"" + RevisionGrid.GetRevisions()[0].Guid + "\"";
                if(Force.Checked)
                    command += " --force";

                new FormProcess(command).ShowDialog();

                //CheckoutDto dto = new CheckoutDto(RevisionGrid.GetRevisions()[0].Guid);
                //GitCommands.Checkout commit = new GitCommands.Checkout(dto);
                //commit.Execute();

                //MessageBox.Show("Command executed \n" + dto.Result, "Checkout");

                Close();
            }
            catch
            {
            }
        }

        private void FormCheckout_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("checkout");
        }

        private void FormCheckout_Load(object sender, EventArgs e)
        {
            RestorePosition("checkout");
        }

        private void Branch_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
