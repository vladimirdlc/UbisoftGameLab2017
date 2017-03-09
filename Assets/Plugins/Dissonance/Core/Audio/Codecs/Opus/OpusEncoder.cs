using System;

namespace Dissonance.Audio.Codecs.Opus
{
    internal class OpusEncoder : IVoiceEncoder
    {
        private readonly OpusNative.OpusEncoder _encoder;

        public int SampleRate
        {
            get { return 48000; }
        }

        private readonly int _frameSize;

        public float PacketLoss
        {
            set { _encoder.PacketLoss = value; }
        }

        public int FrameSize
        {
            get { return _frameSize; }
        }

        public OpusEncoder(AudioQuality quality, FrameSize frameSize)
        {
            _encoder = new OpusNative.OpusEncoder(SampleRate, 1)
            {
                EnableForwardErrorCorrection = false,
                Bitrate = GetTargetBitrate(quality)
            };

            _frameSize = GetFrameSize(frameSize);
        }

        private int GetTargetBitrate(AudioQuality quality)
        {
            // https://wiki.xiph.org/Opus_Recommended_Settings#Recommended_Bitrates
            switch (quality)
            {
                case AudioQuality.Low:
                    return 10000;
                case AudioQuality.Medium:
                    return 17000;
                case AudioQuality.High:
                    return 24000;
                default:
                    throw new ArgumentOutOfRangeException("quality", quality, null);
            }
        }

        public int GetFrameSize(FrameSize size)
        {
            switch (size)
            {
                case Dissonance.FrameSize.Small:
                    return _encoder.PermittedFrameSizes[3]; // 20ms
                case Dissonance.FrameSize.Medium:
                    return _encoder.PermittedFrameSizes[4]; // 40ms
                case Dissonance.FrameSize.Large:
                    return _encoder.PermittedFrameSizes[5]; // 60ms
                default:
                    throw new ArgumentOutOfRangeException("size", size, null);
            }
        }

        public ArraySegment<byte> Encode(ArraySegment<float> samples, ArraySegment<byte> encodedBuffer)
        {
            var encodedByteCount = _encoder.EncodeFloats(samples, encodedBuffer);
            return new ArraySegment<byte>(encodedBuffer.Array, encodedBuffer.Offset, encodedByteCount);
        }

        public void Reset()
        {
            _encoder.Reset();
        }

        public void Dispose()
        {
            _encoder.Dispose();
        }
    }
}
