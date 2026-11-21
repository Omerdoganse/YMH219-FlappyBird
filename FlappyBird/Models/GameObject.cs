namespace FlappyBird.Models;

/// <summary>
/// Ekranda konumu ve boyutu olan tüm oyun nesneleri için temel sınıf.
/// </summary>
public abstract class GameObject
{
    public float X { get; protected set; }
    public float Y { get; protected set; }
    public float Width { get; protected set; }
    public float Height { get; protected set; }

    /// <summary>Nesnenin çarpışma kontrolünde kullanılan dikdörtgeni.</summary>
    public virtual RectangleF Bounds => new(X, Y, Width, Height);

    /// <summary>Her oyun karesinde (timer tick) çağrılır.</summary>
    public abstract void Update();

    /// <summary>Nesneyi verilen grafik yüzeyine çizer.</summary>
    public abstract void Draw(Graphics g);
}
