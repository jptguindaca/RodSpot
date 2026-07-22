using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
Gestor central de som do jogo.

Singleton persistente (DontDestroyOnLoad) que controla:
  - Musica de fundo por cena (menu, jogo, etc.) com fade opcional.
  - Efeitos sonoros (SFX) pontuais: lancar a cana, apanhar o peixe, fisgada...
  - Volumes de musica e SFX, guardados entre sessoes com PlayerPrefs.

Como usar no codigo:
  AudioManager.Instance?.PlayCast();       // som de lancar a cana
  AudioManager.Instance?.PlayCatch();      // som de apanhar o peixe
  AudioManager.Instance?.PlaySfx(clip);    // qualquer AudioClip pontual
  AudioManager.Instance?.PlayMusic(clip);  // trocar musica manualmente
*/
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SceneMusic
    {
        [Tooltip("Nome exato da cena (Scene) onde esta musica deve tocar.")]
        public string sceneName;
        public AudioClip music;
    }

    [Header("Musica por cena")]
    [Tooltip("Associa cada cena a uma musica de fundo. O nome tem de ser igual ao da cena no Build Settings.")]
    [SerializeField] private List<SceneMusic> sceneMusic = new List<SceneMusic>();
    [Tooltip("Musica usada se a cena atual nao estiver na lista acima (opcional).")]
    [SerializeField] private AudioClip defaultMusic;
    [Tooltip("Duracao do fade ao trocar de musica (0 = troca instantanea).")]
    [SerializeField] private float musicFadeDuration = 0.75f;

    [Header("Efeitos sonoros (SFX)")]
    [SerializeField] private AudioClip castSfx;      // lancar a cana
    [SerializeField] private AudioClip catchSfx;     // apanhar o peixe
    [SerializeField] private AudioClip biteSfx;      // fisgada (opcional)
    [SerializeField] private AudioClip clickSfx;     // clique de UI (opcional)

    [Header("Volumes")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.6f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource catchSource;   // dedicado ao catch, para poder ser parado
    private AudioClip currentMusic;
    private Coroutine fadeRoutine;

    private const string MusicVolumeKey = "audio_music_volume";
    private const string SfxVolumeKey = "audio_sfx_volume";
    private const string MasterVolumeKey = "audio_master_volume";

    private void Awake()
    {
        // Garante uma unica instancia que sobrevive a mudanca de cenas.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cria os dois AudioSource por codigo (nao e preciso adicionar no Inspector).
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // AudioSource proprio para o catch: usa Play()/Stop() para poder ser cortado.
        catchSource = gameObject.AddComponent<AudioSource>();
        catchSource.loop = false;
        catchSource.playOnAwake = false;

        LoadVolumes();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Toca a musica da cena inicial (ex: menu principal).
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    // ----------------- MUSICA -----------------

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip clip = defaultMusic;

        for (int i = 0; i < sceneMusic.Count; i++)
        {
            if (sceneMusic[i] != null && sceneMusic[i].sceneName == sceneName && sceneMusic[i].music != null)
            {
                clip = sceneMusic[i].music;
                break;
            }
        }

        PlayMusic(clip);
    }

    /// Troca a musica de fundo (com fade se configurado). Ignora se ja for a mesma.
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            StopMusic();
            return;
        }

        if (clip == currentMusic && musicSource.isPlaying)
        {
            return;
        }

        currentMusic = clip;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (musicFadeDuration > 0f && gameObject.activeInHierarchy)
        {
            fadeRoutine = StartCoroutine(FadeToClip(clip));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
        currentMusic = null;
        musicSource.Stop();
        musicSource.clip = null;
    }

    private IEnumerator FadeToClip(AudioClip clip)
    {
        float target = musicVolume * masterVolume;

        // Fade out da musica atual.
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float t = 0f;
            while (t < musicFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / musicFadeDuration);
                yield return null;
            }
        }

        // Troca e fade in.
        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.Play();

        float t2 = 0f;
        while (t2 < musicFadeDuration)
        {
            t2 += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, target, t2 / musicFadeDuration);
            yield return null;
        }

        musicSource.volume = target;
        fadeRoutine = null;
    }

    // ----------------- SFX -----------------

    /// Toca um efeito sonoro pontual (nao interrompe outros).
    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
        {
            return;
        }
        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume * Mathf.Clamp01(volumeScale));
    }

    public void PlayCast() => PlaySfx(castSfx);
    public void PlayBite() => PlaySfx(biteSfx);
    public void PlayClick() => PlaySfx(clickSfx);

    /// Toca o som do catch num AudioSource proprio (para poder ser parado com StopCatch).
    public void PlayCatch()
    {
        if (catchSfx == null)
        {
            return;
        }
        catchSource.clip = catchSfx;
        catchSource.volume = sfxVolume * masterVolume;
        catchSource.Play();
    }

    /// Corta imediatamente o som do catch, se estiver a tocar.
    public void StopCatch()
    {
        if (catchSource != null && catchSource.isPlaying)
        {
            catchSource.Stop();
        }
    }

    // ----------------- VOLUMES -----------------

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
    }

    private void ApplyMusicVolume()
    {
        // Nao mexe no volume a meio de um fade (o fade ja aponta para o valor certo).
        if (fadeRoutine == null && musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }

    private void LoadVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, masterVolume);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume);
    }
}
