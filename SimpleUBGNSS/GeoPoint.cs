using System.Windows.Media.Media3D;

namespace SimpleUBGNSS
{
    public readonly struct ECEF
    {
        public readonly uint X;
        public static Point3D operator +(ECEF left, ECEF right) => 
            new(left.X + right.X, 0, 0);
    }
}