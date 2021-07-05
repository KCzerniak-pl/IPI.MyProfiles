namespace MyProfiles.Models
{
    public class NetworkInterfaceModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IpDataAutomatically { get; set; }
        public string IpAddress { get; set; }
        public string SubnetMask { get; set; }
        public string Gateway { get; set; }

        public bool DnsDataAutomatically { get; set; }
        public string DnsPreffered { get; set; }
        public string DnsAlternate { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Name, Description);
        }
    }
}
