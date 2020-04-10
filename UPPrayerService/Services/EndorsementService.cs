using System;
using System.Collections.Generic;
using UPPrayerService.Models;

namespace UPPrayerService.Services
{
    public class EndorsementService
    {
        //private List<Endorsement> _testEndorsements;
        private volatile static int _testCurrentIndex = 0;
        private DataContext Context { get; }

        public EndorsementService(DataContext dataContext)
        {
            Context = dataContext;
            //_testEndorsements = new List<Endorsement>();
            //_testEndorsements.Add(new Endorsement() { DonateURL = "https://www.example.com/donate/", HomepageURL = "https://www.example.com/", Summary = "" });
        }

        public IEnumerable<Endorsement> GetEndorsements()
        {
            //return this._testEndorsements;
            return Context.Endorsements;
        }

        public void SetEndorsements(IEnumerable<Endorsement> endorsements)
        {
            //this._testEndorsements = new List<Endorsement>(endorsements);
            Context.Endorsements.RemoveRange(Context.Endorsements);
            Context.Endorsements.AddRange(endorsements);
            Context.SaveChanges();
        }

        public int GetCurrentIndex()
        {
            return _testCurrentIndex;
        }

        public void SetCurrentIndex(int currentIndex)
        {
            _testCurrentIndex = currentIndex;
        }
    }
}
