using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Playing, Paused, GameOver }

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; }
    public event Action<GameState> StateChanged;
    private bool audioSettings;
    private Health playerHealth;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null) new GameObject("GameManager").AddComponent<GameManager>();
    }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SetState(GameState.Menu);
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var player = FindFirstObjectByType<MeleeAttack>();
        playerHealth = player != null ? player.GetComponent<Health>() : null;
    }
    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
        if (audioSettings) { audioSettings = false; PlayerPrefs.Save(); Confirm(); return; }
        if (State == GameState.Playing) Pause();
        else if (State == GameState.Paused) Resume();
    }
    void Confirm() { if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("01_ui_confirm"); }
    public void StartGame()
    {
        if (State != GameState.Menu) return;
        Confirm(); SetState(GameState.Playing);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayAmbience("15_dungeon_ambience");
    }
    public void Pause()
    {
        if (State != GameState.Playing) return;
        Confirm(); SetState(GameState.Paused);
    }
    public void Resume()
    {
        if (State != GameState.Paused) return;
        Confirm(); SetState(GameState.Playing);
    }
    [ContextMenu("Test Game Over")]
    public void GameOver()
    {
        if (State != GameState.Playing) return;
        SetState(GameState.GameOver);
        if (AudioManager.Instance != null) { AudioManager.Instance.StopAmbience(); AudioManager.Instance.PlaySfx("13_game_over"); }
    }
    public void Restart()
    {
        Confirm(); SetState(GameState.Playing);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayAmbience("15_dungeon_ambience");
    }
    public void ReturnToMenu()
    {
        Confirm();
        if (AudioManager.Instance != null) AudioManager.Instance.StopAmbience();
        SetState(GameState.Menu);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void SetState(GameState state)
    {
        audioSettings = false;
        State = state;
        Time.timeScale = state == GameState.Playing ? 1f : 0f;
        StateChanged?.Invoke(state);
    }
    void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerPrefs.Save();
        Instance = null;
        Time.timeScale = 1f;
    }
    static void Fill(Rect rect, Color color)
    {
        var old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = old;
    }
    void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        if (scale <= 0f) return;
        var oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280f * scale) / 2f, (Screen.height - 720f * scale) / 2f, 0), Quaternion.identity, Vector3.one * scale);
        var title = new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        title.normal.textColor = new Color(0.95f, 0.88f, 0.69f);
        var subtitle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        subtitle.normal.textColor = new Color(0.77f, 0.81f, 0.85f);
        var button = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        var text = new GUIStyle(GUI.skin.label) { fontSize = 17, alignment = TextAnchor.MiddleLeft };
        text.normal.textColor = Color.white;
        if (State == GameState.Playing)
        {
            if (playerHealth != null)
            {
                Fill(new Rect(22, 22, 260, 58), new Color(0.04f, 0.06f, 0.1f, 0.88f));
                GUI.Label(new Rect(34, 25, 230, 23), "HEALTH   " + playerHealth.Current + " / " + playerHealth.Max, text);
                Fill(new Rect(34, 55, 230, 11), new Color(0.23f, 0.12f, 0.13f));
                Fill(new Rect(34, 55, 230 * Mathf.Clamp01((float)playerHealth.Current / Mathf.Max(1, playerHealth.Max)), 11), new Color(0.79f, 0.24f, 0.22f));
            }
            if (GUI.Button(new Rect(1124, 22, 130, 44), "PAUSE", button)) Pause();
        }
        else
        {
            Fill(new Rect(0, 0, 1280, 720), new Color(0.01f, 0.02f, 0.04f, 0.62f));
            Fill(new Rect(400, 102, 480, 516), new Color(0.05f, 0.08f, 0.13f, 0.97f));
            Fill(new Rect(400, 102, 480, 5), new Color(0.75f, 0.55f, 0.30f));
            string heading = audioSettings ? "AUDIO SETTINGS" : State == GameState.Menu ? "DUNGEON CRAWLER" : State == GameState.Paused ? "PAUSED" : "GAME OVER";
            string description = audioSettings ? "Adjust your sound" : State == GameState.Menu ? "Enter the dungeon and survive" : State == GameState.Paused ? "Take a breath" : "Your adventure has ended";
            GUI.Label(new Rect(420, 132, 440, 54), heading, title);
            GUI.Label(new Rect(420, 190, 440, 30), description, subtitle);
            if (audioSettings)
            {
                var audio = AudioManager.Instance;
                if (audio != null)
                {
                    GUI.Label(new Rect(456, 249, 368, 30), "Sound effects", text);
                    float sfx = GUI.HorizontalSlider(new Rect(456, 286, 368, 24), audio.SfxVolume, 0f, 1f);
                    if (Mathf.Abs(sfx - audio.SfxVolume) > 0.001f) audio.SetSfxVolume(sfx);
                    GUI.Label(new Rect(456, 335, 368, 30), "Dungeon ambience", text);
                    float ambience = GUI.HorizontalSlider(new Rect(456, 372, 368, 24), audio.AmbienceVolume, 0f, 1f);
                    if (Mathf.Abs(ambience - audio.AmbienceVolume) > 0.001f) audio.SetAmbienceVolume(ambience);
                }
                if (GUI.Button(new Rect(456, 465, 368, 50), "BACK", button)) { audioSettings = false; PlayerPrefs.Save(); Confirm(); }
            }
            else if (State == GameState.Menu)
            {
                if (GUI.Button(new Rect(456, 266, 368, 54), "START", button)) StartGame();
                if (GUI.Button(new Rect(456, 335, 368, 54), "AUDIO SETTINGS", button)) { Confirm(); audioSettings = true; }
                GUI.Label(new Rect(440, 530, 400, 35), "Pause anytime with Esc", subtitle);
            }
            else if (State == GameState.Paused)
            {
                if (GUI.Button(new Rect(456, 244, 368, 48), "RESUME", button)) Resume();
                if (GUI.Button(new Rect(456, 304, 368, 48), "RESTART", button)) Restart();
                if (GUI.Button(new Rect(456, 364, 368, 48), "AUDIO SETTINGS", button)) { Confirm(); audioSettings = true; }
                if (GUI.Button(new Rect(456, 424, 368, 48), "MAIN MENU", button)) ReturnToMenu();
                GUI.Label(new Rect(440, 530, 400, 35), "Press Esc to resume", subtitle);
            }
            else
            {
                if (GUI.Button(new Rect(456, 280, 368, 54), "TRY AGAIN", button)) Restart();
                if (GUI.Button(new Rect(456, 350, 368, 54), "MAIN MENU", button)) ReturnToMenu();
            }
        }
        GUI.matrix = oldMatrix;
    }
}
