namespace PaintDotNet.Data.Dds
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class DdsSquish
    {
        private static unsafe void CallCompressImage(byte[] rgba, int width, int height, byte[] blocks, int flags, ProgressFn progressFn)
        {
            fixed (byte* numRef = rgba)
            {
                fixed (byte* numRef2 = blocks)
                {
                    if (Processor.Architecture == ProcessorArchitecture.X64)
                    {
                        SquishInterfaceX64.SquishCompressImage(numRef, width, height, numRef2, flags, progressFn);
                    }
                    else
                    {
                        SquishInterfaceX86.SquishCompressImage(numRef, width, height, numRef2, flags, progressFn);
                    }
                }
            }
            GC.KeepAlive(progressFn);
        }

        private static unsafe void CallDecompressImage(byte[] rgba, int width, int height, byte[] blocks, int flags, ProgressFn progressFn)
        {
            fixed (byte* numRef = rgba)
            {
                fixed (byte* numRef2 = blocks)
                {
                    if (Processor.Architecture == ProcessorArchitecture.X64)
                    {
                        SquishInterfaceX64.SquishDecompressImage(numRef, width, height, numRef2, flags, progressFn);
                    }
                    else
                    {
                        SquishInterfaceX86.SquishDecompressImage(numRef, width, height, numRef2, flags, progressFn);
                    }
                }
            }
            GC.KeepAlive(progressFn);
        }

        internal static byte[] CompressImage(Surface inputSurface, int squishFlags, ProgressFn progressFn)
        {
            byte[] rgba = new byte[(inputSurface.Width * inputSurface.Height) * 4];
            for (int i = 0; i < inputSurface.Height; i++)
            {
                for (int j = 0; j < inputSurface.Width; j++)
                {
                    ColorBgra point = inputSurface.GetPoint(j, i);
                    int index = ((i * inputSurface.Width) * 4) + (j * 4);
                    rgba[index] = point.R;
                    rgba[index + 1] = point.G;
                    rgba[index + 2] = point.B;
                    rgba[index + 3] = point.A;
                }
            }
            int num = ((inputSurface.Width + 3) / 4) * ((inputSurface.Height + 3) / 4);
            int num2 = ((squishFlags & 1) != 0) ? 8 : 0x10;
            byte[] blocks = new byte[num * num2];
            CallCompressImage(rgba, inputSurface.Width, inputSurface.Height, blocks, squishFlags, progressFn);
            return blocks;
        }

        internal static byte[] DecompressImage(byte[] blocks, int width, int height, int flags)
        {
            byte[] rgba = new byte[(width * height) * 4];
            CallDecompressImage(rgba, width, height, blocks, flags, null);
            return rgba;
        }

        public static void Initialize()
        {
            if (Processor.Architecture == ProcessorArchitecture.X64)
            {
                SquishInterfaceX64.SquishInitialize();
            }
            else
            {
                SquishInterfaceX86.SquishInitialize();
            }
        }

        private static bool Is64Bit() => 
            (Marshal.SizeOf<IntPtr>(IntPtr.Zero) == 8);

        internal delegate void ProgressFn(int workDone, int workTotal);

        [Flags]
        public enum SquishFlags
        {
            kColourClusterFit = 8,
            kColourIterativeClusterFit = 0x100,
            kColourMetricPerceptual = 0x20,
            kColourMetricUniform = 0x40,
            kColourRangeFit = 0x10,
            kDxt1 = 1,
            kDxt3 = 2,
            kDxt5 = 4,
            kWeightColourByAlpha = 0x80
        }

        private sealed class SquishInterfaceX64
        {
            private const string DllName = "PaintDotNet.SystemLayer.Native.x64.dll";

            [DllImport("PaintDotNet.SystemLayer.Native.x64.dll", CallingConvention=CallingConvention.StdCall)]
            internal static extern unsafe void SquishCompressImage(byte* rgba, int width, int height, byte* blocks, int flags, [MarshalAs(UnmanagedType.FunctionPtr)] DdsSquish.ProgressFn progressFn);
            [DllImport("PaintDotNet.SystemLayer.Native.x64.dll", CallingConvention=CallingConvention.StdCall)]
            internal static extern unsafe void SquishDecompressImage(byte* rgba, int width, int height, byte* blocks, int flags, [MarshalAs(UnmanagedType.FunctionPtr)] DdsSquish.ProgressFn progressFn);
            [DllImport("PaintDotNet.SystemLayer.Native.x64.dll", CallingConvention=CallingConvention.StdCall)]
            internal static extern void SquishInitialize();
        }

        private sealed class SquishInterfaceX86
        {
            private const string DllName = "PaintDotNet.SystemLayer.Native.x86.dll";

            [DllImport("PaintDotNet.SystemLayer.Native.x86.dll", CallingConvention=CallingConvention.StdCall)]
            internal static extern unsafe void SquishCompressImage(byte* rgba, int width, int height, byte* blocks, int flags, [MarshalAs(UnmanagedType.FunctionPtr)] DdsSquish.ProgressFn progressFn);
            [DllImport("PaintDotNet.SystemLayer.Native.x86.dll", CallingConvention=CallingConvention.StdCall)]
            internal static extern unsafe void SquishDecompressImage(byte* rgba, int width, int height, byte* blocks, int flags, [MarshalAs(UnmanagedType.FunctionPtr)] DdsSquish.ProgressFn progressFn);
            [DllImport("PaintDotNet.SystemLayer.Native.x86.dll", CallingConvention=CallingConvention.StdCall)]
            internal static extern void SquishInitialize();
        }
    }
}

