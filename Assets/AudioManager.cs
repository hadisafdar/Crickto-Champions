using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public AudioMixerGroup mixerGroup;
    public Sounds[] clips;
    public static AudioManager instance;
    private void Awake()
    {

        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        foreach (var sound in clips)
        {
           sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.audio;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.outputAudioMixerGroup = mixerGroup;
        }
        
    }

    public void Play(string name)
    {
       Sounds sound = Array.Find(clips, sound => sound.name == name);
        if (sound == null || sound.source.isPlaying)
            return;
        sound.source.Play();
    }
    public void Stop(string name)
    {
        Sounds sound = Array.Find(clips, sound => sound.name == name);
        if (sound == null)
            return;
        sound.source.Stop();
    }
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
        audioMixer.SetFloat("Volume", volume);
        PlayerPrefs.SetFloat("Volume", volume); // Save the volume value
        PlayerPrefs.Save(); // Ensure the value is saved to disk
    }
}
