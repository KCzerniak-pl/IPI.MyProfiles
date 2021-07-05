using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using MvvmDialogs;
using MyProfiles.ViewModels.Dialogs;

namespace MyProfiles.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Reset();

            SimpleIoc.Default.Register<IDialogService>(() => new DialogService());
            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<ManageProfilesViewModel>();
            SimpleIoc.Default.Register<NetworkViewModel>();
            SimpleIoc.Default.Register<AdditionalViewModel>();
            SimpleIoc.Default.Register<MessageDialogViewModel>();
            SimpleIoc.Default.Register<InputDialogViewModel>();
            SimpleIoc.Default.Register<ProfilePreviewViewModel>();
        }

        public MainWindowViewModel MainWindow => ServiceLocator.Current.GetInstance<MainWindowViewModel>();
        public ManageProfilesViewModel ManageProfiles => ServiceLocator.Current.GetInstance<ManageProfilesViewModel>();
        public NetworkViewModel Network => ServiceLocator.Current.GetInstance<NetworkViewModel>();
        public AdditionalViewModel Additional => ServiceLocator.Current.GetInstance<AdditionalViewModel>();
        public MessageDialogViewModel MessageDialog => ServiceLocator.Current.GetInstance<MessageDialogViewModel>();
        public InputDialogViewModel InputDialog => ServiceLocator.Current.GetInstance<InputDialogViewModel>();
        public ProfilePreviewViewModel ProfilePreview => ServiceLocator.Current.GetInstance<ProfilePreviewViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}