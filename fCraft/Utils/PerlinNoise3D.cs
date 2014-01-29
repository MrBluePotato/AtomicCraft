using System;
using JetBrains.Annotations;

namespace fCraft
{
    /// <summary> Implementation of 3D Perlin Noise after Ken Perlin's reference implementation. </summary>
    public sealed class PerlinNoise3D
    {
        #region Fields

        private readonly int[] p;
        private readonly int[] permutation;

        #endregion

        #region Properties

        public float Frequency { get; set; }
        public float Amplitude { get; set; }
        public float Persistence { get; set; }
        public int Octaves { get; set; }

        #endregion

        #region Contructors

        public PerlinNoise3D([NotNull] Random rand)
        {
            if (rand == null) throw new ArgumentNullException("rand");
            permutation = new int[256];
            p = new int[permutation.Length*2];
            InitNoiseFunctions(rand);

            // Default values
            Frequency = 0.023f;
            Amplitude = 2.2f;
            Persistence = 0.9f;
            Octaves = 2;
        }

        #endregion

        #region Methods

        public void InitNoiseFunctions([NotNull] Random rand)
        {
            if (rand == null) throw new ArgumentNullException("rand");

            // Fill empty
            for (int i = 0; i < permutation.Length; i++)
            {
                permutation[i] = -1;
            }

            // Generate random numbers
            for (int i = 0; i < permutation.Length; i++)
            {
                while (true)
                {
                    int iP = rand.Next()%permutation.Length;
                    if (permutation[iP] == -1)
                    {
                        permutation[iP] = i;
                        break;
                    }
                }
            }

            // Copy
            for (int i = 0; i < permutation.Length; i++)
            {
                p[permutation.Length + i] = p[i] = permutation[i];
            }
        }


        public float Compute(float x, float y, float z)
        {
            float noise = 0;
            float amp = Amplitude;
            float freq = Frequency;
            for (int i = 0; i < Octaves; i++)
            {
                noise += Noise(x*freq, y*freq, z*freq)*amp;
                freq *= 2; // octave is the double of the previous frequency
                amp *= Persistence;
            }
            return noise;
        }


        private float Noise(float x, float y, float z)
        {
            // Find unit cube that contains point
            int iX = (int) Math.Floor(x) & 255;
            int iY = (int) Math.Floor(y) & 255;
            int iZ = (int) Math.Floor(z) & 255;

            // Find relative x, y, z of the point in the cube.
            x -= (float) Math.Floor(x);
            y -= (float) Math.Floor(y);
            z -= (float) Math.Floor(z);

            // Compute fade curves for each of x, y, z
            float u = Fade(x);
            float v = Fade(y);
            float w = Fade(z);

            // Hash coordinates of the 8 cube corners
            int a = p[iX] + iY;
            int aa = p[a] + iZ;
            int ab = p[a + 1] + iZ;
            int b = p[iX + 1] + iY;
            int ba = p[b] + iZ;
            int bb = p[b + 1] + iZ;

            // And add blended results from 8 corners of cube.
            return Lerp(w, Lerp(v, Lerp(u, Grad(p[aa], x, y, z),
                Grad(p[ba], x - 1, y, z)),
                Lerp(u, Grad(p[ab], x, y - 1, z),
                    Grad(p[bb], x - 1, y - 1, z))),
                Lerp(v, Lerp(u, Grad(p[aa + 1], x, y, z - 1),
                    Grad(p[ba + 1], x - 1, y, z - 1)),
                    Lerp(u, Grad(p[ab + 1], x, y - 1, z - 1),
                        Grad(p[bb + 1], x - 1, y - 1, z - 1))));
        }


        private static float Fade(float t)
        {
            // Smooth interpolation parameter
            return (t*t*t*(t*(t*6 - 15) + 10));
        }


        private static float Lerp(float alpha, float a, float b)
        {
            // Linear interpolation
            return (a + alpha*(b - a));
        }


        private static float Grad(int hashCode, float x, float y, float z)
        {
            // Convert lower 4 bits of hash code into 12 gradient directions
            int h = hashCode & 15;
            float u = h < 8 ? x : y;
            float v = h < 4 ? y : h == 12 || h == 14 ? x : z;
            return (((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v));
        }

        #endregion
    }
}