using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup ambienceGroup;
    [SerializeField] private AudioClip[] clips;
    private AudioSource sfxSource;
    private AudioSource ambienceSource;
    private MeleeAttack melee;
    private Health playerHealth;
    private Health[] enemies;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.outputAudioMixerGroup = sfxGroup;
        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.playOnAwake = false;
        ambienceSource.loop = true;
        ambienceSource.outputAudioMixerGroup = ambienceGroup;
        sfxSource.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("Audio.Sfx", 1f)); ambienceSource.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("Audio.Ambience", 1f));
    }
    private void Start()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
        BindScene();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { BindScene(); }
    private void UnbindScene()
    {
        if (melee != null) melee.OnAttack -= OnAttack;
        if (playerHealth != null) playerHealth.Damaged -= OnPlayerHurt;
        if (enemies != null) foreach (var enemy in enemies)
            if (enemy != null && enemy != playerHealth) enemy.Damaged -= OnEnemyHurt;
    }
    private void BindScene()
    {
        UnbindScene();
        melee = FindFirstObjectByType<MeleeAttack>();
        playerHealth = melee != null ? melee.GetComponent<Health>() : null;
        if (melee != null) melee.OnAttack += OnAttack;
        if (playerHealth != null) playerHealth.Damaged += OnPlayerHurt;
        enemies = FindObjectsByType<Health>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
            if (enemy != null && enemy != playerHealth) enemy.Damaged += OnEnemyHurt;
    }
    private void OnAttack()
    {
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing) PlaySfx("04_sword_swing");
    }
    private void OnPlayerHurt(DamageInfo damage) { PlaySfx("06_player_hurt"); }
    private void OnEnemyHurt(DamageInfo damage) { PlaySfx("05_sword_hit"); PlaySfx("07_enemy_hurt"); }
    private AudioClip FindClip(string clipName)
    {
        if (clips == null) return null;
        return Array.Find(clips, clip => clip != null && clip.name == clipName);
    }
    public void PlaySfx(string clipName)
    {
        var clip = FindClip(clipName);
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }
    public void PlayAmbience(string clipName)
    {
        var clip = FindClip(clipName);
        if (clip == null || ambienceSource == null) return;
        if (ambienceSource.clip == clip && ambienceSource.isPlaying) return;
        ambienceSource.clip = clip;
        ambienceSource.Play();
    }
    public void StopAmbience() { if (ambienceSource != null) ambienceSource.Stop(); }
    public float SfxVolume => sfxSource != null ? sfxSource.volume : 1f;
    public float AmbienceVolume => ambienceSource != null ? ambienceSource.volume : 1f;
    public void SetSfxVolume(float value) { value = Mathf.Clamp01(value); if (sfxSource != null) sfxSource.volume = value; PlayerPrefs.SetFloat("Audio.Sfx", value); }
    public void SetAmbienceVolume(float value) { value = Mathf.Clamp01(value); if (ambienceSource != null) ambienceSource.volume = value; PlayerPrefs.SetFloat("Audio.Ambience", value); }
    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindScene();
        Instance = null;
    }
}
