using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExportExtensionCommon;

namespace CaptureCenter.CMIS
{
    public partial class CMIS_WPFControl : SIEEUserControl
    {
        public CMIS_WPFControl() : base()
        {
            InitializeComponent();
        }

        #region Tab handling
        private Dictionary<string, bool> tabActivation = null;

        private void initializeTabActivation()
        {
            tabActivation = new Dictionary<string, bool>();
            foreach (string name in ((CMISViewModel)DataContext).Tabnames.Keys) tabActivation[name] = false;
        }

        void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((CMISViewModel)DataContext).Tabnames == null) return;
            if (tabActivation == null) initializeTabActivation();
            foreach (string tabName in tabActivation.Keys)
            {
                TabItem pt = (TabItem)LogicalTreeHelper.FindLogicalNode((DependencyObject)sender, tabName);
                if (pt.IsSelected)
                {
                    if (tabActivation[tabName]) return;
                    tabActivation[tabName] = true;
                    try { ((CMISViewModel)DataContext).ActivateTab(tabName); }
                    finally { tabActivation[tabName] = false; }
                    return;
                }
            }
        }
        #endregion

        #region DocumentTab
        private void Button_AddTokenToFile(object sender, RoutedEventArgs e)
        {
            CMISViewModel vm = ((CMISViewModel)DataContext);
            vm.DT.AddTokenToFileHandler((string)((Button)sender).Tag);
        }
        #endregion

        #region Connection
        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((CMISViewModel)DataContext).CT.PasswordChangedHandler();
        }
        private void Button_LoadRepositories_Click(object sender, RoutedEventArgs e)
        {
            ((CMISViewModel)DataContext).LoadRepositoriesButtonHandler();
        }
        private void Button_Connect_Click(object sender, RoutedEventArgs e)
        {
            ((CMISViewModel)DataContext).ConnectdButtonHandler();
        }
        private void Button_CheckConnection_Click(object sender, RoutedEventArgs e)
        {
            ((CMISViewModel)DataContext).CT.CheckConnectionHandler();
        }

        #endregion

        #region Folder
        private void FolderTreeView_Selection_Changed(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if ((TVIViewModel)e.NewValue == null) return;
            ((CMISViewModel)DataContext).FT.SetSelectedFolderHandler((TVIViewModel)e.NewValue);
        }

        private void TypeTreeView_Selection_Changed(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if ((TVIViewModel)e.NewValue == null) return;
            if ( ((CMISTypeNode)((TVIViewModel)e.NewValue).Tvim).CMISType == null) return;
            ((CMISViewModel)DataContext).TT.SetSelectTypeHandler((TVIViewModel)e.NewValue);
        }
        #endregion

        #region Types
        private void Button_LoadProperties_Click(object sender, RoutedEventArgs e)
        {
            ((CMISViewModel)DataContext).LoadPropertiesHandler();
        }
        private void Button_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            ((CMISViewModel)DataContext).TT.SelectAllHandler();
        }
        private void Button_DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            ((CMISViewModel)DataContext).TT.DeselectAllHandler();
        }
        #endregion
    }
}
