namespace PaintDotNet.Shapes
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Shapes.AddIns;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static class ShapeManager
    {
        private static readonly ConcurrentDictionary<ShapeInfo, Shape> shapeCache = new ConcurrentDictionary<ShapeInfo, Shape>();
        private static readonly ConcurrentDictionary<Type, ShapeFactory> shapeFactoryCache = new ConcurrentDictionary<Type, ShapeFactory>();

        internal static Shape CreateShape(ShapeInfo shapeInfo)
        {
            VerifyShapeInfo(shapeInfo);
            return GetShapeFactory(shapeInfo.FactoryType).CreateShape(shapeInfo.ID);
        }

        internal static ShapeFactory CreateShapeFactory(Type factoryType)
        {
            VerifyShapeFactoryType(factoryType);
            return (ShapeFactory) Activator.CreateInstance(factoryType);
        }

        public static IEnumerable<IGrouping<ShapeCategory, ShapeInfo>> GetGroupedShapeInfos() => 
            (from si in GetShapeInfos() group si by GetShape(si).Category);

        public static Shape GetShape(ShapeInfo shapeInfo)
        {
            VerifyShapeInfo(shapeInfo);
            return shapeCache.GetOrAdd(shapeInfo, si => CreateShape(si));
        }

        public static IEnumerable<ShapeFactory> GetShapeFactories() => 
            (from sft in GetShapeFactoryTypes() select GetShapeFactory(sft));

        public static ShapeFactory GetShapeFactory(Type factoryType)
        {
            VerifyShapeFactoryType(factoryType);
            return shapeFactoryCache.GetOrAdd(factoryType, ft => CreateShapeFactory(ft));
        }

        [IteratorStateMachine(typeof(<GetShapeFactoryTypes>d__3))]
        public static IEnumerable<Type> GetShapeFactoryTypes()
        {
            yield return typeof(PdnShapeFactory);
            yield return typeof(CustomXamlShapesFactory);
        }

        [IteratorStateMachine(typeof(<GetShapeInfos>d__8))]
        public static IEnumerable<ShapeInfo> GetShapeInfos() => 
            new <GetShapeInfos>d__8(-2);

        private static void VerifyShapeFactoryType(Type factoryType)
        {
            Validate.IsNotNull<Type>(factoryType, "factoryType");
            if (!typeof(ShapeFactory).IsAssignableFrom(factoryType))
            {
                throw new ArgumentException();
            }
        }

        private static void VerifyShapeInfo(ShapeInfo shapeInfo)
        {
            Validate.IsNotNull<ShapeInfo>(shapeInfo, "shapeInfo");
            VerifyShapeFactoryType(shapeInfo.FactoryType);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ShapeManager.<>c <>9 = new ShapeManager.<>c();
            public static Func<ShapeInfo, Shape> <>9__11_0;
            public static Func<Type, ShapeFactory> <>9__4_0;
            public static Func<Type, ShapeFactory> <>9__6_0;
            public static Func<ShapeInfo, ShapeCategory> <>9__9_0;

            internal ShapeCategory <GetGroupedShapeInfos>b__9_0(ShapeInfo si) => 
                ShapeManager.GetShape(si).Category;

            internal Shape <GetShape>b__11_0(ShapeInfo si) => 
                ShapeManager.CreateShape(si);

            internal ShapeFactory <GetShapeFactories>b__4_0(Type sft) => 
                ShapeManager.GetShapeFactory(sft);

            internal ShapeFactory <GetShapeFactory>b__6_0(Type ft) => 
                ShapeManager.CreateShapeFactory(ft);
        }


        [CompilerGenerated]
        private sealed class <GetShapeInfos>d__8 : IEnumerable<ShapeInfo>, IEnumerable, IEnumerator<ShapeInfo>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private ShapeInfo <>2__current;
            private IEnumerator<ShapeFactory> <>7__wrap1;
            private IEnumerator<string> <>7__wrap2;
            private int <>l__initialThreadId;
            private ShapeFactory <factory>5__1;

            [DebuggerHidden]
            public <GetShapeInfos>d__8(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private void <>m__Finally2()
            {
                this.<>1__state = -3;
                if (this.<>7__wrap2 != null)
                {
                    this.<>7__wrap2.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<>7__wrap1 = ShapeManager.GetShapeFactories().GetEnumerator();
                        this.<>1__state = -3;
                        while (this.<>7__wrap1.MoveNext())
                        {
                            this.<factory>5__1 = this.<>7__wrap1.Current;
                            this.<>7__wrap2 = this.<factory>5__1.GetShapeIDs().GetEnumerator();
                            this.<>1__state = -4;
                            while (this.<>7__wrap2.MoveNext())
                            {
                                string current = this.<>7__wrap2.Current;
                                ShapeInfo info = new ShapeInfo(this.<factory>5__1.GetType(), current);
                                this.<>2__current = info;
                                this.<>1__state = 1;
                                return true;
                            Label_009D:
                                this.<>1__state = -4;
                            }
                            this.<>m__Finally2();
                            this.<>7__wrap2 = null;
                            this.<factory>5__1 = null;
                        }
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        return false;
                    }
                    if (num != 1)
                    {
                        return false;
                    }
                    goto Label_009D;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<ShapeInfo> IEnumerable<ShapeInfo>.GetEnumerator()
            {
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    return this;
                }
                return new ShapeManager.<GetShapeInfos>d__8(0);
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<PaintDotNet.Shapes.ShapeInfo>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                switch (num)
                {
                    case -4:
                    case -3:
                    case 1:
                        try
                        {
                            switch (num)
                            {
                                case -4:
                                case 1:
                                    try
                                    {
                                    }
                                    finally
                                    {
                                        this.<>m__Finally2();
                                    }
                                    break;
                            }
                        }
                        finally
                        {
                            this.<>m__Finally1();
                        }
                        break;
                }
            }

            ShapeInfo IEnumerator<ShapeInfo>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

