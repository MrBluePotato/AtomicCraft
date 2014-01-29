using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraft.Drawing;

namespace fCraft
{
    //draws volume, defined by an inequality 
    public class InequalityDrawOperation : DrawOperation
    {
        private int _count;
        private Expression _expression;
        private Scaler _scaler;

        public InequalityDrawOperation(Player player, Command cmd)
            : base(player)
        {
            string strFunc = cmd.Next();
            if (string.IsNullOrWhiteSpace(strFunc))
                throw new ArgumentException("empty inequality expression");
            if (strFunc.Length < 3)
                throw new ArgumentException("expression is too short (should be like f(x,y,z)>g(x,y,z))");

            strFunc = strFunc.ToLower();

            _expression = SimpleParser.Parse(strFunc, new string[] {"x", "y", "z"});
            if (!_expression.IsInEquality())
                throw new ArgumentException(
                    "the expression given is not an inequality (should be like f(x,y,z)>g(x,y,z))");

            player.Message("Expression parsed as " + _expression.Print());
            string scalingStr = cmd.Next();
            _scaler = new Scaler(scalingStr);
        }

        public override string Name
        {
            get { return "Inequality"; }
        }

        public override int DrawBatch(int maxBlocksToDraw)
        {
            //ignoring maxBlocksToDraw
            _count = 0;
            int exCount = 0;

            for (Coords.X = Bounds.XMin;
                Coords.X <= Bounds.XMax && MathCommands.MaxCalculationExceptions >= exCount;
                ++Coords.X)
            {
                for (Coords.Y = Bounds.YMin;
                    Coords.Y <= Bounds.YMax && MathCommands.MaxCalculationExceptions >= exCount;
                    ++Coords.Y)
                {
                    for (Coords.Z = Bounds.ZMin; Coords.Z <= Bounds.ZMax; ++Coords.Z)
                    {
                        try
                        {
                            if (_expression.Evaluate(_scaler.ToFuncParam(Coords.X, Bounds.XMin, Bounds.XMax),
                                _scaler.ToFuncParam(Coords.Y, Bounds.YMin, Bounds.YMax),
                                _scaler.ToFuncParam(Coords.Z, Bounds.ZMin, Bounds.ZMax)) > 0) //1.0 means true
                            {
                                if (DrawOneBlock())
                                    ++_count;
                            }
                            //if (TimeToEndBatch)
                            //    return _count;
                        }
                        catch (Exception)
                        {
                            //the exception here is kinda of normal, for functions (especially interesting ones)
                            //may have eg punctured points; we just have to keep an eye on the number, since producing 10000
                            //exceptions in the multiclient application is not the best idea
                            if (++exCount > MathCommands.MaxCalculationExceptions)
                            {
                                Player.Message("Drawing is interrupted: too many (>" +
                                               MathCommands.MaxCalculationExceptions +
                                               ") calculation exceptions.");
                                break;
                            }
                        }
                    }
                }
            }

            IsDone = true;
            return _count;
        }

        public override bool Prepare(Vector3I[] marks)
        {
            if (!base.Prepare(marks))
            {
                return false;
            }
            BlocksTotalEstimate = Bounds.Volume;
            return true;
        }
    }
}