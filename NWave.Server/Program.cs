using System.Collections.Concurrent;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Console;
using NAudio.Wave;

namespace NWave.Server;

public class Program
{
	private const string HDR_SOUND    = "s";
	private const int    DEVICE_INDEX = 3;

	/*
	$x = iwr -Method Get -Uri "https://localhost:7182/IsPlaying" -Headers @{'s'="H:\Other Music\Drum N Bass\Darchives\darchives-scenario-(primcd003)-cd-flac-1999-dl\01-darchives-first_offensive.flac"}
	$x
	 */

	/*
	iwr -Method Get -Uri "https://localhost:7182/Play" -Headers @{'s'="H:\Other Music\Drum N Bass\Darchives\darchives-scenario-(primcd003)-cd-flac-1999-dl\01-darchives-first_offensive.flac"}
	 */

	/*
	iwr -Method Get -Uri "https://localhost:7182/Stop" -Headers @{'s'="H:\Other Music\Drum N Bass\Darchives\darchives-scenario-(primcd003)-cd-flac-1999-dl\01-darchives-first_offensive.flac"}
	 */

	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.

		// builder.Services.AddControllers();
		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		App = builder.Build();

		using var loggerFactory = LoggerFactory.Create(b =>
		{
			b.AddConsole().AddDebug().AddTraceSource("TRACE");
		});

		Logger = loggerFactory.CreateLogger<Program>();

		// Configure the HTTP request pipeline.
		if (App.Environment.IsDevelopment()) {
			App.UseSwagger();
			App.UseSwaggerUI();
		}

		App.UseHttpsRedirection();
		// app.UseAuthorization();
		// app.MapControllers();

		App.MapGet("/Play", Play);
		App.MapGet("/Stop", (Stop));
		App.MapGet("/IsPlaying", (IsPlaying));

		Logger.LogDebug("dbg");
		await App.RunAsync();
	}

	private static readonly ConcurrentDictionary<string, WaveOutEvent> Sounds = new();

	private static WebApplication   App;
	private static ILogger<Program> Logger;

	private static async Task Stop(HttpContext context)
	{
		string s = context.Request.Headers[HDR_SOUND];

		if (Sounds.TryRemove(s, out var ss)) {
			ss.Stop();
			ss.Dispose();
		}

		Logger.LogInformation("Stopped {Sound}", s);

	}

	private static async Task IsPlaying(HttpContext context)
	{
		string s = context.Request.Headers[HDR_SOUND];

		var b = Sounds.ContainsKey(s);

		await context.Response.WriteAsync(text: $"{b.ToString()}", Encoding.UTF8);
		await context.Response.CompleteAsync();
	}

	private static async Task Play(HttpContext context)
	{
		// string audioFilePath = @"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file

		string? audioFilePath = context.Request.Headers[HDR_SOUND];

		using var waveOut = new WaveOutEvent()
		{
			DeviceNumber = DEVICE_INDEX
		};

		await using (var audioFileReader = new AudioFileReader(audioFilePath)) {
			waveOut.Init(audioFileReader);
			Sounds.TryAdd(audioFilePath, waveOut);
			waveOut.Play();
			Logger.LogInformation("Playing {Sound}", audioFilePath);

			ThreadPool.QueueUserWorkItem((x) =>
			{

				while (waveOut.PlaybackState == PlaybackState.Playing) {
					System.Threading.Thread.Sleep(100);

					if (!Sounds.ContainsKey(audioFilePath)) {
						Logger.LogInformation("Break");
						break;
					}
				}

				Sounds.TryRemove(audioFilePath, out var wcv);
				wcv?.Dispose();
			});
		}

	}
}