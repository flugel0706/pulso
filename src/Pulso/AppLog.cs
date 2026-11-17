using System;
using System.IO;
using System.Text;

namespace Pulso;

public static class AppLog
{
	private static readonly object Gate = new object();

	public static void Write(string message)
	{
		try
		{
			lock (Gate)
			{
				File.AppendAllText(Paths.LogFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}  {message}{Environment.NewLine}", Encoding.UTF8);
			}
		}
		catch
		{
		}
	}
}
