using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SimpleUBGNSS
{
    [StructLayout(LayoutKind.Explicit, Size = Size), SkipLocalsInit()]
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    [Obsolete("Use high-precision version intead!")]
    public readonly struct UbxNavPosLLH
    {
        public const UbxType MessageID = UbxType.UbxNavPosLLH;
        public const int Size = 28;

        private const double MillimeterToMeter = 1.0d / 1000.0d;
        private const double UbloxUnitToDegree = 1e-7d;

        [FieldOffset(0)] private readonly uint iTOW;
        [FieldOffset(4)] private readonly int lon;
        [FieldOffset(8)] private readonly int lat;
        [FieldOffset(12)] private readonly int height;
        [FieldOffset(16)] private readonly int hMSL;
        [FieldOffset(20)] private readonly uint hAcc;
        [FieldOffset(24)] private readonly uint vAcc;


        public uint ITow => iTOW;


        public double Lon => UbloxUnitToDegree * lon;


        public double Lat => UbloxUnitToDegree * lat;


        public double Height => MillimeterToMeter * height;


        public double HMSL => MillimeterToMeter * hMSL;

        public double HAcc => MillimeterToMeter * hAcc;

        public double VAcc => MillimeterToMeter * vAcc;


        public override string ToString() =>
            string.Format("Lat: {0}°; Lon: {1}°; Height: {2} m", Lat, Lon, Height);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetMultilineRepresentation() =>
            string.Format("Lat:    {0}°  ± {1} m\n" +
                          "Lon:    {2}°  ± {1} m\n" +
                          "Height: {3} m ± {4} m\n" +
                          "Ortho:  {5} m ± {4} m\n",
                           Lat, HAcc, Lon, Height, VAcc, HMSL);
    }
}
