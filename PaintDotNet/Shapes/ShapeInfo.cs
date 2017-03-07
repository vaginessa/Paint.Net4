namespace PaintDotNet.Shapes
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using System;

    [Serializable]
    internal sealed class ShapeInfo : IEquatable<ShapeInfo>
    {
        private Type factoryType;
        private string id;

        public ShapeInfo(Type factoryType, string id)
        {
            Validate.Begin().IsNotNull<Type>(factoryType, "factoryType").IsNotNull<string>(id, "id").Check();
            this.factoryType = factoryType;
            this.id = id;
        }

        public bool Equals(ShapeInfo other)
        {
            if (other == null)
            {
                return false;
            }
            return ((this.factoryType == other.factoryType) && string.Equals(this.id, other.id, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj) => 
            EquatableUtil.Equals<ShapeInfo, object>(this, obj);

        public override int GetHashCode() => 
            HashCodeUtil.CombineHashCodes(this.factoryType.GetHashCode(), this.id.GetHashCode());

        public static bool operator ==(ShapeInfo x, ShapeInfo y) => 
            EquatableUtil.OperatorEquals<ShapeInfo>(x, y);

        public static bool operator !=(ShapeInfo x, ShapeInfo y) => 
            !(x == y);

        public Type FactoryType =>
            this.factoryType;

        public string ID =>
            this.id;
    }
}

