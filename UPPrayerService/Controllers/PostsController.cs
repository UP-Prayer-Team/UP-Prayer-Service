using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UPPrayerService.Models;

namespace UPPrayerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private UserManager<User> UserManager { get; }
        private RoleManager<IdentityRole> RoleManager { get; }

        private DataContext DataContext {get; }
        public PostsController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, DataContext dataContext)
        {
            this.UserManager = userManager;

            this.RoleManager = roleManager;
            this.DataContext = dataContext;
        }

        public class CreateRequest
        {
            public string Title { get; set; }
            public string Date { get; set; }
            public string Content { get; set; }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateRequest request)
        {
            BlogPost Post = new BlogPost();
            Post.Title = request.Title;
            Post.Content = request.Content;
            Post.Date = DateTime.Parse(request.Date);
            //TODO set user
            DataContext.BlogPosts.Add(Post);

            await DataContext.SaveChangesAsync();

            return this.MakeSuccess(new { id = Post.ID });
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            List<BlogPost> emptyPosts = new List<BlogPost>();

            foreach (BlogPost post in DataContext.BlogPosts)
            {
                emptyPosts.Add(new BlogPost(post.ID,post.Title, post.Date, post.Author, ""));
            }

            return this.MakeSuccess(emptyPosts.ToArray());
        }

        [HttpGet("post/{id}")]
        public async Task<IActionResult> Post(string id)
        {
            BlogPost post = DataContext.BlogPosts.FirstOrDefault(p => p.ID == id);

            if (post == null)
            {
                return this.MakeFailure("No such Post", StatusCodes.Status404NotFound);
            }
            else
            {
                return this.MakeSuccess(post);
            }
        }

        public class DeleteRequest
        {
            public String ID { get; set; }
        }

        //[Authorize(Roles = Models.User.ROLE_ADMIN)]
        [HttpPost("delete")]

        public async Task<IActionResult> Delete(DeleteRequest request)
        {
            BlogPost post = DataContext.BlogPosts.FirstOrDefault(p => p.ID == request.ID);

            if (post == null)
            {
                return this.MakeFailure("Post not found.", StatusCodes.Status404NotFound);
            }
            else
            {
                DataContext.BlogPosts.Remove(post);
                return this.MakeSuccess(null);
            }
        }

        public class UpdateRequest
        {
            public string ID { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
            public string Content { get; set; }
        }

        //[Authorize(Roles = Models.User.ROLE_ADMIN)]
        [HttpPost("update")]

        public async Task<IActionResult> Update(UpdateRequest request)
        {
            BlogPost post = DataContext.BlogPosts.FirstOrDefault(p => p.ID == request.ID);

            if (post == null)
            {
                return this.MakeFailure("Post not found.", StatusCodes.Status404NotFound);
            }
            else
            {
                post.Title = request.Title;
                post.Date = DateTime.Parse(request.Date);
                post.Content = request.Content;

                await DataContext.SaveChangesAsync();

                return this.MakeSuccess();
            }
        }


    }
}