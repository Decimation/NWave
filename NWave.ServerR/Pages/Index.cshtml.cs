using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NWave.Lib;

namespace NWave.ServerR.Pages
{
	public class IndexModel : PageModel
	{

		private readonly ILogger<IndexModel> _logger;

		public List<BaseSoundItem> Sound { get; }

		private static readonly SoundLibrary snd          = new SoundLibrary();
		public const            int          DEVICE_INDEX = 3;

		public const string SOUNDS = @"H:\Other Music\Audio resources\NMicspam\";

		public IndexModel(ILogger<IndexModel> logger)
		{
			snd.InitDirectory(SOUNDS, DEVICE_INDEX);
			Sound   = new List<BaseSoundItem>(snd.Sounds.Keys);
			_logger = logger;
		}

		public async Task<IActionResult> OnGet()
		{
			return Page();
		}

		public async Task<IActionResult> OnPostPlayAsync(int? id)
		{
			var soundItem = Sound.FirstOrDefault(x => x.Id == id);

			soundItem?.PlayPause();
			return RedirectToPage();
		}

	}
}