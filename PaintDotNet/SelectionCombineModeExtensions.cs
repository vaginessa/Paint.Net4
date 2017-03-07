namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;

    internal static class SelectionCombineModeExtensions
    {
        public static GeometryCombineMode ToGeometryCombineMode(this SelectionCombineMode mode)
        {
            switch (mode)
            {
                case SelectionCombineMode.Union:
                    return GeometryCombineMode.Union;

                case SelectionCombineMode.Exclude:
                    return GeometryCombineMode.Exclude;

                case SelectionCombineMode.Intersect:
                    return GeometryCombineMode.Intersect;

                case SelectionCombineMode.Xor:
                    return GeometryCombineMode.Xor;
            }
            throw ExceptionUtil.InvalidEnumArgumentException<SelectionCombineMode>(mode, "mode");
        }
    }
}

