using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace FinPlan.Web.Services
{
    public class UserGuidService
    {

        public string UserGuid
        {
            get
            {
                return GetOrCreateUserGuidAsync().GetAwaiter().GetResult();
            }
        }


        private readonly IJSRuntime _jsRuntime;
        private const string CookieName = "userGuid1";
        private const int CookieDays = 1825; // 5 years
        public UserGuidService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
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
                }   
            }
            catch
            {
                Console.WriteLine("Error retrieving cookie");
                // Ignore JS interop errors and use a new GUID
            }
            try
            {
                await _jsRuntime.InvokeVoidAsync("cookieHandlerExtension.SetCookie", CookieName, userGuid, CookieDays);
            }
            catch
            {
                Console.WriteLine("Error setting cookie");
                // Ignore JS interop errors
            }
            //var userGuid = await _jsRuntime.InvokeAsync<string>("cookieHandlerExtension.GetCookie", CookieName);
            //if (string.IsNullOrEmpty(userGuid))
            //{
            //    userGuid = Guid.NewGuid().ToString();
            //    await _jsRuntime.InvokeVoidAsync("cookieHandlerExtension.SetCookie", CookieName, userGuid, CookieDays);
            //}
            return userGuid;
        }
    }
}
