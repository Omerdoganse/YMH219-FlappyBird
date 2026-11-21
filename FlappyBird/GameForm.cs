using System.Drawing.Drawing2D;
using FlappyBird.Game;

namespace FlappyBird;

/// <summary>
/// Oyunun ana penceresi: arayüz kontrolleri, klavye olayları,
/// Timer ile oyun döngüsü ve çizim işlemleri burada yapılır.
/// </summary>
public class GameForm : Form
{
    private const int CanvasWidth = 480;
    private const int CanvasHeight = 640;

    private readonly GameEngine _engine;
    private readonly System.Windows.Forms.Timer _gameTimer;
    private readonly GameCanvas _canvas;

    private readonly Label _lblScore;
    private readonly Label _lblHighScore;
    private readonly Label _lblLevel;
    private readonly Button _btnNewGame;
    private readonly Button _btnPause;

    public GameForm()
    {
        Text = "Kanat Kaçışı - YMH219";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(30, 24, 48);
        KeyPreview = true;

        // --- Üst bilgi paneli ---
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = Color.FromArgb(45, 35, 75),
            Padding = new Padding(8, 0, 8, 0)
        };

        _lblScore = CreateInfoLabel("Skor: 0", 8);
        _lblHighScore = CreateInfoLabel("En Yüksek: 0", 100);
        _lblLevel = CreateInfoLabel("Seviye: 1", 230);

        _btnNewGame = CreateButton("Yeni Oyun", 318);
        _btnNewGame.Click += (_, _) => StartNewGame();

        _btnPause = CreateButton("Duraklat", 398);
        _btnPause.Click += (_, _) => _engine!.TogglePause();

        topPanel.Controls.AddRange([_lblScore, _lblHighScore, _lblLevel, _btnNewGame, _btnPause]);

        // --- Oyun alanı ---
        _canvas = new GameCanvas
        {
            Dock = DockStyle.Fill
        };
        _canvas.Paint += Canvas_Paint;

        Controls.Add(_canvas);
        Controls.Add(topPanel);
        ClientSize = new Size(CanvasWidth, CanvasHeight + topPanel.Height);

        // --- Oyun motoru ---
        _engine = new GameEngine(CanvasWidth, CanvasHeight);
        _engine.ScoreChanged += (_, _) => UpdateInfoLabels();
        _engine.StateChanged += (_, _) => UpdatePauseButton();

        // --- Oyun döngüsü (yaklaşık 60 FPS) ---
        _gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
        _gameTimer.Tick += GameTimer_Tick;
        _gameTimer.Start();

