// Read S NWave.Server ContextHelper.cs
// 2023-10-05 @ 3:53 PM

using Microsoft.Extensions.Primitives;
using Novus.FileTypes;
using System.Buffers;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NWave.Server;

public static class ServerUtil
{

	public const int BUF_LEN_DEFAULT = 0x4000;

	public static Encoding Encoding { get; set; } = Encoding.UTF8;

	public static async Task<string> ReadBodyTextAsync(Stream body, Encoding? encoding = null,
	                                                   int bufLen = BUF_LEN_DEFAULT,
	                                                   CancellationToken c = default)
	{
		encoding ??= Encoding;

		// Build up the request body in a string builder.
		var builder = new StringBuilder();

		// Rent a shared buffer to write the request body into.
		byte[] buffer = ArrayPool<byte>.Shared.Rent(bufLen);
		var    mem    = buffer.AsMemory();

		while (true) {
			var rem = await body.ReadAsync(mem, c);

			if (rem == 0) {
				break;
			}

			// Append the encoded string into the string builder.
			var encodedString = encoding.GetString(buffer, 0, rem);
			builder.Append(encodedString);
		}

		ArrayPool<byte>.Shared.Return(buffer);

		var entireRequestBody = builder.ToString();
		return entireRequestBody;
	}
	

	/*public static Task<string> ReadBodyTextAsync(this HttpContext context)
	{
		return ReadBodyTextAsync(context.Request.Body);
	}*/

	/*public static async Task<string[]> ReadBodyEntriesAsync(this HttpContext c)
	{
		var b = await c.ReadBodyTextAsync();

		if (String.IsNullOrEmpty(b)) {
			return [];
		}

		return b.Split(',');
	}*/


	public static string BuildString<T>(IEnumerable<T> t, Func<T, string> x)
	{
		var sb = new StringBuilder();

		foreach (T v in t) {
			sb.AppendLine(x(v));
		}

		return sb.ToString();
	}

	public static async Task<string?> ReadBodyAsync(this HttpContext context, Encoding? encoding = null)
	{
		encoding ??= Encoding;

		var l = context.Request.ContentLength;

		if (!l.HasValue) {
			return null;
		}

		var lt  = context.Request.ContentType;
		var buf = new byte[l.Value];
		var l2  = await context.Request.Body.ReadAsync(buf, 0, buf.Length);
		var ss  = encoding.GetString(buf);

		return ss;
	}

	[return: MN]
	public static async Task<T> TryReadJsonAsync<T>(this HttpContext ctx, CancellationToken token = default)
	{
		T t = default;

		switch (ctx.Request.Headers.ContentType) {
			case MediaTypeNames.Application.Json:

				t = await ctx.Request.ReadFromJsonAsync<T>(JsonSerializerOptions.Default, token);

				break;

			case MediaTypeNames.Application.FormUrlEncoded:
			default:
				// var s = (await ctx.ReadBodyAsync());

				try {
					t = await JsonSerializer.DeserializeAsync<T>(ctx.Request.Body, JsonSerializerOptions.Default, token);
				}
				catch (Exception e) {
					Program.Logger.LogError(e, "{Context}", ctx);
				}

				// var body = ctx.ReadBodyAsync();

				// strings = s?.Split([',']);

				break;
		}

		return t;
	}

	public static async Task<string[]> TryReadBodyAsync(this HttpContext ctx, CancellationToken token = default)
	{
		string[] t;

		switch (ctx.Request.Headers.ContentType) {
			case MediaTypeNames.Application.Json:

				t = await ctx.Request.ReadFromJsonAsync<string[]>(JsonSerializerOptions.Default, token);

				break;

			case MediaTypeNames.Application.FormUrlEncoded:
			default:
				// var s = (await ctx.ReadBodyAsync());

				var body = await ctx.ReadBodyAsync();

				t = body?.Split([',']);

				break;
		}

		return t;
	}

}