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
            this.ID = Guid.NewGuid().ToString();
        }

        public BlogPost(string id, string title, DateTime date, User author, string content)
        {
            this.ID = id;
            this.Title = title;
            this.Date = date;
            this.Author = author;
            this.Content = content;
        }
    }
}
