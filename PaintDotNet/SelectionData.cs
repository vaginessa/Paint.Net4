namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using System;

    [Serializable]
    internal sealed class SelectionData : ICloneable
    {
        private GeometryList baseGeometry = new GeometryList().EnsureFrozen();
        private SelectionCombineMode continuationCombineMode = SelectionCombineMode.Xor;
        private GeometryList continuationGeometry = new GeometryList().EnsureFrozen();
        private Matrix3x2Double cumulativeTransform = Matrix3x2Double.Identity;
        private Matrix3x2Double interimTransform = Matrix3x2Double.Identity;

        public SelectionData Clone() => 
            new SelectionData { 
                baseGeometry = this.baseGeometry.ToFrozen(),
                continuationGeometry = this.continuationGeometry.ToFrozen(),
                continuationCombineMode = this.continuationCombineMode,
                cumulativeTransform = this.cumulativeTransform,
                interimTransform = this.interimTransform
            };

        object ICloneable.Clone() => 
            this.Clone();

        public GeometryList BaseGeometry
        {
            get => 
                this.baseGeometry;
            set
            {
                this.baseGeometry = value;
            }
        }

        public SelectionCombineMode ContinuationCombineMode
        {
            get => 
                this.continuationCombineMode;
            set
            {
                this.continuationCombineMode = value;
            }
        }

        public GeometryList ContinuationGeometry
        {
            get => 
                this.continuationGeometry;
            set
            {
                this.continuationGeometry = value;
            }
        }

        public Matrix3x2Double CumulativeTransform
        {
            get => 
                this.cumulativeTransform;
            set
            {
                this.cumulativeTransform = value;
            }
        }

        public Matrix3x2Double InterimTransform
        {
            get => 
                this.interimTransform;
            set
            {
                this.interimTransform = value;
            }
        }
    }
}

