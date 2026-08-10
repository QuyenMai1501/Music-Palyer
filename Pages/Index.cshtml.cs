using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Models;
using MusicPlayer.Services;

namespace MusicPlayer.Pages;

public class IndexModel : PageModel
{
    private readonly MusicService _musicService;

    [TempData]
    public string? SuccessMessage { get; set; }

    public IndexModel(MusicService musicService)
    {
        _musicService = musicService;
    }

    public List<Song> TrendingSongs { get; set; } = new List<Song>();

    public void OnGet()
    {
        TrendingSongs = _musicService.GetTrendingSongsThisWeek();
    }

}
