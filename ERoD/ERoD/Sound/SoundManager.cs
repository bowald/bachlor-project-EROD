using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERoD
{
    public class SoundManager
    {
        private static SoundEffect menuSelection;
        private static SoundEffect goSound;
        private static SoundEffect boostSound;

        private static bool[] boostPlaying = new bool[4];

        private static SoundEffectInstance menuSelectionInstance;
        private static SoundEffectInstance goSoundInstance;
        private static SoundEffectInstance[] boostSoundInstance = new SoundEffectInstance[4];
        

        private static SoundEffect menuMusic;
        private static SoundEffect raceMusic;

        private static SoundEffectInstance menuMusicInstance;
        private static SoundEffectInstance raceMusicInstance;

        public static void Initialize(Game game)
        {
            menuSelection = game.Content.Load<SoundEffect>("Sound/MenuSelection");
            raceMusic = game.Content.Load<SoundEffect>("Sound/MusicGame");
            menuMusic = game.Content.Load<SoundEffect>("Sound/Expanse");
            goSound = game.Content.Load<SoundEffect>("Sound/GoStart");
            boostSound = game.Content.Load<SoundEffect>("Sound/BoostLoop");
        }

        public static void PlayMenuSelection()
        {
            menuSelectionInstance = menuSelection.CreateInstance();

            menuSelectionInstance.Play();
        }

        public static void StopMenuMusic()
        {
            if (menuMusicInstance != null && !menuMusicInstance.IsDisposed)
            {
                menuMusicInstance.Pause();
                menuMusicInstance.Dispose();
            }
        }

        public static void PlayRaceMusic()
        {
            StopMenuMusic();

            if (raceMusicInstance == null || raceMusicInstance.IsDisposed)
            {
                raceMusicInstance = raceMusic.CreateInstance();
                raceMusicInstance.IsLooped = true;
            }

            raceMusicInstance.Play();
        }

        public static void PlayMenuMusic()
        {
            if (raceMusicInstance != null && !raceMusicInstance.IsDisposed)
            {
                raceMusicInstance.Pause();
                raceMusicInstance.Dispose();
            }

            if (menuMusicInstance == null || menuMusicInstance.IsDisposed)
            {
                menuMusicInstance = menuMusic.CreateInstance();
                menuMusicInstance.IsLooped = true;
            }

            menuMusicInstance.Play();
        }

        public static void PlayGoSound()
        {
            goSoundInstance = goSound.CreateInstance();

            goSoundInstance.Play();
        }

        public static void FadeBoostSound(int index)
        {
            if (boostSoundInstance[index] != null && !boostSoundInstance[index].IsDisposed)
            {
                boostSoundInstance[index].Stop();
                boostPlaying[index] = false;
            }
        }

        public static void PlayBoostSound(int index)
        {
            if (boostPlaying[index])
            {
                return;
            }
            if (boostSoundInstance[index] == null || boostSoundInstance[index].IsDisposed) 
            {
                boostSoundInstance[index] = boostSound.CreateInstance();
                boostSoundInstance[index].Volume = 0.4f;
            }

            boostSoundInstance[index].Play();
            boostPlaying[index] = true;
        }
    }
}
