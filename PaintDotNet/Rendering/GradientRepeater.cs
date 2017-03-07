namespace PaintDotNet.Rendering
{
    using System;

    internal abstract class GradientRepeater
    {
        protected GradientRepeater()
        {
        }

        public abstract double BoundLerp(double t);
    }
}

