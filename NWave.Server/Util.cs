// Read S NWave.Server ContextHelper.cs
// 2023-10-05 @ 3:53 PM

using Novus.FileTypes;
using System.Buffers;
using System.Text;

namespace NWave.Server;

public static class Util
{
	public const    int      BUFLEN_DEFAULT = 4096;
	internal static Encoding Encoding { get; set; } = Encoding.UTF8;

	public static async Task WriteMessageAsync(this HttpContext context, string text)
	{
		context.Response.ContentType = FileType.MT_TEXT_PLAIN;
		await context.Response.WriteAsync(text: text, Encoding);
		await context.Response.CompleteAsync();
	}

	public static async Task<string> ReadBodyAsync(Stream body,
	                                               int bufLen = BUFLEN_DEFAULT,
	                                               CancellationToken c = default)
	{
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
			var encodedString = Encoding.GetString(buffer, 0, rem);
			builder.Append(encodedString);
		}

		ArrayPool<byte>.Shared.Return(buffer);

		var entireRequestBody = builder.ToString();
		return entireRequestBody;
	}

	public static Task<string> ReadBodyAsync(this HttpContext context)
	{
		return ReadBodyAsync(context.Request.Body);
	}

	/*public static async Task<string?> ReadBodyAsync(this HttpContext context)
	{
		var l = context.Request.ContentLength;

		if (!l.HasValue) {
			return null;
		}

		var lt  = context.Request.ContentType;
		var buf = new byte[l.Value];
		var l2  = await context.Request.Body.ReadAsync(buf, 0, buf.Length);
		var ss  = Encoding.GetString(buf);
		return ss;
	}*/
}