﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fCraft
{
    public class Life2d
    {
        public const byte Newborn = 2;
        public const byte Normal = 1;
        public const byte Dead = 0xff;
        public const byte Nothing = 0;

        public bool Torus = false;
        private byte[,] _a;
        private int _hash = 0;

        public Life2d(int xSize, int ySize)
        {
            _a = new byte[xSize, ySize];
        }

        public int Hash
        {
            get { return _hash; }
        }

        public void Clear()
        {
            Array.Clear(_a, 0, _a.Length);
        }

        public void Set(int x, int y)
        {
            _a[x, y] = Normal;
        }

        public byte Get(int x, int y)
        {
            return _a[x, y];
        }

        public void HalfStep()
        {
            for (int i = 0; i < _a.GetLength(0); ++i)
                for (int j = 0; j < _a.GetLength(1); ++j)
                {
                    if (Empty(i, j) && Neighbors(i, j) == 3)
                        _a[i, j] = Newborn;
                }

            for (int i = 0; i < _a.GetLength(0); ++i)
                for (int j = 0; j < _a.GetLength(1); ++j)
                {
                    if (Alive(i, j))
                    {
                        int n = Neighbors(i, j);

                        if (n > 3 || n < 2)
                            _a[i, j] = Dead;
                    }
                }
        }

        private bool Empty(int x, int y)
        {
            return _a[x, y] == Nothing;
        }

        private bool Alive(int x, int y)
        {
            return _a[x, y] == Normal;
        }

        private int Neighbors(int x, int y)
        {
            int s = 0;

            for (int i = x - 1; i <= x + 1; ++i)
            {
                int ii = i;
                if (!ContinueWithCoord(ref ii, _a.GetLength(0)))
                    continue;
                for (int j = y - 1; j <= y + 1; ++j)
                    if (i != x || j != y)
                    {
                        int jj = j;
                        if (!ContinueWithCoord(ref jj, _a.GetLength(1)))
                            continue;
                        byte b = _a[ii, jj];
                        if (b == Normal || b == Dead)
                            ++s;
                    }
            }
            return s;
        }

        private bool ContinueWithCoord(ref int c, int max)
        {
            if (c < 0)
            {
                if (Torus)
                    c = max - 1;
                else
                    return false;
            }
            else if (c >= max)
            {
                if (Torus)
                    c = 0;
                else
                    return false;
            }
            return true;
        }

        public bool FinalizeStep()
        {
            bool changed = Replace(Dead, Nothing, false);
            changed |= Replace(Newborn, Normal, true);
            return changed;
        }

        private bool Replace(byte from, byte to, bool computeHash)
        {
            bool changed = false;
            if (computeHash)
                _hash = (int) 216713671;
            for (int i = 0; i < _a.GetLength(0); ++i)
                for (int j = 0; j < _a.GetLength(1); ++j)
                {
                    if (_a[i, j] == from)
                    {
                        _a[i, j] = to;
                        changed = true;
                    }
                    if (computeHash && _a[i, j] == Normal)
                    {
                        const int p = 16777619;
                        int h = i | (j << 16);
                        _hash ^= h*p;
                    }
                }
            return changed;
        }

        public byte[,] GetArrayCopy()
        {
            return (byte[,]) _a.Clone();
        }

        public void SetState(byte[,] a)
        {
            _a = (byte[,]) a.Clone();
        }

        public void SetStateToRandom()
        {
            Random r = new Random();
            for (int i = 0; i < _a.GetLength(0); ++i)
                for (int j = 0; j < _a.GetLength(1); ++j)
                    _a[i, j] = r.NextDouble() < 0.3 ? Normal : Nothing;
        }
    }
}