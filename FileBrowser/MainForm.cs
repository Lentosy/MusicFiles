﻿using FileBrowser.FormControls;
using FileBrowser.Forms;
using FileBrowser.Persistence.Repositories;
using FileBrowser.Properties;
using FileBrowser.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using FileBrowser.Domain;
using FileBrowser.Domain.Language;
using FileBrowser.Domain.Models;
using FileBrowser.Domain.Themes;

namespace FileBrowser
{

    /// <summary>
    /// The main entry point of the application.
    /// </summary>
    public partial class MainForm : Form, ILocalizable, IThemeable
    {

        private RepositoryController repositoryController;
        private DependencyController dependencyController;

        private DirectoryTreeView DirectoryTreeView;

        private ICollection<Folder> musicDirectories;
        private ICollection<string> extensions;
        private ICollection<string> filteredExtensions;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MainForm(RepositoryController repositoryController, DependencyController dependencyController)
        {
            InitializeComponent();
            this.repositoryController = repositoryController;
            this.dependencyController = dependencyController;
        }

        /* EVENTS RELATED TO THE WHOLE FORM*/
        #region MAINFORM EVENTS
        /// <summary>
        ///  Occurs when MainForm has loaded. Initializes everything needed to load the DirectoryTreeView
        /// </summary>
        /// <seealso cref="DirectoryTreeView"/>
        /// <remarks>This method should never be called by the User, but by <see cref="MainForm"/> itself</remarks>
        /// <param name="e">EventArgs</param>
        protected override void OnLoad(EventArgs e)
        {

            filteredExtensions = new List<string>();
            musicDirectories = repositoryController.FolderRepository.GetFolders();
            extensions = repositoryController.ExtensionRepository.GetExtensions();
            Icon = Properties.Resources.Icon;
            //Treeview specific UI
            DirectoryTreeView = new DirectoryTreeView(repositoryController, dependencyController);

            PanelContent.Controls.Add(DirectoryTreeView);
            PanelContent.Controls.SetChildIndex(DirectoryTreeView, 0);
            DirectoryTreeView.Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            DirectoryTreeView.BorderStyle = BorderStyle.None;
            DirectoryTreeView.Dock = DockStyle.Fill;

            DirectoryTreeView.NodeMouseClick += DirectoryTreeViews_NodeMouseClick;
            DirectoryTreeView.NodeMouseDoubleClick += DirectoryTreeViews_NodeMouseDoubleClick;

            UpdateSizeAndLocation();
            UpdateExtensionMenu();
            UpdateTheme();
            UpdateText();

            DirectoryTreeView.Generate(Settings.Default.Expand);
            base.OnLoad(e);
        }

        private void UpdateExtensionMenu()
        {
            FlowLayoutPanelExtensions.Controls.Clear();
            foreach (string ext in extensions)
            {
                CheckBox checkBoxExtension = new CheckBox
                {
                    Text = ext,
                    AutoSize = true,
                };

                checkBoxExtension.CheckedChanged += CheckBoxExtension_CheckedChanged;
                FlowLayoutPanelExtensions.Controls.Add(checkBoxExtension);
            }
        }

        private void CheckBoxExtension_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBoxExtension = (CheckBox)sender;
            if (checkBoxExtension.Checked)
            {
                filteredExtensions.Add(checkBoxExtension.Text);
            }
            else
            {
                filteredExtensions.Remove(checkBoxExtension.Text);
            }

            DirectoryTreeView.Search(TextBoxSearch.Text, filteredExtensions);
            //   DirectoryTreeView.FilterExtensions(filteredExtensions);
        }

        /// <summary>
        /// Sets the saved size and location of the form
        /// </summary>
        public void UpdateSizeAndLocation()
        {
            if (!FormUtils.IsOnScreen(this))
            {
                Location = Settings.Default.WindowLocation;
            }
            Size = Settings.Default.WindowSize;
            Location = Settings.Default.WindowLocation;
        }

        public void UpdateTheme()
        {

            MenuButtonCollapseAll.ForeColor = dependencyController.ThemeManager.ColorTheme.DefaultText;
            MenuButtonShowAll.ForeColor = dependencyController.ThemeManager.ColorTheme.DefaultText;
            LabelSearch.ForeColor = dependencyController.ThemeManager.ColorTheme.DefaultText;
            foreach (CheckBox checkBoxExtension in FlowLayoutPanelExtensions.Controls)
            {
                checkBoxExtension.ForeColor = dependencyController.ThemeManager.ColorTheme.DefaultText;
            }


            PanelMainMenu.ForeColor = dependencyController.ThemeManager.ColorTheme.ForeGroundMenu;
            PanelMainMenu.BackColor = dependencyController.ThemeManager.ColorTheme.BackGroundMenu;
            PanelContent.BackColor = dependencyController.ThemeManager.ColorTheme.BackGroundTree;

            DirectoryTreeView.UpdateTheme();
        }

