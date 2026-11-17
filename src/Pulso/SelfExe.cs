using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Pulso;

public static class SelfExe
{
	public sealed record Trailer(string Payload, long DataEnd);

	public const string Magic = "PULSOTSK";

	public static string CurrentExe => Environment.ProcessPath ?? Application.ExecutablePath;

	public static bool IsSingleFilePublished
	{
		get
		{
			string currentExe = CurrentExe;
			if (currentExe.EndsWith("Pulso.exe", StringComparison.OrdinalIgnoreCase))
			{
				return !currentExe.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}
	}

	public static Trailer? ReadTrailer(string path)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		long length = fileStream.Length;
		if (length < 4096)
		{
			return null;
		}
		fileStream.Seek(-12L, SeekOrigin.End);
		Span<byte> buffer = stackalloc byte[12];
		if (fileStream.Read(buffer) != 12)
		{
			return null;
		}
		uint num = BitConverter.ToUInt32(buffer.Slice(0, 4));
		if (Encoding.ASCII.GetString(buffer.Slice(4, 8)) != "PULSOTSK" || num == 0 || num > length - 12)
		{
			return null;
		}
		long num2 = length - 12 - num;
		fileStream.Seek(num2, SeekOrigin.Begin);
		byte[] array = new byte[num];
		int i;
		int num3;
		for (i = 0; i < array.Length; i += num3)
		{
			num3 = fileStream.Read(array, i, array.Length - i);
			if (num3 <= 0)
			{
				break;
			}
		}
		if (i != array.Length)
		{
			return null;
		}
		return new Trailer(Encoding.UTF8.GetString(array), num2);
	}

	public static void AppendTrailer(string path, string payload)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(payload);
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
		fileStream.Seek(0L, SeekOrigin.End);
		fileStream.Write(bytes);
		Span<byte> span = stackalloc byte[12];
		BitConverter.TryWriteBytes(span.Slice(0, 4), (uint)bytes.Length);
		Encoding.ASCII.GetBytes("PULSOTSK").CopyTo(span.Slice(4, span.Length - 4));
		fileStream.Write(span);
		fileStream.Flush(flushToDisk: true);
	}

	public static void CopySelfWithoutTrailer(string dest)
	{
		string currentExe = CurrentExe;
		Trailer trailer = ReadTrailer(currentExe);
		using FileStream fileStream = new FileStream(currentExe, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		long num = trailer?.DataEnd ?? fileStream.Length;
		using FileStream fileStream2 = new FileStream(dest, FileMode.Create, FileAccess.Write);
		long num2 = num;
		byte[] array = new byte[65536];
		fileStream.Seek(0L, SeekOrigin.Begin);
		while (num2 > 0)
		{
			int count = (int)Math.Min(array.Length, num2);
			int num3 = fileStream.Read(array, 0, count);
			if (num3 <= 0)
			{
				break;
			}
			fileStream2.Write(array, 0, num3);
			num2 -= num3;
		}
		fileStream2.Flush(flushToDisk: true);
	}

	public static bool ZeroPeChecksum(string path)
	{
		try
		{
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
			if (fileStream.Length < 64)
			{
				return false;
			}
			byte[] array = new byte[64];
			fileStream.ReadExactly(array, 0, 64);
			if (array[0] != 77 || array[1] != 90)
			{
				return false;
			}
			int num = BitConverter.ToInt32(array, 60);
			if (num <= 0 || num + 92 > fileStream.Length)
			{
				return false;
			}
			fileStream.Seek(num + 88, SeekOrigin.Begin);
			fileStream.Write(new byte[4]);
			fileStream.Flush(flushToDisk: true);
			return true;
		}
		catch (Exception ex)
		{
			AppLog.Write("ZeroPeChecksum: " + ex.Message);
			return false;
		}
	}
}
