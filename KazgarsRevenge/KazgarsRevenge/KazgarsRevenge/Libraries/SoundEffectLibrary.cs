using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace KazgarsRevenge.Libraries
{
    public class SoundEffectLibrary
    {
        Dictionary<string, SoundEffect> soundEffects;
        Random rnd;
        const int numRangedSounds = 4;

        public SoundEffectLibrary(MainGame game)
        {
            soundEffects = new Dictionary<string, SoundEffect>();
            rnd = new Random();
            loadSounds(game);
        }

        public void loadSounds(MainGame game)
        {
            for (int i = 0; i < numRangedSounds; i++)
            {
                soundEffects.Add("bowshoot" + i, game.Content.Load<SoundEffect>("Sound\\bowshoot" + i));
            }
        }

        public void playRangedSound()
        {
            int effectNum = rnd.Next(numRangedSounds);
            soundEffects[("bowshoot" + effectNum)].Play();
        }

        public void FrostboltSound()
        {

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

        public void bruteDeath()
        {

        }

        public void skeletonDeath()
        {

        }

    }
}
