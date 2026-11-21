using System.Drawing.Drawing2D;

namespace FlappyBird.Models;

/// <summary>
/// Üst ve alt sütundan oluşan, ortasında geçiş boşluğu bulunan engel.
/// Sağdan sola hareket eder; ekran dışına çıkınca yeniden konumlandırılır.
/// </summary>
public class Obstacle : GameObject
{
    private readonly float _playAreaHeight;

    /// <summary>Boşluğun üst kenarının Y koordinatı.</summary>
    public float GapY { get; private set; }

    /// <summary>Kuşun geçebileceği boşluğun yüksekliği.</summary>
    public float GapHeight { get; private set; }

    /// <summary>Hareket hızı (piksel/kare). Oyun motoru tarafından ayarlanır.</summary>
    public float Speed { get; set; }

    /// <summary>Kuş bu engeli geçtiyse true olur; skorun iki kez sayılmasını engeller.</summary>
    public bool Passed { get; set; }

    public Obstacle(float x, float playAreaHeight, float width, float gapHeight, float speed)
    {
        _playAreaHeight = playAreaHeight;
        Width = width;
        Height = playAreaHeight;
        GapHeight = gapHeight;
        Speed = speed;
        Reposition(x, gapHeight);
    }

    public RectangleF TopRect => new(X, 0, Width, GapY);
    public RectangleF BottomRect => new(X, GapY + GapHeight, Width, _playAreaHeight - (GapY + GapHeight));

    /// <summary>Engel tamamen ekranın solundan çıktı mı?</summary>
    public bool IsOffScreen => X + Width < 0;

    /// <summary>Engeli yeni bir X konumuna taşır ve rastgele bir boşluk yüksekliği seçer.</summary>
    public void Reposition(float newX, float gapHeight)
    {
        X = newX;
        GapHeight = gapHeight;
        const int margin = 60;
        int maxGapY = (int)(_playAreaHeight - gapHeight - margin);
        GapY = Random.Shared.Next(margin, Math.Max(margin + 1, maxGapY));
        Passed = false;
    }

    public bool CollidesWith(RectangleF other) =>
        TopRect.IntersectsWith(other) || BottomRect.IntersectsWith(other);

    public override void Update() => X -= Speed;

    public override void Draw(Graphics g)
    {
        DrawColumn(g, TopRect, capAtBottom: true);
        DrawColumn(g, BottomRect, capAtBottom: false);
    }

    private static void DrawColumn(Graphics g, RectangleF rect, bool capAtBottom)
    {
        if (rect.Height <= 0) return;

        using (var brush = new LinearGradientBrush(
                   new RectangleF(rect.X, rect.Y, rect.Width, Math.Max(1, rect.Height)),
                   Color.FromArgb(110, 80, 190), Color.FromArgb(70, 40, 140), 0f))
            g.FillRectangle(brush, rect);

        // Sütun başlığı
        const float capHeight = 18;
        var cap = capAtBottom
            ? new RectangleF(rect.X - 5, rect.Bottom - capHeight, rect.Width + 10, capHeight)
            : new RectangleF(rect.X - 5, rect.Y, rect.Width + 10, capHeight);
        using (var capBrush = new SolidBrush(Color.FromArgb(150, 120, 230)))
            g.FillRectangle(capBrush, cap);

        using var pen = new Pen(Color.FromArgb(40, 20, 90), 2);
        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        g.DrawRectangle(pen, cap.X, cap.Y, cap.Width, cap.Height);
    }
}
