using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Console;
using NAudio.Wave;

namespace NWave.Server
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			// builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			_app = builder.Build();

			using var loggerFactory = LoggerFactory.Create(b =>
			{
				b.AddConsole().AddDebug().AddTraceSource("TRACE");
			});

			_logger2 = loggerFactory.CreateLogger<Program>();

			// Configure the HTTP request pipeline.
			if (_app.Environment.IsDevelopment()) {
				_app.UseSwagger();
				_app.UseSwaggerUI();
			}

			_app.UseHttpsRedirection();
			// app.UseAuthorization();
			// app.MapControllers();

			_app.MapGet("/Play", RequestDelegate);
			_app.MapGet("/Stop", (Delegate));
			_logger2.LogDebug("dbg");
			_app.Run();
		}

		private static async Task Delegate(HttpContext context)
		{
			var s = context.Request.Headers["s"];

			if (_dict.TryRemove(s, out var ss)) {
				ss.Stop();

			}

			_logger2.LogInformation("Stopped {Sound}", s);

		}

		private static readonly ConcurrentDictionary<string, WaveOutEvent> _dict =
			new ConcurrentDictionary<string, WaveOutEvent>();

		private static WebApplication   _app;
		private static ILogger<Program> _logger2;

		private static async Task RequestDelegate(HttpContext context)
		{
			int deviceIndex = 3; // Change this to the index of your desired audio device

			// string audioFilePath = @"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file

			string audioFilePath = context.Request.Headers["s"];
			var    waveOut       = new WaveOutEvent();
			waveOut.DeviceNumber = deviceIndex; // Set the desired audio device

			using (var audioFileReader = new AudioFileReader(audioFilePath)) {
				waveOut.Init(audioFileReader);
				waveOut.Play();
				_logger2.LogInformation("Playing {Sound}", audioFilePath);
				_dict.TryAdd(audioFilePath, waveOut);

				while (waveOut.PlaybackState == PlaybackState.Playing) {
					System.Threading.Thread.Sleep(100);
				}
			}

			_dict.TryRemove(audioFilePath, out _);

		}
	}
}