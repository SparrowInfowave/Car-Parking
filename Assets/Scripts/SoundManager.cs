using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Audio;
using Manager;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SoundManager : MonoBehaviour
{
    public static SoundManager inst;

    #region Classes

    [System.Serializable]
    private class SoundInfo
    {
        public string id = "";
        public AudioClip audioClip = null;
        public SoundType type = SoundType.SoundEffect;
        public bool playAndLoopOnStart = false;
        [Range(0, 1)] public float clipVolume = 1;
    }

    private class PlayingSound
    {
        public SoundInfo soundInfo = null;
        public AudioSource audioSource = null;
    }

    #endregion

    #region Enums

    public enum SoundType
    {
        SoundEffect,
        Music
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private List<SoundInfo> soundInfos = null;

    #endregion

    #region Member Variables

    private List<PlayingSound> playingAudioSources;
    private List<PlayingSound> loopingAudioSources;

    #endregion

    #region Properties

    public bool isMusicOn
    {
        get => PrefManager.GetBool(nameof(isMusicOn), true);
        set => PrefManager.SetBool(nameof(isMusicOn), value);
    }

    public bool isSoundEffectsOn
    {
        get => PrefManager.GetBool(nameof(isSoundEffectsOn), true);
        set => PrefManager.SetBool(nameof(isSoundEffectsOn), value);
    }

    #endregion

    #region Unity Methods

    void Awake()
    {
        if (inst == null)
            inst = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        playingAudioSources = new List<PlayingSound>();
        loopingAudioSources = new List<PlayingSound>();
    }

    private void Start()
    {
        StartCoroutine(LoadAllClip());
    }

    private void Update()
    {
        for (int i = 0; i < playingAudioSources.Count; i++)
        {
            var audioSource = playingAudioSources[i].audioSource;

            // If the Audio Source is no longer playing then return it to the pool so it can be re-used
            if (!audioSource.isPlaying)
            {
                Destroy(audioSource.gameObject);
                playingAudioSources.RemoveAt(i);
                i--;
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Plays the sound with the give id
    /// </summary>
    public void Play(string id)
    {
        Play(id, false, 0);
    }

    /// <summary>
    /// Plays the sound with the give id, if loop is set to true then the sound will only stop if the Stop method is called
    /// </summary>
    public void Play(string id, bool loop, float playDelay)
    {
        if (id == "HomeBg" && Is_Playing_Home_Bg("HomeBg", loopingAudioSources))
            Stop("HomeBg");

        var soundInfo = GetSoundInfo(id);
        if (soundInfo == null)
        {
            Debug.LogError("[SoundManager] There is no Sound Info with the given id: " + id);

            return;
        }

        if ((soundInfo.type == SoundType.Music && !isMusicOn) ||
            (soundInfo.type == SoundType.SoundEffect && !isSoundEffectsOn))
        {
            return;
        }

        var audioSource = CreateAudioSource(id);

        audioSource.clip = soundInfo.audioClip;
        audioSource.loop = loop;
        audioSource.time = 0;
        audioSource.volume = soundInfo.clipVolume;

        if (playDelay > 0)
        {
            audioSource.PlayDelayed(playDelay);
        }
        else
        {
            audioSource.Play();
        }

        var playingSound = new PlayingSound();

        playingSound.soundInfo = soundInfo;
        playingSound.audioSource = audioSource;

        if (loop)
        {
            loopingAudioSources.Add(playingSound);
        }
        else
        {
            playingAudioSources.Add(playingSound);
        }
    }

    /// <summary>
    /// Stops all playing sounds with the given id
    /// </summary>
    public void Stop(string id)
    {
        StopAllSounds(id, playingAudioSources);
        StopAllSounds(id, loopingAudioSources);
    }


    /// <summary>
    /// Stops all playing sounds with the given type
    /// </summary>
    private void Stop(SoundType type)
    {
        StopAllSounds(type, playingAudioSources);
        StopAllSounds(type, loopingAudioSources);
    }

    public AudioClip Get_Clip(string id)
    {
        return soundInfos.Find(x => x.id == id).audioClip;
    }

    /// <summary>
    /// Sets the SoundType on/off
    /// </summary>
    public void SetSoundTypeOnOff(SoundType type, bool isOn)
    {
        switch (type)
        {
            case SoundType.SoundEffect:

                if (isOn == isSoundEffectsOn)
                {
                    return;
                }

                isSoundEffectsOn = isOn;

                break;
            case SoundType.Music:

                if (isOn == isMusicOn)
                {
                    return;
                }

                isMusicOn = isOn;

                break;
        }

        // If it was turned off then stop all sounds that are currently playing
        if (!isOn)
        {
            Stop(type);
        }
        // Else it was turned on so play any sounds that have playAndLoopOnStart set to true
        else
        {
            PlayAtStart(type);
        }
    }


    //MY EDIT ----------- this method for stop playing music and start -------------
    public void PlayMusic(bool play)
    {
        if (!play)
        {
            if (isMusicOn)
            {
                Stop(SoundType.Music);
            }
        }
        else
        {
            if (isMusicOn && loopingAudioSources.Count == 0)
            {
                PlayAtStart(SoundType.Music);
            }
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Plays all sounds that are set to play on start and loop and are of the given type
    /// </summary>
    private void PlayAtStart(SoundType type)
    {
        for (int i = 0; i < soundInfos.Count; i++)
        {
            SoundInfo soundInfo = soundInfos[i];

            if (soundInfo.type == type && soundInfo.playAndLoopOnStart)
            {
                Play(soundInfo.id, true, 0);
            }
        }
    }

    /// <summary>
    /// Stops all sounds with the given id
    /// </summary>
    private void StopAllSounds(string id, List<PlayingSound> playingSounds)
    {
        for (int i = 0; i < playingSounds.Count; i++)
        {
            var playingSound = playingSounds[i];

            if (id == playingSound.soundInfo.id)
            {
                playingSound.audioSource.Stop();
                Destroy(playingSound.audioSource.gameObject);
                playingSounds.RemoveAt(i);
                i--;
            }
        }
    }

    public bool Get_Is_Playing_Sound(string id)
    {
        var clip = Get_Clip(id);
        if (playingAudioSources.Any(x => x.audioSource.clip == clip))
            return true;
        if (loopingAudioSources.Any(x => x.audioSource.clip == clip))
            return true;
        return false;
    }

    private bool Is_Playing_Home_Bg(string id, List<PlayingSound> playingSounds)
    {
        return (from playingSound in playingSounds
            where id == playingSound.soundInfo.id
            select playingSound.audioSource.isPlaying).FirstOrDefault();
    }

    /// <summary>
    /// Stops all sounds with the given type
    /// </summary>
    private void StopAllSounds(SoundType type, List<PlayingSound> playingSounds)
    {
        for (int i = 0; i < playingSounds.Count; i++)
        {
            PlayingSound playingSound = playingSounds[i];

            if (type == playingSound.soundInfo.type)
            {
                playingSound.audioSource.Stop();
                Destroy(playingSound.audioSource.gameObject);
                playingSounds.RemoveAt(i);
                i--;
            }
        }
    }

    private SoundInfo GetSoundInfo(string id)
    {
        return soundInfos.FirstOrDefault(t => id == t.id);
    }

    private AudioSource CreateAudioSource(string id)
    {
        var obj = new GameObject("sound_" + id);

        obj.transform.SetParent(transform);

        return obj.AddComponent<AudioSource>();
    }

    public bool Is_Playing_Music(string soundName)
    {
        return loopingAudioSources.Find(x => x.soundInfo.id == soundName).audioSource.isPlaying;
    }

    private IEnumerator LoadAllClip()
    {
        var locations = Addressables.LoadResourceLocationsAsync(new AssetLabelReference{labelString = "AudioInfo"});
        yield return new WaitUntil(()=>locations.IsDone);
        foreach (var location in locations.Result) {
            
            var handle = Addressables.LoadAssetAsync<AudioClipInfo>(location);
            handle.Completed += addInInfo =>
            {
                soundInfos.Add(new SoundInfo
                {
                    id = handle.Result.id,
                    audioClip = handle.Result.audioClip,
                    clipVolume = handle.Result.clipVolume,
                    playAndLoopOnStart = handle.Result.playAndLoopOnStart,
                    type = handle.Result.type
                });
            };
        }
        
        foreach (var soundInfo in soundInfos.Where(soundInfo => soundInfo.playAndLoopOnStart))
        {
            Play(soundInfo.id, true, 0);
        }
    }

    #endregion
}