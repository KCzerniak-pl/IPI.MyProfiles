using System;

namespace MyProfiles.Models
{
    public class ProfileModel
    {
        public string GuidString { get; set; }
        public string Name { get; set; }

        public NetworkInterfaceModel NetworkInterface { get; set; }

        public AdditionalModel Additional { get; set; }

        public ProfileModel()
        {
            GuidString = Guid.NewGuid().ToString();
        }
    }
}
