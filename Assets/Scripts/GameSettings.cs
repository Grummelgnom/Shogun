using UnityEngine;

public static class GameSettings
{
    // Wird vom Menü gesetzt (oder bleibt Default, wenn du ohne Menü startest).
    public static GameMode SelectedMode = GameMode.TwoPlayer;

    // Wenn true: beide Spieler werden von der KI gesteuert (gemeinsame Stärke/Tempo).
    public static bool AIVsAI = false;

    // 1..4 (1 = leicht/langsam, 4 = stark/schnell) – GameManager mappt das auf Depth/Delay.
    public static int AIStrength = 2;
    public static int AITempo = 2;

    // Optional: wenn du es im Inspector/Debug schnell ändern willst.
    public static void ResetToDefaults()
    {
        SelectedMode = GameMode.TwoPlayer;
        AIVsAI = false;
        AIStrength = 2;
        AITempo = 2;
    }

    public static int Clamp1To4(int v)
    {
        if (v < 1) return 1;
        if (v > 4) return 4;
        return v;
    }
}
