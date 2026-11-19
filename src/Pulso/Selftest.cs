using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Pulso;

public static class Selftest
{
	public static (bool Ok, string Report) Run()
	{
		StringBuilder sb = new StringBuilder();
		int pass = 0;
		int fail = 0;
		try
		{
			Hotkey hotkey = Hotkey.Parse("Ctrl+Alt+Espaço");
			Check("atalho: parse Ctrl+Alt+Espaço", hotkey.Ctrl && hotkey.Alt && !hotkey.Shift && !hotkey.Win && hotkey.Vk == 32, hotkey.ToString());
			Check("atalho: round-trip texto", hotkey.ToString() == "Ctrl+Alt+Espaço", hotkey.ToString());
			Hotkey hotkey2 = Hotkey.Parse("Shift+F5");
			Check("atalho: Shift+F5", hotkey2.Shift && hotkey2.Vk == 116 && !hotkey2.Ctrl);
			Check("atalho: inválido vira inválido", !Hotkey.Parse("").IsValid);
			Check("atalho: nome F12", Hotkey.VkName(123) == "F12");
		}
		catch (Exception ex)
		{
			Check("atalho: exceção", ok: false, ex.Message);
		}
		try
		{
			Macro macro = new Macro
			{
				ScreenW = 1920,
				ScreenH = 1080,
				Hwnd = 1234L
			};
			macro.Events.Add(new MacroEvent
			{
				T = 0L,
				Kind = EvKind.KeyDown,
				Vk = 65,
				Scan = 30
			});
			macro.Events.Add(new MacroEvent
			{
				T = 120L,
				Kind = EvKind.MouseMove,
				X = 800,
				Y = 600
			});
			macro.Events.Add(new MacroEvent
			{
				T = 240L,
				Kind = EvKind.MouseDown,
				Button = 0,
				X = 800,
				Y = 600
			});
			macro.Events.Add(new MacroEvent
			{
				T = 300L,
				Kind = EvKind.MouseUp,
				Button = 0,
				X = 800,
				Y = 600
			});
			macro.Events.Add(new MacroEvent
			{
				T = 360L,
				Kind = EvKind.Wheel,
				Wheel = 120,
				X = 800,
				Y = 600
			});
			string path = Path.Combine(Path.GetTempPath(), "pulso-selftest.gmt");
			macro.Save(path);
			Macro macro2 = Macro.Load(path);
			File.Delete(path);
			Check("macro: JSON round-trip", macro2.Events.Count == macro.Events.Count && macro2.Events[0].Vk == 65 && macro2.Events[4].Wheel == 120 && macro2.Duration == 360, $"{macro2.Events.Count} eventos, {macro2.Duration} ms");
		}
		catch (Exception ex2)
		{
			Check("macro: exceção", ok: false, ex2.Message);
		}
		try
		{
			Macro macro3 = new Macro();
			macro3.Events.Add(new MacroEvent
			{
				T = 0L,
				Kind = EvKind.MouseMove,
				X = 10,
				Y = 20,
				Hwnd = 77L
			});
			macro3.Events.Add(new MacroEvent
			{
				T = 90L,
				Kind = EvKind.MouseDown,
				Button = 0,
				X = 10,
				Y = 20,
				Hwnd = 77L
			});
			macro3.Events.Add(new MacroEvent
			{
				T = 100L,
				Kind = EvKind.MouseUp,
				Button = 0,
				X = 10,
				Y = 20,
				Hwnd = 77L
			});
			macro3.Events.Add(new MacroEvent
			{
				T = 200L,
				Kind = EvKind.KeyDown,
				Vk = 65,
				Scan = 30,
				Hwnd = 77L
			});
			macro3.Events.Add(new MacroEvent
			{
				T = 260L,
				Kind = EvKind.KeyUp,
				Vk = 65,
				Scan = 30,
				Hwnd = 77L
			});
			string text = Path.Combine(Path.GetTempPath(), "pulso-selftest.rec");
			int num = macro3.ExportRec(text);
			long length = new FileInfo(text).Length;
			Macro macro4 = Macro.ImportRec(text);
			File.Delete(text);
			bool ok2 = num == 0 && length == macro3.Events.Count * 20 && macro4.Events.Count == macro3.Events.Count && macro4.Events[3].Kind == EvKind.KeyDown && macro4.Events[3].Vk == 65 && macro4.Events[3].Scan == 30 && macro4.Events[2].T == 100 && macro4.Events[4].T == 260;
			Check("rec: exportar/importar", ok2, $"{length} bytes, {macro4.Events.Count} eventos");
		}
		catch (Exception ex3)
		{
			Check("rec: exceção", ok: false, ex3.Message);
		}
		try
		{
			string path2 = Path.Combine(Path.GetTempPath(), "pulso-selftest.bin");
			Random random = new Random(7);
			byte[] array = new byte[5000];
			random.NextBytes(array);
			File.WriteAllBytes(path2, array);
			string text2 = "{\"x\":\"áéí\"}";
			SelfExe.AppendTrailer(path2, text2);
			SelfExe.Trailer? trailer = SelfExe.ReadTrailer(path2);
			long num2 = trailer?.DataEnd ?? (-1);
			string text3 = trailer?.Payload ?? "";
			Check("trailer: escrever/ler", num2 == 5000 && text3 == text2, $"dataEnd={num2}, payload={text3}");
			File.Delete(path2);
		}
		catch (Exception ex4)
		{
			Check("trailer: exceção", ok: false, ex4.Message);
		}
		try
		{
			string path3 = Path.Combine(Path.GetTempPath(), "pulso-selftest.exe");
			byte[] array2 = new byte[1024];
			array2[0] = 77;
			array2[1] = 90;
			BitConverter.GetBytes(128).CopyTo(array2, 60);
			BitConverter.GetBytes(3735928559u).CopyTo(array2, 216);
			File.WriteAllBytes(path3, array2);
			bool flag = SelfExe.ZeroPeChecksum(path3);
			uint num3 = BitConverter.ToUInt32(File.ReadAllBytes(path3), 216);
			File.Delete(path3);
			Check("PE: zerar checksum", flag && num3 == 0, $"checksum=0x{num3:X8}");
		}
		catch (Exception ex5)
		{
			Check("PE: exceção", ok: false, ex5.Message);
		}
		int num4 = Marshal.SizeOf<Native.INPUT>();
		int num5 = (Environment.Is64BitProcess ? 40 : 28);
		Check("SendInput: tamanho de INPUT", num4 == num5, $"{num4} bytes (esperado {num5})");
		try
		{
			Settings settings = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(new Settings
			{
				HotkeyRecord = "Shift+F9",
				SpeedPercent = 175,
				Loops = 3
			}, Macro.JsonOpts), Macro.JsonOpts);
			Check("config: JSON round-trip", settings.HotkeyRecord == "Shift+F9" && settings.SpeedPercent == 175 && settings.Loops == 3 && settings.UseScanCodes);
		}
		catch (Exception ex6)
		{
			Check("config: exceção", ok: false, ex6.Message);
		}
		try
		{
			Macro macro5 = new Macro();
			macro5.Events.Add(new MacroEvent
			{
				T = 0L,
				Kind = EvKind.MouseMove,
				X = 5,
				Y = 6
			});
			macro5.Events.Add(new MacroEvent
			{
				T = 700L,
				Kind = EvKind.MouseDown,
				Button = 0,
				X = 5,
				Y = 6
			});
			MacroPackage macroPackage = JsonSerializer.Deserialize<MacroPackage>(JsonSerializer.Serialize(new MacroPackage
			{
				Name = "teste",
				Events = macro5.Events.Count,
				DurationMs = macro5.Duration,
				Macro = macro5,
				Play = new PlaybackPreset
				{
					SpeedPercent = 150,
					Loops = 4,
					Instant = true,
					StartDelayMs = 250
				}
			}, Macro.JsonOpts), Macro.JsonOpts);
			Settings settings2 = new Settings();
			macroPackage.Play.ApplyTo(settings2);
			Check("pacote: round-trip .pulso", macroPackage.Name == "teste" && macroPackage.Events == 2 && macroPackage.Macro.Events.Count == 2 && settings2.SpeedPercent == 150 && settings2.Loops == 4 && settings2.Instant && settings2.StartDelayMs == 250, $"{macroPackage.Events} eventos, {settings2.SpeedPercent}%, {settings2.Loops}x");
		}
		catch (Exception ex7)
		{
			Check("pacote: exceção", ok: false, ex7.Message);
		}
		try
		{
			Settings settings3 = new Settings
			{
				RecentFiles = new List<string> { "a.gmt", "b.pulso" }
			};
			Settings settings4 = JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(settings3, Macro.JsonOpts), Macro.JsonOpts);
			Check("recentes: JSON round-trip", settings4.RecentFiles.Count == 2 && settings4.RecentFiles[1] == "b.pulso", $"{settings4.RecentFiles.Count} itens");
		}
		catch (Exception ex8)
		{
			Check("recentes: exceção", ok: false, ex8.Message);
		}
		try
		{
			Check("fundo: imagem embutida", Backdrop.Available, Backdrop.Available ? $"{Backdrop.Source!.Width}x{Backdrop.Source!.Height}" : "sem imagem");
			using Bitmap bitmap3 = new Bitmap(560, 360);
			using (Graphics graphics3 = Graphics.FromImage(bitmap3))
			{
				Backdrop.Paint(graphics3, new Rectangle(0, 0, 560, 360), new Size(560, 360), Point.Empty, 0f);
			}
			bitmap3.Save(Path.Combine(Paths.BaseDir, "backdrop-test.png"), System.Drawing.Imaging.ImageFormat.Png);
			Check("fundo: composição", true, "backdrop-test.png");
		}
		catch (Exception ex9)
		{
			Check("fundo: exceção", ok: false, ex9.Message);
		}
		sb.AppendLine();
		StringBuilder stringBuilder = sb;
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(32, 2, stringBuilder);
		handler.AppendLiteral("Resultado: ");
		handler.AppendFormatted(pass);
		handler.AppendLiteral(" passaram, ");
		handler.AppendFormatted(fail);
		handler.AppendLiteral(" falharam.");
		stringBuilder2.AppendLine(ref handler);
		stringBuilder = sb;
		StringBuilder stringBuilder3 = stringBuilder;
		handler = new StringBuilder.AppendInterpolatedStringHandler(23, 3, stringBuilder);
		handler.AppendLiteral("Plataforma: ");
		handler.AppendFormatted(Environment.OSVersion);
		handler.AppendLiteral(" · .NET ");
		handler.AppendFormatted(Environment.Version);
		handler.AppendLiteral(" · ");
		handler.AppendFormatted(Environment.Is64BitProcess ? "x64" : "x86");
		stringBuilder3.AppendLine(ref handler);
		return (Ok: fail == 0, Report: sb.ToString());
		void Check(string name, bool ok, string? detail = null)
		{
			if (ok)
			{
				pass++;
			}
			else
			{
				fail++;
			}
			StringBuilder stringBuilder4 = sb;
			StringBuilder.AppendInterpolatedStringHandler handler2 = new StringBuilder.AppendInterpolatedStringHandler(3, 3, stringBuilder4);
			handler2.AppendLiteral("[");
			handler2.AppendFormatted(ok ? "OK  " : "FALHA");
			handler2.AppendLiteral("] ");
			handler2.AppendFormatted(name);
			handler2.AppendFormatted((detail == null) ? "" : (" — " + detail));
			stringBuilder4.AppendLine(ref handler2);
		}
	}
}
