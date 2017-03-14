using System;
using Dissonance.Audio.Codecs;
using Dissonance.Datastructures;
using Dissonance.Extensions;
using Dissonance.Networking;

namespace Dissonance.Audio.Playback
{
    internal class DecoderPipeline
        : IDecoderPipeline
    {
        private static readonly Log Log = Logs.Create(LogCategory.Playback, typeof (DecoderPipeline).Name);

        private readonly IPriorityManager _priorityManager;
        private readonly Action<DecoderPipeline> _completionHandler;
        private readonly TransferBuffer<EncodedAudio> _inputBuffer;
        private readonly ConcurrentPool<byte[]> _bytePool;
        private readonly BufferedDecoder _source;
        private readonly ISampleSource _output;

        private volatile bool _complete;
        private bool _sourceClosed;

        public int BufferCount
        {
            get { return _source.BufferCount + _inputBuffer.EstimatedUnreadCount; }
        }

        public ChannelPriority Priority { get; private set; }
        public bool Positional { get; private set; }

        public DecoderPipeline(IPriorityManager priority, IVoiceDecoder decoder, uint frameSize, Action<DecoderPipeline> completionHandler, bool softClip = true)
        {
            _priorityManager = priority;
            _completionHandler = completionHandler;
            _inputBuffer = new TransferBuffer<EncodedAudio>();
            _bytePool = new ConcurrentPool<byte[]>(12, () => new byte[frameSize * decoder.Format.Channels]); // todo wrong frame size (although it should still be large enough)

            var source = new BufferedDecoder(decoder, frameSize, decoder.Format, frame => _bytePool.Put(frame.Data.Array));
            var ramped = new VolumeRampedFrameSource(source, () => Priority < _priorityManager.TopPriority ? 0 : 1);
            var samples = new FrameToSampleConverter(ramped);

            ISampleSource toResampler = samples;
            if (softClip)
                toResampler = new SoftClipSampleSource(samples);

            var resampled = new Resampler(toResampler);

            _source = source;
            _output = resampled;
        }

        public void Prepare(SessionContext context)
        {
            _complete = false;
            _sourceClosed = false;
            _output.Prepare(context);
        }

        public bool Read(ArraySegment<float> samples)
        {
            FlushTransferBuffer();
            var complete = _output.Read(samples);

            if (complete)
                _completionHandler(this);

            return complete;
        }

        public void Push(VoicePacket packet)
        {
            Log.Trace("Received frame {0} from network", packet.SequenceNumber);

            // copy the data out of the frame, as the network thread will re-use the array
            var array = _bytePool.Get();
            var frame = packet.EncodedAudioFrame.CopyTo(array);

            // queue the frame onto the transfer buffer
            var copy = new EncodedAudio(packet.SequenceNumber, frame);
            if (!_inputBuffer.Write(copy))
                Log.Warn("Failed to write an encoded audio packet into the input transfer buffer");

            //Copy across the stream metadata
            //N.b. doing this means the metadata is surfaced <buffer length> too early
            Priority = packet.Priority;
            Positional = packet.Positional;
        }

        public void Stop()
        {
            _complete = true;
        }

        public void Reset()
        {
            _output.Reset();
        }

        private void FlushTransferBuffer()
        {
            // empty the transfer buffer into the decoder buffer
            EncodedAudio frame;
            while (_inputBuffer.Read(out frame))
            {
                _source.Push(frame);
            }

            // set the complete flag after flushing the transfer buffer
            if (_complete && !_sourceClosed)
            {
                _sourceClosed = true;
                _source.Stop();
            }
        }
    }
}
