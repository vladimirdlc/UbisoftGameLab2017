using System;
using System.Runtime.InteropServices;

namespace Dissonance.Audio.Codecs.Opus
{
    internal static class BandwidthExtensions
    {
        private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(BandwidthExtensions).Name);

        public static int SampleRate(this OpusNative.Bandwidth bandwidth)
        {
            switch (bandwidth)
            {
                case OpusNative.Bandwidth.Narrowband:
                    return 8000;
                case OpusNative.Bandwidth.Mediumband:
                    return 12000;
                case OpusNative.Bandwidth.Wideband:
                    return 16000;
                case OpusNative.Bandwidth.SuperWideband:
                    return 24000;
                case OpusNative.Bandwidth.Fullband:
                    return 48000;
                default:
                    throw new ArgumentOutOfRangeException("bandwidth", Log.PossibleBugMessage(string.Format("{0} is not a valid value", bandwidth), "B534C9B2-6A9B-455E-875E-A01D93B278C8"));
            }
        }
    }

    internal class OpusNative
    {
        /// <summary>
        /// Wraps the Opus API.
        /// </summary>
        private static class OpusNativeMethods
        {
#if UNITY_IOS
            [DllImport("__Internal")]
#else
            [DllImport("DissonanceNative", CallingConvention = CallingConvention.Cdecl)]
#endif
            internal static extern IntPtr dissonance_opus_encoder_create(int samplingRate, int channels, int application, out int error);

#if UNITY_IOS
            [DllImport("__Internal")]
#else
            [DllImport("DissonanceNative", CallingConvention = CallingConvention.Cdecl)]
#endif
            internal static extern void dissonance_opus_encoder_destroy(IntPtr encoder);

#if UNITY_IOS
            [DllImport("__Internal")]
#else
            [DllImport("DissonanceNative", CallingConvention = CallingConvention.Cdecl)]
#endif
            internal static extern int dissonance_opus_encode_float(IntPtr encoder, float[] pcm, int pcmOffset, int frameSize, byte[] data, int encodedOffset, int maxEncodedLength);

#if UNITY_IOS
            [DllImport("__Internal")]
#else
            [DllImport("DissonanceNative", CallingConvention = CallingConvention.Cdecl)]
#endif
            internal static extern IntPtr dissonance_opus_decoder_create(int samplingRate, int channels, out int error);

#if UNITY_IOS
            [DllImport("__Internal")]
#else
            [DllImport("DissonanceNative", CallingConvention = CallingConvention.Cdecl)]
#endif
            internal static extern IntPtr dissonance_opus_decoder_destroy(IntPtr decoder);

#if UNITY_IOS
            [DllImport("__Internal")]
#else
            [DllImport("DissonanceNative", CallingConvention = CallingConvention.Cdecl)]
#endif
            internal static extern int dissonance_opus_decode_float(IntPtr decoder, byte[] data, int dataOffset, int dataLength, float[] pcm, int pcmOffset, int frameSize, bool decodeFEC);

#if UNITY_IOS
            [DllImport("__Internal")]
#else
            [DllImport("DissonanceNative", CallingConvention = CallingConvention.Cdecl)]
#endif
            internal static extern int dissonance_opus_encoder_ctl_in(IntPtr encoder, Ctl request, int value);

#if UNITY_IOS
            [DllImport("__Internal")]
#else
            [DllImport("DissonanceNative", CallingConvention = CallingConvention.Cdecl)]
#endif
            internal static extern int dissonance_opus_encoder_ctl_out(IntPtr encoder, Ctl request, out int value);

#if UNITY_IOS
            [DllImport("__Internal")]
#else
            [DllImport("DissonanceNative", CallingConvention = CallingConvention.Cdecl)]
#endif
            internal static extern int dissonance_opus_decoder_ctl_out(IntPtr decoder, Ctl request, out int value);

#if UNITY_IOS
            [DllImport("__Internal")]
#else
            [DllImport("DissonanceNative", CallingConvention = CallingConvention.Cdecl)]
#endif
            internal static extern void dissonance_opus_pcm_soft_clip(float[] pcm, int pcmOffset, int frameSize, int channels, float[] softClipMem);
        }

        private enum Ctl
        {
            SetBitrateRequest = 4002,
            GetBitrateRequest = 4003,

            SetInbandFECRequest = 4012,
            GetInbandFECRequest = 4013,

            SetPacketLossPercRequest = 4014,
            GetPacketLossPercRequest = 4015,

            ResetState = 4028,
        }

        public enum Bandwidth
        {
            /// <summary>
            /// 4KHz bandwidth (8KHz sample rate)
            /// </summary>
            Narrowband = 1101,

            /// <summary>
            /// 6Khz bandwidth (12KHz sample rate)
            /// </summary>
            Mediumband = 1102,

            /// <summary>
            /// 8Khz bandwidth (16KHz sample rate)
            /// </summary>
            Wideband = 1103,

            /// <summary>
            /// 12Khz (24KHz sample rate)
            /// </summary>
            SuperWideband = 1104,

            /// <summary>
            /// 20Khz (48KHz sample rate)
            /// </summary>
            Fullband = 1105
        }

        /// <summary>
        /// Supported coding modes.
        /// </summary>
        private enum Application
        {
            // ReSharper disable UnusedMember.Local (Justification passed in and out of opus)

            /// <summary>
            /// Best for most VoIP/videoconference applications where listening quality and intelligibility matter most.
            /// </summary>
            Voip = 2048,

            /// <summary>
            /// Best for broadcast/high-fidelity application where the decoded audio should be as close as possible to input.
            /// </summary>
            Audio = 2049,

            /// <summary>
            /// Only use when lowest-achievable latency is what matters most. Voice-optimized modes cannot be used.
            /// </summary>
            RestrictedLowLatency = 2051

            // ReSharper restore UnusedMember.Local
        }

        private enum OpusErrors
        {
            // ReSharper disable UnusedMember.Local (Justification: Cast from an int returned from opus)

            /// <summary>
            /// No error.
            /// </summary>
            Ok = 0,

            /// <summary>
            /// One or more invalid/out of range arguments.
            /// </summary>
            BadArg = -1,

            /// <summary>
            /// The mode struct passed is invalid.
            /// </summary>
            BufferToSmall = -2,

            /// <summary>
            /// An internal error was detected.
            /// </summary>
            InternalError = -3,

            /// <summary>
            /// The compressed data passed is corrupted.
            /// </summary>
            InvalidPacket = -4,

            /// <summary>
            /// Invalid/unsupported request number.
            /// </summary>
            Unimplemented = -5,

            /// <summary>
            /// An encoder or decoder structure is invalid or already freed.
            /// </summary>
            InvalidState = -6,

            /// <summary>
            /// Memory allocation has failed.
            /// </summary>
            AllocFail = -7

            // ReSharper restore UnusedMember.Local
        }

        public class OpusException : Exception
        {
            public OpusException(string message)
                : base(message)
            {
            }
        }

        /// <summary>
        /// Opus encoder.
        /// </summary>
        public sealed class OpusEncoder : IDisposable
        {
            private static readonly Log Log = Logs.Create(LogCategory.Playback, typeof(OpusEncoder).Name);

            #region fields and properties
            /// <summary>
            /// Opus encoder.
            /// </summary>
            private IntPtr _encoder;

            /// <summary>
            /// Permitted frame sizes in ms.
            /// </summary>
            private static readonly float[] PermittedFrameSizesMs = {
                2.5f, 5, 10, 20, 40, 60
            };

            /// <summary>
            /// Permitted frame sizes in samples per channel.
            /// </summary>
            public int[] PermittedFrameSizes { get; private set; }

            /// <summary>
            /// Gets or sets the bitrate setting of the encoding.
            /// </summary>
            public int Bitrate
            {
                get
                {
                    if (_encoder == IntPtr.Zero)
                        throw new ObjectDisposedException("OpusEncoder", Log.PossibleBugMessage("trying to use decoder after is has been disposed", "B2C0E7E6-DD3E-4686-8A6F-32F9E1C4FC65"));
                    int bitrate;
                    var ret = OpusNativeMethods.dissonance_opus_encoder_ctl_out(_encoder, Ctl.GetBitrateRequest, out bitrate);
                    if (ret < 0)
                        throw new OpusException(Log.PossibleBugMessage(string.Format("Encoder error: {0}", (OpusErrors)ret), "7E85BA26-BC20-4BBF-85CE-55E11A93D834"));
                    return bitrate;
                }
                set
                {
                    if (_encoder == IntPtr.Zero)
                        throw new ObjectDisposedException("OpusEncoder", Log.PossibleBugMessage("trying to use decoder after is has been disposed", "5383603C-8A3E-4766-8FAE-DFB4597AECE0"));
                    var ret = OpusNativeMethods.dissonance_opus_encoder_ctl_in(_encoder, Ctl.SetBitrateRequest, value);
                    if (ret < 0)
                        throw new OpusException(Log.PossibleBugMessage(string.Format("Encoder error: {0}", (OpusErrors)ret), "DC2B438D-0F68-4AB2-9344-0FE17EE3823A"));
                }
            }

            /// <summary>
            /// Gets or sets if Forward Error Correction encoding is enabled.
            /// </summary>
            public bool EnableForwardErrorCorrection
            {
                get
                {
                    if (_encoder == IntPtr.Zero)
                        throw new ObjectDisposedException("OpusEncoder", Log.PossibleBugMessage("trying to use decoder after is has been disposed", "38E3D110-7DA7-414F-BBE0-7BDCAFC42F6D"));

                    int fec;
                    var ret = OpusNativeMethods.dissonance_opus_encoder_ctl_out(_encoder, Ctl.GetInbandFECRequest, out fec);
                    if (ret < 0)
                        throw new Exception(Log.PossibleBugMessage(string.Format("Encoder error: {0}", (OpusErrors)ret), "C8D909EA-4FEE-42DD-A42F-0420EA7C5291"));
                    return fec > 0;
                }
                set
                {
                    if (_encoder == IntPtr.Zero)
                        throw new ObjectDisposedException("OpusEncoder", Log.PossibleBugMessage("trying to use decoder after is has been disposed", "5672EEDC-A1D4-4B18-8EC7-96D09CC441AE"));

                    var ret = OpusNativeMethods.dissonance_opus_encoder_ctl_in(_encoder, Ctl.SetInbandFECRequest, Convert.ToInt32(value));
                    if (ret < 0)
                        throw new Exception(Log.PossibleBugMessage(string.Format("Encoder error: {0}", (OpusErrors)ret), "9CAD5A82-B789-4789-8BB5-FB8BDEC777D2"));
                }
            }

            /// <summary>
            /// Get or set expected packet loss percentage (0 to 1)
            /// </summary>
            public float PacketLoss
            {
                get
                {
                    if (_encoder == IntPtr.Zero)
                        throw new ObjectDisposedException("OpusEncoder", Log.PossibleBugMessage("trying to use decoder after is has been disposed", "F85B3CDD-4088-4A10-A842-45F14583124C"));

                    int bitrate;
                    var ret = OpusNativeMethods.dissonance_opus_encoder_ctl_out(_encoder, Ctl.GetPacketLossPercRequest, out bitrate);
                    if (ret < 0)
                        throw new Exception(Log.PossibleBugMessage(string.Format("Encoder error: {0}", (OpusErrors)ret), "5C3C5D5E-4F9D-4EA8-9DA0-6C432878C07C"));

                    return bitrate / 100f;
                }
                set
                {
                    if (_encoder == IntPtr.Zero)
                        throw new ObjectDisposedException("OpusEncoder", Log.PossibleBugMessage("trying to use decoder after is has been disposed", "10A3BFFB-EC3B-4664-B06C-D5D42F75FE42"));

                    if (value < 0 || value > 1)
                        throw new ArgumentOutOfRangeException("value", Log.PossibleBugMessage("Packet loss percentage must be 0 <= value <= 1", "CFDF590D-C61A-4BB4-BB2D-1FAC1E59C114"));

                    var ret = OpusNativeMethods.dissonance_opus_encoder_ctl_in(_encoder, Ctl.SetPacketLossPercRequest, (int)(value * 100));
                    if (ret < 0)
                        throw new Exception(Log.PossibleBugMessage(string.Format("Encoder error: {0}", (OpusErrors)ret), "4AAA9AA6-8429-4346-B939-D113206FFBA8"));
                }
            }
            #endregion

            #region constructors
            /// <summary>
            /// Creates a new Opus encoder.
            /// </summary>
            /// <param name="srcSamplingRate">The sampling rate of the input stream.</param>
            /// <param name="srcChannelCount">The number of channels in the input stream.</param>
            public OpusEncoder(int srcSamplingRate, int srcChannelCount)
            {
                if (srcSamplingRate != 8000 && srcSamplingRate != 12000 && srcSamplingRate != 16000 && srcSamplingRate != 24000 && srcSamplingRate != 48000)
                    throw new ArgumentOutOfRangeException("srcSamplingRate", Log.PossibleBugMessage("sample rate must be one of the valid values", "3F2C6D2D-338E-495E-8970-42A3C98243A5"));
                if (srcChannelCount != 1 && srcChannelCount != 2)
                    throw new ArgumentOutOfRangeException("srcChannelCount", Log.PossibleBugMessage("channel count must be 1 or 2", "8FE1EC0F-09E0-4CE6-AFD7-04199202D45D"));

                int error;
                var encoder = OpusNativeMethods.dissonance_opus_encoder_create(srcSamplingRate, srcChannelCount, (int)Application.Voip, out error);
                if ((OpusErrors)error != OpusErrors.Ok)
                    throw new OpusException(Log.PossibleBugMessage(string.Format("Exception occured while creating encoder: {0}", (OpusErrors)error), "D77ECA73-413F-40D1-8427-CFD8A59CD5F6"));
                _encoder = encoder;

                PermittedFrameSizes = new int[PermittedFrameSizesMs.Length];
                for (var i = 0; i < PermittedFrameSizesMs.Length; i++)
                    PermittedFrameSizes[i] = (int)(srcSamplingRate / 1000f * PermittedFrameSizesMs[i]);
            }
            #endregion

            /// <summary>
            /// Encode audio samples.
            /// </summary>
            /// <returns>The total number of bytes written to dstOutputBuffer.</returns>
            public int EncodeFloats(ArraySegment<float> sourcePcm, ArraySegment<byte> dstEncoded)
            {
                if (sourcePcm.Array == null)
                    throw new ArgumentNullException("sourcePcm", Log.PossibleBugMessage("source pcm must not be null", "58AE3110-8F9A-4C36-9520-B7F3383096EC"));
                if (dstEncoded.Array == null)
                    throw new ArgumentNullException("dstEncoded", Log.PossibleBugMessage("destination must not be null", "36C327BB-A128-400D-AFB3-FF760A1562C1"));
                if (Array.IndexOf(PermittedFrameSizes, sourcePcm.Count) == -1)
                    throw new ArgumentException(Log.PossibleBugMessage(string.Format("Incorrect frame size '{0}'", sourcePcm.Count), "6AFD9ADF-1D15-4197-99E9-5A19ECB8CD20"), "sourcePcm");

                var encodedLen = OpusNativeMethods.dissonance_opus_encode_float(_encoder, sourcePcm.Array, sourcePcm.Offset, sourcePcm.Count, dstEncoded.Array, dstEncoded.Offset, dstEncoded.Count);

                if (encodedLen < 0)
                    throw new OpusException(Log.PossibleBugMessage(string.Format("Encoding failed: {0}", (OpusErrors)encodedLen), "9C923F57-146B-47CB-8EEE-5BF129FA3124"));
                return encodedLen;
            }

            public void Reset()
            {
                //Wtf is going on here?
                //Reset takes no args, returns no values, and cannot fail. So we're passing in nothing, outputting nothing, and ignoring the return code
                int _;
                OpusNativeMethods.dissonance_opus_encoder_ctl_out(_encoder, Ctl.ResetState, out _);
            }

            #region disposal
            ~OpusEncoder()
            {
                Dispose();
            }

            private bool _disposed;

            public void Dispose()
            {
                if (_disposed)
                    return;

                GC.SuppressFinalize(this);

                if (_encoder != IntPtr.Zero)
                {
                    OpusNativeMethods.dissonance_opus_encoder_destroy(_encoder);
                    _encoder = IntPtr.Zero;
                }

                _disposed = true;
            }
            #endregion
        }

        /// <summary>
        /// Opus decoder.
        /// </summary>
        public sealed class OpusDecoder : IDisposable
        {
            private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(OpusDecoder).Name);

            #region field and properties
            /// <summary>
            /// Opus decoder.
            /// </summary>
            private IntPtr _decoder;

            /// <summary>
            /// Gets or sets if Forward Error Correction decoding is enabled.
            /// </summary>
            public bool EnableForwardErrorCorrection { get; set; }

            private readonly float[] _clipMem;
            #endregion

            #region constructors
            public OpusDecoder(int outputSampleRate, int outputChannelCount)
            {
                if (outputSampleRate != 8000 && outputSampleRate != 12000 && outputSampleRate != 16000 && outputSampleRate != 24000 && outputSampleRate != 48000)
                    throw new ArgumentOutOfRangeException("outputSampleRate", Log.PossibleBugMessage("sample rate must be one of the valid values", "548757DF-DC64-40C9-BEAD-9826B8245A7D"));
                if (outputChannelCount != 1 && outputChannelCount != 2)
                    throw new ArgumentOutOfRangeException("outputChannelCount", Log.PossibleBugMessage("channel count must be 1 or 2", "BA56610F-1FA3-4D68-9507-7B0DFA0E28AB"));

                int error;
                _decoder = OpusNativeMethods.dissonance_opus_decoder_create(outputSampleRate, outputChannelCount, out error);
                if ((OpusErrors)error != OpusErrors.Ok)
                    throw new OpusException(Log.PossibleBugMessage(string.Format("Exception occured while creating decoder: {0}", (OpusErrors)error), "6E09F275-99A1-4CD6-A36A-FA093B146B29"));
            }
            #endregion

            #region disposal
            ~OpusDecoder()
            {
                Dispose();
            }

            private bool _disposed;

            public void Dispose()
            {
                if (_disposed)
                    return;

                GC.SuppressFinalize(this);

                if (_decoder != IntPtr.Zero)
                {
                    OpusNativeMethods.dissonance_opus_decoder_destroy(_decoder);
                    _decoder = IntPtr.Zero;
                }

                _disposed = true;
            }
            #endregion

            /// <summary>
            /// Decodes audio samples.
            /// </summary>
            /// <param name="srcEncodedBuffer">Encoded data (or null, to reconstruct a missing frame)</param>
            /// <param name="dstBuffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values starting at offset replaced with audio samples.</param>
            /// <returns>The number of floats decoded and written to dstBuffer.</returns>
            /// <remarks>Set srcEncodedBuffer to null to instruct the decoder that a packet was dropped.</remarks>
            public int DecodeFloats(ArraySegment<byte>? srcEncodedBuffer, ArraySegment<float> dstBuffer)
            {
                int length;
                if (srcEncodedBuffer.HasValue)
                {
                    length = OpusNativeMethods.dissonance_opus_decode_float(
                        _decoder,
                        srcEncodedBuffer.Value.Array, srcEncodedBuffer.Value.Offset, srcEncodedBuffer.Value.Count,
                        dstBuffer.Array, dstBuffer.Offset, dstBuffer.Count,
                        false
                    );
                }
                else
                {
                    //Call decoder with null pointer and zero length to inform it of packet loss
                    length = OpusNativeMethods.dissonance_opus_decode_float(
                        _decoder,
                        null, 0, 0,
                        dstBuffer.Array, dstBuffer.Offset, dstBuffer.Count,
                        EnableForwardErrorCorrection
                    );
                }

                if (length < 0)
                {
                    if (length == (int)OpusErrors.InvalidPacket)
                    {
                        if (!srcEncodedBuffer.HasValue)
                            throw new OpusException(Log.PossibleBugMessage("Decoding failed: InvalidPacket. 'null' ", "03BE7561-3BCC-4F41-A7CB-C80F03981267"));
                        else
                            throw new OpusException(Log.PossibleBugMessage(string.Format("Decoding failed: InvalidPacket. '{0}'", Convert.ToBase64String(srcEncodedBuffer.Value.Array, srcEncodedBuffer.Value.Offset, srcEncodedBuffer.Value.Count)), "EF4BC24C-491E-45D9-974C-FE5CB61BD54E"));
                    }
                    else
                        throw new OpusException(Log.PossibleBugMessage(string.Format("Decoding failed: {0} ", (OpusErrors)length), "A9C8EF2C-7830-4D8E-9D6E-EF0B9827E0A8"));
                }
                return length;
            }

            public void Reset()
            {
                //Reset takes no args, returns no values, and cannot fail. So we're passing in nothing, outputting nothing, and ignoring the return code
                int _;
                OpusNativeMethods.dissonance_opus_decoder_ctl_out(_decoder, Ctl.ResetState, out _);
            }
        }

        public sealed class OpusSoftClip
        {
            private readonly float[] _memory;

            public OpusSoftClip(int channels = 1)
            {
                if (channels < 0)
                    throw new ArgumentOutOfRangeException("channels", "Channels must be > 0");

                _memory = new float[channels];
            }

            public void Clip(ArraySegment<float> samples)
            {
#if !NCRUNCH
                OpusNativeMethods.dissonance_opus_pcm_soft_clip(
                    samples.Array,
                    samples.Offset,
                    samples.Count / _memory.Length,
                    _memory.Length,
                    _memory
                );
#endif
            }

            public void Reset()
            {
                Array.Clear(_memory, 0, _memory.Length);
            }
        }
    }
}
