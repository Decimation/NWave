// Read S NWave.Server ContextHelper.cs
// 2023-10-05 @ 3:53 PM

using Novus.FileTypes;
using System.Buffers;
using System.Net.Mime;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NWave.Server;

public static class ServerUtil
{
	public const int BUF_LEN_DEFAULT = 0x4000;

	public static Encoding Encoding { get; set; } = Encoding.UTF8;

	public static async Task<string> ReadBodyTextAsync(Stream body, int bufLen = BUF_LEN_DEFAULT,
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

	public static Task<string> ReadBodyTextAsync(this HttpContext context)
	{
		return ReadBodyTextAsync(context.Request.Body);
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
	public static async Task<string[]> ReadBodyEntriesAsync(this HttpContext c)
	{
		var b = await c.ReadBodyTextAsync();

		if (string.IsNullOrEmpty(b)) {
			return Array.Empty<string>();
		}

		return b.Split(',');
	}

	public static string BuildString<T>(IEnumerable<T> t, Func<T, string> x)
	{
		var sb = new StringBuilder();

		foreach (T v in t) {
			sb.AppendLine(x(v));
		}

		return sb.ToString();
	}
}