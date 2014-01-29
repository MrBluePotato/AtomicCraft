// Copyright 2009-2014 Matvei Stefarov <me@matvei.org>

using System;
using System.Windows.Forms;

namespace fCraft.ConfigGUI
{
    public sealed partial class DeleteRankPopup : Form
    {
        public DeleteRankPopup(Rank deletedRank)
        {
            InitializeComponent();
            foreach (Rank rank in RankManager.Ranks)
            {
                if (rank != deletedRank)
                {
                    cSubstitute.Items.Add(MainForm.ToComboBoxOption(rank));
                }
            }
            lWarning.Text = String.Format(lWarning.Text, deletedRank.Name);
            cSubstitute.SelectedIndex = cSubstitute.Items.Count - 1;
        }

        internal Rank SubstituteRank { get; private set; }


        private void cSubstitute_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cSubstitute.SelectedIndex < 0) return;
            foreach (Rank rank in RankManager.Ranks)
            {
                if (cSubstitute.SelectedItem.ToString() != MainForm.ToComboBoxOption(rank)) continue;
                SubstituteRank = rank;
                bDelete.Enabled = true;
                break;
            }
        }
    }
}