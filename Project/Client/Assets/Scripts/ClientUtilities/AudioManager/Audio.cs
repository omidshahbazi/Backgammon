using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientUtilities.AudioMangaer
{
    public class Audio : MonoBehaviour
    {
        private AudioSource source = null;
        private bool alreadyPlayed = false;

        public AudioSource Source
        {
            get { return source; }
            set
            {
                Debug.Assert(value != null, "Source cannot be null");

                source = value;
            }
        }

        public AudioClip Clip
        {
            get { return source.clip; }
            set
            {
                source.clip = value;
                alreadyPlayed = false;
            }
        }

        public bool AutoUnload
        {
            get;
            set;
        }

        public bool Loop
        {
            get { return source.loop; }
            set { source.loop = value; }
        }

        public bool Mute
        {
            get { return source.mute; }
            set { source.mute = value; }
        }

        public float Volume
        {
            get { return source.volume; }
            set { source.volume = value; }
        }

        public bool IsFinished
        {
            get { return alreadyPlayed && !source.isPlaying; }
        }

        private void Update()
        {
            if (alreadyPlayed && AutoUnload && !source.isPlaying)
                AudioManager.Instance.Unload(this);
        }


        public void Play()
        {
            source.Play();

            alreadyPlayed = true;
        }

        public void Stop()
        {
            source.Stop();

            alreadyPlayed = true;
        }

        public void Unload()
        {
            Stop();
            AudioManager.Instance.Unload(this);
        }
    }
}