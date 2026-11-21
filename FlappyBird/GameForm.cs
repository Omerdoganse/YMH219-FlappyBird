namespace FlappyBird;

/// <summary>
/// Oyunun ana penceresi. (İskelet sürüm)
/// </summary>
public class GameForm : Form
{
    public GameForm()
    {
        Text = "Kanat Kaçışı - YMH219";
        ClientSize = new Size(480, 640);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
    }
}
