using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UPPrayerService.Models
{
    public class BlogPost
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public User Author { get; set; }
        public string Content { get; set; }

        public BlogPost()
        {
            this.ID = new Guid().ToString();
        }
    }
}
