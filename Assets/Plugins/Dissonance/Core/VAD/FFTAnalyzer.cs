using System;
using Dissonance.Datastructures;
using NAudio.Wave;
using UnityEngine;

namespace Dissonance.VAD
{
    internal class FFTAnalyzer
    {
        #region fields and properties

        private static readonly Log Log = Logs.Create(LogCategory.Recording, "FFT");

        private int _sampleRate = -1;
        private int _frameSize = -1;

        private RingBuffer<float> _samples;
        private float[] _fftBuffer;
        private readonly LomontFFT _fft = new LomontFFT();

        private int _binWidthHz;
        private const int MinHz = 1250;
        private const int MaxHz = 5450;

        private float _spectralFlatness;
        public float SpectralFlatness { get { return _spectralFlatness; } }
        #endregion

        public void Analyze(ArraySegment<float> data, WaveFormat format)
        {
            ComputeFFT(data, format);
            CalculateSpectralStatistics(out _spectralFlatness);
        }

        public void Clear()
        {
            if (_samples != null)
                _samples.Clear();
            _spectralFlatness = 0;
        }

        private void SetupFFT(WaveFormat format, int frameSize)
        {
            if (_sampleRate != format.SampleRate || _frameSize != frameSize)
            {
                _sampleRate = format.SampleRate;
                _frameSize = frameSize;

                //There are a number of constraints on the number of samples:
                // - Number must be pow2
                // - We want to keep at least one frame in the buffer
                //We could also add an optional constraint:
                // - Bins must be <Target Bandwidth>Hz wide
                //Calculate both of these constraints, and take the tightest

                //Each frequency bin represents a bandwidth of (Sample Rate)/(total number of samples)
                //which means we need SampleRate/Target_Bandwidth samples
                var freqSamplesLog2 = Mathf.Log(format.SampleRate / 100f, 2);

                //Make sure we keep at least 2 frames in the buffer
                var frameSamplesLog2 = Mathf.Log(frameSize * 2, 2);

                //Samples (take max constraint, round up to next integral pow2, calculate actual value with shift)
                var samples = 1 << (int)Math.Ceiling(Math.Max(frameSamplesLog2, freqSamplesLog2));

                //Create a ring buffer for this number of samples
                _samples = new RingBuffer<float>(samples);

                //Create an array for *twice* this number (so we can interleave imaginary values with real values)
                _fftBuffer = new float[_samples.Capacity * 2];

                _binWidthHz = format.SampleRate / _samples.Capacity;
                Log.Debug("VAD FFT: Bins:{0} Bandwidth:{1}Hz Bound:{2}", _samples.Capacity, _binWidthHz, _binWidthHz * _samples.Capacity);
            }
        }

        private void ComputeFFT(ArraySegment<float> buffer, WaveFormat format)
        {
            //Perform one time setup of FFT (once per format change)
            SetupFFT(format, buffer.Count);

            //Copy latest samples into the FFT buffer (rolling window of most recent samples in some Pow2 size)
            _samples.Add(buffer);

            //Copy all of the samples out of the buffer (oldest at index 0)
            _samples.CopyTo(new ArraySegment<float>(_fftBuffer, 0, _samples.Capacity));

            //Interlace FFT data with real and imaginary parts [Real,Imaginary,Real,Imaginary etc]
            //There is data in this buffer already (the previous FFT result) so we make sure to overwrite every single index here
            for (var i = _samples.Capacity - 1; i >= 0; i--)
            {
                _fftBuffer[i * 2] = _fftBuffer[i]; //Real
                _fftBuffer[i * 2 + 1] = 0; //Imaginary
            }

            //perform FFT on this data
            _fft.RealFFT(_fftBuffer, true);

            //Now extract real values from the real/imaginary pairs in the result
            //Squared results will be stored in the first (_fftBuffer.Length / 4) indices
            var bins = _fftBuffer.Length;
            for (var i = 0; i < bins; i += 2)
            {
                var a = _fftBuffer[i];
                var b = _fftBuffer[i + 1];
                var magSqr = a * a + b * b;

                _fftBuffer[i / 2] = magSqr;
            }
        }

        private void CalculateSpectralStatistics(out float flatness)
        {
            //Spectral flatness (https://en.wikipedia.org/wiki/Spectral_flatness) is the geometric mean divided by the arithmetic mean

            //Spectral flatness accumulators
            var arithmetic = 0f;
            var geometric = 0f;

            //Loop over a subset of the FFT (defined by MinHz and MaxHz)
            var count = 0;
            var max = Math.Min(MaxHz / _binWidthHz, _fftBuffer.Length / 2);
            for (var i = MinHz / _binWidthHz; i < max; i++)
            {
                var value = _fftBuffer[i];

                //Update accumulators for spectral flatness calculation
                count++;
                arithmetic += value;
                geometric += Mathf.Log(value);
            }

            geometric = Mathf.Exp(geometric / count);
            arithmetic = arithmetic / count;

            flatness = geometric / arithmetic;
        }
    }
}