        UpdateInfoLabels();
    }

    private static Label CreateInfoLabel(string text, int left) => new()
    {
        Text = text,
        AutoSize = true,
        Left = left,
        Top = 15,
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 10, FontStyle.Bold)
    };

    private static Button CreateButton(string text, int left) => new()
    {
        Text = text,
        Left = left,
        Top = 10,
        Width = 74,
        Height = 28,
        FlatStyle = FlatStyle.Flat,
        ForeColor = Color.White,
        BackColor = Color.FromArgb(90, 70, 160),
        TabStop = false // Boşluk tuşu butonu tetiklemesin
    };

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        _engine.Update();
        _canvas.Invalidate();
    }

    private void StartNewGame()
    {
        _engine.Reset();
        _canvas.Focus();
    }

    /// <summary>
    /// Klavye girişleri. ProcessCmdKey kullanıldığı için odak bir butonda olsa bile
    /// tuşlar oyuna iletilir.
    /// </summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Space:
            case Keys.Up:
            case Keys.W:
                if (_engine.State == GameState.GameOver)
                    StartNewGame();
                else
                    _engine.Flap();
                return true;

            case Keys.Enter:
            case Keys.R:
                StartNewGame();
                return true;

            case Keys.P:
            case Keys.Escape:
                _engine.TogglePause();
                return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void UpdateInfoLabels()
    {
        _lblScore.Text = $"Skor: {_engine.Score}";
        _lblHighScore.Text = $"En Yüksek: {_engine.HighScore}";
        _lblLevel.Text = $"Seviye: {_engine.Level}";
    }

    private void UpdatePauseButton()
    {
        _btnPause.Text = _engine.State == GameState.Paused ? "Devam" : "Duraklat";
        _btnPause.Enabled = _engine.State is GameState.Running or GameState.Paused;
        UpdateInfoLabels();
    }

    // ------------------------------------------------------------------
    // Çizim
    // ------------------------------------------------------------------

    private void Canvas_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        DrawBackground(g);
        foreach (var obstacle in _engine.Obstacles)
            obstacle.Draw(g);
        DrawGround(g);
        _engine.Bird.Draw(g);
        DrawHud(g);
        DrawOverlay(g);
    }

    private void DrawBackground(Graphics g)
    {
        var rect = new Rectangle(0, 0, CanvasWidth, (int)_engine.GroundY);
        using var sky = new LinearGradientBrush(rect, Color.FromArgb(255, 170, 110), Color.FromArgb(120, 90, 200), 90f);
        g.FillRectangle(sky, rect);

        // Arka planda güneş
        using var sun = new SolidBrush(Color.FromArgb(120, 255, 240, 180));
        g.FillEllipse(sun, 320, 90, 110, 110);
    }

    private void DrawGround(Graphics g)
    {
        float y = _engine.GroundY;
        using (var ground = new SolidBrush(Color.FromArgb(70, 55, 40)))
            g.FillRectangle(ground, 0, y, CanvasWidth, _engine.GroundHeight);
        using (var grass = new SolidBrush(Color.FromArgb(90, 170, 90)))
            g.FillRectangle(grass, 0, y, CanvasWidth, 14);

        // Kayan zemin çizgileri hareket hissi verir
        using var stripe = new Pen(Color.FromArgb(60, 130, 60), 4);
        for (float x = -_engine.GroundOffset; x < CanvasWidth; x += 40)
            g.DrawLine(stripe, x, y + 14, x + 20, y + 14);
    }

    private void DrawHud(Graphics g)
    {
        using var font = new Font("Segoe UI", 34, FontStyle.Bold);
        string text = _engine.Score.ToString();
        var size = g.MeasureString(text, font);
        float x = (CanvasWidth - size.Width) / 2;
        g.DrawString(text, font, Brushes.Black, x + 2, 22);
        g.DrawString(text, font, Brushes.White, x, 20);
    }

    private void DrawOverlay(Graphics g)
    {
        switch (_engine.State)
        {
            case GameState.Ready:
                DrawCenteredBox(g, "Hazır mısın?",
                    "Başlamak için BOŞLUK / ↑ tuşuna bas\nP: Duraklat   R: Yeniden Başlat");
                break;
            case GameState.Paused:
                DrawCenteredBox(g, "Duraklatıldı", "Devam etmek için P tuşuna bas");
                break;
            case GameState.GameOver:
                DrawCenteredBox(g, "GAME OVER",
                    $"Skor: {_engine.Score}    En Yüksek: {_engine.HighScore}\n" +
                    "Yeniden başlamak için BOŞLUK veya R");
                break;
        }
    }

    private static void DrawCenteredBox(Graphics g, string title, string message)
    {
        var box = new RectangleF(50, 220, CanvasWidth - 100, 150);
        using (var bg = new SolidBrush(Color.FromArgb(200, 25, 20, 45)))
            g.FillRectangle(bg, box);
        using (var border = new Pen(Color.FromArgb(200, 160, 130, 255), 3))
            g.DrawRectangle(border, box.X, box.Y, box.Width, box.Height);

        using var titleFont = new Font("Segoe UI", 24, FontStyle.Bold);
        using var msgFont = new Font("Segoe UI", 11);
        using var center = new StringFormat { Alignment = StringAlignment.Center };

        g.DrawString(title, titleFont, Brushes.White, new RectangleF(box.X, box.Y + 18, box.Width, 50), center);
        g.DrawString(message, msgFont, Brushes.Gainsboro, new RectangleF(box.X, box.Y + 75, box.Width, 70), center);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _gameTimer.Stop();
        base.OnFormClosing(e);
    }
}
