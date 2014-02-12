using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace KazgarsRevenge
{
    public class LoadingMenu //: AMenu
    {
        // Add a '.' every 1 second
        //private static readonly int ADD_PERIOD_MS = 1000;

        //private static readonly string[] PERIOD_STRS = { "", ".", "..", "..."};

        //// Current number of periods to draw
        //private int curPeriods;

        //private int timeLeft;

        //public LoadingMenu(SpriteBatch sb, string title, SpriteFont sf, Vector2 guiScale, Vector2 titleLoc)
        //    : base(sb, title, sf, guiScale, titleLoc)
        //{
        //    curPeriods = 0;
        //    timeLeft = ADD_PERIOD_MS;
        //}

        //public override void Load()
        //{
        //    // TODO call loading stuff
        //    throw new NotImplementedException();
        //}

        //public override void Unload()
        //{
        //    // Do nothing
        //}

        //public override void Draw(GameTime gameTime)
        //{
        //    timeLeft -= gameTime.ElapsedGameTime.Milliseconds;
        //    if (timeLeft <= 0)
        //    {
        //        curPeriods = (++curPeriods) % PERIOD_STRS.Length;
        //        base.title = base.title + PERIOD_STRS[curPeriods];
        //        base.drawCenter = base.sf.MeasureString(title) / 2;
        //        timeLeft = ADD_PERIOD_MS;
        //    }
        //    base.Draw(gameTime);
        //}
    }
}
