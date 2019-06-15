﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class ManageModsPage : ReloadedIIPage
    {
        public ManageModsViewModel ViewModel { get; set; }
        private readonly ResourceManipulator _manipulator;
        private CollectionViewSource _modsViewSource;
        private CollectionViewSource _appsViewSource;

        public ManageModsPage() : base()
        {  
            InitializeComponent();
            ViewModel = IoC.Get<ManageModsViewModel>();
            this.DataContext = ViewModel;

            this.AnimateOutStarted += SaveCurrentMod;

            // Setup filters
            _manipulator = new ResourceManipulator(this.Contents);
            _modsViewSource = _manipulator.Get<CollectionViewSource>("SortedMods");
            _appsViewSource = _manipulator.Get<CollectionViewSource>("SortedApps");
            _modsViewSource.Filter += ModsViewSourceOnFilter;
            _appsViewSource.Filter += AppsViewSourceOnFilter;
        }

        private void AppsViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (this.AppsFilter.Text.Length <= 0)
            {
                e.Accepted = true;
                return;
            }

            var tuple = (BooleanApplicationTuple)e.Item;
            e.Accepted = tuple.AppConfig.AppName.IndexOf(this.AppsFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void ModsViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (this.ModsFilter.Text.Length <= 0)
            {
                e.Accepted = true;
                return;
            }

            var tuple = (ImageModPathTuple)e.Item;
            e.Accepted = tuple.ModConfig.ModName.IndexOf(this.ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void Button_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var createModDialog = new CreateModDialog();
                createModDialog.Owner = Window.GetWindow(this);
                createModDialog.ShowDialog();
            }
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ViewModel.SelectedModPathTuple != null)
                ViewModel.Icon = Imaging.BitmapFromUri(new Uri(ViewModel.SelectedModPathTuple.Image));

            // Tell viewmodel to swap ModId compatibility chart.
            ImageModPathTuple oldModTuple = null;
            ImageModPathTuple newModTuple = null;
            if (e.RemovedItems.Count > 0)
                oldModTuple = e.RemovedItems[0] as ImageModPathTuple;

            if (e.AddedItems.Count > 0)
                newModTuple = e.AddedItems[0] as ImageModPathTuple;

            ViewModel.SwapMods(oldModTuple, newModTuple);
            e.Handled = true;
        }

        private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _modsViewSource.View.Refresh();
        }

        private void AppsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _appsViewSource.View.Refresh();
        }

        private void SaveCurrentMod()
        {
            ViewModel.SelectedModPathTuple?.Save();
        }
    }
}
