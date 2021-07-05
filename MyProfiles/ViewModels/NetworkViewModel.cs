using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmDialogs;
using MyProfiles.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace MyProfiles.ViewModels
{
    public class NetworkViewModel : ViewModelBase
    {
        #region Fields

        // MVVM Dialogs.
        private readonly IDialogService dialogService = new DialogService();

        #endregion



        #region Constructors

        public NetworkViewModel()
        {
            // Pobranie dostępnych interfejsów sieciowych.
            GetNetworkInterfaces();

            // Jeżeli w profilu są ustawienia dla interfejsu sieciowego.
            if (Common.SelectedProfile.NetworkInterface != null)
            {
                SelectedNetworkInterfaceIndex = NetworkInterfacesList.FindIndex(i => i.Id == Common.SelectedProfile.NetworkInterface.Id);
                IpDataIsChecked = true;

                // IP - statyczny.
                if (!Common.SelectedProfile.NetworkInterface.IpDataAutomatically)
                {
                    IpDataAutoIsChecked = Common.SelectedProfile.NetworkInterface.IpDataAutomatically;
                    IpAddress = Common.SelectedProfile.NetworkInterface.IpAddress;
                    SubnetMask = Common.SelectedProfile.NetworkInterface.SubnetMask;
                    Gateway = Common.SelectedProfile.NetworkInterface.Gateway;
                }

                // DNS - statyczny.
                if (!Common.SelectedProfile.NetworkInterface.DnsDataAutomatically)
                {
                    DnsDataAutoIsChecked = Common.SelectedProfile.NetworkInterface.DnsDataAutomatically;
                    DnsPreffered = Common.SelectedProfile.NetworkInterface.DnsPreffered;
                    DnsAlternate = Common.SelectedProfile.NetworkInterface.DnsAlternate;
                }
            }
        }

        #endregion



        #region Commands

        // Pobranie informacji o wybranym interfejsie sieciowym (IP, maska podsieci, brama domyślna).
        private ICommand _getIpDataCommand;
        public ICommand GetIpDataCommand { get => _getIpDataCommand ?? (_getIpDataCommand = new RelayCommand(GetIpData)); }



        // Pobranie informacji o wybranym interfejsie sieciowym (DNS).
        private ICommand _getDnsDataCommand;
        public ICommand GetDnsDataCommand { get => _getDnsDataCommand ?? (_getDnsDataCommand = new RelayCommand(GetDnsData)); }


        // Zapisanie ustawień w profilu.
        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get => _saveCommand ?? (_saveCommand = new RelayCommand(Save, () => (Common.SelectedProfile.NetworkInterface != null || IpDataIsChecked) 
                && (IpDataAutoIsChecked || (!IpDataAutoIsChecked && !string.IsNullOrEmpty(IpAddress) && !string.IsNullOrEmpty(SubnetMask)))));
        }

        #endregion



        #region Properties

        // Lista przechowująca dostępne interfejsy sieciowe.
        public List<NetworkInterfaceModel> NetworkInterfacesList { get; set; }



        // Wybrany interfejs sieciowy.
        private NetworkInterfaceModel _selectedNetworkInterface;
        public NetworkInterfaceModel SelectedNetworkInterface
        {
            get { return _selectedNetworkInterface; }
            set { _selectedNetworkInterface = value; RaisePropertyChanged(); }
        }



        // Jeżeli w pliku z profilem są ustawienia dla intefrejsu sieciowego, to właściwość przechowuje jego indeks w kontrolce ComboBox.
        // W przeciwnym wypadku nie jest wybrany żaden intefrejs sieciowy.
        public int SelectedNetworkInterfaceIndex { get; set; } = -1;



        // Właściwość powiązana z kontrolką CheckBox - dla danych IP.
        private bool _ipDataIsChecked;
        public bool IpDataIsChecked
        {
            get { return _ipDataIsChecked; }
            set { _ipDataIsChecked = value; RaisePropertyChanged(); }
        }



        // Właściwość powiązana z kontrolką RadioButton - dla danych IP.
        private bool _ipDataAutoIsChecked = true;
        public bool IpDataAutoIsChecked
        {
            get { return _ipDataAutoIsChecked; }
            set
            {
                _ipDataAutoIsChecked = value;

                if (!value) { DnsDataAutoIsChecked = false; } // Jeżeli false - czyli adresacja IP jest statyczna - adrsacja dla serwerów DNS jest również statyczna.
                RaisePropertyChanged();
            }
        }



        // Przechowuje adres IP wybranego interfejsu sieciowego.
        private string _ipAddress;
        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; RaisePropertyChanged(); }
        }



        // Przechowuje maskę podsieci wybranego interfejsu sieciowego.
        private string _subnetMask;
        public string SubnetMask
        {
            get { return _subnetMask; }
            set { _subnetMask = value; RaisePropertyChanged(); }
        }



        // Przechowuje adres bramy domyślnej wybranego interfejsu sieciowego.
        private string _gateway;
        public string Gateway
        {
            get { return _gateway; }
            set { _gateway = value; RaisePropertyChanged(); }
        }



        // Właściwość powiązana z kontrolką RadioButton - dla serwerów DNS.
        private bool _dnsDataAutoIsChecked = true;
        public bool DnsDataAutoIsChecked
        {
            get { return _dnsDataAutoIsChecked; }
            set { _dnsDataAutoIsChecked = value; RaisePropertyChanged(); }
        }



        // Przechowuje adres preferowanego serwera DNS.
        private string _dnsPreffered;
        public string DnsPreffered
        {
            get { return _dnsPreffered; }
            set { _dnsPreffered = value; RaisePropertyChanged(); }
        }



        // Przechowuje adres alternatywnego serwera DNS.
        private string _dnsAlternate;
        public string DnsAlternate
        {
            get { return _dnsAlternate; }
            set { _dnsAlternate = value; RaisePropertyChanged(); }
        }



        // Aktualny stan komunikatu z potwierdzeniem zapisania zmian.
        private bool _saveConfirmed;
        public bool SaveConfirmed
        {
            get { return _saveConfirmed; }
            set { _saveConfirmed = value; RaisePropertyChanged(); }
        }

        #endregion



        #region Methods

        // Dodanie do listy dostepnych interfejsów sieciowych.
        public void GetNetworkInterfaces()
        {
            // Utworzenie obiektu listy.
            NetworkInterfacesList = new List<NetworkInterfaceModel>();

            // Metoda GetAllNetworkInterfaces() zwraca tablicę NetworkInterface[] z obiektami opisującymi interfejsy sieciowe w komputerze.
            // Tablica może być pusta, jeżeli nie wykryto żadnych interfejsów.
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Warunek sprwadzający stan i typ interfejsu. 
                // OperationalStatus.Up -  interfejs sieciowy jest w pełni skonfigurowany i może przesyłać pakiety danych.
                // Wireless80211 - interfejs sieciowy używa połączenia bezprzewodowej sieci LAN.
                // Ethernet - interfejs sieciowy używa połączenia Ethernet.
                if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    NetworkInterfacesList.Add(new NetworkInterfaceModel() { Id = nic.Id, Name = nic.Name, Description = nic.Description });
                }
            }
        }



        // Pobranie informacji o wybranym interfejsie sieciowym (IP, maska podsieci, brama domyślna).
        private void GetIpData()
        {
            // Wyczyszczenie danych.
            IpAddress = SubnetMask = Gateway = null;

            // Metoda GetAllNetworkInterfaces() zwraca tablicę NetworkInterface[] z obiektami opisującymi interfejsy sieciowe na komputerze lokalnym.
            // Tablica może być pusta, jeżeli nie wykryto żadnych interfejsów.
            // Użyto LINQ (zamiast dodatkowego warunku IF wewnątrz pętli) do sprawdzenia czy znaleziono wybrany - w ComboBox - interfejs sieciowy.
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces().Where(i => i.Id == SelectedNetworkInterface.Id))
            {
                // Metoda GetIPProperties() zwraca obiekt IPInterfaceProperties, który opisuje konfigurację danego interfejsu sieciowego.
                // Właściwość UnicastAddresses pobiera informacje na temat adresu IP danego interfejsu sieciowego.
                // Użyto LINQ (Where zamiast dodatkowego warunku if wewnątrz pętli) do sprawdzenia czy schemat adresowania jest zgodny z IPv4.
                foreach (UnicastIPAddressInformation nicData in nic.GetIPProperties().UnicastAddresses.Where(i => i.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                {
                    IpAddress = nicData.Address.ToString();
                    SubnetMask = nicData.IPv4Mask.ToString();
                }

                // Metoda GetIPProperties() zwraca obiekt IPInterfaceProperties, który opisuje konfigurację danego interfejsu sieciowego.
                // Właściwość GatewayAddresses pobiera adresy bramy sieci IPv4 dla danego interfejsu.
                // Pętla wykonuje się tylko dla pierwszego elementu.
                foreach (GatewayIPAddressInformation nicGateway in nic.GetIPProperties().GatewayAddresses.Take(1))
                {
                    Gateway = nicGateway.Address.ToString();
                }
            }
        }



        // Pobranie informacji o wybranym interfejsie sieciowym (DNS).
        private void GetDnsData()
        {
            // Wyczyszczenie danych.
            DnsPreffered = DnsAlternate = null;

            // Metoda GetAllNetworkInterfaces() zwraca tablicę NetworkInterface[] z obiektami opisującymi interfejsy sieciowe na komputerze lokalnym.
            // Tablica może być pusta, jeżeli nie wykryto żadnych interfejsów.
            // Użyto LINQ (zamiast dodatkowego warunku if wewnątrz pętli) do sprawdzenia czy znaleziono wybrany - w ComboBox - interfejs sieciowy.
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces().Where(i => i.Id == SelectedNetworkInterface.Id))
            {
                // Metoda GetIPProperties() zwraca obiekt IPInterfaceProperties, który opisuje konfigurację danego interfejsu sieciowego.
                // Właściwość DnsAddresses pobiera informacje na temat adresów DNS danego interfejsu sieciowego.
                // Pętla wykonuje się tylko dla dwóch pierwszych elementów - prefenerowanego i alternatywnego adresu DNS.

                // Od C# 7.0 LINQ zapewnia wbudowaną obsługę krotek. Poniżej - w pętli foreach - użytko tego mechanizmu, żeby odwoływać się do kolejndego indeksu pętli.
                // Użyto LINQ (Where zamiast dodatkowego warunku if wewnątrz pętli) do sprawdzenia czy schemat adresowania jest zgodny z IPv4.
                foreach ((IPAddress nicData, Int32 index) in nic.GetIPProperties().DnsAddresses.Where(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select((nicData, index) => (nicData, index)).Take(2))
                {
                    if (index == 0) { DnsPreffered = nicData.ToString(); }
                    else if (index == 1) { DnsAlternate = nicData.ToString(); }
                }
            }
        }



        // Zapisanie ustawień w profilu.
        private void Save()
        {
            // Aktualizacja ustawień IP dla wybranego interfejsu sieciowego.
            if (IpDataIsChecked)
            {
                // IP - dynamiczny lub statyczny.
                if (IpDataAutoIsChecked)
                {
                    SelectedNetworkInterface.IpDataAutomatically = true;
                    SelectedNetworkInterface.IpAddress = SelectedNetworkInterface.SubnetMask = SelectedNetworkInterface.Gateway = null;
                }
                else
                {
                    SelectedNetworkInterface.IpDataAutomatically = false;
                    SelectedNetworkInterface.IpAddress = IpAddress;
                    SelectedNetworkInterface.SubnetMask = SubnetMask;
                    SelectedNetworkInterface.Gateway = string.IsNullOrEmpty(Gateway) ? null : Gateway;
                }

                // DNS - dynamiczny lub statyczny.
                if (DnsDataAutoIsChecked)
                {
                    SelectedNetworkInterface.DnsDataAutomatically = true;
                    SelectedNetworkInterface.DnsPreffered = SelectedNetworkInterface.DnsAlternate = null;
                }
                else
                {
                    SelectedNetworkInterface.DnsDataAutomatically = false;
                    SelectedNetworkInterface.DnsPreffered = string.IsNullOrEmpty(DnsPreffered) ? null : DnsPreffered;
                    SelectedNetworkInterface.DnsAlternate = string.IsNullOrEmpty(DnsAlternate) ? null : DnsAlternate;
                }

                // Aktualizacja ustawień TCP/IP dla wybranego profilu.
                Common.SelectedProfile.NetworkInterface = SelectedNetworkInterface;
            }
            else
            {
                Common.SelectedProfile.NetworkInterface = null;
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
