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
            loadSounds(game);

            songs = new Dictionary<string, Song>();
            loadSongs(game);
        }

        GameState prevState = GameState.Playing;
        public override void Update(GameTime gameTime)
        {
            GameState state = (Game as MainGame).gameState;
            if (state != prevState)
            { 
                //HandleStateChange(state);
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

        public void loadSounds(MainGame game)
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
        }

        #region attacks


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
    }
}
