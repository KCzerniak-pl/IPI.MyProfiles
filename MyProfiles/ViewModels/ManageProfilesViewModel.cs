using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MvvmDialogs;
using MyProfiles.Models;

namespace MyProfiles.ViewModels
{
    public class ManageProfilesViewModel : ViewModelBase
    {
        #region Fields

        // MVVM Dialogs.
        private readonly IDialogService dialogService = new DialogService();

        #endregion



        #region Constructors

        public ManageProfilesViewModel()
        {
            // Pobranie dostępnych profili.
            GetProfiles();
        }

        #endregion



        #region Commnads

        // Dodanie nowego profliu.
        private ICommand _newProfileCommnad;
        public ICommand NewProfileCommnad { get => _newProfileCommnad ?? (_newProfileCommnad = new RelayCommand(NewProfile)); }


        // Zmiana  nazwy profilu.
        private ICommand _renameProfileCommnad;
        public ICommand RenameProfileCommnad { get => _renameProfileCommnad ?? (_renameProfileCommnad = new RelayCommand(RenameProfile, () => Common.SelectedProfile != null)); }



        // Usunięcie profilu.
        private ICommand _removeProfileCommnad;
        public ICommand RemoveProfileCommnad { get => _removeProfileCommnad ?? (_removeProfileCommnad = new RelayCommand(RemoveProfile, () => Common.SelectedProfile != null)); }



        // Aktywacja profilu.
        private ICommand _activateProfileCommnad;
        public ICommand ActivateProfileCommnad { get => _activateProfileCommnad ?? (_activateProfileCommnad = new RelayCommand(ActivateProfile, () => Common.SelectedProfile != null)); }



        // Podgląd zawartości pliku z profilem.
        private ICommand _profilePreviewCommand;
        public ICommand ProfilePreviewCommand { get => _profilePreviewCommand ?? (_profilePreviewCommand = new RelayCommand(ProfilePreview, () => Common.SelectedProfile != null)); }

        #endregion



        #region Properties

        // Lista przechowująca dostępne profile.
        private List<ProfileModel> _profilesList;
        public List<ProfileModel> ProfilesList
        {
            get { return _profilesList; }
            set { _profilesList = value; RaisePropertyChanged(); }
        }


        // Przypisanie wybranego profilu do statycznej właściwości.
        public ProfileModel SelectedProfile
        {
            set
            {
                Common.SelectedProfile = value;
                Messenger.Default.Send<bool, MainWindowViewModel>(value != null ? true : false); // MVVM Light - Messenger. Odblokowanie menu.
            }
        }

        #endregion



        #region Methods

        // Dodanie do kolekcji dostępnych profili.
        public void GetProfiles()
        {
            ProfilesList = new List<ProfileModel>();

            try
            {
                // Sprawdzenie czy istnieje folder z profilami.
                if (Directory.Exists(Common.PathDirProfiles))
                {
                    // Pobranie wszystkich plików json z profilami.
                    IEnumerable<string> allFiles = Directory.EnumerateFiles(Common.PathDirProfiles, "*.json", SearchOption.TopDirectoryOnly);

                    // Przejście po wszystkich plikach z profilami.
                    foreach (string currentFile in allFiles)
                    {
                        // Deserializacja - odczytanie danych json jako obiekty.
                        string jsonString = File.ReadAllText(currentFile);
                        ProfileModel currentProfile = JsonSerializer.Deserialize<ProfileModel>(jsonString);

                        //Dodanie profilu do listy.
                        ProfilesList.Add(currentProfile);
                    }

                    // Posortowanie listy z profilami po nazwie.
                    ProfilesList = ProfilesList.OrderBy(p => p.Name).ToList();
                }
            }
            catch
            {
                // MVVM Dialogs. Komunikat z błędem.
                _ = dialogService.ShowDialog<Views.Dialogs.MessageDialogView>(this, new Dialogs.MessageDialogViewModel("Error", "Cannot read profile files.", MessageBoxButton.OK));
            }
        }



        // Dodanie nowego profliu.
        private void NewProfile()
        {
            // MVVM Dialogs. Okno do podania nazwy nowego profliu.
            Dialogs.InputDialogViewModel dialogViewModel = new Dialogs.InputDialogViewModel("New Profile", "Name of the new profile (max. length 50):");
            bool? result = dialogService.ShowDialog<Views.Dialogs.InputDialogView>(this, dialogViewModel);

            if (result == true)
            {
                // Utworzenie instancji profilu.
                ProfileModel profile = new ProfileModel() { Name = dialogViewModel.Text };

                try
                {
                    // Jeżeli folder z profilami nie istnieje, to zostanie utworzony.
                    Directory.CreateDirectory(Common.PathDirProfiles);

                    // Ścieżka do zapisu pliku json (w lokalizacji, w której działa program).
                    string pathFile = Common.PathFileProfile(profile.GuidString);

                    // Serializacja klasy do pliku json.
                    string jsonString = JsonSerializer.Serialize(profile, new JsonSerializerOptions() { WriteIndented = true });
                    File.WriteAllText(pathFile, jsonString);

                    // Pobranie aktualnej listy profili.
                    GetProfiles();
                }
                catch
                {
                    // MVVM Dialogs. Komunikat z błędem.
                    _ = dialogService.ShowDialog<Views.Dialogs.MessageDialogView>(this, new Dialogs.MessageDialogViewModel("Error", "Cannot write profile file", MessageBoxButton.OK));
                }
            }
        }


