using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraft.Drawing;

namespace RandomMaze
{
    internal class MazeCuboidDrawOperation : DrawOperation
    {
        private int _count = 0;
        private Maze _maze;

        public MazeCuboidDrawOperation(Player player)
            : base(player)
        {
        }

        public override string Name
        {
            get { return "MazeCuboid"; }
        }


        public override bool Prepare(Vector3I[] marks)
        {
            if (!base.Prepare(marks))
                return false;
            if (Bounds.Width < 3 || Bounds.Length < 3)
            {
                Player.Message("Too small area marked (at least 3x3 blocks by X and Y)");
                return false;
            }
            if (Bounds.Width%2 != 1 || Bounds.Length%2 != 1)
            {
                Player.Message(
                    "Warning: bounding box X and Y dimensions must be uneven, current bounding box will be cropped!");
            }
            BlocksTotalEstimate = Bounds.Volume;

            _maze = new Maze((Bounds.Width - 1)/2, (Bounds.Length - 1)/2, 1);

            return true;
        }


        public override int DrawBatch(int maxBlocksToDraw)
        {
            for (int j = 0; j < _maze.YSize; ++j)
            {
                for (int i = 0; i < _maze.XSize; ++i)
                {
                    DrawAtXY(i*2, j*2);
                    if (_maze.GetCell(i, j, 0).Wall(Direction.All[3]))
                        DrawAtXY(i*2 + 1, j*2);
                    if (_maze.GetCell(i, j, 0).Wall(Direction.All[2]))
                        DrawAtXY(i*2, j*2 + 1);
                }
                DrawAtXY(_maze.XSize*2, j*2);
                if (_maze.GetCell(_maze.XSize - 1, j, 0).Wall(Direction.All[0]))
                    DrawAtXY(_maze.XSize*2, j*2 + 1);
            }
            for (int i = 0; i < _maze.XSize; ++i)
            {
                DrawAtXY(i*2, _maze.YSize*2);
                if (_maze.GetCell(i, _maze.YSize - 1, 0).Wall(Direction.All[1]))
                    DrawAtXY(i*2 + 1, _maze.YSize*2);
            }
            DrawAtXY(_maze.XSize*2, _maze.YSize*2);

            IsDone = true;
            return _count;
        }

        private void DrawAtXY(int x, int y)
        {
            Coords.X = x + Bounds.XMin;
            Coords.Y = y + Bounds.YMin;
            for (Coords.Z = Bounds.ZMin; Coords.Z <= Bounds.ZMax; ++Coords.Z)
                if (DrawOneBlock())
                    ++_count;
        }
    }
}