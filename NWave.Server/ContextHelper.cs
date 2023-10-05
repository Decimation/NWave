// Read S NWave.Server ContextHelper.cs
// 2023-10-05 @ 3:53 PM

using Novus.FileTypes;
using System.Text;

namespace NWave.Server;

public static class ContextHelper
{
	public static async Task WriteMessageAsync(this HttpContext context, string text)
	{
		context.Response.ContentType = FileType.MT_TEXT_PLAIN;
		await context.Response.WriteAsync(text: text, Encoding.UTF8);
		await context.Response.CompleteAsync();
	}

	public static async Task<string?> ReadBodyAsync(this HttpContext context)
	{
		var l = context.Request.ContentLength;

		if (!l.HasValue) {
			return null;
		}

		var lt  = context.Request.ContentType;
		var buf = new byte[l.Value];
		var l2  = await context.Request.Body.ReadAsync(buf, 0, buf.Length);
		var ss  = Encoding.UTF8.GetString(buf);
		return ss;
	}
}