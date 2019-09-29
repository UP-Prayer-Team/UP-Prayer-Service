using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace UPPrayerService
{
    public static class ControllerExtensions
    {
        public static IActionResult MakeSuccess(this Controller _this, object data = null, int statusCode = StatusCodes.Status200OK)
        {
            JsonResult result = new JsonResult(new { success = true, data = data });
            result.StatusCode = statusCode;
            return result;
        }

        public static IActionResult MakeFailure(this Controller _this, string message, int statusCode)
        {
            JsonResult result = new JsonResult(new { success = false, message = message });
            result.StatusCode = statusCode;
            return result;
        }
    }
}