        // Zmiana nazwy profilu.
        private void RenameProfile()
        {
            // MVVM Dialogs. Okno do podania nowej nazwy profilu.
            Dialogs.InputDialogViewModel dialogViewModel = new Dialogs.InputDialogViewModel("Rename Profile", "Rename of the profile (max. length 50):", Common.SelectedProfile.Name);
            bool? result = dialogService.ShowDialog<Views.Dialogs.InputDialogView>(this, dialogViewModel);

            if (result == true)
            {
                // Aktualizacja nazwy profilu.
                Common.SelectedProfile.Name = dialogViewModel.Text;

                try
                {
                    // Jeżeli folder z profilami nie istnieje, to zostanie utworzony.
                    Directory.CreateDirectory(Common.PathDirProfiles);

                    // Ścieżka do zapisu pliku json (w lokalizacji, w której działa program).
                    string pathFile = Common.PathFileProfile(Common.SelectedProfile.GuidString);

                    // Serializacja klasy do pliku json.
                    string jsonString = JsonSerializer.Serialize(Common.SelectedProfile, new JsonSerializerOptions() { WriteIndented = true });
                    File.WriteAllText(pathFile, jsonString);
                }
                catch
                {
                    // MVVM Dialogs. Komunikat z błędem.
                    _ = dialogService.ShowDialog<Views.Dialogs.MessageDialogView>(this, new Dialogs.MessageDialogViewModel("Error", "Cannot write profile file.", MessageBoxButton.OK));
                }

                // Pobranie aktualnej listy profili.
                GetProfiles();
            }
        }



        // Usunięcie profliu.
        private void RemoveProfile()
        {
            // MVVM Dialogs. Okno z potwierdzeniem usunięcia profilu.
            Dialogs.MessageDialogViewModel dialogViewModel = new Dialogs.MessageDialogViewModel("Notice", "Do you want to permanently remove selected profile?", MessageBoxButton.YesNo);
            bool? result = dialogService.ShowDialog<Views.Dialogs.MessageDialogView>(this, dialogViewModel);

            if (result == true)
            {
                try
                {
                    // Ścieżka do pliku z profilem.
                    string selectedProfileFile = Common.PathFileProfile(Common.SelectedProfile.GuidString);

                    // Sprawdzenie czy istnieje plik z profilem.
                    if (File.Exists(selectedProfileFile))
                    {
                        // Usunięcie pliku z profilem.
                        File.Delete(selectedProfileFile);

                        // Pobranie aktualnej listy profili.
                        GetProfiles();
                    }
                    else
                    {
                        // MVVM Dialogs. Komunikat z błędem.
                        _ = dialogService.ShowDialog<Views.Dialogs.MessageDialogView>(this, new Dialogs.MessageDialogViewModel("Error", "Cannot found profile file.", MessageBoxButton.OK));
                    }
                }
                catch
                {
                    // MVVM Dialogs. Komunikat z błędem.
                    _ = dialogService.ShowDialog<Views.Dialogs.MessageDialogView>(this, new Dialogs.MessageDialogViewModel("Error", "Cannot remove profile file.", MessageBoxButton.OK));
                }
            }
        }



