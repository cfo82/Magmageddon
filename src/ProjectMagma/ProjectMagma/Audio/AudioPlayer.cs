using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace ProjectMagma
{
    public class AudioPlayer
    {
        public AudioPlayer()
        {
            this.effectInstances = new HashSet<SoundEffectInstance>();
        }

        public SoundEffectInstance Play(string sound)
        {
            return Play(sound, 1.0f);
        }

        public SoundEffectInstance Play(string sound, float volume)
        {
            return Play(sound, volume, false);
        }

        public SoundEffectInstance Play(string sound, float volume, bool loop)
        {
            if (sound.Trim().Length == 0)
            {
                return null;
            }

            EnsureEffectInstanceAvailable();
            string chosenSound = PickOne(sound);
            SoundEffect soundEffect = Game.Instance.ContentManager.Load<SoundEffect>(chosenSound);
            SoundEffectInstance instance = soundEffect.Play(volume * Game.Instance.EffectsVolume, 0, 0, loop);
            effectInstances.Add(instance);
            return instance;
        }

        public void Stop(SoundEffectInstance instance)
        {
            Stop(instance, true);
        }

        public void Stop(SoundEffectInstance instance, bool immediate)
        {
            if (instance == null)
            {
                return;
            }

            effectInstances.Remove(instance);
            DisposeEffect(instance);
        }

        private string PickOne(string soundList)
        {
            string[] sounds = soundList.Split(' ');
            return sounds[random.Next(0, sounds.Length - 1)];
        }

        private void EnsureEffectInstanceAvailable()
        {
            if (effectInstances.Count >= collectionThreshold)
            {
                List<SoundEffectInstance> obsoleteInstances = new List<SoundEffectInstance>();
                foreach (SoundEffectInstance instance in effectInstances)
                {
                    if (instance.IsDisposed || instance.State == SoundState.Stopped)
                    {
                        obsoleteInstances.Add(instance);
                    }
                }

                foreach (SoundEffectInstance instance in obsoleteInstances)
                {
                    effectInstances.Remove(instance);
                    DisposeEffect(instance);
                }
            }

            if (effectInstances.Count >= maxSoundEffectInstances)
            {
                throw new Exception("no more sound effect instances are available!");
            }
        }

        private void DisposeEffect(SoundEffectInstance instance)
        {
            try
            {
                if (!instance.IsDisposed && instance.State != SoundState.Stopped)
                {
                    instance.Stop();
                }
                if (!instance.IsDisposed)
                {
                    instance.Dispose();
                }
            }
            catch (ObjectDisposedException)
            { }
        }

        private static readonly int collectionThreshold = 50;
        private static readonly int maxSoundEffectInstances = 200;
        private HashSet<SoundEffectInstance> effectInstances;
        private static Random random = new Random();
    }
}
