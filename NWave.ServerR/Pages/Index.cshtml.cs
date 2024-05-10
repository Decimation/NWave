using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NWave.Lib;

namespace NWave.ServerR.Pages
{
	public class IndexModel : PageModel
	{

		private readonly ILogger<IndexModel> _logger;

		[BindProperty]
		public List<BaseSoundItem> Sounds { get; set; }

		public static readonly SoundLibrary snd = new SoundLibrary();

		public const int DEVICE_INDEX = 3;

		public const string SOUNDS = @"H:\Other Music\Audio resources\NMicspam\";

		public IndexModel(ILogger<IndexModel> logger)
		{
			snd.InitDirectory(SOUNDS, DEVICE_INDEX);
			_logger = logger;
			Sounds  = new List<BaseSoundItem>(snd.Sounds.Keys);
		}

		/*public async Task<IActionResult> OnGet()
		{
			_logger.LogDebug($"{nameof(OnGet)}");
			return Page();
		}*/

		public async Task<IActionResult> OnGet()
		{
			_logger.LogDebug($"{nameof(OnGet)}");
			return Page();
		}

		public async Task<IActionResult> OnPostPlayAsync(int? id)
		{
			if (!ModelState.IsValid) {
				return Page();
			}

			var            pr        = new PageResult() { ViewData = { } };
			BaseSoundItem? soundItem = null;

			if (!id.HasValue) {
				goto ret;
			}

			_logger.LogDebug($"{nameof(OnPostPlayAsync)} :: {id}");

			// soundItem = snd.Sounds.Keys.FirstOrDefault(x => x.Id.HasValue && x.Id.Value == id.Value);
			soundItem = Sounds.FirstOrDefault(x => x.Id.HasValue && x.Id.Value == id.Value);

			soundItem?.PlayPause();
			_logger.LogDebug($"{soundItem}");

		ret:
			ViewData[id.ToString()] = soundItem;
			return Page();
		}

	}
}