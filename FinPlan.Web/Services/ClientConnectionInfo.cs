namespace FinPlan.Web.Services
{
    // Circuit-scoped container for details about the real browser client
    public class ClientConnectionInfo
    {
        public string? RemoteIp { get; set; }
        public string? XForwardedFor { get; set; }
        public string? XRealIp { get; set; }
        public string? XForwardedProto { get; set; }
        public string? XForwardedHost { get; set; }
    }
}
