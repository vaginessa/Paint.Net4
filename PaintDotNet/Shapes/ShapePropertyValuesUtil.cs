namespace PaintDotNet.Shapes
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static class ShapePropertyValuesUtil
    {
        public static IDictionary<ShapeInfo, IDictionary<object, object>> ToMap(IEnumerable<TupleStruct<ShapeInfo, object, object>> shapePropertyValues)
        {
            Dictionary<ShapeInfo, IDictionary<object, object>> dictionary = new Dictionary<ShapeInfo, IDictionary<object, object>>();
            foreach (TupleStruct<ShapeInfo, object, object> struct2 in shapePropertyValues)
            {
                IDictionary<object, object> dictionary2;
                if (!dictionary.TryGetValue(struct2.Item1, out dictionary2))
                {
                    dictionary2 = new Dictionary<object, object>();
                    dictionary.Add(struct2.Item1, dictionary2);
                }
                dictionary2.Add(struct2.Item2, struct2.Item3);
            }
            return dictionary;
        }

        public static IDictionary<ShapeInfo, IDictionary<object, object>> ToReadOnlyMap(IEnumerable<TupleStruct<ShapeInfo, object, object>> shapePropertyValues)
        {
            IDictionary<ShapeInfo, IDictionary<object, object>> dictionary = ToMap(shapePropertyValues);
            foreach (ShapeInfo info in dictionary.Keys.ToArrayEx<ShapeInfo>())
            {
                IDictionary<object, object> dictionary3 = dictionary[info].AsReadOnly<object, object>();
                dictionary[info] = dictionary3;
            }
            return dictionary.AsReadOnly<ShapeInfo, IDictionary<object, object>>();
        }

        [IteratorStateMachine(typeof(<ToTable>d__0))]
        public static IEnumerable<TupleStruct<ShapeInfo, object, object>> ToTable(IEnumerable<KeyValuePair<ShapeInfo, IDictionary<object, object>>> shapePropertyValues) => 
            new <ToTable>d__0(-2) { <>3__shapePropertyValues = shapePropertyValues };

        [CompilerGenerated]
        private sealed class <ToTable>d__0 : IEnumerable<TupleStruct<ShapeInfo, object, object>>, IEnumerable, IEnumerator<TupleStruct<ShapeInfo, object, object>>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private TupleStruct<ShapeInfo, object, object> <>2__current;
            public IEnumerable<KeyValuePair<ShapeInfo, IDictionary<object, object>>> <>3__shapePropertyValues;
            private IEnumerator<KeyValuePair<ShapeInfo, IDictionary<object, object>>> <>7__wrap1;
            private IEnumerator<KeyValuePair<object, object>> <>7__wrap2;
            private int <>l__initialThreadId;
            private ShapeInfo <shapeInfo>5__1;
            private IEnumerable<KeyValuePair<ShapeInfo, IDictionary<object, object>>> shapePropertyValues;

            [DebuggerHidden]
            public <ToTable>d__0(int <>1__state)
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
                        this.<>7__wrap1 = this.shapePropertyValues.GetEnumerator();
                        this.<>1__state = -3;
                        while (this.<>7__wrap1.MoveNext())
                        {
                            KeyValuePair<ShapeInfo, IDictionary<object, object>> current = this.<>7__wrap1.Current;
                            this.<shapeInfo>5__1 = current.Key;
                            this.<>7__wrap2 = current.Value.GetEnumerator();
                            this.<>1__state = -4;
                            while (this.<>7__wrap2.MoveNext())
                            {
                                KeyValuePair<object, object> pair2 = this.<>7__wrap2.Current;
                                this.<>2__current = TupleStruct.Create<ShapeInfo, object, object>(this.<shapeInfo>5__1, pair2.Key, pair2.Value);
                                this.<>1__state = 1;
                                return true;
                            Label_00AB:
                                this.<>1__state = -4;
                            }
                            this.<>m__Finally2();
                            this.<>7__wrap2 = null;
                            this.<shapeInfo>5__1 = null;
                        }
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        return false;
                    }
                    if (num != 1)
                    {
                        return false;
                    }
                    goto Label_00AB;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TupleStruct<ShapeInfo, object, object>> IEnumerable<TupleStruct<ShapeInfo, object, object>>.GetEnumerator()
            {
                ShapePropertyValuesUtil.<ToTable>d__0 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new ShapePropertyValuesUtil.<ToTable>d__0(0);
                }
                d__.shapePropertyValues = this.<>3__shapePropertyValues;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<PaintDotNet.TupleStruct<PaintDotNet.Shapes.ShapeInfo,System.Object,System.Object>>.GetEnumerator();

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

            TupleStruct<ShapeInfo, object, object> IEnumerator<TupleStruct<ShapeInfo, object, object>>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

