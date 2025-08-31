namespace FinPlan.Web.Components.Custom
{
    public static class HttpCustomClientProvider
    {
        public const string RetryClient = "RetryClient";

        // Helper to centralize creating the named retry HttpClient
        public static HttpClient CreateRetryClient(IHttpClientFactory factory)
        {
            return factory.CreateClient(RetryClient);
        }
    }
}
