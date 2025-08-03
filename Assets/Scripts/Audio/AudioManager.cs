using System.Collections.Generic;
using UnityEngine;

public class AudioManager : StaticReference<AudioManager>
{
    [Header("SFX Clips")]
    public AudioClip sfxTap;
    public AudioClip sfxDrag;

    [Header("Pooling")]
    [Tooltip("Initial number of pooled AudioSources.")]
    public int poolSize = 10;

    [Tooltip("Default volume for playback (0 to 1).")]
    [Range(0f, 1f)]
    public float defaultVolume = 1f;

    // Internal pool
    private List<AudioSource> audioSourcePool;
    private int poolIndex = 0; // round-robin fallback

    void Awake()
    {
        BaseAwake(this);
        if (poolSize <= 0) poolSize = 5;
        BuildPool();

    }

    private void OnDestroy()
    {
        BaseOnDestroy();
        // Clean up pooled AudioSources
        foreach (var src in audioSourcePool)
        {
            if (src != null)
                Destroy(src.gameObject);
        }
        audioSourcePool.Clear();
    }

    private void BuildPool()
    {
        audioSourcePool = new List<AudioSource>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject($"PooledAudioSource_{i}");
            go.transform.SetParent(transform);
            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            audioSourcePool.Add(src);
        }
    }

    /// <summary>
    /// Gets an available (not currently playing) AudioSource from the pool.
    /// If all are busy, uses round-robin to pick one (will interrupt).
    /// </summary>
    private AudioSource GetPooledSource()
    {
        // Try to find a free one first
        foreach (var src in audioSourcePool)
        {
            if (!src.isPlaying)
                return src;
        }

        // Fallback: round-robin overwrite
        AudioSource fallback = audioSourcePool[poolIndex];
        poolIndex = (poolIndex + 1) % audioSourcePool.Count;
        return fallback;
    }

    /// <summary>
    /// Plays the tap sound with optional volume and pitch variation.
    /// </summary>
    public void PlayTapSound(float volume = -1f, float pitch = 1f)
    {
        if (sfxTap == null) return;
        AudioSource src = GetPooledSource();
        src.clip = sfxTap;
        src.volume = volume < 0f ? defaultVolume : Mathf.Clamp01(volume);
        src.pitch = pitch;
        src.Play();
    }

    /// <summary>
    /// Plays the drag sound with optional volume and pitch variation.
    /// </summary>
    public void PlayDragSound(float volume = -1f, float pitch = 1f)
    {
        if (sfxDrag == null) return;
        AudioSource src = GetPooledSource();
        src.clip = sfxDrag;
        src.volume = volume < 0f ? defaultVolume : Mathf.Clamp01(volume);
        src.pitch = pitch;
        src.Play();
    }

    /// <summary>
    /// Optional: play arbitrary clip via pool.
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        AudioSource src = GetPooledSource();
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch = pitch;
        src.Play();
    }

    /// <summary>
    /// Expand pool at runtime if needed (not automatic—call manually).
    /// </summary>
    public void ExpandPool(int additional)
    {
        for (int i = 0; i < additional; i++)
        {
            GameObject go = new GameObject($"PooledAudioSource_{audioSourcePool.Count}");
            go.transform.SetParent(transform);
            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            audioSourcePool.Add(src);
        }
    }
}
