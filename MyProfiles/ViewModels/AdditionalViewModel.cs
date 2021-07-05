using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;
using MyProfiles.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace MyProfiles.ViewModels
{
    public class AdditionalViewModel : ViewModelBase
    {
        #region Fields

        // MVVM Dialogs.
        private readonly IDialogService dialogService = new DialogService();

        #endregion



        #region Constructors

        public AdditionalViewModel()
        {
            GetInstalledPrinters();

            // Jeżeli w profilu są dodatkowe ustawienia.
            if (Common.SelectedProfile.Additional != null)
            {
                // Domyślna drukarka.
                if (!string.IsNullOrEmpty(Common.SelectedProfile.Additional.DefaultPrinter))
                {
                    DefaultPrinterIsChecked = true;
                    SelectedDefaultPrinter = Common.SelectedProfile.Additional.DefaultPrinter;
                }

                // Nazwa komputera.
                if (!string.IsNullOrEmpty(Common.SelectedProfile.Additional.ComputerName))
                {
                    ComputerNameIsChecked = true;
                    ComputerName = Common.SelectedProfile.Additional.ComputerName;
                }
            }
        }

        #endregion



        #region Commands

        // Pobranie informacji o nazwie komputera.
        private ICommand _getComputerNameCommand;
        public ICommand GetComputerNameCommand { get => _getComputerNameCommand ?? (_getComputerNameCommand = new RelayCommand(GetComputerName)); }



        // Zapisanie ustawień w profilu.
        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get => _saveCommand ?? (_saveCommand = new RelayCommand(Save, () => (Common.SelectedProfile.Additional != null || DefaultPrinterIsChecked || ComputerNameIsChecked)
                && (!DefaultPrinterIsChecked || (DefaultPrinterIsChecked && SelectedDefaultPrinter != null)) && (!ComputerNameIsChecked || (ComputerNameIsChecked && !string.IsNullOrEmpty(ComputerName)))));
        }

        #endregion



        #region Properties

        // Lista przechowująca zainstalowane drukarki.
        public List<string> InstalledPrintersList { get; set; }



        // Właściwość powiązana z kontrolką CheckBox - dla domyślnej drukarki.
        private bool _defaultPrinterIsChecked;
        public bool DefaultPrinterIsChecked
        {
            get { return _defaultPrinterIsChecked; }
            set { _defaultPrinterIsChecked = value; RaisePropertyChanged(); }
        }

        // Drukarka wybrana jako domyślna.
        public string SelectedDefaultPrinter { get; set; }



        // Właściwość powiązana z kontrolką CheckBox - dla nazwy komputera.
        private bool _computerNameIsChecked;
        public bool ComputerNameIsChecked
        {
            get { return _computerNameIsChecked; }
            set { _computerNameIsChecked = value; RaisePropertyChanged(); }
        }

        // Nazwa komputera;
        private string _computerName;
        public string ComputerName
        {
            get { return _computerName; }
            set { _computerName = value; RaisePropertyChanged(); }
        }

        #endregion



        #region Methods

        // Pobranie zainstalowanych drukarek.
        private void GetInstalledPrinters()
        {
            InstalledPrintersList = new List<string>();

            // Dodanie do listy zainstalowanych drukarek.
            for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
            {
                InstalledPrintersList.Add(PrinterSettings.InstalledPrinters[i]);
            }
        }



        // Pobranie informacji o nazwie komputera.
        private void GetComputerName()
        {
            ComputerName = Environment.MachineName;
        }



        // Zapisanie ustawień w profilu.
        private void Save()
        {
            if (DefaultPrinterIsChecked || ComputerNameIsChecked)
            {
                AdditionalModel Additional = new AdditionalModel();

                // Aktualizacja ustawień
                if (DefaultPrinterIsChecked && SelectedDefaultPrinter != null) { Additional.DefaultPrinter = SelectedDefaultPrinter; }
                if (ComputerNameIsChecked && !string.IsNullOrEmpty(ComputerName)) { Additional.ComputerName = ComputerName; }

                // Aktualizacja ustawień dla wybranego profilu.
                Common.SelectedProfile.Additional = Additional;
            }
            else
            {
                Common.SelectedProfile.Additional = null;
            }

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
        }

        #endregion
    }
}
