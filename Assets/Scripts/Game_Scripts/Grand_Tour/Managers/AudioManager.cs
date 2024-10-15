using System.Collections.Generic;
using UnityEngine;

namespace GrandTour
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        public List<Sound> sounds = new List<Sound>();

        void Awake()
        {
            instance = this;
            Initialize();
        }

        void Initialize()
        {
            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.loop = s.loop;
            }
        }

        public void Play(SoundType name)
        {
            Sound sound = sounds.Find(sound => sound.name == name);
            sound.source.Play();
        }


        public void PlayOneShot(SoundType name)
        {
            Sound sound = sounds.Find(sound => sound.name == name);
            sound.source.PlayOneShot(sound.clip);
        }

        public void Stop(SoundType name)
        {
            Sound sound = sounds.Find(sound => sound.name == name);
            sound.source.Stop();
        }
    }

    [System.Serializable]
    public class Sound
    {
        public SoundType name;
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume;

        public bool loop;

        [HideInInspector]
        public AudioSource source;
    }

    public enum SoundType
    {
        BG,
        Engine,
        Horn
    }
}