        // Aktywacja profilu.
        private void ActivateProfile()
        {
            // polecenia do wykonania w cmd.
            StringBuilder command = new StringBuilder("/c ");

            // Jeżeli zostały zapisane ustawienia TCP/IP dla wybranego interfejsu sieciowego.
            if (Common.SelectedProfile.NetworkInterface != null)
            {
                // IP automatyczne i DNS automatyczne.
                if (Common.SelectedProfile.NetworkInterface.IpDataAutomatically && Common.SelectedProfile.NetworkInterface.DnsDataAutomatically)
                {
                    command.AppendFormat("netsh interface ipv4 set address name=\"{0}\" dhcp &", Common.SelectedProfile.NetworkInterface.Name);
                    command.AppendFormat("netsh interface ipv4 set dns name=\"{0}\" dhcp &", Common.SelectedProfile.NetworkInterface.Name);
                }
                // IP automatyczne i DNS statyczne.
                else if (Common.SelectedProfile.NetworkInterface.IpDataAutomatically && !Common.SelectedProfile.NetworkInterface.DnsDataAutomatically)
                {
                    command.AppendFormat("netsh interface ipv4 set address name=\"{0}\" dhcp &", Common.SelectedProfile.NetworkInterface.Name);
                    command.AppendFormat("netsh interface ipv4 set dns name=\"{0}\" static {1} &", Common.SelectedProfile.NetworkInterface.Name, Common.SelectedProfile.NetworkInterface.DnsPreffered);
                    command.AppendFormat("netsh interface ipv4 add dns name=\"{0}\" {1} index=2 &", Common.SelectedProfile.NetworkInterface.Name, Common.SelectedProfile.NetworkInterface.DnsAlternate);
                }
                // IP statyczne i DNS statyczne.
                else
                {
                    command.AppendFormat("netsh interface ipv4 set address name=\"{0}\" static {1} {2} {3} &", Common.SelectedProfile.NetworkInterface.Name, Common.SelectedProfile.NetworkInterface.IpAddress, Common.SelectedProfile.NetworkInterface.SubnetMask, Common.SelectedProfile.NetworkInterface.Gateway);
                    command.AppendFormat("netsh interface ipv4 set dns name=\"{0}\" static {1} &", Common.SelectedProfile.NetworkInterface.Name, Common.SelectedProfile.NetworkInterface.DnsPreffered);
                    command.AppendFormat("netsh interface ipv4 add dns name=\"{0}\" {1} index=2 &", Common.SelectedProfile.NetworkInterface.Name, Common.SelectedProfile.NetworkInterface.DnsAlternate);
                }
            }

            // Jeżeli zostały zapisane dodatkowe ustawienia.
            if (Common.SelectedProfile.Additional != null)
            {
                // Ustawienie domyślnej drukarki.
                if (!string.IsNullOrEmpty(Common.SelectedProfile.Additional.DefaultPrinter))
                {
                    command.AppendFormat("wmic printer where name=\"{0}\" call setdefaultprinter &", Common.SelectedProfile.Additional.DefaultPrinter);
                }

                // Zmiana nazwy komputera.
                if (!string.IsNullOrEmpty(Common.SelectedProfile.Additional.ComputerName))
                {
                    command.AppendFormat("wmic computersystem where name=\"{0}\" call rename name=\"{1}\" &", Environment.MachineName, Common.SelectedProfile.Additional.ComputerName);
                }
            }

            // Wykonanie poleceń.
            if (Common.SelectedProfile.NetworkInterface != null || Common.SelectedProfile.Additional != null)
            {
                // MVVM Dialogs. Okno z informacją o konieczności zezwolenia na wprowadzenie zmian na urządzeniu.
                Dialogs.MessageDialogViewModel dialogViewModel = new Dialogs.MessageDialogViewModel("Notice", "In order to activate settings, in the next prompt you must agree to allow app to make changes to your device.", MessageBoxButton.OK);
                _ = dialogService.ShowDialog<Views.Dialogs.MessageDialogView>(this, dialogViewModel);

                // Wykonanie wszystkich poleceń w cmd.
                CommandLineRun(command.ToString());

                // Jeżeli została wykonana zmiana nazwy komputera - pytanie o restart komputera.
                if (Common.SelectedProfile.Additional != null && !string.IsNullOrEmpty(Common.SelectedProfile.Additional.ComputerName))
                {
                    dialogViewModel = new Dialogs.MessageDialogViewModel("Notice", "You must restart your computer to apply new name this computer. Restart now?", MessageBoxButton.YesNo);
                    bool? result = dialogService.ShowDialog<Views.Dialogs.MessageDialogView>(this, dialogViewModel);

                    // Restart komputera.
                    if (result == true) { CommandLineRun("/c shutdown /r /t 0", false); }
                }
            }
        }



        // Wykonanie poleceń w cmd.
        private void CommandLineRun(string command, bool asAdmin = true)
        {
            try
            {
                using (Process myProcess = new Process())
                {
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = @"cmd.exe";
                    myProcess.StartInfo.Arguments = command;
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    if (asAdmin) { myProcess.StartInfo.Verb = @"runas"; }
                    myProcess.Start();
                }
            }
            catch
            {
                // MVVM Dialogs. Komunikat z błędem.
                _ = dialogService.ShowDialog<MyProfiles.Views.Dialogs.MessageDialogView>(this, new MyProfiles.ViewModels.Dialogs.MessageDialogViewModel("Error", "Cannot run commands.", MessageBoxButton.OK));
            }
        }



        // Podgląd zawartości pliku z profilem.
        private void ProfilePreview()
        {
            // MVVM Dialogs. Okno z podglądem zawartości pliku z profilem.
            var dialogViewModel = new ProfilePreviewViewModel();
            _ = dialogService.ShowDialog<Views.ProfilePreviewView>(this, dialogViewModel);
        }

        #endregion
    }
}