namespace PaintDotNet.Shapes.AddIns
{
    using PaintDotNet.AppModel;
    using PaintDotNet.Collections;
    using PaintDotNet.Resources;
    using PaintDotNet.Shapes;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Markup;

    internal sealed class CustomXamlShapesFactory : ShapeFactory
    {
        private Dictionary<string, Func<Shape>> shapeIDToShapeValueFactoryMap;
        private readonly object sync = new object();

        public override Shape CreateShape(string id)
        {
            Func<Shape> func = this.GetShapeIDToShapeValueFactoryMap()[id];
            return func();
        }

        [IteratorStateMachine(typeof(<EnumerateXamlFiles>d__7))]
        private IEnumerable<string> EnumerateXamlFiles() => 
            new <EnumerateXamlFiles>d__7(-2);

        public override IEnumerable<string> GetShapeIDs() => 
            this.GetShapeIDToShapeValueFactoryMap().Keys;

        private IDictionary<string, Func<Shape>> GetShapeIDToShapeValueFactoryMap()
        {
            object sync = this.sync;
            lock (sync)
            {
                if (this.shapeIDToShapeValueFactoryMap == null)
                {
                    Func<object, Func<Shape>> loadShapeFn = path => this.LoadShape((string) path);
                    Task<Func<Shape>>[] tasks = (from xamlPath in this.EnumerateXamlFiles() select Task.Factory.StartNew<Func<Shape>>(loadShapeFn, xamlPath)).ToArrayEx<Task<Func<Shape>>>();
                    try
                    {
                        Task.WaitAll(tasks);
                    }
                    catch (AggregateException)
                    {
                    }
                    this.shapeIDToShapeValueFactoryMap = new Dictionary<string, Func<Shape>>(tasks.Length);
                    foreach (Task<Func<Shape>> task in tasks)
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            Func<Shape> result = task.Result;
                            string asyncState = (string) task.AsyncState;
                            this.shapeIDToShapeValueFactoryMap.Add(asyncState, result);
                        }
                        else
                        {
                            string filePath = (string) task.AsyncState;
                            PluginErrorService.Instance.ReportShapeLoadError(filePath, task.Exception);
                        }
                    }
                }
                return this.shapeIDToShapeValueFactoryMap;
            }
        }

        private Func<Shape> LoadShape(string xamlPath)
        {
            using (FileStream stream = new FileStream(xamlPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                object obj2 = XamlReader.Load(stream);
                if (obj2 == null)
                {
                    throw new FormatException("XamlReader.Load() returned null");
                }
                SimpleGeometryShape asSimpleGeometryShape = obj2 as SimpleGeometryShape;
                if (asSimpleGeometryShape == null)
                {
                    throw new InvalidCastException("XAML contained an invalid root object type (" + obj2.GetType().FullName + ")");
                }
                if (asSimpleGeometryShape.Geometry == null)
                {
                    throw new FormatException("SimpleGeometryShape.Geometry is null");
                }
                asSimpleGeometryShape.Freeze();
                return () => new SimpleGeometryShapeWrapper(xamlPath, asSimpleGeometryShape);
            }
        }

        [CompilerGenerated]
        private sealed class <EnumerateXamlFiles>d__7 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private string <>2__current;
            private int <>l__initialThreadId;
            private bool <anyLeft>5__2;
            private string <current>5__3;
            private IEnumerator<string> <xamlShapesEnum>5__1;

            [DebuggerHidden]
            public <EnumerateXamlFiles>d__7(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<xamlShapesEnum>5__1 != null)
                {
                    this.<xamlShapesEnum>5__1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        string path = Path.Combine(PdnInfo.ApplicationDir, "Shapes");
                        if (!Directory.Exists(path))
                        {
                            goto Label_00E3;
                        }
                        this.<xamlShapesEnum>5__1 = Directory.EnumerateFiles(path, "*.xaml", SearchOption.AllDirectories).GetEnumerator();
                        this.<>1__state = -3;
                        this.<anyLeft>5__2 = true;
                        while (this.<anyLeft>5__2)
                        {
                            try
                            {
                                this.<anyLeft>5__2 = this.<xamlShapesEnum>5__1.MoveNext();
                            }
                            catch (Exception)
                            {
                                flag = false;
                                goto Label_00D4;
                            }
                            if (!this.<anyLeft>5__2)
                            {
                                break;
                            }
                            try
                            {
                                this.<current>5__3 = this.<xamlShapesEnum>5__1.Current;
                            }
                            catch (Exception)
                            {
                                flag = false;
                                goto Label_00D4;
                            }
                            this.<>2__current = this.<current>5__3;
                            this.<>1__state = 1;
                            return true;
                        Label_00B5:
                            this.<>1__state = -3;
                            this.<current>5__3 = null;
                        }
                        this.<>m__Finally1();
                        goto Label_00DC;
                    }
                    if (num != 1)
                    {
                        return false;
                    }
                    goto Label_00B5;
                Label_00D4:
                    this.<>m__Finally1();
                    return flag;
                Label_00DC:
                    this.<xamlShapesEnum>5__1 = null;
                Label_00E3:
                    flag = false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    return this;
                }
                return new CustomXamlShapesFactory.<EnumerateXamlFiles>d__7(0);
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.String>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case -3:
                    case 1:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally1();
                        }
                        break;
                }
            }

            string IEnumerator<string>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

