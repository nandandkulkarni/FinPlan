using Microsoft.Extensions.Configuration;

namespace FinPlan.Web.Services
{
    public class ApiUrlProvider
    {
        private readonly IConfiguration _configuration;

        public ApiUrlProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetApiBaseUrl()
        {
#if DEBUG
            return _configuration["FinPlanSettings:ApiBaseUrlLocal"];
#else
            return _configuration["FinPlanSettings:ApiBaseUrlCloud"];
#endif
        }
    }
}