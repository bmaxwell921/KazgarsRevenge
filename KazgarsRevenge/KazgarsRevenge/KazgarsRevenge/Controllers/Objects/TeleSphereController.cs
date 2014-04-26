using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class TeleSphereController : Component
    {
        SharedGraphicsParams modelParams;
        double duration;
        public TeleSphereController(KazgarsRevengeGame game, GameEntity entity, double duration)
            : base(game, entity)
        {
            this.duration = duration;
            modelParams = entity.GetSharedData(typeof(SharedGraphicsParams)) as SharedGraphicsParams;
            size = 0;
            modelParams.size = Vector3.Zero;
        }

        bool exploding = true;
        float rateincrease = 0;
        float rate = 100;
        float size = 0;
        double pauseDuration = 250;
        float maxSize;
        public override void Update(GameTime gameTime)
        {
            double elapsed = gameTime.ElapsedGameTime.TotalMilliseconds;
            //myData.Position = followData.Position;
            if (exploding)
            {
                rateincrease += 10;
                rate += rateincrease;
                size += (float)((elapsed / 1500) * (rate));
                modelParams.size.X = size;
                modelParams.size.Y = size;
                modelParams.size.Z = size;

                duration -= elapsed;
                if (duration <= duration * 3 / 4)
                {
                    exploding = false;
                    maxSize = size;
                }
            }
            else
            {
                pauseDuration -= elapsed;
                if (pauseDuration > 0)
                {
                    return;
                }

                size -= (float)( (elapsed / (duration / 4)) *  maxSize );
                modelParams.size.X = size;
                modelParams.size.Y = size;
                modelParams.size.Z = size;

                duration -= elapsed;
                if (duration <= 0)
                {
                    Entity.KillEntity();
                }
            }
        }
    }
}
