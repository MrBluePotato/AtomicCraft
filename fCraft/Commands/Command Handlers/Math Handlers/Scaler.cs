using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraft.Drawing;

namespace fCraft
{
    //scales coords according to the defined possibilities
    public class Scaler
    {
        //scalings:
        //ZeroToMaxBound means that every dimension of the selected area is measured from 0 to its size in cubes minus 1
        //Normalized means that every dimension is measured from 0 to 1

        private Scaling _scaling;

        public Scaler(string scaling)
        {
            if (string.IsNullOrWhiteSpace(scaling))
                _scaling = Scaling.ZeroToMaxBound;
            else if (scaling.ToLower() == "u")
                _scaling = Scaling.Normalized;
            else if (scaling.ToLower() == "uu")
                _scaling = Scaling.DoubleNormalized;
            else
                throw new ArgumentException("unrecognized scaling " + scaling);
        }

        public double ToFuncParam(double coord, double min, double max)
        {
            switch (_scaling)
            {
                case Scaling.ZeroToMaxBound:
                    return coord - min;
                case Scaling.Normalized:
                    return (coord - min)/Math.Max(1, max - min);
                case Scaling.DoubleNormalized:
                    return max == min ? 0 : 2.0*(coord - min)/Math.Max(1, max - min) - 1;
                default:
                    throw new Exception("unknown scaling");
            }
        }

        public int FromFuncResult(double result, double min, double max)
        {
            switch (_scaling)
            {
                case Scaling.ZeroToMaxBound:
                    return (int) (result + min);
                case Scaling.Normalized:
                    return (int) (result*Math.Max(1, max - min) + min);
                case Scaling.DoubleNormalized:
                    return (int) ((result + 1)*Math.Max(1, max - min)/2.0 + min);
                default:
                    throw new Exception("unknown scaling");
            }
        }

        private enum Scaling
        {
            ZeroToMaxBound,
            Normalized,
            DoubleNormalized,
        }
    }
}