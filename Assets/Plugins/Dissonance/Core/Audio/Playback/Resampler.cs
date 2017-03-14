using System;
using NAudio.Dsp;
using NAudio.Wave;
using UnityEngine;

namespace Dissonance.Audio.Playback
{
    internal class Resampler : ISampleSource
    {
        private static readonly Log Log = Logs.Create(LogCategory.Playback, typeof (Resampler).Name);

        private readonly ISampleSource _source;

        private volatile WaveFormat _outputFormat;
        private WdlResampler _resampler;

        public Resampler(ISampleSource source)
        {
            _source = source;

            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            OnAudioConfigurationChanged(false);
        }

        public WaveFormat WaveFormat
        {
            get { return _outputFormat; }
        }

        public void Prepare(SessionContext context)
        {
            _source.Prepare(context);
        }

        public bool Read(ArraySegment<float> samples)
        {
            var inFormat = _source.WaveFormat;
            var outFormat = _outputFormat;

            if (outFormat.SampleRate == inFormat.SampleRate)
                return _source.Read(samples);

            if (_resampler == null || outFormat.SampleRate != (int) _resampler.OutputSampleRate)
            {
                Log.Debug("Initializing resampler to resample {0}Hz source to {1}Hz output", inFormat.SampleRate, outFormat.SampleRate);

                _resampler = new WdlResampler();
                _resampler.SetMode(true, 2, false);
                _resampler.SetFilterParms();
                _resampler.SetFeedMode(false); // output driven
                _resampler.SetRates(inFormat.SampleRate, outFormat.SampleRate);
            }

            var channels = inFormat.Channels;

            // prepare buffers
            float[] inBuffer;
            int inBufferOffset;
            var samplesPerChannelRequested = samples.Count / channels;
            var samplesPerChannelRequired = _resampler.ResamplePrepare(samplesPerChannelRequested, channels, out inBuffer, out inBufferOffset);
            var sourceBuffer = new ArraySegment<float>(inBuffer, inBufferOffset, samplesPerChannelRequired * channels);

            // read source
            var complete = _source.Read(sourceBuffer);

            // resample
            Log.Trace("Resampling {0}Hz -> {1}Hz", inFormat.SampleRate, outFormat.SampleRate);
            _resampler.ResampleOut(samples.Array, samples.Offset, samplesPerChannelRequired, samplesPerChannelRequested, channels);

            return complete;
        }

        public void Reset()
        {
            _source.Reset();
        }

        private void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
#if NCRUNCH
            _outputFormat = new WaveFormat(_source.WaveFormat.Channels, 44100);
#else
            _outputFormat = new WaveFormat(_source.WaveFormat.Channels, AudioSettings.outputSampleRate);
#endif
        }
    }
}