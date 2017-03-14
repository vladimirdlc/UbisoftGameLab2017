using System;
using System.IO;
using NAudio.Wave;

namespace Dissonance.Audio
{
    internal class AudioFileWriter
        : IDisposable
    {
        private readonly SpinLock _lock = new SpinLock();

        private readonly WaveFileWriter _writer;
        private bool _disposed;

        public AudioFileWriter(string filename, WaveFormat format)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            if (format == null)
                throw new ArgumentNullException("format");

            if (string.IsNullOrEmpty(Path.GetExtension(filename)))
                filename += ".wav";

            var directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            _writer = new WaveFileWriter(File.Open(filename, FileMode.CreateNew), format);
        }

        public void Dispose()
        {
            using (_lock.Lock())
            {
                _disposed = true;
                _writer.Dispose();
            }
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void WriteSamples(ArraySegment<float> samples)
        {
            if (_disposed)
                return;

            using (_lock.Lock())
            {
                if (_disposed)
                    return;

                _writer.WriteSamples(samples.Array, samples.Offset, samples.Count);
            }
        }
    }
}
