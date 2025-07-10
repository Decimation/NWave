using System.IO.Ports;

namespace NWave.Serial;

public static class Program
{
	public static async Task Main(string[] args)
	{
		var sp = new SerialPort("COM22", 115200, Parity.None, 8, StopBits.One)
		{
			Handshake = Handshake.RequestToSend
		};

		sp.Open();

		while (sp.IsOpen)
		{
			try
			{
				var b= sp.BytesToRead;
			}
			catch (TimeoutException)
			{
				Console.WriteLine("Read timed out.");
			}
		}

		sp.Close();
	}
}
