namespace PaintDotNet.Collections
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Runtime;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FastUncheckedRectInt32ArrayWrapper : IList<RectInt32>, ICollection<RectInt32>, IEnumerable<RectInt32>, IEnumerable, IReadOnlyList<RectInt32>, IReadOnlyCollection<RectInt32>, IDisposable
    {
        private SafeGCHandle<RectInt32[]> arrayGCHandle;
        private unsafe RectInt32* pArray;
        private int count;
        public FastUncheckedRectInt32ArrayWrapper(RectInt32[] array) : this(array, 0, array.Length)
        {
        }

        public unsafe FastUncheckedRectInt32ArrayWrapper(RectInt32[] array, int startIndex, int length)
        {
            this.arrayGCHandle = SafeGCHandle.Alloc<RectInt32[]>(array, GCHandleType.Pinned);
            this.pArray = (RectInt32*) ((((IntPtr) startIndex) * sizeof(RectInt32)) + ((void*) this.arrayGCHandle.AddrOfPinnedObject()));
            this.count = length;
        }

        public FastUncheckedRectInt32ArrayWrapper(UnsafeList<RectInt32> list) : this(list, 0, list.Count)
        {
        }

        public unsafe FastUncheckedRectInt32ArrayWrapper(UnsafeList<RectInt32> list, int startIndex, int length)
        {
            RectInt32[] numArray;
            int num;
            list.GetArray(out numArray, out num);
            this.arrayGCHandle = SafeGCHandle.Alloc<RectInt32[]>(numArray, GCHandleType.Pinned);
            this.pArray = (RectInt32*) ((((IntPtr) startIndex) * sizeof(RectInt32)) + ((void*) this.arrayGCHandle.AddrOfPinnedObject()));
            this.count = length;
        }

        public unsafe void Dispose()
        {
            DisposableUtil.Free<SafeGCHandle<RectInt32[]>>(ref this.arrayGCHandle);
            this.pArray = null;
            this.count = -1;
        }

        public int IndexOf(RectInt32 item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, RectInt32 item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public RectInt32 this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => 
                this.pArray[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                this.pArray[index] = value;
            }
        }
        public void Add(RectInt32 item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(RectInt32 item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(RectInt32[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count =>
            this.count;
        public bool IsReadOnly =>
            false;
        public bool Remove(RectInt32 item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<RectInt32> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}

