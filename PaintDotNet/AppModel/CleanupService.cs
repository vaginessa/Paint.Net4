namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using PaintDotNet.Runtime;
    using System;

    internal static class CleanupService
    {
        private static CleanupManagerController controller;

        public static void AddCleanupSource(CleanupSource cleanupSource)
        {
            controller.AddCleanupSource(cleanupSource);
        }

        public static void Initialize()
        {
            if (controller == null)
            {
                controller = CleanupManager.GetController();
            }
        }

        public static void RegisterTrimmableObject(ITrimmable trimmable)
        {
            controller.RegisterTrimmableObject(trimmable);
        }

        public static void RemoveCleanupSource(CleanupSource cleanupSource)
        {
            controller.RemoveCleanupSource(cleanupSource);
        }
    }
}

