using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Pulso;

public sealed class MainForm : Form
{
	private Settings _s;

	private readonly InputEngine _engine = new InputEngine();

	private readonly Player _player = new Player();

	private Macro _macro = new Macro();

	private string? _currentPath;

	private string? _readyName;

	private readonly List<string> _undo = new List<string>();

	private readonly List<string> _redo = new List<string>();

	private const int UndoLimit = 40;

	private bool _dirty;

	private bool _recording;

	private bool _playing;

	private bool _suspendHotkeys;

	private readonly HashSet<int> _down = new HashSet<int>();

	private DateTime _recStart;

	private readonly HudGrid _grid = new HudGrid();

	private readonly HudScrollBar _vbar = new HudScrollBar();

	private readonly BackdropPanel _chrome = new BackdropPanel
	{
		Dim = 0.12f
	};

	private readonly HeroStrip _hero = new HeroStrip();

	private readonly BrandMark _brandMark = new BrandMark();

	private readonly Label _brandTitle = new Label();

	private readonly Label _brandSub = new Label();

	private readonly Label _lblChromeFile = new Label();

	private readonly HudButton _btnMin = new HudButton
	{
		Icon = ButtonIcon.Minimize,
		Kind = HudKind.Bare
	};

	private readonly HudButton _btnMax = new HudButton
	{
		Icon = ButtonIcon.Maximize,
		Kind = HudKind.Bare
	};

	private readonly HudButton _btnClose = new HudButton
	{
		Icon = ButtonIcon.Close,
		Kind = HudKind.Bare,
		Accent = Palette.Danger
	};

	private Rectangle _maxRestore = Rectangle.Empty;

	private bool _maxed;

	private readonly BackdropPanel _body = new BackdropPanel
	{
		Dim = 0.24f
	};

	private readonly BackdropPanel _sidebar = new BackdropPanel
	{
		Dim = 0.28f
	};

	private readonly CardPanel _sideCard = new CardPanel();

	private readonly BackdropPanel _work = new BackdropPanel
	{
		Dim = 0.24f
	};

	private readonly BackdropPanel _workTop = new BackdropPanel
	{
		Dim = 0.1f
	};

	private readonly CardPanel _gridCard = new CardPanel();

	private readonly Label _lblWorkFile = new Label();

	private readonly Label _lblStatusNote = new Label();

	private readonly StatusDot _statusDot = new StatusDot();

	private readonly Label _lblStatusSub = new Label();

	private readonly BackdropPanel _bottom = new BackdropPanel
	{
		Dim = 0.18f
	};

	private readonly Label _lblState = new Label();

	private readonly Label _lblCount = new Label();

	private readonly Label _lblDur = new Label();

	private readonly Label _lblSpeedVal = new Label();

	private readonly Label _lblLoopsVal = new Label();

	private readonly HudButton _btnRecord = new HudButton
	{
		Kind = HudKind.Danger,
		Accent = Palette.Danger,
		Text = "Gravar",
		Icon = ButtonIcon.Record,
		LeftAlign = true
	};

	private readonly HudButton _btnPlay = new HudButton
	{
		Kind = HudKind.Primary,
		Accent = Palette.Accent,
		Text = "Reproduzir",
		Icon = ButtonIcon.Play,
		LeftAlign = true
	};

	private readonly HudButton _btnPause = new HudButton
	{
		Kind = HudKind.Ghost,
		Accent = Palette.Accent,
		Text = "Pausar",
		Icon = ButtonIcon.Pause,
		LeftAlign = true
	};

	private readonly HudButton _btnStop = new HudButton
	{
		Kind = HudKind.Ghost,
		Accent = Palette.Danger,
		Text = "Parar",
		Icon = ButtonIcon.Stop,
		LeftAlign = true
	};

	private readonly HudButton _btnInf = new HudButton
	{
		Kind = HudKind.Ghost,
		Accent = Palette.Accent,
		Icon = ButtonIcon.Infinity,
		TextVisible = false
	};

	private readonly NotifyIcon _tray = new NotifyIcon();

	private readonly ToolTip _tips = new ToolTip
	{
		InitialDelay = 220,
		ReshowDelay = 80,
		AutoPopDelay = 7000
	};

	private readonly ToolStripMenuItem _miSave = new ToolStripMenuItem();

	private readonly ToolStripMenuItem _miExportRec = new ToolStripMenuItem();

	private readonly ToolStripMenuItem _miUndo = new ToolStripMenuItem();

	private readonly ToolStripMenuItem _miRedo = new ToolStripMenuItem();

	private readonly ToolStripMenuItem _miRecent = new ToolStripMenuItem();

	private readonly ToolStripMenuItem _miCompact = new ToolStripMenuItem();

	private readonly ToolStripMenuItem _miTopMost = new ToolStripMenuItem();

	private MenuStrip _menu;

	private ToolStripMenuItem? _miMiniLoop;

	private ToolStripMenuItem? _miMiniTop;

	private MiniBar? _miniBar;

	private ContextMenuStrip? _miniMenu;

	private bool _compactFromMax;

	private readonly Stopwatch _playWatch = new Stopwatch();

	private double _playTotalMs;

	private bool _compact;

	private Rectangle _normalBounds = Rectangle.Empty;

	private readonly Timer _timer = new Timer
	{
		Interval = 120
	};

	private int _lastRowCount = -1;

	private const int WM_NCHITTEST = 132;

	private readonly List<Panel> _grips = new List<Panel>();

