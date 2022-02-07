using System.Runtime.InteropServices;

namespace SimpleUBGNSS
{
    [StructLayout(LayoutKind.Explicit, Size = Size)]
    public readonly ref struct MessageHeader
    {
        public const int Size = 4;

        [FieldOffset(0)] public readonly UbxType MessageType;
        [FieldOffset(2)] public readonly ushort Length;
        public int LengthToReadWithFlatcher => 2 + Length;
        public int LengthToTest => 4 + Length;
    }
}