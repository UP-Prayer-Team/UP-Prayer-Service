using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace UPPrayerService.Controllers
{
    [Route("api/lets-encrypt")]
    [ApiController()]
    public class LetsEncryptController : ControllerBase
    {
        // POST api/lets-encrypt/secret
        [HttpPost("secret")]
        public void Secret([FromBody]string secret)
        {
            Services.LetsEncryptService.SetSecret(secret);
        }
    }
}
