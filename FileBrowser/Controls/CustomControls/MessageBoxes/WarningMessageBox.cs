﻿using System.Windows.Forms;

namespace FileBrowser.Controls.CustomControls.MessageBoxes {
    public class WarningMessageBox {

        public static void Show( string title, string description ) {
            MessageBox.Show(description, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
