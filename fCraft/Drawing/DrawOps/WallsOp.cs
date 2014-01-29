﻿using System;
using System.Collections.Generic;

namespace fCraft.Drawing
{
    public sealed class WallsDrawOperation : DrawOperation
    {
        private IEnumerator<Vector3I> coordEnumerator;
        private bool fillInner;

        public WallsDrawOperation(Player player)
            : base(player)
        {
        }

        public override string Name
        {
            get { return "Walls"; }
        }

        public override string Description
        {
            get { return Name; }
        }


        public override bool Prepare(Vector3I[] marks)
        {
            if (!base.Prepare(marks)) return false;


            fillInner = Brush.HasAlternateBlock && Bounds.Width > 2 && Bounds.Length > 2 && Bounds.Height > 2;

            BlocksTotalEstimate = Bounds.Volume;
            if (!fillInner)
            {
                BlocksTotalEstimate -= Math.Max(0, Bounds.Width - 2)*Math.Max(0, Bounds.Length - 2)*
                                       Math.Max(0, Bounds.Height - 2);
            }

            coordEnumerator = BlockEnumerator().GetEnumerator();
            return true;
        }


        public override int DrawBatch(int maxBlocksToDraw)
        {
            int blocksDone = 0;
            while (coordEnumerator.MoveNext())
            {
                Coords = coordEnumerator.Current;
                if (DrawOneBlock())
                {
                    blocksDone++;
                    if (blocksDone >= maxBlocksToDraw) return blocksDone;
                }
            }
            IsDone = true;
            return blocksDone;
        }

        //all works. Maybe look at Block estimation.
        private IEnumerable<Vector3I> BlockEnumerator()
        {
            for (int x = Bounds.XMin; x <= Bounds.XMax; x++)
            {
                for (int z = Bounds.ZMin - 1; z < Bounds.ZMax; z++)
                {
                    yield return new Vector3I(x, Bounds.YMin, z + 1);
                    if (Bounds.YMin != Bounds.YMax)
                    {
                        yield return new Vector3I(x, Bounds.YMax, z + 1);
                    }
                }
                for (int y = Bounds.YMin; y < Bounds.YMax; y++)
                {
                    for (int z = Bounds.ZMin - 1; z < Bounds.ZMax; z++)
                    {
                        yield return new Vector3I(Bounds.XMin, y, z + 1);
                        if (Bounds.XMin != Bounds.XMax)
                        {
                            yield return new Vector3I(Bounds.XMax, y, z + 1);
                        }
                    }
                }
            }

            if (fillInner)
            {
                UseAlternateBlock = true;
                for (int x = Bounds.XMin + 1; x < Bounds.XMax; x++)
                {
                    for (int y = Bounds.YMin + 1; y < Bounds.YMax; y++)
                    {
                        for (int z = Bounds.ZMin; z < Bounds.ZMax + 1; z++)
                        {
                            yield return new Vector3I(x, y, z);
                        }
                    }
                }
            }
        }
    }
}