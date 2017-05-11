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
            this.Loaded += bindCommands;
        }

        #region Event handler
       
        // Connection tab
        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((CMISViewModel)DataContext).CT.PasswordChangedHandler();
        }

        // Folder tab
        private void FolderTreeView_Selection_Changed(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if ((TVIViewModel)e.NewValue == null) return;
            ((CMISViewModel)DataContext).FT.SetSelectedFolderHandler((TVIViewModel)e.NewValue);
        }

        // Types tab
        private void TypeTreeView_Selection_Changed(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if ((TVIViewModel)e.NewValue == null) return;
            if ( ((CMISTypeNode)((TVIViewModel)e.NewValue).Tvim).CMISType == null) return;
            ((CMISViewModel)DataContext).TT.SetSelectTypeHandler((TVIViewModel)e.NewValue);
        }

        // Document Tab
        private void Button_AddTokenToFile(object sender, RoutedEventArgs e)
        {
            CMISViewModel vm = ((CMISViewModel)DataContext);
            vm.DT.AddTokenToFileHandler((string)((Button)sender).Tag);
        }
        
        #endregion

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

        #region Commands

        // Connection tab
        public static RoutedUICommand TestConnection { get { return testConnection; } }
        private static RoutedUICommand testConnection = new RoutedUICommand(
            "Test connection", "testConnection", typeof(CMIS_WPFControl));

        public static RoutedUICommand Version { get { return version; } }
        private static RoutedUICommand version = new RoutedUICommand(
            "Show version", "showVersion", typeof(CMIS_WPFControl),
            new InputGestureCollection(new List<InputGesture>() {
                new KeyGesture(Key.V, ModifierKeys.Alt)
        }));

        public static RoutedUICommand LoadRepositories { get { return loadRepositories; } }
        private static RoutedUICommand loadRepositories = new RoutedUICommand(
            "Load repositories", "loadRepositories", typeof(CMIS_WPFControl),
            new InputGestureCollection(new List<InputGesture>() {
                new KeyGesture(Key.L, ModifierKeys.Alt)
        }));

        public static RoutedUICommand Connect { get { return connect; } }
        private static RoutedUICommand connect = new RoutedUICommand(
            "Connect", "connect", typeof(CMIS_WPFControl),
            new InputGestureCollection(new List<InputGesture>() {
                new KeyGesture(Key.C, ModifierKeys.Alt)
        }));

        // Types tab
        public static RoutedUICommand LoadProperties { get { return loadProperties; } }
        private static RoutedUICommand loadProperties = new RoutedUICommand(
            "Load Properties", "loadProperties", typeof(CMIS_WPFControl),
            new InputGestureCollection(new List<InputGesture>() {
                new KeyGesture(Key.P, ModifierKeys.Alt)
        }));

        public static RoutedUICommand SelectAll { get { return selectAll; } }
        private static RoutedUICommand selectAll = new RoutedUICommand(
            "Select all", "selectAll", typeof(CMIS_WPFControl),
            new InputGestureCollection(new List<InputGesture>() {
                new KeyGesture(Key.S, ModifierKeys.Alt)
        }));

        public static RoutedUICommand DeSelectAll { get { return deSelectAll; } }
        private static RoutedUICommand deSelectAll = new RoutedUICommand(
            "De-select all", "typeof", typeof(CMIS_WPFControl),
            new InputGestureCollection(new List<InputGesture>() {
                new KeyGesture(Key.D, ModifierKeys.Alt)
        }));

        private void bindCommands(object sender, RoutedEventArgs ee)
        {
            // Connection tab
            CommandBindings.Add(new CommandBinding(
                CMIS_WPFControl.TestConnection,
                (s, e) => { ((CMISViewModel)DataContext).CT.CheckConnectionHandler(); }));
            CommandBindings.Add(new CommandBinding(
                CMIS_WPFControl.Version,
                (s, e) => { ((CMISViewModel)DataContext).ShowVersion(); }));
            CommandBindings.Add(new CommandBinding(
                CMIS_WPFControl.LoadRepositories,
                (s, e) => { ((CMISViewModel)DataContext).LoadRepositoriesButtonHandler(); }));
            CommandBindings.Add(new CommandBinding(
                CMIS_WPFControl.Connect,
                (s, e) => { ((CMISViewModel)this.DataContext).ConnectdButtonHandler(); },
                (s, e) => { e.CanExecute = ((CMISViewModel)DataContext).CanConnect(); }));

            // Types tab
            CommandBindings.Add(new CommandBinding(
                CMIS_WPFControl.LoadProperties,
                (s, e) => { ((CMISViewModel)this.DataContext).LoadPropertiesHandler(); },
                (s, e) => { e.CanExecute = ((CMISViewModel)DataContext).CanLoadProperties(); }));
            CommandBindings.Add(new CommandBinding(
                CMIS_WPFControl.SelectAll,
                (s, e) => { ((CMISViewModel)this.DataContext).TT.SelectAllHandler(); },
                (s, e) => { e.CanExecute = ((CMISViewModel)DataContext).TT.CanSelect(); }));
            CommandBindings.Add(new CommandBinding(
                CMIS_WPFControl.DeSelectAll,
                (s, e) => { ((CMISViewModel)this.DataContext).TT.DeselectAllHandler(); },
                (s, e) => { e.CanExecute = ((CMISViewModel)DataContext).TT.CanSelect(); }));

        }
        #endregion
    }
}
