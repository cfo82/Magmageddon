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
            EnsureEffectInstanceAvailable();
            string chosenSound = PickOne(sound);
            SoundEffect soundEffect = Game.Instance.ContentManager.Load<SoundEffect>(chosenSound);
            SoundEffectInstance instance = soundEffect.Play();
            effectInstances.Add(instance);
            return instance;
        }

        public SoundEffectInstance Play(string sound, float volume)
        {
            EnsureEffectInstanceAvailable();
            string chosenSound = PickOne(sound);
            SoundEffect soundEffect = Game.Instance.ContentManager.Load<SoundEffect>(chosenSound);
            SoundEffectInstance instance = soundEffect.Play(volume);
            effectInstances.Add(instance);
            return instance;
        }

        public SoundEffectInstance Play(string sound, float volume, float pitch, float pan, bool loop)
        {
            EnsureEffectInstanceAvailable();
            string chosenSound = PickOne(sound);
            SoundEffect soundEffect = Game.Instance.ContentManager.Load<SoundEffect>(chosenSound);
            SoundEffectInstance instance = soundEffect.Play(volume * Game.Instance.EffectsVolume, pitch, pan, loop);
            effectInstances.Add(instance);
            return instance;
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
                    if (!instance.IsDisposed)
                    {
                        instance.Dispose();
                    }
                }
            }

            if (effectInstances.Count >= maxSoundEffectInstances)
            {
                throw new Exception("no more sound effect instances are available!");
            }
        }

        private static readonly int collectionThreshold = 50;
        private static readonly int maxSoundEffectInstances = 200;
        private HashSet<SoundEffectInstance> effectInstances;
        private static Random random = new Random();
    }
}
