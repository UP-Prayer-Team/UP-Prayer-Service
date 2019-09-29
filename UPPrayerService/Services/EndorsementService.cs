using System;
using System.Collections.Generic;
using UPPrayerService.Models;

namespace UPPrayerService.Services
{
    public class EndorsementService
    {
        private List<Endorsement> _testEndorsements;
        private int _testCurrentIndex = 0;

        public EndorsementService()
        {
            _testEndorsements = new List<Endorsement>();
            _testEndorsements.Add(new Endorsement() { DonateURL = "https://www.example.com/donate/", HomepageURL = "https://www.example.com/", Summary = "" });
        }

        public IEnumerable<Endorsement> GetEndorsements()
        {
            return this._testEndorsements;
        }

        public void SetEndorsements(IEnumerable<Endorsement> endorsements)
        {
            this._testEndorsements = new List<Endorsement>(endorsements);
        }

        public int GetCurrentIndex()
        {
            return this._testCurrentIndex;
        }

        public void SetCurrentIndex(int currentIndex)
        {
            this._testCurrentIndex = currentIndex;
        }
    }
}
