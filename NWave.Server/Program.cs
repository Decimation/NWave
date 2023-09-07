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
		var s = context.Request.Headers[HDR_SOUND];

		if (Sounds.TryRemove(s, out var ss)) {
			ss.Stop();

		}

		Logger.LogInformation("Stopped {Sound}", s);

	}
	private static async Task IsPlaying(HttpContext context)
	{
		var s = context.Request.Headers[HDR_SOUND];
		
		var b = Sounds.ContainsKey(s);
		
		await context.Response.WriteAsync(text: $"{b.ToString()}", Encoding.UTF8);
		await context.Response.CompleteAsync();
	}

	private static async Task Play(HttpContext context)
	{
		// string audioFilePath = @"H:\Other Music\Audio resources\NMicspam\ow.wav"; // Change this to the path of your audio file

		string? audioFilePath = context.Request.Headers[HDR_SOUND];

		var waveOut = new WaveOutEvent()
		{
			DeviceNumber = DEVICE_INDEX
		};

		await using (var audioFileReader = new AudioFileReader(audioFilePath)) {
			waveOut.Init(audioFileReader);
			waveOut.Play();
			Logger.LogInformation("Playing {Sound}", audioFilePath);
			Sounds.TryAdd(audioFilePath, waveOut);

			/*while (waveOut.PlaybackState == PlaybackState.Playing) {
				System.Threading.Thread.Sleep(100);
			}*/
		}

		Sounds.TryRemove(audioFilePath, out _);

	}
}