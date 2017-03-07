namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Runtime;
    using System;
    using System.Runtime.InteropServices;

    internal sealed class RendererRowCache<TPixel> : Disposable where TPixel: struct, INaturalPixelInfo
    {
        private int currentRowCount;
        private TPixel[] freeRow;
        private int freeRowHitCount;
        private int freeRowMissCount;
        private int height;
        private int maxRowCount;
        private SafeGCHandle<TPixel[]>[] pinnedRows;
        private int[] rowGetCounts;
        private IntPtr[] rowPtrs;
        private int[] rowRefCounts;
        private int[] rowRenderCounts;
        private TPixel[][] rows;
        private static readonly int sizeOfTPixel;
        private IRenderer<TPixel> source;
        private int width;

        static RendererRowCache()
        {
            TPixel local = default(TPixel);
            RendererRowCache<TPixel>.sizeOfTPixel = local.BytesPerPixel;
        }

        public RendererRowCache(IRenderer<TPixel> source)
        {
            Validate.IsNotNull<IRenderer<TPixel>>(source, "source");
            this.source = source;
            this.width = this.source.Width;
            this.height = this.source.Height;
            this.rows = new TPixel[this.height][];
            this.pinnedRows = new SafeGCHandle<TPixel[]>[this.height];
            this.rowPtrs = new IntPtr[this.height];
            this.rowRefCounts = new int[this.height];
            this.rowGetCounts = new int[this.height];
            this.rowRenderCounts = new int[this.height];
        }

        public void AddRowRef(int row)
        {
            if (Int32Util.IsClamped(row, 0, this.height - 1))
            {
                this.rowRefCounts[row]++;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.rows != null))
            {
                for (int i = 0; i < this.height; i++)
                {
                    if (this.rowRefCounts[i] >= 1)
                    {
                        this.rowRefCounts[i] = 1;
                        this.ReleaseRowRef(i);
                    }
                }
            }
            this.freeRow = null;
            this.rows = null;
            this.pinnedRows = null;
            this.rowPtrs = null;
            this.rowRefCounts = null;
            base.Dispose(disposing);
        }

        private void FreeRow(int row)
        {
            this.rowPtrs[row] = IntPtr.Zero;
            if (!(this.source is ISurface<TPixel>))
            {
                DisposableUtil.Free<SafeGCHandle<TPixel[]>>(ref this.pinnedRows[row]);
                this.freeRow = this.rows[row];
                this.rows[row] = null;
                this.currentRowCount--;
            }
        }

        public IntPtr GetRow(int row)
        {
            this.rowGetCounts[row]++;
            if (this.rowRefCounts[row] == 0)
            {
                ExceptionUtil.ThrowInvalidOperationException($"row {row} has a ref count of 0");
            }
            if (this.rowPtrs[row] == IntPtr.Zero)
            {
                this.InitRow(row);
            }
            return this.rowPtrs[row];
        }

        public int GetRowRefCount(int row) => 
            this.rowRefCounts[row];

        private void InitRow(int row)
        {
            ISurface<TPixel> source = this.source as ISurface<TPixel>;
            if (source != null)
            {
                this.rowPtrs[row] = source.GetRowPointer<TPixel>(row);
            }
            else
            {
                if (this.freeRow == null)
                {
                    this.rows[row] = new TPixel[this.width];
                    this.freeRowMissCount++;
                }
                else
                {
                    this.rows[row] = this.freeRow;
                    this.freeRow = null;
                    this.freeRowHitCount++;
                }
                this.pinnedRows[row] = SafeGCHandle.Alloc<TPixel[]>(this.rows[row], GCHandleType.Pinned);
                this.rowPtrs[row] = this.pinnedRows[row].AddrOfPinnedObject();
                SharedSurface<TPixel> dst = new SharedSurface<TPixel>(this.rowPtrs[row], this.width, 1, this.width * RendererRowCache<TPixel>.sizeOfTPixel);
                this.source.Render(dst, new PointInt32(0, row));
                this.rowRenderCounts[row]++;
                this.currentRowCount++;
                this.maxRowCount = Math.Max(this.currentRowCount, this.maxRowCount);
            }
        }

        public bool IsRowCached(int row) => 
            (this.rowPtrs[row] != IntPtr.Zero);

        public void ReleaseRowRef(int row)
        {
            if (Int32Util.IsClamped(row, 0, this.height - 1))
            {
                this.rowRefCounts[row]--;
                if ((this.rowRefCounts[row] == 0) && this.IsRowCached(row))
                {
                    this.FreeRow(row);
                }
            }
        }

        public IRenderer<TPixel> Source =>
            this.source;
    }
}

