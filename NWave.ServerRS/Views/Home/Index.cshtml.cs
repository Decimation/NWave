using Microsoft.AspNetCore.Mvc.RazorPages;
using NWave.Lib;
using NWave.ServerRS.Models;

namespace NWave.ServerRS.Views.Home
{
	public class IndexModel : PageModel
	{

		public List<SoundModel> AudioFiles { get; set; }

		public const int DEVICE_INDEX = 3;

		public const string SOUNDS = @"H:\Other Music\Audio resources\NMicspam\";

		private static readonly SoundLibrary snd = new SoundLibrary();

		public IndexModel() { }

		public void OnGet()
		{
			snd.InitDirectory(SOUNDS, DEVICE_INDEX);

			AudioFiles =
				new List<SoundModel>(snd.Sounds.Keys.Select(x => new SoundModel() { Name = x.Name, Sound = x }));
		}

	}
}