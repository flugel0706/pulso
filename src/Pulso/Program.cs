using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace Pulso;

internal static class Program
{
	public const string Version = "3.1.0";

	public const string ShortVersion = "v3.1";

	[STAThread]
	private static int Main(string[] args)
	{
		bool flag = Environment.GetEnvironmentVariable("PULSO_PROBE") == "1";
		if (args.Length != 0 || flag)
		{
			AttachConsoleOutput();
		}
		Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		ToolStripManager.Renderer = new DarkRenderer();
		Application.ThreadException += delegate(object _, ThreadExceptionEventArgs e)
		{
			AppLog.Write("Erro não tratado (UI): " + e.Exception);
			try
			{
				MessageBox.Show(e.Exception.Message, "Pulso — erro", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			catch
			{
			}
		};
		AppDomain.CurrentDomain.UnhandledException += delegate(object _, UnhandledExceptionEventArgs e)
		{
			AppLog.Write("Erro não tratado: " + e.ExceptionObject);
		};
		SelfExe.Trailer trailer = SafeReadTrailer();
		CompiledPayload compiledPayload = (((object)trailer == null) ? null : TryParsePayload(trailer.Payload));
		if (flag)
		{
			WriteReport("probe.txt", ProbeReport(trailer, compiledPayload, args), alsoCwd: true);
			return 0;
		}
		if (args.Length != 0)
		{
			string text = args[0].TrimStart('-', '/').ToLowerInvariant();
			if (text != null)
			{
				switch (text.Length)
				{
				case 7:
				{
					char c = text[0];
					if (c != 'c')
					{
						if (c != 'v' || !(text == "version"))
						{
							break;
						}
						goto IL_0207;
					}
					if (!(text == "compile"))
					{
						break;
					}
					if (args.Length < 3)
					{
						WriteReport("compile.txt", "uso: --compile <entrada.gmt|.rec> <saida.exe>");
						return 2;
					}
					return Compile(args[1], args[2]);
				}
				case 1:
				{
					char c = text[0];
					if (c != '?' && c != 'h')
					{
						if (c != 'v')
						{
							break;
						}
						goto IL_0207;
					}
					goto IL_0219;
				}
				case 4:
				{
					char c = text[0];
					if (c != 'd')
					{
						if (c != 'h')
						{
							if (c != 'p' || !(text == "play"))
							{
								break;
							}
							if (args.Length < 2)
							{
								WriteReport("play.txt", "uso: --play <arquivo.gmt|.rec>");
								return 2;
							}
							try
							{
								Application.Run(new PlayerForm(args[1].EndsWith(".rec", StringComparison.OrdinalIgnoreCase) ? Macro.ImportRec(args[1]) : Macro.Load(args[1]), Settings.Load()));
								return 0;
							}
							catch (Exception ex)
							{
								WriteReport("play.txt", "erro: " + ex.Message);
								return 1;
							}
						}
						if (!(text == "help"))
						{
							break;
						}
						goto IL_0219;
					}
					if (!(text == "demo"))
					{
						break;
					}
					if (args.Length < 2)
					{
						WriteReport("demo.txt", "uso: --demo <arquivo.gmt>");
						return 2;
					}
					Macro macro = BuildDemo();
					macro.Save(args[1]);
					WriteReport("demo.txt", $"Macro de demonstração criada em {args[1]} ({macro.Events.Count} eventos).");
					return 0;
				}
				case 8:
					if (!(text == "selftest"))
					{
						break;
					}
					var (flag2, text2) = Selftest.Run();
					WriteReport("selftest.txt", text2);
					return (!flag2) ? 1 : 0;
				case 5:
					{
						if (!(text == "smoke"))
						{
							break;
						}
						return Smoke();
					}
					IL_0207:
					WriteReport("version.txt", "Pulso 3.1.0");
					return 0;
					IL_0219:
					WriteReport("help.txt", Usage());
					return 0;
				}
			}
		}
		if (compiledPayload != null)
		{
			Macro macro2 = compiledPayload.Macro ?? new Macro();
			Settings settings = compiledPayload.Play ?? Settings.Load();
			Application.Run(new PlayerForm(macro2, settings));
			return 0;
		}
		if (args.Length != 0 && File.Exists(args[0]))
		{
			try
			{
				MainForm mainForm = new MainForm(Settings.Load());
				mainForm.OpenFile(args[0]);
				Application.Run(mainForm);
				return 0;
			}
			catch (Exception ex2)
			{
				AppLog.Write("Falha ao abrir macro: " + ex2);
				WriteReport("open.txt", "erro ao abrir macro: " + ex2.Message);
			}
		}
		Application.Run(new MainForm(Settings.Load()));
		return 0;
	}

	private static void AttachConsoleOutput()
	{
		try
		{
			Native.AttachConsole(-1);
			Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
			{
				AutoFlush = true
			});
		}
		catch
		{
		}
	}

	private static void WriteReport(string name, string text, bool alsoCwd = false)
	{
		try
		{
			File.WriteAllText(Path.Combine(AppContext.BaseDirectory, name), text, Encoding.UTF8);
		}
		catch
		{
		}
		if (alsoCwd)
		{
			try
			{
				File.WriteAllText(Path.Combine(Environment.CurrentDirectory, name), text, Encoding.UTF8);
			}
			catch
			{
			}
		}
		try
		{
			Console.WriteLine(text);
		}
		catch
		{
		}
	}

	private static string Usage()
	{
		return "Pulso 3.1.0\r\n\r\nUso:\r\n  Pulso.exe                       abre o estúdio\r\n  Pulso.exe --version             mostra a versão\r\n  Pulso.exe --selftest            executa os testes internos (selftest.txt)\r\n  Pulso.exe --smoke               teste rápido de criação das janelas\r\n  Pulso.exe --demo <arquivo.gmt>  gera uma automação de demonstração\r\n  Pulso.exe --compile <in> <out>  compila uma macro em executável\r\n  Pulso.exe --play <arquivo>      reproduz uma macro sem abrir a interface\r\n  Pulso.exe <arquivo.gmt|.rec>    abre uma macro direto na linha do tempo\r\n\r\nUm executável com macro embutida reproduz sozinho ao ser aberto (F12 encerra).\r\n";
	}

	private static SelfExe.Trailer? SafeReadTrailer()
	{
		try
		{
			return SelfExe.ReadTrailer(SelfExe.CurrentExe);
		}
		catch (Exception ex)
		{
			AppLog.Write("Falha ao ler trailer: " + ex.Message);
			return null;
		}
	}

	private static CompiledPayload? TryParsePayload(string json)
	{
		try
		{
			CompiledPayload compiledPayload = JsonSerializer.Deserialize<CompiledPayload>(json, Macro.JsonOpts);
			if (compiledPayload?.Macro == null)
			{
				return null;
			}
			Macro macro = compiledPayload.Macro;
			if (macro.Events == null)
			{
				List<MacroEvent> list2 = (macro.Events = new List<MacroEvent>());
			}
			compiledPayload.Macro.Events.Sort((MacroEvent a, MacroEvent b) => a.T.CompareTo(b.T));
			return compiledPayload;
		}
		catch (Exception ex)
		{
			AppLog.Write("Payload compilado inválido: " + ex.Message);
			return null;
		}
	}

	private static string ProbeReport(SelfExe.Trailer? tr, CompiledPayload? p, string[] args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Pulso probe");
		stringBuilder.AppendLine("version=3.1.0");
		stringBuilder.AppendLine("exe=" + SelfExe.CurrentExe);
		try
		{
			stringBuilder.AppendLine("size=" + new FileInfo(SelfExe.CurrentExe).Length);
		}
		catch
		{
		}
		stringBuilder.AppendLine("trailer=" + (((object)tr == null) ? "none" : "found"));
		if ((object)tr != null)
		{
			stringBuilder.AppendLine("payloadBytes=" + Encoding.UTF8.GetByteCount(tr.Payload));
			stringBuilder.AppendLine("dataEnd=" + tr.DataEnd);
		}
		stringBuilder.AppendLine("compiled=" + ((p == null) ? "no" : "yes"));
		if (p != null)
		{
			stringBuilder.AppendLine("events=" + p.Macro.Events.Count);
			stringBuilder.AppendLine("durationMs=" + p.Macro.Duration);
			stringBuilder.AppendLine("loops=" + p.Play.Loops + " continuous=" + p.Play.Continuous);
			stringBuilder.AppendLine("speed=" + p.Play.SpeedPercent);
		}
		stringBuilder.AppendLine("args=" + string.Join(" ", args));
		stringBuilder.AppendLine("process=" + Process.GetCurrentProcess().Id);
		return stringBuilder.ToString();
	}

	private static Macro BuildDemo()
	{
		Macro i = new Macro
		{
			ScreenW = Native.GetSystemMetrics(0),
			ScreenH = Native.GetSystemMetrics(1)
		};
		int num = i.ScreenW / 2;
		int y = i.ScreenH / 2;
		long t = 0L;
		Add(new MacroEvent
		{
			Kind = EvKind.MouseMove,
			X = num - 220,
			Y = y
		}, 0L);
		Add(new MacroEvent
		{
			Kind = EvKind.MouseMove,
			X = num + 220,
			Y = y
		}, 500L);
		Add(new MacroEvent
		{
			Kind = EvKind.MouseDown,
			Button = 0,
			X = num + 220,
			Y = y
		}, 120L);
		Add(new MacroEvent
		{
			Kind = EvKind.MouseUp,
			Button = 0,
			X = num + 220,
			Y = y
		}, 80L);
		string text = "pulso";
		for (int j = 0; j < text.Length; j++)
		{
			int num2 = char.ToUpperInvariant(text[j]);
			int scan = (int)(Native.MapVirtualKey((uint)num2, 0u) & 0xFF);
			Add(new MacroEvent
			{
				Kind = EvKind.KeyDown,
				Vk = num2,
				Scan = scan
			}, 70L);
			Add(new MacroEvent
			{
				Kind = EvKind.KeyUp,
				Vk = num2,
				Scan = scan
			}, 45L);
		}
		return i;
		void Add(MacroEvent e, long dt)
		{
			t += dt;
			e.T = t;
			i.Events.Add(e);
		}
	}

	private static int Compile(string input, string output)
	{
		try
		{
			Macro macro = (input.EndsWith(".rec", StringComparison.OrdinalIgnoreCase) ? Macro.ImportRec(input) : Macro.Load(input));
			string payload = JsonSerializer.Serialize(new CompiledPayload
			{
				Macro = macro,
				Play = Settings.Load()
			}, Macro.JsonOpts);
			SelfExe.CopySelfWithoutTrailer(output);
			SelfExe.AppendTrailer(output, payload);
			SelfExe.ZeroPeChecksum(output);
			long length = new FileInfo(output).Length;
			string text = $"Executável gerado: {output}\r\nEventos: {macro.Events.Count}\r\nTamanho: {length:N0} bytes";
			AppLog.Write(text.Replace("\r\n", " | "));
			WriteReport("compile.txt", text);
			return 0;
		}
		catch (Exception ex)
		{
			WriteReport("compile.txt", "erro: " + ex);
			return 1;
		}
	}

	private static int Smoke()
	{
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			Settings settings = Settings.Load();
			using (MainForm mainForm = new MainForm(settings))
			{
				mainForm.CreateControl();
				stringBuilder.AppendLine("MainForm: " + mainForm.Text);
				stringBuilder.AppendLine("handle=0x" + ((IntPtr)mainForm.Handle).ToString("X"));
				stringBuilder.AppendLine("clientSize=" + mainForm.ClientSize.Width + "x" + mainForm.ClientSize.Height);
				stringBuilder.AppendLine("controls=" + mainForm.Controls.Count);
				stringBuilder.AppendLine("gridColumns=" + FindGrid(mainForm)?.ColumnCount);
				stringBuilder.AppendLine("icon=" + ((mainForm.Icon == null) ? "null" : (mainForm.Icon.Width + "x" + mainForm.Icon.Height)));
				stringBuilder.AppendLine("iconSmall=" + AppIcon.Small.Width + " app=" + AppIcon.App.Width);
				mainForm.Close();
			}
			using (PlayerForm playerForm = new PlayerForm(new Macro(), settings))
			{
				playerForm.CreateControl();
				stringBuilder.AppendLine("PlayerForm: handle=0x" + ((IntPtr)playerForm.Handle).ToString("X"));
				playerForm.Dispose();
			}
			stringBuilder.AppendLine("resultado=ok");
			WriteReport("smoke.txt", stringBuilder.ToString());
			return 0;
		}
		catch (Exception ex)
		{
			stringBuilder.AppendLine("resultado=erro: " + ex);
			WriteReport("smoke.txt", stringBuilder.ToString());
			return 1;
		}
	}

	private static DataGridView? FindGrid(Control root)
	{
		foreach (Control control in root.Controls)
		{
			if (control is DataGridView result)
			{
				return result;
			}
			DataGridView dataGridView = FindGrid(control);
			if (dataGridView != null)
			{
				return dataGridView;
			}
		}
		return null;
	}
}
