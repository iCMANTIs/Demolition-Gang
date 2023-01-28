using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SoundEffect
{

    public class SoundEffectManager : DontDestroySingleton<SoundEffectManager>
    {        
        public List<AudioSource> singleAudioSourceList = new List<AudioSource>();
        public List<AudioSource> loopAudioSourceList = new List<AudioSource>();


        public AudioSource FindAvailableSingleAudioSource()
        {
            foreach (AudioSource audioSource in singleAudioSourceList)
            {
                if (!audioSource.isPlaying)
                    return audioSource;
            }

            return null;
        }


        public AudioSource FindAvailableLoopAudioSource()
        {
            foreach (AudioSource audioSource in loopAudioSourceList)
            {
                if (!audioSource.isPlaying)
                    return audioSource;
            }

            return null;
        }


        public void PlayOneShot(AudioSource audioSource, string soundID)
        {
            AudioClip audio = SoundEffectAssets.instance.FindSoundEffect(soundID);
            audioSource.PlayOneShot(audio);
        }


        public void PlayInLoop(AudioSource audioSource, string soundID)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = SoundEffectAssets.instance.FindSoundEffect(soundID);
                audioSource.Play();
            }
        }


        public void Stop(AudioSource audioSource)
        {
            audioSource.Stop();           
        }

    }

}

