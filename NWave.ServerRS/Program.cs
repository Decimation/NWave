namespace NWave.ServerRS;

public class Program
{
	public static ILogger<Program> _logger;
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		
		// Add services to the container.
		builder.Services.AddControllersWithViews();
		builder.Services.AddRazorPages();
		builder.Services.AddControllers();
		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Home/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}
		using var loggerFactory = LoggerFactory.Create(b =>
		{
			b.AddConsole().AddDebug().AddTraceSource("TRACE");
		});

		_logger = loggerFactory.CreateLogger<Program>();
		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();
		app.MapRazorPages();
		app.UseAuthorization();
		app.MapControllers();
		app.MapControllerRoute("default", "{controller=Audio}");
		/*app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");*/

		app.Run();
	}
}