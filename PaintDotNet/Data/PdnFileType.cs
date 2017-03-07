namespace PaintDotNet.Data
{
    using PaintDotNet;
    using PaintDotNet.IO;
    using System;
    using System.IO;

    internal sealed class PdnFileType : FileType
    {
        public PdnFileType() : base(PdnInfo.BareProductName, FileTypeFlags.None | FileTypeFlags.SavesWithProgress | FileTypeFlags.SupportsCustomHeaders | FileTypeFlags.SupportsLayers | FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, textArray1)
        {
            string[] textArray1 = new string[] { ".pdn" };
        }

        private long ApproximateMaxOutputOffset(Document measureMe) => 
            (((measureMe.Layers.Count * measureMe.Width) * measureMe.Height) * 4L);

        public override bool IsReflexive(SaveConfigToken token) => 
            true;

        protected override Document OnLoad(Stream input) => 
            Document.FromStream(input);

        protected override void OnSave(Document input, Stream output, SaveConfigToken token, Surface scratchSurface, ProgressEventHandler callback)
        {
            if (callback == null)
            {
                input.SaveToStream(output);
            }
            else
            {
                UpdateProgressTranslator translator = new UpdateProgressTranslator(this.ApproximateMaxOutputOffset(input), callback);
                input.SaveToStream(output, new IOEventHandler(translator.IOEventHandler));
            }
        }

        private sealed class UpdateProgressTranslator
        {
            private ProgressEventHandler callback;
            private long maxBytes;
            private long totalBytes;

            public UpdateProgressTranslator(long maxBytes, ProgressEventHandler callback)
            {
                this.maxBytes = maxBytes;
                this.callback = callback;
                this.totalBytes = 0L;
            }

            public void IOEventHandler(object sender, IOEventArgs e)
            {
                double num;
                PdnFileType.UpdateProgressTranslator translator = this;
                lock (translator)
                {
                    this.totalBytes += e.Count;
                    num = Math.Max(0.0, Math.Min((double) 100.0, (double) ((this.totalBytes * 100.0) / ((double) this.maxBytes))));
                }
                this.callback(sender, new ProgressEventArgs(num));
            }
        }
    }
}

