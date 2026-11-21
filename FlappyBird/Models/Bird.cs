using System.Drawing.Drawing2D;

namespace FlappyBird.Models;

/// <summary>
/// Oyuncunun kontrol ettiği kuş. Yer çekimi ile düşer, kanat çırpınca yükselir.
/// </summary>
public class Bird : GameObject
{
    public const float Gravity = 0.55f;
    public const float FlapStrength = -8.5f;
    public const float MaxFallSpeed = 11f;

    private readonly float _startX;
    private readonly float _startY;

    /// <summary>Dikey hız (pozitif değer aşağı yönü gösterir).</summary>
    public float VelocityY { get; private set; }

    public Bird(float startX, float startY, float size = 34)
    {
        _startX = startX;
        _startY = startY;
        Width = size;
        Height = size;
        Reset();
    }

    /// <summary>Çarpışma kutusunu biraz küçültür; oyun daha adil hissettirir.</summary>
    public override RectangleF Bounds => new(X + 4, Y + 4, Width - 8, Height - 8);

    public void Reset()
    {
        X = _startX;
        Y = _startY;
        VelocityY = 0;
    }

    /// <summary>Kuşu yukarı doğru iter.</summary>
    public void Flap() => VelocityY = FlapStrength;

    public override void Update()
    {
        VelocityY = Math.Min(VelocityY + Gravity, MaxFallSpeed);
        Y += VelocityY;
        if (Y < 0)
        {
            Y = 0;
            VelocityY = 0;
        }
    }

    /// <summary>Kuşun zemine çarpıp çarpmadığını kontrol eder.</summary>
    public bool HitsGround(float groundY) => Y + Height >= groundY;

    public override void Draw(Graphics g)
    {
        var state = g.Save();
        // Hıza göre kuşu hafifçe döndür (yükselirken yukarı, düşerken aşağı bakar)
        float angle = Math.Clamp(VelocityY * 4f, -30f, 70f);
        g.TranslateTransform(X + Width / 2, Y + Height / 2);
        g.RotateTransform(angle);

        var body = new RectangleF(-Width / 2, -Height / 2, Width, Height);
        using (var brush = new LinearGradientBrush(body, Color.FromArgb(255, 140, 60), Color.FromArgb(220, 60, 40), 90f))
            g.FillEllipse(brush, body);
        using (var pen = new Pen(Color.FromArgb(120, 30, 20), 2))
            g.DrawEllipse(pen, body);

        // Kanat
        using (var wing = new SolidBrush(Color.FromArgb(255, 210, 120)))
            g.FillEllipse(wing, -Width / 2 + 2, -2, Width / 2, Height / 3);

        // Göz
        g.FillEllipse(Brushes.White, Width / 8, -Height / 3, Width / 3, Width / 3);
        g.FillEllipse(Brushes.Black, Width / 8 + 6, -Height / 3 + 4, Width / 7, Width / 7);

        // Gaga
        PointF[] beak =
        [
            new(Width / 2 - 2, -3),
            new(Width / 2 + 9, 2),
            new(Width / 2 - 2, 7)
        ];
        g.FillPolygon(Brushes.Gold, beak);

        g.Restore(state);
    }
}