	private bool _fullScreen;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ClassStyle |= 131072;
			return createParams;
		}
	}

	public MainForm(Settings s, Macro? preloaded = null, bool autoPlay = false, string? path = null)
	{
		_s = s;
		_macro = preloaded ?? new Macro();
		_currentPath = path;
		_dirty = preloaded != null && path == null;
		Text = "Pulso — Estúdio de Automação";
		base.StartPosition = FormStartPosition.CenterScreen;
		base.ClientSize = new Size(1160, 760);
		MinimumSize = new Size(960, 580);
		base.AutoScaleMode = AutoScaleMode.Dpi;
		Font = UiFont.Body;
		BackColor = Palette.Bg;
		ForeColor = Palette.Text;
		base.KeyPreview = true;
		AllowDrop = true;
		base.TopMost = _s.TopMost;
		base.Icon = AppIcon.App;
		DoubleBuffered = true;
		BuildUi();
		WireGrid();
		WireTray();
		ApplySettingsToUi();
		RefreshTitle();
		UpdateStateUi();
		base.DragEnter += OnDragEnter;
		base.DragDrop += OnDragDrop;
		if (!_engine.Install())
		{
			MessageBox.Show(this, "Não foi possível instalar os ganchos globais de teclado/mouse.\r\n" + _engine.LastError, "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
		_engine.OwnWindow = base.Handle;
		_engine.OwnProcessId = Environment.ProcessId;
		_engine.KeyActivity += OnKeyActivity;
		ApplyRecordingFlags();
		base.Shown += delegate
		{
			DarkUi.DarkTitle(this);
			DarkUi.DarkScrollbars(_grid);
			DarkUi.DarkScrollbars(_sideCard);
		};
		_player.Status += delegate(string st)
		{
			string st2 = st;
			Ui(delegate
			{
				_lblState.Text = st2.ToUpperInvariant();
				SyncMini();
			});
		};
		_player.LoopProgress += delegate(int loop, int total)
		{
			Ui(delegate
			{
				if (_s.Continuous && _playing)
				{
					_playWatch.Restart();
				}
				_lblState.Text = ((total > 0) ? $"Reproduzindo {loop}/{total}" : $"Reproduzindo {loop}");
				SyncMini();
			});
		};
		_player.Finished += delegate
		{
			Ui(OnPlaybackFinished);
		};
		_timer.Tick += delegate
		{
			OnTimerTick();
		};
		_timer.Start();
		if (autoPlay && !_macro.IsEmpty)
		{
			BeginInvoke(StartPlayback);
		}
	}

	private void BuildUi()
	{
		base.FormBorderStyle = FormBorderStyle.None;
		ToolStripManager.Renderer = new DarkRenderer();
		BuildChrome();
		BuildMenu();
		BuildHero();
		BuildBody();
		BuildBottomBar();
		BuildMini();
		BuildGrips();
		base.Controls.Add(_body);
		base.Controls.Add(_bottom);
		base.Controls.Add(_hero);
		base.Controls.Add(_menu);
		base.Controls.Add(_chrome);
		Theme.Glass(_chrome);
		Theme.Glass(_workTop);
		Theme.Glass(_bottom);
		base.Shown += delegate
		{
			ModernWindow.Apply(this, rounded: true);
		};
	}

	private void BuildMenu()
	{
		_menu = new MenuStrip
		{
			Font = UiFont.Body,
			BackColor = Palette.Bg2,
			ForeColor = Palette.TextSoft,
			Renderer = new DarkRenderer(),
			Padding = new Padding(18, 2, 0, 2),
			AutoSize = false,
			Height = 32,
			Dock = DockStyle.Top
		};
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("&Arquivo");
		toolStripMenuItem.DropDownItems.Add(Item("&Nova macro", Keys.N | Keys.Control, delegate
		{
			NewMacro();
		}));
		toolStripMenuItem.DropDownItems.Add(Item("&Abrir…", Keys.O | Keys.Control, OpenMacro));
		_miRecent.Text = "Abrir &recente";
		_miRecent.DropDownOpening += delegate
		{
			BuildRecentMenu();
		};
		toolStripMenuItem.DropDownItems.Add(_miRecent);
		_miSave.Text = "&Salvar";
		_miSave.ShortcutKeys = Keys.S | Keys.Control;
		_miSave.Click += delegate
		{
			SaveMacro(saveAs: false);
		};
		toolStripMenuItem.DropDownItems.Add(_miSave);
		toolStripMenuItem.DropDownItems.Add(Item("Salvar &como…", Keys.S | Keys.Shift | Keys.Control, delegate
		{
			SaveMacro(saveAs: true);
		}));
		toolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
		toolStripMenuItem.DropDownItems.Add(Item("&Importar do TinyTask (.rec)…", Keys.None, ImportRec));
		_miExportRec.Text = "&Exportar para TinyTask (.rec)…";
		_miExportRec.Click += delegate
		{
			ExportRec();
		};
		toolStripMenuItem.DropDownItems.Add(_miExportRec);
		toolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
		toolStripMenuItem.DropDownItems.Add(Item("&Exportar macro pronta…", Keys.E | Keys.Control, ExportPackage));
		toolStripMenuItem.DropDownItems.Add(Item("&Carregar macro pronta…", Keys.O | Keys.Shift | Keys.Control, OpenReadyMacro));
		toolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
		toolStripMenuItem.DropDownItems.Add(Item("Sai&r", Keys.F4 | Keys.Alt, base.Close));
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem("&Editar");
		_miUndo.Text = "&Desfazer";
		_miUndo.ShortcutKeys = Keys.Z | Keys.Control;
		_miUndo.Click += delegate
		{
			Undo();
		};
		toolStripMenuItem2.DropDownItems.Add(_miUndo);
		_miRedo.Text = "&Refazer";
		_miRedo.ShortcutKeys = Keys.Y | Keys.Control;
		_miRedo.Click += delegate
		{
			Redo();
		};
		toolStripMenuItem2.DropDownItems.Add(_miRedo);
		toolStripMenuItem2.DropDownItems.Add(new ToolStripSeparator());
		toolStripMenuItem2.DropDownItems.Add(Item("&Remover selecionados", Keys.Delete, RemoveSelected));
		toolStripMenuItem2.DropDownItems.Add(Item("&Editar evento…", Keys.None, EditSelected));
		toolStripMenuItem2.DropDownItems.Add(Item("&Duplicar selecionados", Keys.D | Keys.Control, DuplicateSelected));
		toolStripMenuItem2.DropDownItems.Add(Item("Inserir &pausa a partir daqui…", Keys.None, InsertPause));
		toolStripMenuItem2.DropDownItems.Add(new ToolStripSeparator());
		toolStripMenuItem2.DropDownItems.Add(Item("Ajustar &escala de tempo…", Keys.None, ScaleTimes));
		toolStripMenuItem2.DropDownItems.Add(Item("Remover espera &inicial", Keys.None, RemoveInitialDelay));
		toolStripMenuItem2.DropDownItems.Add(new ToolStripSeparator());
		toolStripMenuItem2.DropDownItems.Add(Item("Apagar &tudo (limpar macro)", Keys.Delete | Keys.Control, ClearMacro));
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem("&Reproduzir");
		toolStripMenuItem3.DropDownItems.Add(Item("&Gravar / parar gravação", Keys.F9, delegate
		{
			ToggleRecording();
		}));
		toolStripMenuItem3.DropDownItems.Add(Item("&Reproduzir", Keys.F5, StartPlayback));
		toolStripMenuItem3.DropDownItems.Add(Item("&Pausar / continuar", Keys.None, TogglePause));
		toolStripMenuItem3.DropDownItems.Add(Item("P&arar tudo (F12)", Keys.None, EmergencyStop));
		ToolStripMenuItem toolStripMenuItem4 = new ToolStripMenuItem("E&xibir");
		_miCompact.Text = "Modo &compacto";
		_miCompact.ShortcutKeys = Keys.M | Keys.Control | Keys.Alt;
		_miCompact.CheckOnClick = true;
		_miCompact.CheckedChanged += delegate
		{
			SetCompact(_miCompact.Checked);
		};
		toolStripMenuItem4.DropDownItems.Add(_miCompact);
		_miTopMost.Text = "Sempre no &topo";
		_miTopMost.CheckOnClick = true;
		_miTopMost.Checked = _s.TopMost;
		_miTopMost.CheckedChanged += delegate
		{
			_s.TopMost = _miTopMost.Checked;
			if (!_compact)
			{
				base.TopMost = _s.TopMost;
			}
		};
		toolStripMenuItem4.DropDownItems.Add(_miTopMost);
		toolStripMenuItem4.DropDownItems.Add(new ToolStripSeparator());
		toolStripMenuItem4.DropDownItems.Add(Item("Abrir pasta de &macros", Keys.None, delegate
		{
			OpenFolder(Paths.MacrosDir);
		}));
		toolStripMenuItem4.DropDownItems.Add(Item("Tela &cheia", Keys.F11, ToggleFullScreen));
		ToolStripMenuItem toolStripMenuItem5 = new ToolStripMenuItem("&Ferramentas");
		toolStripMenuItem5.DropDownItems.Add(Item("&Configurações…", Keys.Oemcomma | Keys.Control, OpenSettings));
		toolStripMenuItem5.DropDownItems.Add(new ToolStripSeparator());
		toolStripMenuItem5.DropDownItems.Add(Item("Abrir pasta de &dados", Keys.None, delegate
		{
			OpenFolder(Paths.BaseDir);
		}));
		toolStripMenuItem5.DropDownItems.Add(Item("Ver &log…", Keys.None, ShowLog));
		ToolStripMenuItem toolStripMenuItem6 = new ToolStripMenuItem("A&juda");
		toolStripMenuItem6.DropDownItems.Add(Item("&Atalhos configurados", Keys.F1, ShowHotkeys));
		toolStripMenuItem6.DropDownItems.Add(Item("&Sobre o Pulso", Keys.None, ShowAbout));
		_menu.Items.AddRange(new ToolStripItem[6] { toolStripMenuItem, toolStripMenuItem2, toolStripMenuItem3, toolStripMenuItem4, toolStripMenuItem5, toolStripMenuItem6 });
		base.MainMenuStrip = _menu;
	}

	private void BuildHero()
	{
		_hero.Dock = DockStyle.Top;
		_hero.Height = 96;
		_hero.Callsign = "macro sem nome";
	}

	private void BuildBody()
	{
		_body.Dock = DockStyle.Fill;
		_body.BackColor = Palette.Bg;
		_body.Padding = new Padding(16, 10, 16, 14);
		_sidebar.Dock = DockStyle.Left;
		_sidebar.Width = 306;
		_sidebar.BackColor = Palette.Bg;
		_sidebar.Padding = new Padding(0, 0, 16, 0);
		_work.Dock = DockStyle.Fill;
		_work.BackColor = Palette.Bg;
		_work.Padding = new Padding(4, 0, 0, 0);
		BuildSidebar();
		BuildWorkspace();
		_body.Controls.Add(_work);
		_body.Controls.Add(_sidebar);
	}

	private void BuildSidebar()
	{
		_sideCard.Dock = DockStyle.Fill;
		_sideCard.Radius = 4;
		_sideCard.Brackets = true;
		_sideCard.Fill = Palette.Panel;
		_sideCard.BorderColor = Palette.BorderSoft;
		_sideCard.Padding = new Padding(18, 14, 14, 12);
		FlowLayoutPanel col = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			BackColor = Palette.Panel,
			Padding = new Padding(0),
			AutoScroll = true,
			Margin = new Padding(0)
		};
		DarkUi.DarkScroll(col);
		Add(Section("CENTRAL DE OPERAÇÕES"), 18, 4);
		Panel panel = new Panel
		{
			Width = 236,
			Height = 50,
			BackColor = Palette.Panel
		};
		_statusDot.Location = new Point(0, 12);
		_statusDot.Size = new Size(14, 14);
		_statusDot.On = false;
		_lblState.AutoSize = false;
		_lblState.Location = new Point(20, 1);
		_lblState.Size = new Size(214, 26);
		_lblState.Font = UiFont.Title;
		_lblState.ForeColor = Palette.Text;
		_lblState.BackColor = Palette.Panel;
		_lblState.TextAlign = ContentAlignment.MiddleLeft;
		_lblState.Text = "Pronto";
		_lblStatusSub.AutoSize = false;
		_lblStatusSub.Location = new Point(21, 27);
		_lblStatusSub.Size = new Size(214, 18);
		_lblStatusSub.Font = UiFont.Tiny;
		_lblStatusSub.ForeColor = Palette.Muted;
		_lblStatusSub.BackColor = Palette.Panel;
		_lblStatusSub.Text = "Aguardando uma ação";
		panel.Controls.Add(_statusDot);
		panel.Controls.Add(_lblState);
		panel.Controls.Add(_lblStatusSub);
		Add(panel, 48, 6);
		Panel panel2 = new Panel
		{
			Width = 236,
			Height = 46,
			BackColor = Palette.Panel
		};
		Label value = new Label
		{
			Text = "EVENTOS",
			AutoSize = false,
			Location = new Point(0, 0),
			Size = new Size(92, 16),
			Font = UiFont.Tiny,
			ForeColor = Palette.Muted,
			BackColor = Palette.Panel
		};
		_lblCount.AutoSize = false;
		_lblCount.Location = new Point(0, 17);
		_lblCount.Size = new Size(92, 22);
		_lblCount.Font = UiFont.Head;
		_lblCount.ForeColor = Palette.Text;
		_lblCount.BackColor = Palette.Panel;
		_lblCount.Text = "0";
		Label value2 = new Label
		{
			Text = "DURAÇÃO",
			AutoSize = false,
			Location = new Point(126, 0),
			Size = new Size(106, 16),
			Font = UiFont.Tiny,
			ForeColor = Palette.Muted,
			BackColor = Palette.Panel
		};
		_lblDur.AutoSize = false;
		_lblDur.Location = new Point(126, 17);
		_lblDur.Size = new Size(106, 22);
		_lblDur.Font = UiFont.Head;
		_lblDur.ForeColor = Palette.Text;
		_lblDur.BackColor = Palette.Panel;
		_lblDur.Text = "0,0 s";
		panel2.Controls.Add(value);
		panel2.Controls.Add(_lblCount);
		panel2.Controls.Add(value2);
		panel2.Controls.Add(_lblDur);
		Add(panel2, 44);
		Add(_btnRecord, 42, 6);
		Add(_btnPlay, 42, 6);
		Panel panel3 = new Panel
		{
			Width = 236,
			Height = 40,
			BackColor = Palette.Panel
		};
		_btnPause.Bounds = new Rectangle(0, 0, 114, 40);
		_btnStop.Bounds = new Rectangle(122, 0, 114, 40);
		panel3.Controls.Add(_btnPause);
		panel3.Controls.Add(_btnStop);
		Add(panel3, 40, 10);
		Add(Section("ARQUIVO"), 18, 2);
		Panel panel4 = new Panel
		{
			Width = 236,
			Height = 119,
			BackColor = Palette.Panel
		};
		HudButton hudButton = new HudButton
		{
			Text = "Abrir",
			Icon = ButtonIcon.Folder,
			Kind = HudKind.Ghost,
			LeftAlign = true,
			Radius = 8
		};
		hudButton.Bounds = new Rectangle(0, 0, 114, 35);
		hudButton.Click += delegate
		{
			OpenMacro();
		};
		_tips.SetToolTip(hudButton, "Abrir macro (.gmt / .rec / .pulso)");
		HudButton hudButton2 = new HudButton
		{
			Text = "Salvar",
			Icon = ButtonIcon.Save,
			Kind = HudKind.Ghost,
			LeftAlign = true,
			Radius = 8
		};
		hudButton2.Bounds = new Rectangle(122, 0, 114, 35);
		hudButton2.Click += delegate
		{
			SaveMacro(saveAs: false);
		};
		_tips.SetToolTip(hudButton2, "Salvar macro (.gmt)");
		HudButton hudButton3 = new HudButton
		{
			Text = "Exportar",
			Icon = ButtonIcon.Export,
			Kind = HudKind.Outline,
			Accent = Palette.Accent,
			LeftAlign = true,
			Radius = 8
		};
		hudButton3.Bounds = new Rectangle(0, 42, 114, 35);
		hudButton3.Click += delegate
		{
			ExportPackage();
		};
		_tips.SetToolTip(hudButton3, "Exportar macro pronta (.pulso) — Ctrl+E");
		HudButton hudButton4 = new HudButton
		{
			Text = "Mini",
			Icon = ButtonIcon.Expand,
			Kind = HudKind.Ghost,
			LeftAlign = true,
			Radius = 8
		};
		hudButton4.Bounds = new Rectangle(122, 42, 114, 35);
		hudButton4.Click += delegate
		{
			SetCompact(on: true);
		};
		_tips.SetToolTip(hudButton4, "Modo compacto / barra (" + _s.HotkeyCompact + ")");
		HudButton hudButton5 = new HudButton
		{
			Text = "Apagar tudo",
			Icon = ButtonIcon.Trash,
			Kind = HudKind.Outline,
			Accent = Palette.Danger,
			LeftAlign = true,
			Radius = 8
		};
		hudButton5.Bounds = new Rectangle(0, 84, 236, 35);
		hudButton5.Click += delegate
		{
			ClearMacro();
		};
		_tips.SetToolTip(hudButton5, "Apagar todos os eventos da macro (Ctrl+Del)");
		panel4.Controls.Add(hudButton);
		panel4.Controls.Add(hudButton2);
		panel4.Controls.Add(hudButton3);
		panel4.Controls.Add(hudButton4);
		panel4.Controls.Add(hudButton5);
		Add(panel4, 119, 10);
		Label hint = new Label
		{
			Text = "F1 abre a lista completa de atalhos",
			Width = 236,
			Height = 16,
			Font = UiFont.Tiny,
			ForeColor = Palette.Muted,
			BackColor = Palette.Panel,
			Margin = new Padding(0)
		};
		col.Controls.Add(hint);
		Label footer = new Label
		{
			Text = "F12  //  PARADA DE EMERGÊNCIA",
			Dock = DockStyle.Bottom,
			Height = 24,
			Font = UiFont.TinyBold,
			ForeColor = Palette.Danger,
			BackColor = Palette.Panel,
			TextAlign = ContentAlignment.MiddleLeft,
			Padding = new Padding(2, 0, 0, 6)
		};
		_sideCard.Controls.Add(footer);
		_sideCard.Controls.Add(col);
		_sidebar.Controls.Add(_sideCard);
		void Add(Control c, int height, int marginBottom = 8)
		{
			c.Width = 236;
			c.Height = height;
			c.Margin = new Padding(0, 0, 0, marginBottom);
			col.Controls.Add(c);
		}
		static Label Section(string text)
		{
			return new Label
			{
				Text = "// " + text,
				Width = 236,
				Height = 18,
				Font = UiFont.TinyBold,
				ForeColor = Palette.Alpha(Palette.Accent, 200),
				BackColor = Palette.Panel,
				Margin = new Padding(0, 0, 0, 8),
				TextAlign = ContentAlignment.MiddleLeft
			};
		}
	}

	private void BuildWorkspace()
	{
		_workTop.Dock = DockStyle.Top;
		_workTop.Height = 58;
		_workTop.BackColor = Palette.Bg;
		_workTop.Padding = new Padding(4, 2, 0, 6);
		Label value = new Label
		{
			Text = "// LINHA DO TEMPO",
			AutoSize = true,
			Location = new Point(6, 2),
			Font = UiFont.Display,
			ForeColor = Palette.Text,
			BackColor = Palette.Bg
		};
		_lblWorkFile.AutoSize = false;
		_lblWorkFile.Location = new Point(8, 33);
		_lblWorkFile.Size = new Size(360, 18);
		_lblWorkFile.Font = UiFont.MonoSmall;
		_lblWorkFile.ForeColor = Palette.Muted;
		_lblWorkFile.BackColor = Palette.Bg;
		_lblWorkFile.Text = "macro sem nome";
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Right,
			Width = 430,
			Height = 44,
			FlowDirection = FlowDirection.RightToLeft,
			WrapContents = false,
			BackColor = Palette.Bg,
			Padding = new Padding(0, 8, 0, 0)
		};
		HudButton hudButton = new HudButton
		{
			Icon = ButtonIcon.Settings,
			Kind = HudKind.Bare,
			Width = 38,
			Height = 36,
			Radius = 8
		};
		hudButton.Margin = new Padding(8, 0, 0, 0);
		hudButton.Click += delegate
		{
			OpenSettings();
		};
		_tips.SetToolTip(hudButton, "Configurações e atalhos");
		HudButton hudButton2 = new HudButton
		{
			Icon = ButtonIcon.Trash,
			Kind = HudKind.Bare,
			Width = 38,
			Height = 36,
			Radius = 8
		};
		hudButton2.Margin = new Padding(6, 0, 0, 0);
		hudButton2.Click += delegate
		{
			ClearMacro();
		};
		_tips.SetToolTip(hudButton2, "Apagar tudo (Ctrl+Del)");
		HudButton hudButton3 = new HudButton
		{
			Text = "Exportar",
			Icon = ButtonIcon.Export,
			Kind = HudKind.Outline,
			Accent = Palette.Accent,
			Width = 118,
			Height = 36,
			Radius = 8
		};
		hudButton3.Margin = new Padding(6, 0, 0, 0);
		hudButton3.Click += delegate
		{
			ExportPackage();
		};
		_tips.SetToolTip(hudButton3, "Exportar macro pronta (.pulso) — Ctrl+E");
		HudButton hudButton4 = new HudButton
		{
			Text = "Salvar",
			Icon = ButtonIcon.Save,
			Kind = HudKind.Ghost,
			Width = 96,
			Height = 36,
			Radius = 8
		};
		hudButton4.Margin = new Padding(6, 0, 0, 0);
		hudButton4.Click += delegate
		{
			SaveMacro(saveAs: false);
		};
		_tips.SetToolTip(hudButton4, "Salvar macro");
		HudButton hudButton5 = new HudButton
		{
			Text = "Abrir",
			Icon = ButtonIcon.Folder,
			Kind = HudKind.Ghost,
			Width = 94,
			Height = 36,
			Radius = 8
		};
		hudButton5.Margin = new Padding(6, 0, 0, 0);
		hudButton5.Click += delegate
		{
			OpenMacro();
		};
		_tips.SetToolTip(hudButton5, "Abrir macro (.gmt / .rec / .pulso)");
		flowLayoutPanel.Controls.Add(hudButton);
		flowLayoutPanel.Controls.Add(hudButton2);
		flowLayoutPanel.Controls.Add(hudButton3);
		flowLayoutPanel.Controls.Add(hudButton4);
		flowLayoutPanel.Controls.Add(hudButton5);
		_workTop.Controls.Add(flowLayoutPanel);
		_workTop.Controls.Add(value);
		_workTop.Controls.Add(_lblWorkFile);
		_gridCard.Dock = DockStyle.Fill;
		_gridCard.Radius = 4;
		_gridCard.Fill = Palette.Panel;
		_gridCard.BorderColor = Palette.BorderSoft;
		_gridCard.Padding = new Padding(10, 4, 22, 10);
		_grid.Dock = DockStyle.Fill;
		_grid.VirtualMode = true;
		_grid.ScrollBars = ScrollBars.None;
		_grid.RowHeadersVisible = false;
		_grid.AllowUserToAddRows = false;
		_grid.AllowUserToDeleteRows = false;
		_grid.AllowUserToResizeRows = false;
		_grid.MultiSelect = true;
		_grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		_grid.EmptyTitle = "Nenhuma automação por aqui";
		_grid.EmptyHint = "Grave sua primeira macro com " + _s.HotkeyRecord + " ou arraste um arquivo .gmt";
		_grid.AllowDrop = true;
		DarkUi.StyleGrid(_grid);
		_grid.Columns.Add(new DataGridViewTextBoxColumn
		{
			HeaderText = "#",
			Width = 54,
			SortMode = DataGridViewColumnSortMode.NotSortable,
			DefaultCellStyle = new DataGridViewCellStyle
			{
				Alignment = DataGridViewContentAlignment.MiddleRight,
				ForeColor = Palette.Muted,
				Font = UiFont.MonoSmall,
				Padding = new Padding(0, 0, 10, 0)
			}
		});
		_grid.Columns.Add(new DataGridViewTextBoxColumn
		{
			HeaderText = "TEMPO",
			Width = 92,
			SortMode = DataGridViewColumnSortMode.NotSortable,
			DefaultCellStyle = new DataGridViewCellStyle
			{
				Alignment = DataGridViewContentAlignment.MiddleRight,
				Font = UiFont.Mono,
				ForeColor = Palette.TextSoft,
				Padding = new Padding(0, 0, 10, 0)
			}
		});
		_grid.Columns.Add(new DataGridViewTextBoxColumn
		{
			HeaderText = "EVENTO",
			Width = 138,
			SortMode = DataGridViewColumnSortMode.NotSortable,
			DefaultCellStyle = new DataGridViewCellStyle
			{
				ForeColor = Palette.AccentSoft
			}
		});
		_grid.Columns.Add(new DataGridViewTextBoxColumn
		{
			HeaderText = "DETALHE",
			AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
			SortMode = DataGridViewColumnSortMode.NotSortable
		});
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip
		{
			Renderer = new DarkRenderer()
		};
		contextMenuStrip.Items.Add(Item("Editar evento…", Keys.None, EditSelected));
		contextMenuStrip.Items.Add(Item("Inserir pausa a partir daqui…", Keys.None, InsertPause));
		contextMenuStrip.Items.Add(new ToolStripSeparator());
		contextMenuStrip.Items.Add(Item("Remover selecionados", Keys.None, RemoveSelected));
		_grid.ContextMenuStrip = contextMenuStrip;
		_grid.RowsAdded += delegate
		{
			_vbar.Sync();
		};
		_grid.RowsRemoved += delegate
		{
			_vbar.Sync();
		};
		_grid.Scroll += delegate
		{
			_vbar.Sync();
		};
		_grid.SizeChanged += delegate
		{
			LayoutScrollbar();
		};
		_gridCard.Controls.Add(_grid);
		_vbar.Grid = _grid;
		_gridCard.Controls.Add(_vbar);
		_vbar.BringToFront();
		_gridCard.Resize += delegate
		{
			LayoutScrollbar();
		};
		_work.Controls.Add(_gridCard);
		_work.Controls.Add(_workTop);
	}

	private void LayoutScrollbar()
	{
		int height = Math.Max(8, _grid.Height - 4);
		_vbar.Bounds = new Rectangle(_gridCard.Width - 16, _grid.Top + 2, 10, height);
	}

	private void BuildChrome()
	{
		_chrome.Dock = DockStyle.Top;
		_chrome.Height = 48;
		_chrome.BackColor = Palette.Bg;
		_chrome.Paint += DrawChrome;
		_chrome.MouseDown += ChromeDrag;
		_chrome.DoubleClick += delegate
		{
			ToggleMax();
		};
		_chrome.Resize += delegate
		{
			LayoutChrome();
		};
		_brandMark.Location = new Point(16, 11);
		_brandMark.Size = new Size(26, 26);
		_brandMark.BackColor = Palette.Bg;
		_brandTitle.AutoSize = true;
		_brandTitle.Location = new Point(50, 8);
		_brandTitle.Font = UiFont.Title;
		_brandTitle.ForeColor = Palette.Text;
		_brandTitle.BackColor = Palette.Bg;
		_brandTitle.Text = "PULSO";
		_brandTitle.MouseDown += ChromeDrag;
		_brandTitle.DoubleClick += delegate
		{
			ToggleMax();
		};
		_brandSub.AutoSize = true;
		_brandSub.Location = new Point(51, 27);
		_brandSub.Font = UiFont.Tiny;
		_brandSub.ForeColor = Palette.Muted;
		_brandSub.BackColor = Palette.Bg;
		_brandSub.Text = "ESTÚDIO TÁTICO  //  v3.1";
		_brandSub.MouseDown += ChromeDrag;
		_lblChromeFile.AutoSize = false;
		_lblChromeFile.Location = new Point(206, 14);
		_lblChromeFile.Size = new Size(420, 20);
		_lblChromeFile.Font = UiFont.MonoSmall;
		_lblChromeFile.ForeColor = Palette.Muted;
		_lblChromeFile.BackColor = Palette.Bg;
		_lblChromeFile.TextAlign = ContentAlignment.MiddleRight;
		_lblChromeFile.Text = "macro sem nome";
		_lblChromeFile.MouseDown += ChromeDrag;
		HudButton[] array = new HudButton[3] { _btnMin, _btnMax, _btnClose };
		foreach (HudButton hudButton in array)
		{
			hudButton.Size = new Size(42, 30);
			hudButton.Radius = 7;
			hudButton.Font = UiFont.BodyBold;
			hudButton.BackColor = Palette.Bg;
			_chrome.Controls.Add(hudButton);
		}
		_btnClose.Accent = Palette.Danger;
		_tips.SetToolTip(_btnMin, "Minimizar");
		_tips.SetToolTip(_btnMax, "Maximizar / restaurar (duplo clique na barra)");
		_tips.SetToolTip(_btnClose, "Fechar");
		_btnMin.Click += delegate
		{
			base.WindowState = FormWindowState.Minimized;
		};
		_btnMax.Click += delegate
		{
			ToggleMax();
		};
		_btnClose.Click += delegate
		{
			Close();
		};
		_chrome.Controls.Add(_lblChromeFile);
		_chrome.Controls.Add(_brandSub);
		_chrome.Controls.Add(_brandTitle);
		_chrome.Controls.Add(_brandMark);
		LayoutChrome();
	}

	private void LayoutChrome()
	{
		int y = (_chrome.Height - 30) / 2;
		_btnClose.Location = new Point(_chrome.Width - 50, y);
		_btnMax.Location = new Point(_chrome.Width - 50 - 44, y);
		_btnMin.Location = new Point(_chrome.Width - 50 - 88, y);
		_lblChromeFile.Width = Math.Max(80, _chrome.Width - 206 - 150);
	}

	private void DrawChrome(object? sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		using (SolidBrush brush = new SolidBrush(Palette.Bg))
		{
			graphics.FillRectangle(brush, _chrome.ClientRectangle);
		}
		using Pen pen = new Pen(Palette.BorderSoft);
		graphics.DrawLine(pen, 0, _chrome.Height - 1, _chrome.Width, _chrome.Height - 1);
	}

	private void ChromeDrag(object? sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Native.ReleaseCapture();
			Native.SendMessage(base.Handle, 161, 2, IntPtr.Zero);
		}
	}

	private void ToggleMax()
	{
		if (_compact)
		{
			SetCompact(on: false);
			return;
		}
		if (_maxed)
		{
			_maxed = false;
			base.Bounds = (_maxRestore.IsEmpty ? Screen.FromControl(this).WorkingArea : _maxRestore);
		}
		else
		{
			_maxRestore = base.Bounds;
			_maxed = true;
			base.Bounds = Screen.FromControl(this).WorkingArea;
		}
		_btnMax.Text = (_maxed ? "❐" : "□");
		_btnMax.Invalidate();
		LayoutGrips();
	}

	private void BuildGrips()
	{
		(int, Cursor)[] array = new(int, Cursor)[8]
		{
			(10, Cursors.SizeWE),
			(11, Cursors.SizeWE),
			(12, Cursors.SizeNS),
			(15, Cursors.SizeNS),
			(13, Cursors.SizeNWSE),
			(14, Cursors.SizeNESW),
			(16, Cursors.SizeNESW),
			(17, Cursors.SizeNWSE)
		};
		for (int i = 0; i < array.Length; i++)
		{
			(int, Cursor) tuple = array[i];
			int item = tuple.Item1;
			Cursor item2 = tuple.Item2;
			Panel panel = new Panel
			{
				TabStop = false,
				Cursor = item2,
				BackColor = Palette.Bg
			};
			int ht = item;
			panel.MouseDown += delegate(object? _, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Left && !_compact && !_maxed && !_fullScreen)
				{
					Native.ReleaseCapture();
					Native.SendMessage(base.Handle, 161, ht, IntPtr.Zero);
				}
			};
			_grips.Add(panel);
			base.Controls.Add(panel);
		}
		LayoutGrips();
	}

	private void LayoutGrips()
	{
		bool on;
		if (_grips.Count >= 8)
		{
			int num = 5;
			int num2 = 12;
			int width = base.ClientSize.Width;
			int height = base.ClientSize.Height;
			on = !_compact && !_maxed && !_fullScreen;
			Place(0, 0, num2, num, height - 2 * num2);
			Place(1, width - num, num2, num, height - 2 * num2);
			Place(2, num2, 0, width - 2 * num2, num);
			Place(3, num2, height - num, width - 2 * num2, num);
			Place(4, 0, 0, num2, num2);
			Place(5, width - num2, 0, num2, num2);
			Place(6, 0, height - num2, num2, num2);
			Place(7, width - num2, height - num2, num2, num2);
		}
		void Place(int i, int x, int y, int cw, int ch)
		{
			Panel panel = _grips[i];
			panel.Bounds = new Rectangle(x, y, Math.Max(0, cw), Math.Max(0, ch));
			panel.Visible = on;
			if (on)
			{
				panel.BringToFront();
			}
		}
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
		if (m.Msg == 132 && !_compact && !_maxed && !_fullScreen)
		{
			long num = ((IntPtr)m.LParam).ToInt64();
			Point point = PointToClient(new Point((short)(num & 0xFFFF), (short)((num >> 16) & 0xFFFF)));
			bool flag = point.X <= 6;
			bool flag2 = point.X >= base.ClientSize.Width - 6;
			bool flag3 = point.Y <= 6;
			bool flag4 = point.Y >= base.ClientSize.Height - 6;
			if (flag3 && flag)
			{
				m.Result = 13;
			}
			else if (flag3 && flag2)
			{
				m.Result = 14;
			}
			else if (flag4 && flag)
			{
				m.Result = 16;
			}
			else if (flag4 && flag2)
			{
				m.Result = 17;
			}
			else if (flag)
			{
				m.Result = 10;
			}
			else if (flag2)
			{
				m.Result = 11;
			}
			else if (flag3)
			{
				m.Result = 12;
			}
			else if (flag4)
			{
				m.Result = 15;
			}
		}
	}

	private void BuildBottomBar()
	{
		_bottom.Dock = DockStyle.Bottom;
		_bottom.Height = 54;
		_bottom.BackColor = Palette.Bg;
		_bottom.Paint += delegate(object? _, PaintEventArgs e)
		{
			using Pen pen = new Pen(Palette.BorderSoft);
			e.Graphics.DrawLine(pen, 0, 0, _bottom.Width, 0);
		};
		_lblStatusNote.AutoSize = false;
		_lblStatusNote.Location = new Point(26, 0);
		_lblStatusNote.Size = new Size(380, 54);
		_lblStatusNote.TextAlign = ContentAlignment.MiddleLeft;
		_lblStatusNote.Font = UiFont.Body;
		_lblStatusNote.ForeColor = Palette.Muted;
		_lblStatusNote.BackColor = Palette.Bg;
		_lblStatusNote.Text = "Pronto para gravar";
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Right,
			Width = 520,
			FlowDirection = FlowDirection.RightToLeft,
			BackColor = Palette.Bg,
			Padding = new Padding(0, 11, 18, 0),
			WrapContents = false
		};
		_btnInf.Width = 34;
		_btnInf.Height = 30;
		_btnInf.Radius = 7;
		_btnInf.Margin = new Padding(8, 0, 0, 0);
		_btnInf.Click += delegate
		{
			ToggleContinuous();
		};
		_tips.SetToolTip(_btnInf, "Repetição contínua");
		_lblLoopsVal.AutoSize = false;
		_lblLoopsVal.Text = _s.Loops.ToString();
		_lblLoopsVal.Width = 48;
		_lblLoopsVal.Height = 30;
		_lblLoopsVal.TextAlign = ContentAlignment.MiddleCenter;
		_lblLoopsVal.Font = UiFont.Mono;
		_lblLoopsVal.ForeColor = Palette.AccentSoft;
		_lblLoopsVal.BackColor = Palette.Bg;
		_lblLoopsVal.Cursor = Cursors.Hand;
		_lblLoopsVal.Margin = new Padding(0);
		_lblLoopsVal.Click += delegate
		{
			_s.Loops = 1;
			_lblLoopsVal.Text = "1";
		};
		_tips.SetToolTip(_lblLoopsVal, "Repetições — clique para voltar a 1");
		_lblSpeedVal.AutoSize = false;
		_lblSpeedVal.Text = _s.SpeedPercent + "%";
		_lblSpeedVal.Width = 62;
		_lblSpeedVal.Height = 30;
		_lblSpeedVal.TextAlign = ContentAlignment.MiddleCenter;
		_lblSpeedVal.Font = UiFont.Mono;
		_lblSpeedVal.ForeColor = Palette.AccentSoft;
		_lblSpeedVal.BackColor = Palette.Bg;
		_lblSpeedVal.Cursor = Cursors.Hand;
		_lblSpeedVal.Margin = new Padding(0);
		_lblSpeedVal.Click += delegate
		{
			_s.SpeedPercent = 100;
			_lblSpeedVal.Text = "100%";
		};
		_tips.SetToolTip(_lblSpeedVal, "Velocidade — clique para voltar a 100%");
		flowLayoutPanel.Controls.Add(_btnInf);
		flowLayoutPanel.Controls.Add(Mini(ButtonIcon.Plus, 30, "Aumentar repetições", delegate
		{
			ChangeLoops(1);
		}));
		flowLayoutPanel.Controls.Add(_lblLoopsVal);
		flowLayoutPanel.Controls.Add(Mini(ButtonIcon.Minus, 30, "Diminuir repetições", delegate
		{
			ChangeLoops(-1);
		}));
		flowLayoutPanel.Controls.Add(Tag("REPETIÇÕES", 78));
		flowLayoutPanel.Controls.Add(Mini(ButtonIcon.Plus, 30, "Aumentar velocidade", delegate
		{
			ChangeSpeed(10);
		}));
		flowLayoutPanel.Controls.Add(_lblSpeedVal);
		flowLayoutPanel.Controls.Add(Mini(ButtonIcon.Minus, 30, "Diminuir velocidade", delegate
		{
			ChangeSpeed(-10);
		}));
		flowLayoutPanel.Controls.Add(Tag("VELOCIDADE", 78));
		_bottom.Controls.Add(flowLayoutPanel);
		_bottom.Controls.Add(_lblStatusNote);
		HudButton Mini(ButtonIcon icon, int width, string tip, Action act)
		{
			Action act2 = act;
			HudButton hudButton = new HudButton
			{
				Icon = icon,
				Kind = HudKind.Bare,
				TextVisible = false,
				Width = width,
				Height = 30,
				Radius = 7,
				Margin = new Padding(2, 0, 2, 0)
			};
			hudButton.Click += delegate
			{
				act2();
			};
			_tips.SetToolTip(hudButton, tip);
			return hudButton;
		}
		static Label Tag(string text, int width)
		{
			return new Label
			{
				Text = text,
				Width = width,
				Height = 30,
				TextAlign = ContentAlignment.MiddleRight,
				Font = UiFont.TinyBold,
				ForeColor = Palette.Muted,
				BackColor = Palette.Bg,
				Margin = new Padding(12, 0, 6, 0)
			};
		}
	}

	private void BuildMini()
	{
		if (_miniBar != null)
		{
			return;
		}
		_miniBar = new MiniBar
		{
			Dock = DockStyle.Fill,
			Visible = false
		};
		_miniBar.RecordClick += ToggleRecording;
		_miniBar.PlayClick += StartPlayback;
		_miniBar.PauseClick += TogglePause;
		_miniBar.StopClick += EmergencyStop;
		_miniBar.SpeedDownClick += delegate
		{
			ChangeSpeed(-5);
			SyncMini();
		};
		_miniBar.SpeedUpClick += delegate
		{
			ChangeSpeed(5);
			SyncMini();
		};
		_miniBar.LoopClick += delegate
		{
			ToggleContinuous();
			SyncMini();
		};
		_miniBar.SettingsClick += OpenSettings;
		_miniBar.MoreClick += ShowMiniMenu;
		_miniBar.ExpandClick += delegate
		{
			SetCompact(on: false);
		};
		_miniBar.CloseClick += Close;
		base.Controls.Add(_miniBar);
	}

	private void SetCompact(bool on)
	{
		if (_compact == on)
		{
			return;
		}
		if (on)
		{
			BuildMini();
			_normalBounds = (_maxed ? (_maxRestore.IsEmpty ? base.RestoreBounds : _maxRestore) : ((base.WindowState == FormWindowState.Normal) ? base.Bounds : base.RestoreBounds));
			_compactFromMax = _maxed;
			_compact = true;
			_chrome.Visible = false;
			_menu.Visible = false;
			_body.Visible = false;
			_bottom.Visible = false;
			_hero.Visible = false;
			if (_maxed)
			{
				_maxed = false;
				base.Bounds = (_maxRestore.IsEmpty ? _normalBounds : _maxRestore);
			}
			Size size = new Size(LogicalToDeviceUnits(MiniBar.BaseWidth), LogicalToDeviceUnits(MiniBar.BaseHeight));
			MinimumSize = size;
			MaximumSize = size;
			base.ClientSize = size;
			_miniBar!.Visible = true;
			_miniBar.BringToFront();
			base.TopMost = true;
			SyncMini();
		}
		else
		{
			_compact = false;
			MaximumSize = Size.Empty;
			MinimumSize = new Size(960, 580);
			_chrome.Visible = true;
			_menu.Visible = true;
			_body.Visible = true;
			_bottom.Visible = true;
			_hero.Visible = true;
			if (_miniBar != null)
			{
				_miniBar.Visible = false;
			}
			if (!_normalBounds.IsEmpty)
			{
				base.Bounds = _normalBounds;
			}
			if (_compactFromMax)
			{
				ToggleMax();
			}
			_compactFromMax = false;
			base.TopMost = _s.TopMost;
		}
		_miCompact.Checked = _compact;
		LayoutGrips();
		UpdateStateUi();
		Invalidate(invalidateChildren: true);
	}

	private void SyncMini()
	{
		if (_miniBar == null)
		{
			return;
		}
		_miniBar.Callsign = ((_currentPath != null) ? Path.GetFileName(_currentPath) : ((!string.IsNullOrEmpty(_readyName)) ? _readyName : "macro sem nome"));
		_miniBar.Duration = _lblDur.Text;
		_miniBar.EventCount = (_recording ? _engine.EventCount : _macro.Events.Count);
		_miniBar.StateText = _lblState.Text;
		_miniBar.Recording = _recording;
		_miniBar.Playing = _playing;
		_miniBar.Paused = _player.IsPaused;
		_miniBar.SpeedPercent = _s.SpeedPercent;
		_miniBar.Loops = _s.Loops;
		_miniBar.Continuous = _s.Continuous;
		if (!_playing || _playTotalMs <= 0.0)
		{
			_miniBar.Progress = -1.0;
		}
		else
		{
			_miniBar.Progress = _playWatch.Elapsed.TotalMilliseconds / _playTotalMs;
		}
	}

	private void ShowMiniMenu()
	{
		if (_miniMenu == null)
		{
			ContextMenuStrip menu = new ContextMenuStrip
			{
				Font = UiFont.Body,
				BackColor = Palette.Bg2,
				ForeColor = Palette.TextSoft,
				Renderer = new DarkRenderer()
			};
			menu.Items.Add(Item("Gravar / parar", Keys.None, ToggleRecording));
			menu.Items.Add(Item("Reproduzir", Keys.None, StartPlayback));
			menu.Items.Add(Item("Pausar / continuar", Keys.None, TogglePause));
			menu.Items.Add(Item("Parar tudo (F12)", Keys.None, EmergencyStop));
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add(Item("Abrir macro…", Keys.None, OpenMacro));
			menu.Items.Add(Item("Salvar", Keys.None, delegate
			{
				SaveMacro(saveAs: false);
			}));
			menu.Items.Add(Item("Salvar como…", Keys.None, delegate
			{
				SaveMacro(saveAs: true);
			}));
			menu.Items.Add(new ToolStripSeparator());
			_miMiniLoop = Item("Repetir continuamente", Keys.None, delegate
			{
				ToggleContinuous();
				SyncMini();
			});
			_miMiniLoop.CheckOnClick = true;
			menu.Items.Add(_miMiniLoop);
			_miMiniTop = Item("Sempre no topo", Keys.None, delegate
			{
				if (_miMiniTop != null)
				{
					_miTopMost.Checked = _miMiniTop.Checked;
				}
			});
			_miMiniTop.CheckOnClick = true;
			menu.Items.Add(_miMiniTop);
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add(Item("Configurações…", Keys.None, OpenSettings));
			menu.Items.Add(Item("Voltar à janela normal", Keys.None, delegate
			{
				SetCompact(on: false);
			}));
			menu.Items.Add(Item("Fechar o Pulso", Keys.None, Close));
			menu.Opening += delegate
			{
				if (_miMiniLoop != null)
				{
					_miMiniLoop.Checked = _s.Continuous;
				}
				if (_miMiniTop != null)
				{
					_miMiniTop.Checked = _s.TopMost;
				}
			};
			_miniMenu = menu;
		}
		_miniMenu.Show(Cursor.Position);
	}

	private void ToggleFullScreen()
	{
		_fullScreen = !_fullScreen;
		if (_fullScreen)
		{
			_maxRestore = base.Bounds;
			_chrome.Visible = false;
			_menu.Visible = false;
			_bottom.Visible = false;
			base.Bounds = Screen.FromControl(this).Bounds;
		}
		else
		{
			_chrome.Visible = !_compact;
			_menu.Visible = !_compact;
			_body.Visible = !_compact;
			_bottom.Visible = !_compact;
			base.Bounds = (_maxRestore.IsEmpty ? base.Bounds : _maxRestore);
		}
		LayoutGrips();
	}

	private void ChangeSpeed(int delta)
	{
		_s.SpeedPercent = Math.Clamp(_s.SpeedPercent + delta, 5, 2000);
		_lblSpeedVal.Text = _s.SpeedPercent + "%";
		SyncMini();
	}

	private void ChangeLoops(int delta)
	{
		_s.Loops = Math.Clamp(_s.Loops + delta, 1, 100000);
		_lblLoopsVal.Text = _s.Loops.ToString();
		SyncMini();
	}

	private void ToggleContinuous()
	{
		_s.Continuous = !_s.Continuous;
		_btnInf.Active = _s.Continuous;
		_btnInf.Invalidate();
		_tips.SetToolTip(_btnInf, _s.Continuous ? "Repetição contínua ativa — clique para desligar" : "Repetir continuamente");
		SyncMini();
	}

	private static ToolStripMenuItem Item(string text, Keys keys, Action onClick)
	{
		Action onClick2 = onClick;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(text);
		if (keys != 0)
		{
			toolStripMenuItem.ShortcutKeys = keys;
		}
		toolStripMenuItem.Click += delegate
		{
			onClick2();
		};
		return toolStripMenuItem;
	}

	private void WireGrid()
	{
		_grid.CellValueNeeded += delegate(object? _, DataGridViewCellValueEventArgs e)
		{
			MacroEvent macroEvent = (_recording ? _engine.GetEvent(e.RowIndex) : ((e.RowIndex >= 0 && e.RowIndex < _macro.Events.Count) ? _macro.Events[e.RowIndex] : null));
			if (macroEvent != null)
			{
				e.Value = e.ColumnIndex switch
				{
					0 => (e.RowIndex + 1).ToString(), 
					1 => macroEvent.T.ToString("N0"), 
					2 => macroEvent.KindName, 
					3 => macroEvent.Detail, 
					_ => "", 
				};
			}
		};
		_grid.CellDoubleClick += delegate(object? _, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0)
			{
				EditEventAt(e.RowIndex);
			}
		};
		_grid.KeyDown += delegate(object? _, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				RemoveSelected();
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Return)
			{
				EditSelected();
				e.Handled = true;
			}
		};
	}

	private void WireTray()
	{
		_tray.Icon = AppIcon.Small;
		_tray.Text = "Pulso";
		_tray.Visible = true;
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		contextMenuStrip.Items.Add(Item("Mostrar janela", Keys.None, RestoreFromTray));
		contextMenuStrip.Items.Add(new ToolStripSeparator());
		contextMenuStrip.Items.Add(Item("Gravar / parar", Keys.None, delegate
		{
			ToggleRecording();
		}));
		contextMenuStrip.Items.Add(Item("Reproduzir", Keys.None, StartPlayback));
		contextMenuStrip.Items.Add(Item("Parar tudo", Keys.None, EmergencyStop));
		contextMenuStrip.Items.Add(new ToolStripSeparator());
		contextMenuStrip.Items.Add(Item("Sair", Keys.None, base.Close));
		_tray.ContextMenuStrip = contextMenuStrip;
		_tray.DoubleClick += delegate
		{
			RestoreFromTray();
		};
	}

	private void RestoreFromTray()
	{
		Show();
		base.WindowState = FormWindowState.Normal;
		Activate();
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		LayoutGrips();
		if (base.WindowState != FormWindowState.Minimized)
		{
			return;
		}
		if (_s.MinimizeToTray)
		{
			Hide();
			if (_s.ShowTrayBubble)
			{
				Balloon("Pulso continua ativo na bandeja.");
			}
		}
		else if (_s.MinimizeToBar)
		{
			base.WindowState = FormWindowState.Normal;
			SetCompact(on: true);
		}
	}

	private void Balloon(string text)
	{
		try
		{
			_tray.ShowBalloonTip(1800, "Pulso", text, ToolTipIcon.Info);
		}
		catch
		{
		}
	}

	private void ApplySettingsToUi()
	{
		_lblSpeedVal.Text = _s.SpeedPercent + "%";
		_lblLoopsVal.Text = _s.Loops.ToString();
		_btnInf.Active = _s.Continuous;
		_btnInf.Invalidate();
		_miTopMost.Checked = _s.TopMost;
		_grid.EmptyHint = "Grave sua primeira macro com " + _s.HotkeyRecord + " ou arraste um arquivo .gmt";
		_tips.SetToolTip(_btnRecord, "Gravar / parar gravação (" + _s.HotkeyRecord + ")");
		_tips.SetToolTip(_btnPlay, "Reproduzir macro (" + _s.HotkeyPlay + ")");
		_tips.SetToolTip(_btnPause, "Pausar / continuar (" + _s.HotkeyPause + ")");
		_tips.SetToolTip(_btnStop, "Parar tudo — F12 é emergência");
		base.TopMost = _s.TopMost;
		ApplyRecordingFlags();
		UpdateStateUi();
	}

	private void ApplyRecordingFlags()
	{
		_engine.RecordMouse = _s.RecordMouse;
		_engine.RecordKeyboard = _s.RecordKeyboard;
		_engine.IgnoreOwnWindow = _s.IgnoreOwnWindow;
	}

	private void OpenSettings()
	{
		using SettingsForm settingsForm = new SettingsForm(_s, (float)base.DeviceDpi / 96f);
		if (ShowDialogSafe(settingsForm) == DialogResult.OK)
		{
			_s = settingsForm.Result;
			_s.Save();
			ApplySettingsToUi();
			UpdateStateUi();
			Balloon("Configurações salvas. Atalhos ativos imediatamente.");
		}
	}

	private DialogResult ShowDialogSafe(Form d)
	{
		_suspendHotkeys = true;
		d.TopMost = base.TopMost;
		try
		{
			return d.ShowDialog(this);
		}
		finally
		{
			_suspendHotkeys = false;
			_down.Clear();
		}
	}

	private void Ui(Action a)
	{
		try
		{
			if (!base.IsDisposed)
			{
				if (base.InvokeRequired)
				{
					BeginInvoke(a);
				}
				else
				{
					a();
				}
			}
		}
		catch
		{
		}
	}

	private void RefreshTitle()
	{
		string text = ((_currentPath != null) ? Path.GetFileName(_currentPath) : ((!string.IsNullOrEmpty(_readyName)) ? _readyName : "macro sem nome"));
		Text = "Pulso — " + text + (_dirty ? " *" : "");
		_hero.Callsign = text;
		_lblChromeFile.Text = text + (_dirty ? "  •  não salvo" : "");
		_lblWorkFile.Text = text + (_dirty ? "  •  não salvo" : "");
		SyncMini();
	}

	private void UpdateStateUi()
	{
		string text = (_recording ? "Gravando" : (_playing ? "Reproduzindo" : "Pronto"));
		_lblState.Text = text;
		_lblState.ForeColor = (_recording ? Palette.Danger : (_playing ? Palette.AccentSoft : Palette.Text));
		_statusDot.Tint = (_recording ? Palette.Danger : (_playing ? Palette.Accent : Palette.Success));
		_statusDot.On = _recording || _playing;
		_lblStatusSub.Text = (_recording ? "Capturando mouse e teclado" : ((!_playing) ? ((!string.IsNullOrEmpty(_readyName) && !_macro.IsEmpty) ? "Macro pronta para uso" : "Aguardando uma ação") : (_player.IsPaused ? "Reprodução pausada" : "Executando a automação")));
		_lblStatusNote.Text = (_recording ? "Gravação em andamento — use o atalho para encerrar" : (_playing ? "Reprodução em andamento — F12 interrompe tudo" : ((!string.IsNullOrEmpty(_readyName) && !_macro.IsEmpty) ? "Macro pronta — aperte Reproduzir (F5)" : "Pronto para gravar")));
		_btnRecord.Text = (_recording ? "Parar gravação" : "Gravar");
		_btnPlay.Text = "Reproduzir";
		_btnRecord.Active = _recording;
		_btnPlay.Active = _playing && !_player.IsPaused;
		_btnPlay.Enabled = !_recording && !_playing && !_macro.IsEmpty;
		_btnPause.Enabled = _playing;
		_btnStop.Enabled = _recording || _playing;
		_miSave.Enabled = !_macro.IsEmpty;
		_miExportRec.Enabled = !_macro.IsEmpty;
		UpdateUndoUi();
		UpdateCounters();
	}

	private void UpdateCounters()
	{
		int num = (_recording ? _engine.EventCount : _macro.Events.Count);
		_lblCount.Text = num.ToString("N0");
		double value = (_recording ? (DateTime.Now - _recStart).TotalSeconds : ((double)_macro.Duration / 1000.0));
		_lblDur.Text = $"{value:N1} s";
		_hero.EventCount = num;
		_hero.Duration = $"{value:N1} s";
		_hero.Mode = (_recording ? "Gravando" : (_playing ? (_player.IsPaused ? "Pausado" : "Executando") : "Pronto"));
		_hero.Live = _recording || _playing;
		SyncMini();
	}

	private void SetRowCount()
	{
		int num = (_recording ? _engine.EventCount : _macro.Events.Count);
		if (num == _lastRowCount)
		{
			return;
		}
		_lastRowCount = num;
		_grid.RowCount = num;
		if (num > 0)
		{
			try
			{
				_grid.FirstDisplayedScrollingRowIndex = num - 1;
			}
			catch
			{
			}
		}
		UpdateCounters();
	}

	private void OnTimerTick()
	{
		SetRowCount();
		if (_recording)
		{
			UpdateCounters();
			TimeSpan value = DateTime.Now - _recStart;
			string text = $"Gravando  {value:mm\\:ss}";
			_lblState.Text = text;
			_lblStatusSub.Text = "Capturando mouse e teclado";
		}
		SyncMini();
	}

	public void OnKeyActivity(int vk, bool down)
	{
		if (!down)
		{
			_down.Remove(vk);
		}
		else
		{
			if (!_down.Add(vk) || _suspendHotkeys)
			{
				return;
			}
			if (vk == 123)
			{
				if (_recording)
				{
					_engine.SuppressKey(vk);
				}
				Post(EmergencyStop);
			}
			else if (Matches(_s.HotkeyRecord, vk))
			{
				if (_recording)
				{
					_engine.SuppressKey(vk);
					Post(StopRecording);
				}
				else
				{
					Post(delegate
					{
						StartRecording(vk);
					});
				}
			}
			else if (Matches(_s.HotkeyPlay, vk))
			{
				if (_recording)
				{
					_engine.SuppressKey(vk);
					Post(StopRecording);
				}
				else
				{
					Post(StartPlayback);
				}
			}
			else if (Matches(_s.HotkeyPause, vk))
			{
				Post(TogglePause);
			}
			else if (Matches(_s.HotkeyCompact, vk))
			{
				Post(delegate
				{
					SetCompact(!_compact);
				});
			}
			else if (Matches(_s.HotkeyStop, vk))
			{
				if (_recording)
				{
					_engine.SuppressKey(vk);
				}
				Post(EmergencyStop);
			}
		}
	}

	private void Post(Action action)
	{
		try
		{
			if (!base.IsDisposed)
			{
				BeginInvoke(action);
			}
		}
		catch
		{
		}
	}

	private bool Matches(string combo, int vk)
	{
		Hotkey hotkey = Hotkey.Parse(combo);
		if (!hotkey.IsValid || hotkey.Vk != vk)
		{
			return false;
		}
		if (hotkey.Ctrl != DownVk(17))
		{
			return false;
		}
		if (hotkey.Alt != DownVk(18))
		{
			return false;
		}
		if (hotkey.Shift != DownVk(16))
		{
			return false;
		}
		bool flag = DownVk(91) || DownVk(92);
		if (hotkey.Win != flag)
		{
			return false;
		}
		return true;
	}

	private static bool DownVk(int vk)
	{
		return (Native.GetAsyncKeyState(vk) & 0x8000) != 0;
	}

	private void ToggleRecording()
	{
		if (_recording)
		{
			StopRecording();
		}
		else
		{
			StartRecording(0);
		}
	}

	private void StartRecording(int suppressVk)
	{
		if (_playing)
		{
			_player.Stop();
		}
		if (_dirty && !_macro.IsEmpty && !_recording)
		{
			switch (MessageBox.Show(this, "A macro atual será substituída pela nova gravação. Salvar antes?", "Pulso", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
			case DialogResult.Cancel:
				return;
			case DialogResult.Yes:
				if (!SaveMacro(saveAs: false))
				{
					return;
				}
				break;
			}
		}
		_macro = new Macro();
		_currentPath = null;
		_readyName = null;
		_lastRowCount = -1;
		SetRowCount();
		ClearHistory();
		_down.Clear();
		if (suppressVk != 0)
		{
			_engine.SuppressKey(suppressVk);
		}
		_engine.StartRecording(suppressVk);
		_recording = true;
		_recStart = DateTime.Now;
		_dirty = false;
		RefreshTitle();
		UpdateStateUi();
		Balloon("Gravação iniciada. Pressione o atalho de novo para parar.");
	}

	private void StopRecording()
	{
		Macro macro = _engine.StopRecording();
		_recording = false;
		if (macro != null)
		{
			macro.Events.RemoveAll((MacroEvent e) => e == null);
			_macro = macro;
			_dirty = macro.Events.Count > 0;
		}
		_lastRowCount = -1;
		SetRowCount();
		RefreshTitle();
		UpdateStateUi();
		if (_macro.IsEmpty)
		{
			Balloon("Nenhum evento foi gravado.");
		}
		else
		{
			Balloon($"Gravação concluída: {_macro.Events.Count} eventos em {(double)_macro.Duration / 1000.0:N1} s.");
		}
		AppLog.Write($"Gravação concluída: {_macro.Events.Count} eventos.");
	}

	private void StartPlayback()
	{
		if (_recording)
		{
			StopRecording();
		}
		if (!_playing)
		{
			if (_macro.IsEmpty)
			{
				MessageBox.Show(this, "Não há nada para reproduzir. Grave ou abra uma macro primeiro.", "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}
			_playing = true;
			double num = (_s.Instant ? 0.0 : ((double)_macro.Duration * (double)(_s.Continuous ? 1 : _s.Loops) * 100.0 / (double)Math.Max(1, _s.SpeedPercent)));
			if (num > 0.0 && !_s.Continuous)
			{
				num += (double)_s.StartDelayMs + (double)_s.LoopDelayMs * (double)Math.Max(0, _s.Loops - 1);
			}
			_playTotalMs = num;
			_playWatch.Restart();
			UpdateStateUi();
			_player.Start(_macro, _s);
		}
	}

	private void TogglePause()
	{
		if (_playing)
		{
			if (_player.IsPaused)
			{
				_player.Resume();
				_playWatch.Start();
			}
			else
			{
				_player.Pause();
				_playWatch.Stop();
			}
			SyncMini();
		}
	}

	private void OnPlaybackFinished()
	{
		_playing = false;
		_playWatch.Reset();
		_playTotalMs = 0.0;
		UpdateStateUi();
	}

	private void EmergencyStop()
	{
		bool flag = _recording || _playing;
		_player.Stop();
		if (_recording)
		{
			StopRecording();
		}
		_playing = false;
		UpdateStateUi();
		_lblState.Text = (flag ? "Parado (F12)" : "Pronto");
		_lblState.ForeColor = (flag ? Palette.Gold : Palette.Text);
		_statusDot.Tint = (flag ? Palette.Gold : Palette.Success);
		_statusDot.On = false;
		_lblStatusSub.Text = (flag ? "Interrompido pela tecla de emergência" : "Aguardando uma ação");
		_lblStatusNote.Text = (flag ? "Tudo parado — pronto para recomeçar" : "Pronto para gravar");
		_playWatch.Reset();
		_playTotalMs = 0.0;
		SyncMini();
	}

	private void NewMacro()
	{
		if (ConfirmDiscard())
		{
			_macro = new Macro();
			_currentPath = null;
			_readyName = null;
			_dirty = false;
			_lastRowCount = -1;
			SetRowCount();
			ClearHistory();
			RefreshTitle();
			UpdateStateUi();
		}
	}

	private bool ConfirmDiscard()
	{
		if (!_dirty || _macro.IsEmpty)
		{
			return true;
		}
		return MessageBox.Show(this, "Descartar as alterações não salvas?", "Pulso", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
	}

	private void OpenMacro()
	{
		if (!ConfirmDiscard())
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Title = "Abrir macro",
			Filter = "Macro do Pulso (*.gmt)|*.gmt|Macro pronta (*.pulso)|*.pulso|TinyTask (*.rec)|*.rec|JSON (*.json)|*.json|Todos (*.*)|*.*",
			InitialDirectory = ((_s.LastFolder.Length > 0 && Directory.Exists(_s.LastFolder)) ? _s.LastFolder : Paths.MacrosDir)
		};
		if (openFileDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		try
		{
			LoadFile(openFileDialog.FileName);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Falha ao abrir:\r\n" + ex.Message, "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void OpenFile(string path)
	{
		LoadFile(path);
	}

	private void LoadFile(string path)
	{
		string text = Path.GetExtension(path).ToLowerInvariant();
		bool flag = text == ".pulso";
		Macro macro;
		if (flag)
		{
			MacroPackage? obj = JsonSerializer.Deserialize<MacroPackage>(File.ReadAllText(path), Macro.JsonOpts) ?? throw new InvalidDataException("Arquivo .pulso inválido.");
			macro = obj.Macro ?? new Macro();
			Macro macro2 = macro;
			if (macro2.Events == null)
			{
				List<MacroEvent> list2 = (macro2.Events = new List<MacroEvent>());
			}
			macro.Events.Sort((MacroEvent a, MacroEvent b) => a.T.CompareTo(b.T));
			obj.Play.ApplyTo(_s);
			_s.Save();
			ApplySettingsToUi();
		}
		else
		{
			macro = ((text == ".rec") ? Macro.ImportRec(path) : Macro.Load(path));
		}
		_macro = macro;
		_currentPath = ((flag || text == ".rec") ? null : path);
		_readyName = (flag ? Path.GetFileNameWithoutExtension(path) : null);
		_s.LastFolder = Path.GetDirectoryName(path) ?? "";
		AddRecent(path);
		ClearHistory();
		_dirty = text == ".rec";
		_lastRowCount = -1;
		SetRowCount();
		RefreshTitle();
		UpdateStateUi();
		_lblState.Text = (flag ? ("Pronta para reproduzir: " + Path.GetFileName(path)) : ("Carregado: " + Path.GetFileName(path)));
		AppLog.Write($"Macro carregada de {path} ({_macro.Events.Count} eventos).");
	}

	private bool SaveMacro(bool saveAs)
	{
		if (_macro.IsEmpty)
		{
			MessageBox.Show(this, "A macro está vazia.", "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return false;
		}
		string text = _currentPath ?? "";
		if (saveAs || string.IsNullOrEmpty(text))
		{
			using SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Title = "Salvar macro",
				Filter = "Macro do Pulso (*.gmt)|*.gmt|JSON (*.json)|*.json",
				FileName = ((_currentPath == null) ? "macro.gmt" : Path.GetFileName(_currentPath)),
				InitialDirectory = ((_s.LastFolder.Length > 0 && Directory.Exists(_s.LastFolder)) ? _s.LastFolder : Paths.MacrosDir)
			};
			if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
			{
				return false;
			}
			text = saveFileDialog.FileName;
		}
		try
		{
			_macro.Save(text);
			_currentPath = text;
			_readyName = null;
			_s.LastFolder = Path.GetDirectoryName(text) ?? "";
			AddRecent(text);
			_dirty = false;
			RefreshTitle();
			_lblState.Text = "Salvo: " + Path.GetFileName(text);
			AppLog.Write("Macro salva em " + text + ".");
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Falha ao salvar:\r\n" + ex.Message, "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return false;
		}
	}

	private void ImportRec()
	{
		if (!ConfirmDiscard())
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Title = "Importar arquivo do TinyTask",
			Filter = "TinyTask (*.rec)|*.rec|Todos (*.*)|*.*"
		};
		if (openFileDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		try
		{
			_macro = Macro.ImportRec(openFileDialog.FileName);
			_currentPath = null;
			_readyName = null;
			_dirty = true;
			_lastRowCount = -1;
			SetRowCount();
			RefreshTitle();
			UpdateStateUi();
			MessageBox.Show(this, $"Importado: {_macro.Events.Count} eventos.\r\n" + "Observação: o formato .rec não guarda roda do mouse nem botões X.", "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Falha na importação:\r\n" + ex.Message, "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void ExportRec()
	{
		if (_macro.IsEmpty)
		{
			return;
		}
		using SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Title = "Exportar para TinyTask",
			Filter = "TinyTask (*.rec)|*.rec",
			FileName = "macro.rec"
		};
		if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		try
		{
			int num = _macro.ExportRec(saveFileDialog.FileName);
			string text = ((num > 0) ? $"\r\n\r\n{num} evento(s) sem equivalente no formato .rec foram omitidos." : "");
			MessageBox.Show(this, "Arquivo .rec gerado:\r\n" + saveFileDialog.FileName + text, "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Falha ao exportar:\r\n" + ex.Message, "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OpenReadyMacro()
	{
		if (!ConfirmDiscard())
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Title = "Carregar macro pronta",
			Filter = "Macro pronta do Pulso (*.pulso)|*.pulso|Macro do Pulso (*.gmt)|*.gmt|Todos (*.*)|*.*",
			InitialDirectory = ((_s.LastFolder.Length > 0 && Directory.Exists(_s.LastFolder)) ? _s.LastFolder : Paths.MacrosDir)
		};
		if (openFileDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		try
		{
			LoadFile(openFileDialog.FileName);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Falha ao carregar a macro pronta:\r\n" + ex.Message, "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void ExportPackage()
	{
		if (_macro.IsEmpty)
		{
			MessageBox.Show(this, "Grave ou abra uma macro antes de exportar.", "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		using SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Title = "Exportar macro pronta",
			Filter = "Macro pronta do Pulso (*.pulso)|*.pulso",
			FileName = (string.IsNullOrEmpty(_currentPath) ? "macro.pulso" : (Path.GetFileNameWithoutExtension(_currentPath) + ".pulso")),
			InitialDirectory = ((_s.LastFolder.Length > 0 && Directory.Exists(_s.LastFolder)) ? _s.LastFolder : Paths.MacrosDir)
		};
		if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		try
		{
			MacroPackage macroPackage = new MacroPackage
			{
				Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName),
				CreatedAt = DateTime.Now,
				Events = _macro.Events.Count,
				DurationMs = _macro.Duration,
				Macro = _macro,
				Play = PlaybackPreset.From(_s)
			};
			File.WriteAllText(saveFileDialog.FileName, JsonSerializer.Serialize(macroPackage, Macro.JsonOpts));
			_s.LastFolder = Path.GetDirectoryName(saveFileDialog.FileName) ?? "";
			_lblState.Text = "Macro pronta: " + Path.GetFileName(saveFileDialog.FileName);
			AppLog.Write($"Macro pronta exportada: {saveFileDialog.FileName} ({macroPackage.Events} eventos).");
			MessageBox.Show(this, "Arquivo de macro pronto criado:\r\n" + saveFileDialog.FileName + $"\r\n\r\nEventos: {macroPackage.Events:N0}\r\nDuração: {(double)macroPackage.DurationMs / 1000.0:N1} s\r\n" + $"Velocidade: {macroPackage.Play.SpeedPercent}% · Repetições: {(macroPackage.Play.Continuous ? "contínuas" : macroPackage.Play.Loops.ToString())}\r\n\r\n" + "Para usar: Arquivo → Carregar macro pronta (Ctrl+Shift+O) e aperte Reproduzir.", "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Falha ao exportar:\r\n" + ex.Message, "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OpenFolder(string path)
	{
		try
		{
			Directory.CreateDirectory(path);
			Process.Start(new ProcessStartInfo
			{
				FileName = path,
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			Balloon("Falha ao abrir pasta: " + ex.Message);
		}
	}

	private void ShowLog()
	{
		try
		{
			string text = (File.Exists(Paths.LogFile) ? File.ReadAllText(Paths.LogFile) : "(log vazio)");
			if (text.Length > 200000)
			{
				string text2 = text;
				int length = text2.Length;
				int num = length - 200000;
				text = text2.Substring(num, length - num);
			}
			using Form form = new Form
			{
				Text = "Log — Pulso",
				ClientSize = new Size(820, 520),
				StartPosition = FormStartPosition.CenterParent,
				Font = new Font("Consolas", 9f)
			};
			TextBox textBox = new TextBox
			{
				Multiline = true,
				ReadOnly = true,
				ScrollBars = ScrollBars.Both,
				WordWrap = false,
				Dock = DockStyle.Fill,
				Text = text
			};
			form.Controls.Add(textBox);
			DarkUi.Apply(form);
			DarkUi.DarkScroll(textBox);
			HudChrome.Attach(form, "Log do Pulso");
			textBox.BackColor = Palette.PanelAlt;
			textBox.ForeColor = Palette.Text;
			ShowDialogSafe(form);
		}
		catch (Exception ex)
		{
			Balloon("Falha ao abrir log: " + ex.Message);
		}
	}

	private void ShowHotkeys()
	{
		string text = "Atalhos globais (configuráveis em Ferramentas → Configurações):\r\n\r\n  Gravar:            " + _s.HotkeyRecord + "\r\n  Reproduzir:        " + _s.HotkeyPlay + "\r\n  Pausar/continuar:  " + _s.HotkeyPause + "\r\n  Parar tudo:        " + _s.HotkeyStop + "\r\n  Modo compacto:     " + _s.HotkeyCompact + "\r\n\r\n  F12:               parada de emergência (sempre ativo)\r\n\r\nOs atalhos funcionam mesmo com o Pulso em segundo plano.";
		MessageBox.Show(this, text, "Atalhos", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private void ShowAbout()
	{
		using AboutForm aboutForm = new AboutForm();
		aboutForm.ShowDialog(this);
	}

	private void RemoveSelected()
	{
		if (_grid.SelectedRows.Count == 0 && _grid.CurrentRow == null)
		{
			return;
		}
		if (_recording || _playing)
		{
			return;
		}
		PushUndo();
		if (_grid.SelectedRows.Count == 0)
		{
			_macro.Events.RemoveAt(_grid.CurrentRow.Index);
		}
		else
		{
			foreach (int item in (from i in (from DataGridViewRow r in _grid.SelectedRows
					select r.Index).Distinct()
				orderby i descending
				select i).ToList())
			{
				if (item >= 0 && item < _macro.Events.Count)
				{
					_macro.Events.RemoveAt(item);
				}
			}
		}
		_macro.Events.Sort((MacroEvent a, MacroEvent b) => a.T.CompareTo(b.T));
		AfterEdit();
	}

	private void EditSelected()
	{
		int num = _grid.CurrentRow?.Index ?? (-1);
		if (num >= 0)
		{
			EditEventAt(num);
		}
	}

	private void EditEventAt(int index)
	{
		if (index < 0 || index >= _macro.Events.Count)
		{
			return;
		}
		using EventEditDialog eventEditDialog = new EventEditDialog(_macro.Events[index], index);
		if (ShowDialogSafe(eventEditDialog) == DialogResult.OK)
		{
			PushUndo();
			_macro.Events[index] = eventEditDialog.Result;
			_macro.Events.Sort((MacroEvent a, MacroEvent b) => a.T.CompareTo(b.T));
			AfterEdit();
		}
	}

	private void InsertPause()
	{
		int value = _grid.CurrentRow?.Index ?? 0;
		if (_macro.Events.Count == 0)
		{
			return;
		}
		value = Math.Clamp(value, 0, _macro.Events.Count - 1);
		_ = _macro.Events[value].T;
		int? num = NumericPrompt.Ask(this, "Inserir pausa", $"Adiar em quantos milissegundos todos os eventos a partir do evento #{value + 1}?", 1, 3600000, 1000, 100);
		if (num.HasValue)
		{
			PushUndo();
			for (int i = value; i < _macro.Events.Count; i++)
			{
				_macro.Events[i].T += num.Value;
			}
			AfterEdit();
		}
	}

	private void ScaleTimes()
	{
		if (_macro.Events.Count == 0)
		{
			return;
		}
		int? num = NumericPrompt.Ask(this, "Escala de tempo", "Multiplicador em % (100 = sem mudança, 200 = dobro, 50 = metade):", 1, 10000, 100, 5);
		if (!num.HasValue)
		{
			return;
		}
		PushUndo();
		double num2 = (double)num.Value / 100.0;
		foreach (MacroEvent @event in _macro.Events)
		{
			@event.T = (long)Math.Round((double)@event.T * num2);
		}
		AfterEdit();
	}

	private void RemoveInitialDelay()
	{
		if (_macro.Events.Count == 0)
		{
			return;
		}
		long t = _macro.Events[0].T;
		if (t <= 0)
		{
			return;
		}
		PushUndo();
		foreach (MacroEvent @event in _macro.Events)
		{
			@event.T -= t;
		}
		AfterEdit();
	}

	private void ClearMacro()
	{
		if (_macro.Events.Count <= 0 || !_s.ConfirmClear || MessageBox.Show(this, "Limpar todos os eventos da macro?", "Pulso", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			if (_macro.Events.Count > 0)
			{
				PushUndo();
			}
			_macro = new Macro();
			_currentPath = null;
			_readyName = null;
			_dirty = false;
			_lastRowCount = -1;
			SetRowCount();
			RefreshTitle();
			UpdateStateUi();
		}
	}

	private void AfterEdit()
	{
		_dirty = true;
		_readyName = null;
		_lastRowCount = -1;
		SetRowCount();
		_grid.Invalidate();
		RefreshTitle();
		UpdateStateUi();
	}

	private static string Snapshot(Macro macro)
	{
		return JsonSerializer.Serialize(macro, Macro.JsonOpts);
	}

	private void PushUndo()
	{
		string item = Snapshot(_macro);
		if (_undo.Count > 0 && _undo[_undo.Count - 1] == item)
		{
			return;
		}
		_undo.Add(item);
		if (_undo.Count > UndoLimit)
		{
			_undo.RemoveAt(0);
		}
		_redo.Clear();
		UpdateUndoUi();
	}

	private void Undo()
	{
		if (_recording || _playing)
		{
			return;
		}
		if (_undo.Count == 0)
		{
			_lblStatusNote.Text = "Nada para desfazer";
			return;
		}
		_redo.Add(Snapshot(_macro));
		string json = _undo[_undo.Count - 1];
		_undo.RemoveAt(_undo.Count - 1);
		ApplySnapshot(json);
		_lblState.Text = "Desfeito";
	}

	private void Redo()
	{
		if (_recording || _playing)
		{
			return;
		}
		if (_redo.Count == 0)
		{
			_lblStatusNote.Text = "Nada para refazer";
			return;
		}
		_undo.Add(Snapshot(_macro));
		string json = _redo[_redo.Count - 1];
		_redo.RemoveAt(_redo.Count - 1);
		ApplySnapshot(json);
		_lblState.Text = "Refeito";
	}

	private void ApplySnapshot(string json)
	{
		Macro? macro = JsonSerializer.Deserialize<Macro>(json, Macro.JsonOpts);
		if (macro == null)
		{
			return;
		}
		if (macro.Events == null)
		{
			macro.Events = new List<MacroEvent>();
		}
		macro.Events.RemoveAll((MacroEvent e) => e == null);
		macro.Events.Sort((MacroEvent a, MacroEvent b) => a.T.CompareTo(b.T));
		_macro = macro;
		_readyName = null;
		_dirty = true;
		_lastRowCount = -1;
		SetRowCount();
		_grid.Invalidate();
		RefreshTitle();
		UpdateStateUi();
		UpdateUndoUi();
	}

	private void UpdateUndoUi()
	{
		_miUndo.Enabled = _undo.Count > 0;
		_miRedo.Enabled = _redo.Count > 0;
	}

	private void ClearHistory()
	{
		_undo.Clear();
		_redo.Clear();
		UpdateUndoUi();
	}

	private void DuplicateSelected()
	{
		if (_recording || _playing || _macro.IsEmpty)
		{
			return;
		}
		List<int> indexes = (_grid.SelectedRows.Count > 0
			? (from DataGridViewRow r in _grid.SelectedRows select r.Index)
			: ((_grid.CurrentRow == null) ? new List<int>() : new List<int> { _grid.CurrentRow.Index }))
			.Where((int i) => i >= 0 && i < _macro.Events.Count).Distinct().OrderBy((int i) => i).ToList();
		if (indexes.Count == 0)
		{
			return;
		}
		PushUndo();
		int last = indexes[indexes.Count - 1];
		List<MacroEvent> block = indexes.Select((int i) => _macro.Events[i]).ToList();
		long start = block[0].T;
		long end = block[block.Count - 1].T;
		long span = Math.Max(0L, end - start);
		long gap = Math.Max(120L, Math.Min(1000L, span / 10));
		long delta = span + gap;
		for (int j = last + 1; j < _macro.Events.Count; j++)
		{
			_macro.Events[j].T += delta;
		}
		foreach (MacroEvent source in block)
		{
			MacroEvent copy = source.Clone();
			copy.T = source.T + delta;
			_macro.Events.Add(copy);
		}
		_macro.Events.Sort((MacroEvent a, MacroEvent b) => a.T.CompareTo(b.T));
		AfterEdit();
		_lblState.Text = $"{block.Count} evento(s) duplicado(s)";
	}

	private void BuildRecentMenu()
	{
		_miRecent.DropDownItems.Clear();
		List<string> list = _s.RecentFiles.Where((string p) => !string.IsNullOrEmpty(p) && File.Exists(p)).ToList();
		if (list.Count == 0)
		{
			_miRecent.DropDownItems.Add(new ToolStripMenuItem("Nenhum arquivo recente")
			{
				Enabled = false
			});
			return;
		}
		foreach (string path in list)
		{
			string file = path;
			ToolStripMenuItem item = new ToolStripMenuItem(Path.GetFileName(file))
			{
				ToolTipText = file
			};
			item.Click += delegate
			{
				if (ConfirmDiscard())
				{
					try
					{
						LoadFile(file);
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, "Falha ao abrir:\r\n" + ex.Message, "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
				}
			};
			_miRecent.DropDownItems.Add(item);
		}
		_miRecent.DropDownItems.Add(new ToolStripSeparator());
		_miRecent.DropDownItems.Add(Item("Limpar lista", Keys.None, delegate
		{
			_s.RecentFiles.Clear();
			_s.Save();
		}));
	}

	private void AddRecent(string path)
	{
		try
		{
			path = Path.GetFullPath(path);
		}
		catch
		{
			return;
		}
		_s.RecentFiles.RemoveAll((string p) => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
		_s.RecentFiles.Insert(0, path);
		if (_s.RecentFiles.Count > 8)
		{
			_s.RecentFiles.RemoveRange(8, _s.RecentFiles.Count - 8);
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == (Keys.Z | Keys.Control | Keys.Shift))
		{
			Redo();
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void OnDragEnter(object? sender, DragEventArgs e)
	{
		IDataObject? data = e.Data;
		if (data != null && data.GetDataPresent(DataFormats.FileDrop))
		{
			e.Effect = DragDropEffects.Copy;
		}
	}

	private void OnDragDrop(object? sender, DragEventArgs e)
	{
		if (!(e.Data?.GetData(DataFormats.FileDrop) is string[] array) || array.Length == 0)
		{
			return;
		}
		string path = array[0];
		string text = Path.GetExtension(path).ToLowerInvariant();
		bool flag;
		switch (text)
		{
		case ".gmt":
		case ".rec":
		case ".json":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			Balloon("Formato não suportado: " + text);
		}
		else if (ConfirmDiscard())
		{
			try
			{
				LoadFile(path);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Falha ao abrir:\r\n" + ex.Message, "Pulso", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (_recording || _playing)
		{
			if (MessageBox.Show(this, "Há uma gravação/reprodução em andamento. Parar e sair?", "Pulso", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				e.Cancel = true;
				return;
			}
			_player.Stop();
			if (_recording)
			{
				_engine.StopRecording();
				_recording = false;
			}
		}
		if (!ConfirmDiscard())
		{
			e.Cancel = true;
			return;
		}
		_s.Save();
		_timer.Stop();
		_tray.Visible = false;
		_engine.Dispose();
		_player.Stop();
		base.OnFormClosing(e);
	}
}
