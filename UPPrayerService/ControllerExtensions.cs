using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace UPPrayerService
{
    public static class ControllerExtensions
    {
        public static IActionResult MakeSuccess(this ControllerBase _this, object data = null, int statusCode = StatusCodes.Status200OK)
        {
            JsonResult result = new JsonResult(new { success = true, data = data });
            result.StatusCode = statusCode;
            return result;
        }

        public static IActionResult MakeFailure(this ControllerBase _this, string message, int statusCode)
        {
            JsonResult result = new JsonResult(new { success = false, message = message });
            result.StatusCode = statusCode;
            return result;
        }

        public static bool SlotIsValid(this ControllerBase _this, int year, int monthIndex, int dayIndex, int slotIndex)
        {
            return year >= 0 && year < 10000 && monthIndex >= 0 && monthIndex < 12 && dayIndex >= 0 && dayIndex < DateTime.DaysInMonth(year, monthIndex + 1) && slotIndex >= 0 && slotIndex < 48;
        }
    }
}
