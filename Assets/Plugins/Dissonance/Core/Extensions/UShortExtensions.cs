namespace Dissonance.Extensions
{
    internal static class UShortExtensions
    {
        internal static int WrappedDelta(this ushort a, ushort b)
        {
            var delta = b - a;
            if (delta < -ushort.MaxValue / 2)
                return b + (ushort.MaxValue - a);
            if (delta > ushort.MaxValue / 2)
                return a + (ushort.MaxValue - b);

            return delta;
        }
    }
}
