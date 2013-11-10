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

        public SoundEffectLibrary(MainGame game)
        {
            soundEffects = new Dictionary<string, SoundEffect>();
            loadSounds(game);
        }

        public void loadSounds(MainGame game)
        {
            soundEffects.Add("arrow", game.Content.Load<SoundEffect>("Sound\\arrow"));
        }

        public void playRangedSound()
        {
            soundEffects["arrow"].Play();
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
    }
}
