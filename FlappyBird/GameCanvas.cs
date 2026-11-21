namespace FlappyBird;

/// <summary>
/// Titreşimsiz çizim için çift tamponlama (double buffering) açık panel.
/// </summary>
public class GameCanvas : Panel
{
    public GameCanvas()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
        TabStop = false;
    }
}
