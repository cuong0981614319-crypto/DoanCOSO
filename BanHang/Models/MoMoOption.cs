namespace BanHang.Models
{
    public class MoMoOption
    {
        public string PartnerCode { get; set; } = "";
        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public string CreateEndpoint { get; set; } = "";
        public string QueryEndpoint { get; set; } = "";
        public string ReturnUrl { get; set; } = "";
        public string IpnUrl { get; set; } = "";
    }
}