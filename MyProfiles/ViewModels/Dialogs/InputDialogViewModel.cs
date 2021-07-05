using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;
using System.Windows.Input;

namespace MyProfiles.ViewModels.Dialogs
{
    public class InputDialogViewModel : ViewModelBase, IModalDialogViewModel
    {
        #region Constructors

        [GalaSoft.MvvmLight.Ioc.PreferredConstructorAttribute]
        public InputDialogViewModel() { }



        public InputDialogViewModel(string title, string content, string profileName = null)
        {
            Title = title;
            Content = content;
            Text = profileName;
        }

        #endregion



        #region Commands 

        // Potwierdzenie.
        private ICommand _confirmCommand;
        public ICommand ConfirmCommand { get => _confirmCommand ?? (_confirmCommand = new RelayCommand(Confirm, () => !string.IsNullOrEmpty(Text))); }

        #endregion



        #region Properties

        // MVVM Dialogs.
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => Set(nameof(DialogResult), ref _dialogResult, value);
        }



        // Zawartość okna.
        public string Title { get; set; }
        public string Content { get; set; }
        public string Text { get; set; }

        #endregion



        #region Methods

        // Potwierdzenie (np. przyciski Yes/Ok/Save).
        private void Confirm()
        {
            // MVVM Dialogs.
            DialogResult = true;
        }

        #endregion
    }
}