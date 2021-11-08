using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ImageSharpRotateTest001
{
    class Program
    {
        static void Main(string[] args)
        {
            var bytes = File.ReadAllBytes("Images\\TestImage.png");

            List<(string Name, IResampler Resampler)> resamplers = new List<(string Name, IResampler Resampler)>()
            {
                (nameof(KnownResamplers.Bicubic), KnownResamplers.Bicubic),
                (nameof(KnownResamplers.Box), KnownResamplers.Box),
                (nameof(KnownResamplers.CatmullRom), KnownResamplers.CatmullRom),
                (nameof(KnownResamplers.Hermite), KnownResamplers.Hermite),
                (nameof(KnownResamplers.Lanczos2), KnownResamplers.Lanczos2),
                (nameof(KnownResamplers.Lanczos3), KnownResamplers.Lanczos3),
                (nameof(KnownResamplers.Lanczos5), KnownResamplers.Lanczos5),
                (nameof(KnownResamplers.Lanczos8), KnownResamplers.Lanczos8),
                (nameof(KnownResamplers.MitchellNetravali), KnownResamplers.MitchellNetravali),
                (nameof(KnownResamplers.NearestNeighbor), KnownResamplers.NearestNeighbor),
                (nameof(KnownResamplers.Robidoux), KnownResamplers.Robidoux),
                (nameof(KnownResamplers.RobidouxSharp), KnownResamplers.RobidouxSharp),
                (nameof(KnownResamplers.Spline), KnownResamplers.Spline),
                (nameof(KnownResamplers.Triangle), KnownResamplers.Triangle),
                (nameof(KnownResamplers.Welch), KnownResamplers.Welch)
            };


            foreach(var resampler in resamplers)
            {
                RunWithStopWatch(() => RotateImageWithImageSharp(bytes, 10, resampler.Resampler), $"RotateImageWithImageSharp() - resampler {resampler.Name}{Environment.NewLine}", "RotateImageWithImageSharp() completed");
            }

            Console.WriteLine();
            RunWithStopWatch(() => RotateImageWithSystemDrawing(bytes, 10), $"RotateImageWithSystemDrawing(){Environment.NewLine}", "RotateImageWithSystemDrawing() completed");

            Console.WriteLine("Done.");
        }

        public static byte[] RotateImageWithImageSharp(byte[] imageBytes, float angleDegrees, IResampler resampler)
        {
            using MemoryStream inputStream = RunWithStopWatch(() => new MemoryStream(imageBytes), "Initialising input stream.", "Input stream initialised");
            using Image image = RunWithStopWatch(() => Image.Load(inputStream), "Loading image.", "Image loaded");
            using MemoryStream outputStream = new MemoryStream();

            RunWithStopWatch(() => image.Mutate(i => i.Rotate((int)angleDegrees, resampler)), "Rotating.", "Rotation completed");
            RunWithStopWatch(() => image.SaveAsPng(outputStream), "Saving as png to stream.", "Save completed");

            return outputStream.ToArray();
        }

        public static byte[] RotateImageWithSystemDrawing(byte[] imageBytes, float angleDegrees)
        {
            using MemoryStream inputStream = RunWithStopWatch(() => new MemoryStream(imageBytes), "Initialising input stream.", "Input stream initialised");
            using System.Drawing.Image image = RunWithStopWatch(() => System.Drawing.Image.FromStream(inputStream), "Loading image.", "Image loaded");
            using var bitmap = RunWithStopWatch(() => new System.Drawing.Bitmap(image.Width, image.Height), "Creating bitmap.", "Bitmap created");
            using var graphics = RunWithStopWatch(() => System.Drawing.Graphics.FromImage(bitmap), "Creating graphics.", "Graphics created");
            using MemoryStream outputStream = new MemoryStream();

            Point center = new Point(image.Width / 2, image.Height / 2);

            RunWithStopWatch(() => {
                graphics.TranslateTransform(center.X, center.Y);
                graphics.RotateTransform(angleDegrees);
                graphics.TranslateTransform(-center.X, -center.Y);
                graphics.DrawImage(image, 0, 0);
            }, "Rotating.", "Rotation completed");

            RunWithStopWatch(() => image.Save(outputStream, System.Drawing.Imaging.ImageFormat.Png), "Saving as png to stream.", "Save completed");

            return outputStream.ToArray();
        }

        public static void RunWithStopWatch(Action action, string startMessage, string finishMessage)
        {
            Console.WriteLine(startMessage);
            var sw = Stopwatch.StartNew();

            action();

            sw.Stop();
            Console.WriteLine($"{finishMessage} in {sw.Elapsed}");
            Console.WriteLine();
        }

        public static T RunWithStopWatch<T>(Func<T> action, string startMessage, string finishMessage)
        {
            Console.WriteLine(startMessage);
            var sw = Stopwatch.StartNew();

            T result = action();

            sw.Stop();
            Console.WriteLine($"{finishMessage} in {sw.Elapsed}");
            Console.WriteLine();

            return result;
        }
    }
}
