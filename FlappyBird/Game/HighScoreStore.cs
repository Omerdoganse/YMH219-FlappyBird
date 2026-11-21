namespace FlappyBird.Game;

/// <summary>
/// En yüksek skoru kullanıcının AppData klasöründeki bir metin dosyasında saklar.
/// </summary>
public class HighScoreStore
{
    private readonly string _filePath;

    public HighScoreStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YMH219-FlappyBird", "highscore.txt");
    }

    /// <summary>Kayıtlı skoru okur. Dosya yoksa veya bozuksa 0 döner.</summary>
    public int Load()
    {
        try
        {
            if (!File.Exists(_filePath)) return 0;
            string text = File.ReadAllText(_filePath).Trim();
            return int.TryParse(text, out int score) && score >= 0 ? score : 0;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return 0;
        }
    }

    /// <summary>Skoru dosyaya yazar. Başarısız olursa false döner.</summary>
    public bool Save(int score)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, score.ToString());
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
