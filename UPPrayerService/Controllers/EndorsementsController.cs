using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using UPPrayerService.Models;
using UPPrayerService.Services;

namespace UPPrayerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController()]
    public class EndorsementsController : ControllerBase
    {
        private EndorsementService EndorsementService { get; }

        public EndorsementsController(EndorsementService endorsementService)
        {
            this.EndorsementService = endorsementService;
        }

        public class EndorsementListViewModel
        {
            public int CurrentIndex { get; set; }
            public Endorsement[] Endorsements { get; set; }

            public EndorsementListViewModel()
            {

            }
        }

        // GET: api/endorsements/list
        [HttpGet("list")]
        public IActionResult List()
        {
            EndorsementListViewModel result = new EndorsementListViewModel();
            result.CurrentIndex = EndorsementService.GetCurrentIndex();
            result.Endorsements = EndorsementService.GetEndorsements().ToArray();
            return this.MakeSuccess(result);
        }

        // GET: api/endorsements/current
        [HttpGet("current")]
        public IActionResult Current()
        {
            Endorsement[] endorsements = EndorsementService.GetEndorsements().ToArray();
            if (endorsements.Length == 0)
            {
                return this.MakeFailure("No endorsements available.", 404);
            }
            else
            {
                Endorsement currentEndorsement = endorsements[EndorsementService.GetCurrentIndex()];
                return this.MakeSuccess(currentEndorsement);
            }
        }

        // POST: api/endorsements/update
        [HttpPost("update")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(EndorsementListViewModel request)
        {
            if (request.Endorsements.Length <= request.CurrentIndex)
            {
                if (request.Endorsements.Length > 0)
                {
                    // Cannot set CurrentIndex to a non-existant Endorsement.
                    return this.MakeFailure("Cannot set CurrentIndex to a non-existant Endorsement!", StatusCodes.Status400BadRequest);
                }
                else if (request.CurrentIndex != 0)
                {
                    // If there are 0 Endorsements, CurrentIndex must be 0.
                    return this.MakeFailure("If there aren't any Endorsements, CurrentIndex must be 0!", StatusCodes.Status400BadRequest);
                }
            }

            // Request was good, now do it
            EndorsementService.SetEndorsements(request.Endorsements);
            EndorsementService.SetCurrentIndex(request.CurrentIndex);
            return this.MakeSuccess();
        }
    }
}
