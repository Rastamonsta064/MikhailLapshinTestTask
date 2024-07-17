using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private Sound[] sounds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name, bool isPitchRandom = false)
    {
        Sound sound = FindSound(name);
        if (sound != null)
        {
            if (isPitchRandom)
            {
                float randomPitch = UnityEngine.Random.Range(sound.pitch - sound.pitchVariation, sound.pitch + sound.pitchVariation);
                sound.source.pitch = randomPitch;
            }
            sound.source.Play();
        }
    }

    public void Stop(string name)
    {
        Sound sound = FindSound(name);
        sound.source.Stop();
    }

    public void Pause(string name)
    {
        Sound sound = FindSound(name);
        sound.source.Pause();

    }

    private Sound FindSound(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if (sound == null)
        {
            Debug.LogWarning("sound not found " + name);
        }
        return sound;
    }

    public void StopAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
    }

    void OnApplicationFocus(bool hasFocus) => Silence(!hasFocus);

    void OnApplicationPause(bool isPaused) => Silence(isPaused);

    public void Silence(bool silence) => AudioListener.pause = silence;
}
