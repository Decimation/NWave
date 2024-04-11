using Microsoft.AspNetCore.Mvc;
using NWave.Lib;
using NWave.ServerR.Controllers;

namespace NWave.ServerR;

public class Program
{

	public static  ILogger<Program> _logger;
	private static WebApplication   _app;

	public static async Task Main(string[] args)
	{

		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		// builder.Services.AddRazorPages();
		builder.Services.AddControllers();

		// builder.Services.AddMvc().AddControllersAsServices();

		builder.Services.AddScoped<ISoundItemService, SoundItemService>();
		// builder.Services.AddControllersWithViews().AddControllersAsServices();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		_app = builder.Build();

		// Configure the HTTP request pipeline.
		if (!_app.Environment.IsDevelopment()) {
			_app.UseExceptionHandler("/Error");

			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			_app.UseHsts();
		}

		else {
			_app.UseSwagger();
			_app.UseSwaggerUI();
		}

		_app.MapControllers();
		/*_app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Sound}/{action=id}")*/
		;
		_app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Sound}/{action=Index}");
		
		_app.MapControllerRoute("list", "list/", new { controller = "Sound", action = "List" });

		/*
		private void RegisterCheckoutRoute(IEndpointRouteBuilder endpointRouteBuilder, string pattern)
		{
			//checkout pages
			endpointRouteBuilder.MapControllerRoute("Checkout",
			                                        pattern + "checkout/",
			                                        new { controller = "Checkout", action = "Start" });

			endpointRouteBuilder.MapControllerRoute("CheckoutCompleted",
			                                        pattern + "checkout/completed/{orderId?}",
			                                        new { controller = "Checkout", action = "Completed" });
		}
		*/

		_app.UseHttpsRedirection();
		// _app.UseStaticFiles();
		// _app.UseDefaultFiles();

		// _app.UseRouting();
		// _app.UseMvc();
		_app.UseAuthorization();
		// _app.MapRazorPages();

		using var loggerFactory = LoggerFactory.Create(b =>
		{
			b.AddConsole().AddDebug().AddTraceSource("TRACE");
		});

		_logger = loggerFactory.CreateLogger<Program>();

		// Configure the HTTP request pipeline.
		if (_app.Environment.IsDevelopment()) {
			// _app.UseSwagger();
			// _app.UseSwaggerUI();
		}

		/*_app.MapPost("/Play", SoundController.PlayAsync);
		_app.MapPost("/Stop", (SoundController.StopAsync));
		_app.MapPost("/Pause", (SoundController.PauseAsync));
		_app.MapGet("/Status", (SoundController.StatusAsync));
		_app.MapGet("/List", (SoundController.ListAsync));
		_app.MapPost("/Add", SoundController.AddAsync);
		_app.MapPost("/Remove", SoundController.RemoveAsync);
		_app.MapPost("/Update", SoundController.UpdateAsync);
		_app.MapPost("/AddYouTubeFile", SoundController.AddYouTubeAudioFileAsync);
		_app.MapPost("/AddYouTubeUrl", SoundController.AddYouTubeAudioUrlAsync);*/

		_logger.LogDebug("dbg");

		await _app.RunAsync();
	}

}