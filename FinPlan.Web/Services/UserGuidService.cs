using Microsoft.JSInterop;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FinPlan.Web.Services
{
    public class UserGuidService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<UserGuidService> _logger;
        private const string CookieName = "userGuid1";
        private const int CookieDays = 1825; // 5 years

        public UserGuidService(IJSRuntime jsRuntime, ILogger<UserGuidService> logger)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public string UserGuid
        {
            get
            {
                return GetOrCreateUserGuidAsync().GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Gets or creates a user GUID using JS interop.
        /// This method should only be called after the component is rendered (e.g., in OnAfterRenderAsync).
        /// </summary>
        public async Task<string> GetOrCreateUserGuidAsync()
        {
            string userGuid = string.Empty;

            try
            {
                userGuid = await _jsRuntime.InvokeAsync<string>("cookieHandlerExtension.GetCookie", CookieName);

                if(string.IsNullOrEmpty(userGuid))
                {
                    userGuid = Guid.NewGuid().ToString();
                    _logger.LogInformation("Generated new userGuid: {UserGuid}", userGuid);
                }
                else
                {
                    _logger.LogInformation("Retrieved userGuid from cookie: {UserGuid}", userGuid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cookie");
                // Ignore JS interop errors and use a new GUID
                userGuid = Guid.NewGuid().ToString();
            }
            try
            {
                await _jsRuntime.InvokeVoidAsync("cookieHandlerExtension.SetCookie", CookieName, userGuid, CookieDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cookie");
                // Ignore JS interop errors
            }

            return userGuid;
        }
    }
}
