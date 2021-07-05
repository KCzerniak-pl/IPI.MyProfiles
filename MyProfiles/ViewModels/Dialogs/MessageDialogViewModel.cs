using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;
using System.Windows;
using System.Windows.Input;

namespace MyProfiles.ViewModels.Dialogs
{
    public class MessageDialogViewModel : ViewModelBase, IModalDialogViewModel
    {
        #region Constructors

        [GalaSoft.MvvmLight.Ioc.PreferredConstructorAttribute]
        public MessageDialogViewModel() { }



        public MessageDialogViewModel(string title, string content, MessageBoxButton typeButton)
        {
            Title = title;
            Content = content;

            if (typeButton == MessageBoxButton.OK) { MessageTypeOk = true; } // Konfiguracja dla przycisku "Ok".
            else if (typeButton == MessageBoxButton.YesNo) { MessageTypeYesNo = true; } // Konfiguracja dla przycisków "Yes/No".
        }

        #endregion



        #region Commands 

        // Potwierdzenie.
        private ICommand _confirmCommand;
        public ICommand ConfirmCommand { get => _confirmCommand ?? (_confirmCommand = new RelayCommand(Confirm)); }

        #endregion



        #region Properties

        // MVVM Dialogs.
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => Set(nameof(DialogResult), ref _dialogResult, value);
        }



        // Konfiguracja okna.
        public bool MessageTypeOk { get; set; }
        public bool MessageTypeYesNo { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

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
