namespace FlappyBird.Game;

/// <summary>Oyunun bulunabileceği durumlar.</summary>
public enum GameState
{
    /// <summary>Oyun başlamayı bekliyor.</summary>
    Ready,
    /// <summary>Oyun oynanıyor.</summary>
    Running,
    /// <summary>Oyun duraklatıldı.</summary>
    Paused,
    /// <summary>Kuş çarptı, oyun bitti.</summary>
    GameOver
}
