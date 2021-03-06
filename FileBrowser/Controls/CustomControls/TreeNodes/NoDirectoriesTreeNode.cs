﻿using FileBrowser.Models.Language;
using FileBrowser.Models.Themes;
using System.Drawing;
using System.Windows.Forms;

namespace FileBrowser.Controls.CustomControls.TreeNodes {



    /// <summary>
    /// TreeNode that represents a message when the user has not selected a single directory yet.
    /// </summary>
    public class NoDirectoriesTreeNode : TreeNode, ILocalizable, IThemeable {

        private ThemeManager themeManager;

        public NoDirectoriesTreeNode(ThemeManager themeManager) {
            this.themeManager = themeManager;
            Text = Resources.Strings.ErrorNoDirectories;
            ForeColor = themeManager.ColorTheme.ErrorText;
        }

        public void UpdateText() {
            Text = Resources.Strings.ErrorNoDirectories;
        }

        public void UpdateTheme() {
            ForeColor = themeManager.ColorTheme.ErrorText;
        }
    }
}
