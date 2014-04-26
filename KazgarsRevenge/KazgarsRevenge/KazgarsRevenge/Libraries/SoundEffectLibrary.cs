using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace KazgarsRevenge
{
    public class SoundEffectLibrary : GameComponent
    {
        Dictionary<string, SoundEffect> soundEffects;
        Random rand;

        Dictionary<string, Song> songs;
        MediaLibrary media;
        public SoundEffectLibrary(MainGame game) 
            : base(game)
        {
            rand = RandSingleton.U_Instance;
            media = new MediaLibrary();

            soundEffects = new Dictionary<string, SoundEffect>();
            loadSoundEffects(game);

            songs = new Dictionary<string, Song>();
            loadSongs(game);
        }

        GameState prevState = GameState.Playing;
        public override void Update(GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;
            GameState state = (Game as MainGame).gameState;
            if (state != prevState)
            { 
                //HandleStateChange(state);
            }

            if (nextScream != -10000)
            {
                nextScream -= elapsed;
                if (nextScream <= 0)
                {
                    gopherScream.Stop();
                    gopherScream.Play();
                    nextScream = screamLength + rand.Next(1000, 3000);
                }
            }

            prevState = state;
 	        base.Update(gameTime);
        }


        private void HandleStateChange(GameState newState)
        {
            switch (newState)
            {
                case GameState.StartMenu:
                    MediaPlayer.Play(songs["menu"]);
                    break;
            }
        }

        public void loadSongs(MainGame game)
        {
            songs.Add("menu", game.Content.Load<Song>("Sound\\Music\\menuSong"));
        }

        double screamLength;
        public void loadSoundEffects(MainGame game)
        {
            for (int i = 0; i < numBowSounds; i++)
            {
                soundEffects.Add("bowshoot" + i, game.Content.Load<SoundEffect>("Sound\\Ranged\\bowshoot" + i));
            }

            for (int i = 0; i < numSwordSounds; ++i)
            {
                soundEffects.Add("sword_hit" + i, game.Content.Load<SoundEffect>("Sound\\Melee\\sword_hit" + i));
                soundEffects.Add("sword_miss" + i, game.Content.Load<SoundEffect>("Sound\\Melee\\whoosh" + i));
                soundEffects.Add("sword_clang" + i, game.Content.Load<SoundEffect>("Sound\\Melee\\clang" + i));
            }


            soundEffects.Add("devastate", game.Content.Load<SoundEffect>("Sound\\Melee\\devastate"));

            soundEffects.Add("pig_death", game.Content.Load<SoundEffect>("Sound\\Enemies\\pigdeath"));
            soundEffects.Add("s_death", game.Content.Load<SoundEffect>("Sound\\Enemies\\s_death"));

            soundEffects.Add("unlockDoors", game.Content.Load<SoundEffect>("Sound\\keyUnlock"));
            soundEffects.Add("levelup", game.Content.Load<SoundEffect>("Sound\\level_up"));
            soundEffects.Add("gopher_death", game.Content.Load<SoundEffect>("Sound\\Misc\\crunchsplat"));
            soundEffects.Add("gopher_pickup", game.Content.Load<SoundEffect>("Sound\\Misc\\g_pick_up"));
            soundEffects.Add("gopher_spin", game.Content.Load<SoundEffect>("Sound\\Misc\\g_spin"));
            soundEffects.Add("gopher_smash", game.Content.Load<SoundEffect>("Sound\\Misc\\g_extra"));
            screamLength = soundEffects["gopher_spin"].Duration.TotalMilliseconds;
        }

        public void PlaySound(string s)
        {
            if (soundEffects.ContainsKey(s))
            {
                soundEffects[s].Play();
            }
        }

        #region Attacks


        #region melee
        const int numSwordSounds = 4;
        public void playMeleeHitSound()
        {
            soundEffects["sword_hit" + rand.Next(numBowSounds)].Play();
        }
        public void playMeleeMissSound()
        {
            soundEffects["sword_miss" + rand.Next(numBowSounds)].Play();
        }
        public void playMeleeHitFloorSound()
        {
            soundEffects["sword_clang" + rand.Next(numBowSounds)].Play();
        }

        public void PlayAbilitySound(AbilityName ability)
        {
            switch (ability)
            {
                case AbilityName.DevastatingStrike:
                    soundEffects["devastate"].Play();
                    break;
            }
        }
        #endregion


        #region Ranged
        const int numBowSounds = 3;
        public void playRangedSound()
        {
            soundEffects["bowshoot" + rand.Next(numBowSounds)].Play();
        }
        #endregion


        #region Magic
        public void FrostboltSound()
        {

        }

        public void playMagicSound()
        {

        }
        #endregion

        #endregion

        #region Deaths
        public void playDeathKazgar()
        {

        }

        public void playEnemyDeath(string enemyPrefix)
        {
            string key = enemyPrefix + "death";
            if (soundEffects.ContainsKey(key))
            {
                soundEffects[key].Play();
            }
        }

        #endregion

        #region Level Scripts
        public void playUnlockSound()
        {
            soundEffects["unlockDoors"].Play();
        }
        #endregion


        #region misc
        public void playGopherPickUp()
        {
            soundEffects["gopher_pickup"].Play();
        }

        SoundEffectInstance gopherScream;
        double nextScream = -10000;
        public void startGopherSpin()
        {
            if (nextScream == -10000)
            {
                if (gopherScream == null)
                {
                    gopherScream = soundEffects["gopher_spin"].CreateInstance();
                }
                gopherScream.Stop();
                gopherScream.Play();

                nextScream = screamLength + rand.Next(1000, 5000);
            }
        }
        public void endGopherSpin(bool dead)
        {
            nextScream = -10000;
            gopherScream.Stop();
            if (dead)
            {
                soundEffects["gopher_death"].Play();
            }
            else
            {
                soundEffects["gopher_smash"].Play();
            }
        }
        #endregion
    }
}
