using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace UPPrayerService.Services
{
    public static class LetsEncryptService
    {
        private static string ResponseSecret = "";

        public static void SetSecret(string secret)
        {
            ResponseSecret = secret;
        }

        public static async Task HandleRequest(HttpContext context)
        {
            string requestID = context.GetRouteValue("id") as string;
            await context.Response.WriteAsync(ResponseSecret);
        }
    }
}
