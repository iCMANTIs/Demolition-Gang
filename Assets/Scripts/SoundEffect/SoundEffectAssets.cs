using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SoundEffect
{

    [CreateAssetMenu(fileName = "SoundEffectAssets", menuName = "ScriptableObject/SoundEffectAssets")]
    public class SoundEffectAssets : ScriptableSingleton<SoundEffectAssets>
    {
        [Serializable]
        public struct SoundEffectConfig
        {
            public string soundID;
            public AudioClip soundFile;
        }

        [SerializeField]
        private List<SoundEffectConfig> soundEffectList = new List<SoundEffectConfig>();



        public AudioClip FindSoundEffect(string soundID)
        {
            return soundEffectList.Find(item => item.soundID == soundID).soundFile;
        }


    }

}