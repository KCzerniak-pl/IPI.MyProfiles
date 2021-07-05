using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MyProfiles.Views;
using System.Windows;
using System.Windows.Input;

namespace MyProfiles.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Constructors

        public MainWindowViewModel()
        {
            // MVVM Light - Messenger.
            Messenger.Default.Register<bool>(this, (m) => { IsSelectedProfile = m; });
        }

        #endregion



        #region Commands 

        // Zamkniêcie aplikacji.
        private ICommand _closeAppCommand;
        public ICommand CloseAppCommand { get => _closeAppCommand ?? (_closeAppCommand = new RelayCommand(() => Application.Current.Shutdown())); }



        // Zminimalizowanie aplikacji.
        private ICommand _minimizeAppCommand;
        public ICommand MinimizeAppCommand { get => _minimizeAppCommand ?? (_minimizeAppCommand = new RelayCommand(() => SystemCommands.MinimizeWindow(Application.Current.MainWindow))); }

        #endregion



        #region Properties

        // Aktywny widok (zaznaczona pozycja w menu).
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; RaisePropertyChanged(); }
        }



        // Widoki.
        private object ManageProfilesView { get; set; }
        private object NetworkView { get; set; }
        private object AdditionalView { get; set; }



        // Odblokowanie menu na podstawie przes³anej wartoœci z ManagerProfilesViewModel przez MVVM Light - Messanger. 
        // Przes³anie wartoœci odbywa sie z momencie wybrania profilu.
        private bool _isSelectedProfile;
        public bool IsSelectedProfile
        {
            get { return _isSelectedProfile; }
            set
            {
                _isSelectedProfile = value;
                NetworkView = AdditionalView = null; // Po wybraniu nowego profilu nastêpuje wyczyszczenie widoków.
                RaisePropertyChanged();
            }
        }



        // Znacznik pozycji w menu.
        private Thickness _slidebarMak;
        public Thickness SlidebarMark
        {
            get { return _slidebarMak; }
            set { _slidebarMak = value; RaisePropertyChanged(); }
        }



        // Wybranie pozycji w menu.
        public int SidebarSelectedIndex
        {
            set
            {
                switch (value)
                {
                    case 1:
                        CurrentView = NetworkView ?? (NetworkView = new NetworkView() { DataContext = new NetworkViewModel() });
                        SlidebarMark = new Thickness(0, 60 * value, 0, 0);
                        break;
                    case 2:
                        CurrentView = AdditionalView ?? (AdditionalView = new AdditionalView() { DataContext = new AdditionalViewModel() });
                        SlidebarMark = new Thickness(0, 60 * value, 0, 0);
                        break;
                    default:
                        CurrentView = ManageProfilesView ?? (ManageProfilesView = new ManageProfilesView());
                        SlidebarMark = new Thickness(0, 60 * value, 0, 0);
                        break;
                }
            }
        }

        #endregion
    }
}