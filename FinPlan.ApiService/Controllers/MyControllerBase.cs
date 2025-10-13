using Microsoft.AspNetCore.Mvc;

namespace FinPlan.ApiService.Controllers
{
    public class MyControllerBase : ControllerBase
    {
        // Common functionality for all controllers can go here

        protected string GetClientIpAddress()
        {
            string? ipAddress = null;

            // Check for forwarded IP addresses (when behind proxy/load balancer)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // X-Forwarded-For can contain multiple IPs, take the first one
                    ipAddress = forwardedFor.Split(',')[0].Trim();
                }
            }

            // Check for X-Real-IP header
            if (string.IsNullOrEmpty(ipAddress) && Request.Headers.ContainsKey("X-Real-IP"))
            {
                ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
            }

            // Check CF-Connecting-IP (Cloudflare)
            if (string.IsNullOrEmpty(ipAddress) && Request.Headers.ContainsKey("CF-Connecting-IP"))
            {
                ipAddress = Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            }

            // Check X-Forwarded-Proto and other common headers
            if (string.IsNullOrEmpty(ipAddress) && Request.Headers.ContainsKey("X-Client-IP"))
            {
                ipAddress = Request.Headers["X-Client-IP"].FirstOrDefault();
            }

            // Fall back to remote IP address
            if (string.IsNullOrEmpty(ipAddress))
            {
                var remoteIp = HttpContext.Connection.RemoteIpAddress;
                if (remoteIp != null)
                {
                    // Handle IPv6 loopback (::1) and IPv4 loopback (127.0.0.1)
                    if (remoteIp.IsIPv4MappedToIPv6)
                    {
                        ipAddress = remoteIp.MapToIPv4().ToString();
                    }
                    else
                    {
                        ipAddress = remoteIp.ToString();
                    }

                    // Replace localhost addresses with a more meaningful value
                    if (ipAddress == "::1" || ipAddress == "127.0.0.1")
                    {
                        ipAddress = "localhost";
                    }
                }
            }

            // Validate the IP address format
            if (!string.IsNullOrEmpty(ipAddress) &&
                !ipAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                System.Net.IPAddress.TryParse(ipAddress, out var validIp))
            {
                // Additional filtering for private/local addresses if needed
                if (IsPrivateOrLocalAddress(validIp))
                {
                    return $"private-{ipAddress}";
                }
                return ipAddress;
            }

            return "Unknown";
        }

        protected bool IsPrivateOrLocalAddress(System.Net.IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var bytes = ipAddress.GetAddressBytes();

                // Check for private IPv4 ranges
                // 10.0.0.0/8
                if (bytes[0] == 10) return true;

                // 172.16.0.0/12
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;

                // 192.168.0.0/16
                if (bytes[0] == 192 && bytes[1] == 168) return true;

                // 127.0.0.0/8 (loopback)
                if (bytes[0] == 127) return true;
            }
            else if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                // Check for IPv6 loopback and private addresses
                if (ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6SiteLocal ||
                    System.Net.IPAddress.IsLoopback(ipAddress))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
