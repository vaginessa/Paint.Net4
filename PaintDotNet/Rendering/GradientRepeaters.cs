namespace PaintDotNet.Rendering
{
    using System;

    internal static class GradientRepeaters
    {
        internal sealed class NoRepeat : GradientRepeater
        {
            public override double BoundLerp(double t)
            {
                if (t < 0.0)
                {
                    return 0.0;
                }
                if (t > 1.0)
                {
                    return 1.0;
                }
                return t;
            }
        }

        internal sealed class RepeatReflected : GradientRepeater
        {
            public override double BoundLerp(double t)
            {
                if (t < 0.0)
                {
                    t = 2.0 + (t - (2 * (((int) t) / 2)));
                }
                else
                {
                    t -= 2 * (((int) t) / 2);
                }
                if (t > 1.0)
                {
                    t = 2.0 - t;
                }
                return t;
            }
        }

        internal sealed class RepeatWrapped : GradientRepeater
        {
            public override double BoundLerp(double t)
            {
                if (t < 0.0)
                {
                    return (1.0 + (t - ((int) t)));
                }
                return (t - ((int) t));
            }
        }
    }
}

