using GalaSoft.MvvmLight;
using MvvmDialogs;
using System.IO;

namespace MyProfiles.ViewModels
{
    public class ProfilePreviewViewModel : ViewModelBase, IModalDialogViewModel
    {
        #region Constructors

        public ProfilePreviewViewModel()
        {
            PathFile = Common.PathFileProfile(Common.SelectedProfile.GuidString);

            // Pobranie wybranego profilu.
            GetProfile(PathFile);
        }

        #endregion



        #region Properties

        // MVVM Dialogs.
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => Set(nameof(DialogResult), ref _dialogResult, value);
        }



        // Ścieżka do pliku json z wybranym profilem.
        public string PathFile { get; set; }



        // Zawartość pliku json z wybranym profilem.
        public string ProfilePreview { get; set; }

        #endregion



        #region Methods

        // Pobranie zawartości pliku json z wybranym profilem.
        private void GetProfile(string pathFile)
        {
            ProfilePreview = File.ReadAllText(pathFile);
        }

        #endregion
    }
}
