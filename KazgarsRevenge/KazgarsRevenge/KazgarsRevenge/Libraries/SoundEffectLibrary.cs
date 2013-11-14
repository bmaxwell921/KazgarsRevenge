using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace KazgarsRevenge.Libraries
{
    class SoundEffectLibrary
    {
        Dictionary<string, SoundEffect> soundEffects;
        Random rnd;
        int numRangedSounds;

        public SoundEffectLibrary(MainGame game)
        {
            soundEffects = new Dictionary<string, SoundEffect>();
            rnd = new Random();
            numRangedSounds = 3;
            loadSounds(game);
        }

        public void loadSounds(MainGame game)
        {
            for (int i = 0; i < numRangedSounds; i++)
            {
                soundEffects.Add("arrow" + i, game.Content.Load<SoundEffect>("Sound\\arrow" + i));
            }

            soundEffects.Add("scratching", game.Content.Load<SoundEffect>("Sound\\scratching"));
        }

        public void playRangedSound()
        {
            int effectNum = rnd.Next(numRangedSounds);
            soundEffects[("arrow" + effectNum)].Play();
        }

        public void playMeleeSound()
        {

        }

        public void playMagicSound()
        {

        }

        public void playKazgarDying()
        {

        }

        public void playEnemyDying()
        {

        }

        public void playScratch()
        {
            soundEffects[("scratching")].Play();
        }
    }
}