        public void UpdateText()
        {

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(dependencyController.LanguageManager.GetPreferredLanguageCode());
            TitleBuilder titleBuilder = new TitleBuilder();
            Text = titleBuilder.BuildPrimaryTitle();
            MenuButtonGuide.Text = Resources.Strings.MenuHelp;
            MenuButtonRefresh.Text = Resources.Strings.MenuRefresh;
            MenuButtonSettings.Text = Resources.Strings.MenuSettings;
            LabelSearch.Text = Resources.Strings.Search;
            MenuButtonCollapseAll.Text = Resources.Strings.CollapseAll;
            MenuButtonShowAll.Text = Resources.Strings.ShowAll;
            DirectoryTreeView.UpdateText();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Settings.Default.WindowLocation = Location;

            if (WindowState == FormWindowState.Normal)
            {
                Settings.Default.WindowSize = Size;
            }
            else
            {
                Settings.Default.WindowSize = RestoreBounds.Size;
            }

            Settings.Default.Save();
            base.OnFormClosing(e);
        }

        #endregion

        /* EVENTS RELATED THE THE MENU */
        #region MENU EVENTS

        /// <summary>
        /// Refreshes the TreeView
        /// <seealso cref="DirectoryTreeView"/>
        /// </summary>
        private void MenuButtonRefresh_Click(object sender, EventArgs e)
        {
            musicDirectories = repositoryController.FolderRepository.GetFolders();
            extensions = repositoryController.ExtensionRepository.GetExtensions();

            DirectoryTreeView.Generate(Settings.Default.Expand);
            UpdateExtensionMenu();
        }


        /// <summary>
        /// Open the settings view
        /// <seealso cref="SettingsForm"/>
        /// </summary>
        private void ButtonSettings_Click(object sender, EventArgs e)
        {

            SettingsForm settingsForm = new SettingsForm(repositoryController, dependencyController);
            settingsForm.LanguageChanged += SettingsForm_LanguageChanged;
            settingsForm.ColorChanged += SettingsForm_ColorChanged;

            FormUtils.OpenForm(settingsForm, Location);


        }

        /// <summary>
        /// Occurs when a color is changed in the SettingsForm
        /// <seealso cref="SettingsForm"/>
        /// </summary>
        private void SettingsForm_ColorChanged(object sender, EventArgs e)
        {
            UpdateTheme();
        }

        /// <summary>
        /// Occurs when the language is changed in the SettingsForm
        /// <seealso cref="SettingsForm"/>
        /// </summary>
        private void SettingsForm_LanguageChanged(object sender, EventArgs e)
        {
            UpdateText();
        }

        /// <summary>
        /// Opens the help view
        /// <seealso cref="HelpForm"/>
        /// </summary>
        private void MenuButtonGuide_Click(object sender, EventArgs e)
        {
            FormUtils.OpenForm(new HelpForm(), Location);
        }


        #endregion

        /* EVENTS RELATED TO THE TREEVIEW */
        #region TREE

        /// <summary>
        /// Occurs when a doubleclick occured in the TreeView
        /// <seealso cref="DirectoryTreeView.DoubleClicked(TreeNode)"/>
        /// </summary>
        private void DirectoryTreeViews_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DirectoryTreeView.DoubleClicked(e.Node);
            }

        }

        /// <summary>
        /// Occurs when right clicking inside the treeview. 
        /// <seealso cref="DirectoryTreeView.RightClicked(TreeNode)"/>
        /// </summary>
        private void DirectoryTreeViews_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DirectoryTreeView.RightClicked(e.Node);
            }

        }

        /// <summary>
        /// Occurs when Enter is pressed while focused on TextBoxSearch and searches The DirectoryTreeView
        /// </summary>
        private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            e.SuppressKeyPress = true;
            DirectoryTreeView.Search(TextBoxSearch.Text, filteredExtensions);
        }

        /// <summary>
        /// Occurs when the search input has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            string input = TextBoxSearch.Text;
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrEmpty(input))
            {
                DirectoryTreeView.Search(null, filteredExtensions);
            }
        }

        /// <summary>
        /// Collapes the whole TreeView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuButtonCollapseAll_Click(object sender, EventArgs e)
        {
            DirectoryTreeView.BeginUpdate();
            DirectoryTreeView.CollapseAll();
            DirectoryTreeView.EndUpdate();
        }

        /// <summary>
        /// Expands the whole treeview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuButtonShowAll_Click(object sender, EventArgs e)
        {
            DirectoryTreeView.BeginUpdate();
            DirectoryTreeView.ExpandAll();
            DirectoryTreeView.Nodes[0].EnsureVisible(); // scroll to top
            DirectoryTreeView.EndUpdate();
        }


        #endregion

        private void ButtonClearSearch_Click(object sender, EventArgs e)
        {
            TextBoxSearch.Clear();
        }
    }
}
