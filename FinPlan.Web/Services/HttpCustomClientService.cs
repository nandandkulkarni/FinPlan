namespace FinPlan.Web.Services
{
    public static class HttpCustomClientService
    {
        public const string RetryClient = "RetryClient";

        // Helper to centralize creating the named retry HttpClient
        public static HttpClient CreateRetryClient(IHttpClientFactory factory)
        {
            return factory.CreateClient(RetryClient);
        }
    }
}
