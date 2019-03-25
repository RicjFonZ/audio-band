using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AudioBand.Commands;
using AudioBand.Models;
using AudioBand.Settings;

namespace AudioBand.ViewModels
{
    /// <summary>
    /// Viewmodel for all the custom labels.
    /// </summary>
    public class CustomLabelsVM : ViewModelBase
    {
        private readonly ICustomLabelService _labelService;
        private readonly HashSet<CustomLabelVM> _added = new HashSet<CustomLabelVM>();
        private readonly HashSet<CustomLabelVM> _removed = new HashSet<CustomLabelVM>();
        private readonly List<CustomLabel> _customLabels;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomLabelsVM"/> class
        /// with the list of custom labels and a label host.
        /// </summary>
        /// <param name="appsettings">The app setings.</param>
        /// <param name="labelService">The host for the labels.</param>
        public CustomLabelsVM(IAppSettings appsettings, ICustomLabelService labelService)
        {
            _customLabels = appsettings.CustomLabels;
            CustomLabels = new ObservableCollection<CustomLabelVM>(_customLabels.Select(customLabel => new CustomLabelVM(customLabel)));
            _labelService = labelService;

            foreach (var customLabelVm in CustomLabels)
            {
                _labelService.AddCustomTextLabel(customLabelVm);
            }

            AddLabelCommand = new RelayCommand(AddLabelCommandOnExecute);
            RemoveLabelCommand = new AsyncRelayCommand<CustomLabelVM>(RemoveLabelCommandOnExecute);
        }

        /// <summary>
        /// Gets the collection of custom label viewmodels.
        /// </summary>
        public ObservableCollection<CustomLabelVM> CustomLabels { get; }

        /// <summary>
        /// Gets the command to add a new label.
        /// </summary>
        public RelayCommand AddLabelCommand { get; }

        /// <summary>
        /// Gets the command to remove a label.
        /// </summary>
        /// <remarks>Async so its easier to show a dialog.</remarks>
        public AsyncRelayCommand<CustomLabelVM> RemoveLabelCommand { get; }

        /// <summary>
        /// Gets or sets the dialog service used to show a dialog.
        /// </summary>
        public IDialogService DialogService { get; set; }

        /// <inheritdoc/>
        protected override void OnBeginEdit()
        {
            _added.Clear();
            _removed.Clear();

            foreach (var customLabelVm in CustomLabels)
            {
                customLabelVm.BeginEdit();
            }
        }

        /// <inheritdoc/>
        protected override void OnCancelEdit()
        {
            foreach (var label in _added)
            {
                CustomLabels.Remove(label);
                _labelService.RemoveCustomTextLabel(label);
            }

            foreach (var label in _removed)
            {
                CustomLabels.Add(label);
                _labelService.AddCustomTextLabel(label);
            }

            _added.Clear();
            _removed.Clear();

            foreach (var customLabelVm in CustomLabels)
            {
                customLabelVm.CancelEdit();
            }
        }

        /// <inheritdoc/>
        protected override void OnEndEdit()
        {
            _added.Clear();
            _removed.Clear();

            _customLabels.Clear();

            foreach (var customLabelVm in CustomLabels)
            {
                customLabelVm.EndEdit();
                _customLabels.Add(customLabelVm.GetModel());
            }
        }

        private void AddLabelCommandOnExecute(object o)
        {
            var newLabel = new CustomLabelVM(new CustomLabel()) { Name = "New Label" };
            CustomLabels.Add(newLabel);
            _labelService.AddCustomTextLabel(newLabel);

            _added.Add(newLabel);
        }

        private async Task RemoveLabelCommandOnExecute(CustomLabelVM labelVm)
        {
            if (!await DialogService.ShowConfirmationDialogAsync("Delete Label", $"Are you sure you want to delete the label '{labelVm.Name}'?"))
            {
                return;
            }

            CustomLabels.Remove(labelVm);
            _labelService.RemoveCustomTextLabel(labelVm);

            // Only add to removed if not a new label
            if (!_added.Remove(labelVm))
            {
                _removed.Add(labelVm);
            }
        }
    }
}
