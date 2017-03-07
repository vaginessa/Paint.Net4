namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class BitmapHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;
        private int layerIndex;
        private const ulong maxChunkSize = 0x10000L;
        private DeleteFileOnFree tempFileHandle;
        private string tempFileName;

        public BitmapHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex, IEnumerable<RectInt32> changedRegion) : this(name, image, historyWorkspace, layerIndex, changedRegion, ((BitmapLayer) historyWorkspace.Document.Layers[layerIndex]).Surface)
        {
        }

        public BitmapHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex, IEnumerable<RectInt32> changedRegion, Surface copyFromThisSurface) : base(name, image)
        {
            RectInt32[] scans = changedRegion.ToArrayEx<RectInt32>();
            this.historyWorkspace = historyWorkspace;
            this.layerIndex = layerIndex;
            this.tempFileName = FileSystem.GetTempFileName();
            FileStream output = null;
            try
            {
                output = FileSystem.OpenStreamingFile(this.tempFileName, FileAccess.Write);
                SaveSurfaceRegion(output, copyFromThisSurface, scans);
            }
            finally
            {
                if (output != null)
                {
                    output.Dispose();
                    output = null;
                }
            }
            this.tempFileHandle = new DeleteFileOnFree(this.tempFileName);
            BitmapHistoryMementoData data = new BitmapHistoryMementoData(scans);
            base.Data = data;
        }

        private static unsafe void LoadOrSaveSurfaceRegion(Stream stream, Surface surface, RectInt32[] scans, bool trueForSave)
        {
            int length = scans.Length;
            if (length != 0)
            {
                void*[] voidPtrArray;
                ulong[] numArray;
                RectInt32 b = surface.Bounds<ColorBgra>();
                RectInt32 bounds = RectInt32.Intersect(scans.Bounds(), b);
                int num5 = 0;
                long num6 = (bounds.Width * bounds.Height) * 4L;
                if (((length == 1) && (num6 <= 0xffffffffL)) && surface.IsContiguousMemoryRegion(bounds))
                {
                    voidPtrArray = new void*[] { surface.GetPointAddressUnchecked(bounds.Location.ToGdipPoint()) };
                    numArray = new ulong[] { num6 };
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        RectInt32 num9 = RectInt32.Intersect(scans[i], b);
                        if ((num9.Width != 0) && (num9.Height != 0))
                        {
                            num5 += num9.Height;
                        }
                    }
                    int index = 0;
                    voidPtrArray = new void*[num5];
                    numArray = new ulong[num5];
                    for (int j = 0; j < length; j++)
                    {
                        RectInt32 num11 = RectInt32.Intersect(scans[j], b);
                        if ((num11.Width != 0) && (num11.Height != 0))
                        {
                            for (int k = num11.Y; k < (num11.Y + num11.Height); k++)
                            {
                                voidPtrArray[index] = (void*) surface.GetPointAddress(num11.X, k);
                                numArray[index] = (ulong) (num11.Width * 4L);
                                index++;
                            }
                        }
                    }
                }
                if (trueForSave)
                {
                    WriteToStreamGather(stream, voidPtrArray, numArray);
                }
                else
                {
                    ReadFromStreamScatter(stream, voidPtrArray, numArray);
                }
            }
        }

        private static void LoadSurfaceRegion(FileStream input, Surface surface, RectInt32[] scans)
        {
            LoadOrSaveSurfaceRegion(input, surface, scans, false);
        }

        protected override HistoryMemento OnUndo(ProgressEventHandler progressCallback)
        {
            BitmapHistoryMementoData data = base.Data as BitmapHistoryMementoData;
            BitmapLayer layer = (BitmapLayer) this.historyWorkspace.Document.Layers[this.layerIndex];
            RectInt32[] savedRegion = data.SavedRegion;
            MaskedSurface surface = null;
            BitmapHistoryMemento memento = new BitmapHistoryMemento(base.Name, base.Image, this.historyWorkspace, this.layerIndex, savedRegion);
            if (surface != null)
            {
                surface.Draw(layer.Surface);
            }
            else
            {
                using (FileStream stream = FileSystem.OpenStreamingFile(this.tempFileName, FileAccess.Read))
                {
                    LoadSurfaceRegion(stream, layer.Surface, data.SavedRegion);
                }
                this.tempFileHandle.Dispose();
                this.tempFileHandle = null;
            }
            if (savedRegion.Length != 0)
            {
                RectInt32 roi = savedRegion.Bounds();
                layer.Invalidate(roi);
            }
            return memento;
        }

        private static unsafe void ReadFromStream(Stream input, void* pBuffer, ulong cbBuffer)
        {
            int num = (int) Math.Min(0x10000L, cbBuffer);
            byte[] buffer = new byte[num];
            ulong num2 = cbBuffer;
            fixed (byte* numRef = buffer)
            {
                while (num2 != 0)
                {
                    int count = (int) Math.Min((ulong) buffer.Length, num2);
                    int num4 = input.Read(buffer, 0, count);
                    if (num4 == 0)
                    {
                        throw new EndOfStreamException();
                    }
                    Memory.Copy(pBuffer, (void*) numRef, (ulong) num4);
                    pBuffer += num4;
                    num2 -= num4;
                    if (num2 > cbBuffer)
                    {
                        throw new InternalErrorException("cb > cbBuffer");
                    }
                }
            }
        }

        private static unsafe void ReadFromStreamScatter(Stream input, void*[] ppvBuffers, ulong[] lengths)
        {
            for (int i = 0; i < ppvBuffers.Length; i++)
            {
                ReadFromStream(input, ppvBuffers[i], lengths[i]);
            }
        }

        private static void SaveSurfaceRegion(FileStream output, Surface surface, RectInt32[] scans)
        {
            LoadOrSaveSurfaceRegion(output, surface, scans, true);
        }

        private static unsafe void WriteToStream(Stream output, void* pBuffer, ulong cbBuffer)
        {
            int num = (int) Math.Min(0x10000L, cbBuffer);
            byte[] buffer = new byte[num];
            ulong num2 = cbBuffer;
            fixed (byte* numRef = buffer)
            {
                while (num2 != 0)
                {
                    ulong length = Math.Min((ulong) num, num2);
                    Memory.Copy((void*) numRef, pBuffer, length);
                    num2 -= length;
                    pBuffer += (void*) length;
                    output.Write(buffer, 0, (int) length);
                }
            }
        }

        private static unsafe void WriteToStreamGather(Stream output, void*[] ppvBuffers, ulong[] lengths)
        {
            for (int i = 0; i < ppvBuffers.Length; i++)
            {
                WriteToStream(output, ppvBuffers[i], lengths[i]);
            }
        }

        [Serializable]
        private sealed class BitmapHistoryMementoData : HistoryMementoData
        {
            private RectInt32[] savedRegion;

            public BitmapHistoryMementoData(RectInt32[] savedRegion)
            {
                this.savedRegion = savedRegion;
            }

            protected override void Dispose(bool disposing)
            {
                this.savedRegion = null;
                base.Dispose(disposing);
            }

            public RectInt32[] SavedRegion =>
                this.savedRegion;
        }

        private class DeleteFileOnFree : IDisposable
        {
            private IntPtr bstrFileName;

            public DeleteFileOnFree(string fileName)
            {
                this.bstrFileName = Marshal.StringToBSTR(fileName);
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (this.bstrFileName != IntPtr.Zero)
                {
                    string filePath = Marshal.PtrToStringBSTR(this.bstrFileName);
                    Marshal.FreeBSTR(this.bstrFileName);
                    this.bstrFileName = IntPtr.Zero;
                    bool flag = FileSystem.TryDeleteFile(filePath);
                }
            }

            ~DeleteFileOnFree()
            {
                this.Dispose(false);
            }
        }
    }
}

