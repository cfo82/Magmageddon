/*
xWinForms © 2007-2009
Eric Grossinger - ericgrossinger@gmail.com
*/
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace xWinFormsLib
{
    class Math
    {
        public static float GetAngleFrom2DVectors(Vector2 OriginLoc, Vector2 TargetLoc, bool bRadian)
        {
            double Angle;
            float xDist = OriginLoc.X - TargetLoc.X;
            float yDist = OriginLoc.Y - TargetLoc.Y;
            double norm = System.Math.Abs(xDist) + System.Math.Abs(yDist);

            if ((xDist >= 0) & (yDist >= 0))
            {
                //Lower Right Quadran
                Angle = 90 * (yDist / norm) + 270;
            }
            else if ((xDist <= 0) && (yDist >= 0))
            {
                //Lower Left Quadran
                Angle = -90 * (yDist / norm) + 90;
            }
            else if (((xDist) <= 0) && ((yDist) <= 0))
            {
                //Upper Left Quadran
                Angle = 90 * (xDist / norm) + 180;
            }
            else
            {
                //Upper Right Quadran
                Angle = 90 * (xDist / norm) + 180;
            }

            if (bRadian)
                Angle = MathHelper.ToRadians(System.Convert.ToSingle(Angle));

            return System.Convert.ToSingle(Angle);
        }
    }
}
