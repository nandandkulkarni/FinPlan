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
        
        // Cache the user GUID so we don't need to keep calling JS
        private string? _cachedUserGuid;

        public UserGuidService(IJSRuntime jsRuntime, ILogger<UserGuidService> logger)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        /// <summary>
        /// Gets or creates a user GUID safely. Can be called during prerendering.
        /// If JavaScript is not available, returns a temporary GUID.
        /// </summary>
        public async Task<string> GetOrCreateUserGuidAsync()
        {
            // Return cached value if available
            if (!string.IsNullOrEmpty(_cachedUserGuid))
            {
                return _cachedUserGuid;
            }

            // Check if JavaScript is available
            if (!IsJavaScriptAvailable())
            {
                // During prerendering, generate a temporary GUID
                var tempGuid = Guid.NewGuid().ToString();
                _logger.LogInformation("Generated temporary userGuid during prerendering: {UserGuid}", tempGuid);
                return tempGuid;
            }

            return await GetOrCreateUserGuidWithJSAsync();
        }

        /// <summary>
        /// Gets or creates a user GUID using JS interop.
        /// This method should only be called after the component is rendered (e.g., in OnAfterRenderAsync).
        /// </summary>
        public async Task<string> GetOrCreateUserGuidWithJSAsync()
        {
            // Return cached value if available
            if (!string.IsNullOrEmpty(_cachedUserGuid))
            {
                return _cachedUserGuid;
            }

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
                // Cache the value after successful cookie set
                _cachedUserGuid = userGuid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cookie");
                // Still cache the value even if cookie setting failed
                _cachedUserGuid = userGuid;
            }

            return userGuid;
        }

        /// <summary>
        /// Gets a temporary user GUID without JavaScript interop.
        /// Safe to call during prerendering.
        /// </summary>
        public string GetTemporaryUserGuid()
        {
            if (!string.IsNullOrEmpty(_cachedUserGuid))
            {
                return _cachedUserGuid;
            }

            var tempGuid = Guid.NewGuid().ToString();
            _logger.LogInformation("Generated temporary userGuid: {UserGuid}", tempGuid);
            return tempGuid;
        }

        /// <summary>
        /// Clears the cached user GUID. Call this if you need to refresh the value from cookies.
        /// </summary>
        public void ClearCache()
        {
            _cachedUserGuid = null;
        }

        /// <summary>
        /// Checks if JavaScript interop is available (i.e., not in prerendering mode).
        /// </summary>
        private bool IsJavaScriptAvailable()
        {
            try
            {
                // For Blazor Server, check if we're in a circuit context with JS available
                // During prerendering, IJSRuntime will be an UnsupportedJavaScriptRuntime
                var jsRuntimeType = _jsRuntime.GetType().Name;
                
                // UnsupportedJavaScriptRuntime is used during prerendering
                if (jsRuntimeType.Contains("UnsupportedJavaScriptRuntime") || 
                    jsRuntimeType.Contains("UnsupportedRemoteJSRuntime"))
                {
                    return false;
                }
                
                // If it's RemoteJSRuntime, check if we have an active circuit
                if (jsRuntimeType.Contains("RemoteJSRuntime"))
                {
                    return true; // RemoteJSRuntime usually means we have an active connection
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
