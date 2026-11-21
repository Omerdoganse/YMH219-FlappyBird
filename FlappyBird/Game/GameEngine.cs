using FlappyBird.Models;

namespace FlappyBird.Game;

/// <summary>
/// Oyunun kurallarını ve durumunu yöneten sınıf.
/// Form bu sınıfı her timer tick'inde günceller ve ardından çizer.
/// </summary>
public class GameEngine
{
    private const int ObstacleCount = 3;
    private const float ObstacleWidth = 70;
    private const float ObstacleSpacing = 230;
    private const float BaseSpeed = 3.5f;
    private const float SpeedStep = 0.5f;
    private const float MaxSpeed = 8f;
    private const float BaseGap = 175;
    private const float MinGap = 130;
    private const int PointsPerLevel = 5;

    private readonly List<Obstacle> _obstacles = [];

    public float Width { get; }
    public float Height { get; }
    public float GroundHeight { get; } = 70;
    public float GroundY => Height - GroundHeight;

    public Bird Bird { get; }
    public IReadOnlyList<Obstacle> Obstacles => _obstacles;

    public GameState State { get; private set; } = GameState.Ready;
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public int Level => Score / PointsPerLevel + 1;
    public float Speed { get; private set; } = BaseSpeed;

    /// <summary>Zeminin kayma miktarı (çizimde kullanılır).</summary>
    public float GroundOffset { get; private set; }

    public event EventHandler? ScoreChanged;
    public event EventHandler? GameEnded;
    public event EventHandler? StateChanged;

    public GameEngine(float width, float height, int highScore = 0)
    {
        Width = width;
        Height = height;
        HighScore = highScore;
        Bird = new Bird(width * 0.25f, height * 0.4f);
        CreateObstacles();
    }

    private void CreateObstacles()
    {
        _obstacles.Clear();
        for (int i = 0; i < ObstacleCount; i++)
        {
            float x = Width + 100 + i * ObstacleSpacing;
            _obstacles.Add(new Obstacle(x, GroundY, ObstacleWidth, BaseGap, Speed));
        }
    }

    /// <summary>Oyunu baştan başlatmak için tüm değerleri sıfırlar.</summary>
    public void Reset()
    {
        Score = 0;
        Speed = BaseSpeed;
        GroundOffset = 0;
        Bird.Reset();
        CreateObstacles();
        SetState(GameState.Ready);
        ScoreChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Oyuncu zıplama tuşuna bastığında çağrılır.</summary>
    public void Flap()
    {
        switch (State)
        {
            case GameState.Ready:
                SetState(GameState.Running);
                Bird.Flap();
                break;
            case GameState.Running:
                Bird.Flap();
                break;
        }
    }

    public void TogglePause()
    {
        if (State == GameState.Running) SetState(GameState.Paused);
        else if (State == GameState.Paused) SetState(GameState.Running);
    }

    /// <summary>Bir oyun karesini işler: hareket, skor ve çarpışma kontrolü.</summary>
    public void Update()
    {
        if (State != GameState.Running) return;

        Bird.Update();
        GroundOffset = (GroundOffset + Speed) % 40;

        float lastX = _obstacles.Max(o => o.X);
        foreach (var obstacle in _obstacles)
        {
            obstacle.Speed = Speed;
            obstacle.Update();

            // Ekran dışına çıkan engeli en sondaki engelin arkasına taşı
            if (obstacle.IsOffScreen)
            {
                obstacle.Reposition(lastX + ObstacleSpacing, CurrentGap());
                lastX = obstacle.X;
            }

            // Kuş engeli geçtiyse skor artır
            if (!obstacle.Passed && obstacle.X + obstacle.Width < Bird.X)
            {
                obstacle.Passed = true;
                AddPoint();
            }

            if (obstacle.CollidesWith(Bird.Bounds))
            {
                EndGame();
                return;
            }
        }

        if (Bird.HitsGround(GroundY))
            EndGame();
    }

    private float CurrentGap() => Math.Max(MinGap, BaseGap - (Level - 1) * 8);

    private void AddPoint()
    {
        int oldLevel = Level;
        Score++;
        if (Level > oldLevel)
            Speed = Math.Min(MaxSpeed, Speed + SpeedStep);
        ScoreChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EndGame()
    {
        if (Score > HighScore)
            HighScore = Score;
        SetState(GameState.GameOver);
        GameEnded?.Invoke(this, EventArgs.Empty);
    }

    private void SetState(GameState newState)
    {
        if (State == newState) return;
        State = newState;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
