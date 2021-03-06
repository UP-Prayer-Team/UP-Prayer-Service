﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [Authorize(Roles = Models.User.ROLE_ADMIN)]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateRequest request)
        {
            BlogPost Post = new BlogPost();
            Post.Title = request.Title;
            Post.Content = request.Content;
            Post.Date = DateTime.Parse(request.Date);
            Post.Author = await UserManager.FindByIdAsync(User.Claims.First(c => c.Type == "id").Value);

            DataContext.BlogPosts.Add(Post);

            await DataContext.SaveChangesAsync();

            return this.MakeSuccess(new { id = Post.ID });
        }

        /// <summary>
        /// Returns a copy of the given post, but with the author info replaced with the author name.
        /// </summary>
        /// <param name="post">The post to anonymize.</param>
        /// <returns></returns>
        private object AnonymizePost(BlogPost post)
        {
            return new { ID = post.ID, Title = post.Title, Date = post.Date.ToShortDateString(), Author = post.Author?.DisplayName ?? "<nobody>", Content = post.Content };
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            bool includeFuture = HttpContext.User.IsInRole(Models.User.ROLE_SPECTATOR);
            List<object> emptyPosts = new List<object>();

            IQueryable<BlogPost> posts = includeFuture ? DataContext.BlogPosts : DataContext.BlogPosts.Where(p => p.Date < DateTime.Now);
            foreach (BlogPost post in posts.Include(p => p.Author).OrderByDescending(p => p.Date))
            {
                emptyPosts.Add(AnonymizePost(new BlogPost(post.ID,post.Title, post.Date, post.Author, "")));
            }

            return this.MakeSuccess(emptyPosts.ToArray());
        }

        [HttpGet("post/{id}")]
        public async Task<IActionResult> Post(string id)
        {
            bool includeFuture = HttpContext.User.IsInRole(Models.User.ROLE_SPECTATOR);
            BlogPost post = DataContext.BlogPosts.Include(p => p.Author).FirstOrDefault(p => p.ID == id);

            if (post == null || (post.Date > DateTime.Now && !includeFuture))
            {
                return this.MakeFailure("No such Post", StatusCodes.Status404NotFound);
            }
            else
            {
                return this.MakeSuccess(AnonymizePost(post));
            }
        }

        public class DeleteRequest
        {
            public String ID { get; set; }
        }

        [Authorize(Roles = Models.User.ROLE_ADMIN)]
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
                await DataContext.SaveChangesAsync();
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

        [Authorize(Roles = Models.User.ROLE_ADMIN)]
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