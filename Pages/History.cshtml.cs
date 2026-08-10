using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace MusicPlayer.Pages
{
    public class HistoryModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        public List<Message> UserMessages { get; set; } = new();

        public void OnGet()
        {
            var useremail = HttpContext.Session.GetString("Useremail");
            if (useremail == null)
            {
                Response.Redirect("/Account/Login");
                return;
            }
            UserMessages = _context.Messages
                .Where(m => m.UserEmail == useremail)
                .OrderByDescending(m => m.CreatedAt)
                .ToList();

        }
    }
}