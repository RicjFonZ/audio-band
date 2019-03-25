using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms.Integration;
using AudioBand.Commands;
using AudioBand.ViewModels;

namespace AudioBand.Views.Wpf
{
    /// <summary>
    /// The code behind for the settings window.
    /// </summary>
    public partial class SettingsWindow : ISettingsWindow
    {
        private static readonly HashSet<string> _bindingHelpAssemblies = new HashSet<string>
        {
            "MahApps.Metro.IconPacks.Material",
            "ColorPickerWPF"
        };

        private bool _shouldSave;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsWindow"/> class
        /// with the settings viewmodel.
        /// </summary>
        /// <param name="vm">The settings window viewmodel.</param>
        public SettingsWindow(
            SettingsWindowVM vm,
            AudioBandVM audioBandVM,
            AlbumArtPopupVM albumArtPopupVM,
            AlbumArtVM albumArtVM,
            CustomLabelsVM customLabelsVM,
            AboutVM aboutVm,
            NextButtonVM nextButtonVM,
            PlayPauseButtonVM playPauseButtonVM,
            PreviousButtonVM previousButtonVM,
            ProgressBarVM progressBarVM)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            ElementHost.EnableModelessKeyboardInterop(this);

            CancelCloseCommand = new RelayCommand(CancelCloseCommandOnExecute);
            SaveCloseCommand = new RelayCommand(SaveCloseCommandOnExecute);

            AudioBandVM = audioBandVM;
            AlbumArtPopupVM = albumArtPopupVM;
            AlbumArtVM = albumArtVM;
            CustomLabelsVM = customLabelsVM;
            AboutVM = aboutVm;
            NextButtonVM = nextButtonVM;
            PlayPauseButtonVM = playPauseButtonVM;
            PreviousButtonVM = previousButtonVM;
            ProgressBarVM = progressBarVM;

            InitializeComponent();
            DataContext = vm;
        }

        /// <inheritdoc/>
        public event EventHandler Saved;

        /// <inheritdoc/>
        public event EventHandler Canceled;

        /// <summary>
        /// Gets the command to cancel changes and close.
        /// </summary>
        public RelayCommand CancelCloseCommand { get; }

        /// <summary>
        /// Gets the command to save changes and close.
        /// </summary>
        public RelayCommand SaveCloseCommand { get; }

        public AudioBandVM AudioBandVM { get; private set; }

        public AlbumArtPopupVM AlbumArtPopupVM { get; private set; }

        public AlbumArtVM AlbumArtVM { get; private set; }

        public CustomLabelsVM CustomLabelsVM { get; private set; }

        public AboutVM AboutVm { get; private set; }

        public NextButtonVM NextButtonVM { get; private set; }

        public PlayPauseButtonVM PlayPauseButtonVM { get; private set; }

        public PreviousButtonVM PreviousButtonVM { get; private set; }

        public ProgressBarVM ProgressBarVM { get; private set; }

        public AboutVM AboutVM { get; private set; }

        public ObservableCollection<AudioSourceSettingsVM> AudioSourceSettingsVM => new ObservableCollection<AudioSourceSettingsVM>();

        public void ShowWindow()
        {
            Show();
        }

        /// <inheritdoc/>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;

            if (_shouldSave)
            {
                Saved?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Canceled?.Invoke(this, EventArgs.Empty);
            }

            _shouldSave = false;
            Hide();
        }

        // Problem with late binding. Fuslogvw shows its not probing the original location.
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // name is in this format Xceed.Wpf.Toolkit, Version=3.4.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4
            var asmName = args.Name.Substring(0, args.Name.IndexOf(','));
            if (!_bindingHelpAssemblies.Contains(asmName))
            {
                return null;
            }

            var filename = Path.Combine(DirectoryHelper.BaseDirectory, asmName + ".dll");
            return File.Exists(filename) ? Assembly.LoadFrom(filename) : null;
        }

        private void SaveCloseCommandOnExecute(object o)
        {
            _shouldSave = true;
            Close();
        }

        private void CancelCloseCommandOnExecute(object o)
        {
            _shouldSave = false;
            Close();
        }
    }
}
