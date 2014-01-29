﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

//Copyright (C) <2011 - 2014> <Jon Baker, Glenn Mariën and Lao Tszy>

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace fCraft
{
    public class FontHandler
    {
        #region Instance and Drawing

        public int blockCount; //blockcount for player message. ++ when drawing

        private int blocks = 0,
            //drawn blocks
            blocksDenied = 0; //denied blocks (zones, ect)

        private Direction direction; //direction of the blocks (x++-- ect)
        public Player player; //player using command
        private fCraft.Drawing.UndoState undoState; //undostate

        //instance
        public FontHandler(Block textColor, Vector3I[] Marks, Player p, Direction dir)
        {
            direction = dir;
            blockCount = 0;
            player = p;
            PixelData.X = Marks[0].X;
            PixelData.Y = Marks[0].Y;
            PixelData.Z = Marks[0].Z;
            PixelData.BlockColor = textColor;
            undoState = player.DrawBegin(null);
        }

        public void CreateGraphicsAndDraw(string Sentence)
        {
            SizeF size = MeasureTextSize(Sentence, player.font); //measure the text size to create a bmp)
            Bitmap img = new Bitmap((int) size.Width, (int) size.Height); //make an image based on string size
            using (Graphics g = Graphics.FromImage(img))
            {
                //IDisposable
                g.FillRectangle(Brushes.White, 0, 0, img.Width, img.Height); //make background, else crop will not work
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit; //fix to bleeding
                g.DrawString(Sentence, player.font, Brushes.Black, 0, 0); //draw some sexytext
                img = Crop(img); //crop the image to fix all problems with location
                img.RotateFlip(RotateFlipType.Rotate180FlipX); //flip this badboy
                Draw(img); //make the blockupdates
                img.Dispose(); //gtfo homeslice
            }
        }

        public void Draw(Bitmap img)
        {
            //guess how big the draw will be
            int Count = 0;
            for (int x = 0; x < img.Width; x++)
            {
                for (int z = 0; z < img.Height; z++)
                {
                    if (img.GetPixel(x, z).ToArgb() != System.Drawing.Color.White.ToArgb())
                    {
                        Count++;
                    }
                }
            }
            //check if player can make the drawing
            if (!player.CanDraw(Count))
            {
                player.MessageNow(
                    String.Format(
                        "You are only allowed to run commands that affect up to {0} blocks. This one would affect {1} blocks.",
                        player.Info.Rank.DrawLimit, Count));
                return;
            }
            //check direction and draw
            switch (direction)
            {
                case Direction.one:
                    for (int x = 0; x < img.Width; x++)
                    {
                        for (int z = 0; z < img.Height; z++)
                        {
                            if (img.GetPixel(x, z).ToArgb() != System.Drawing.Color.White.ToArgb())
                            {
                                DrawOneBlock(player, player.World.Map, PixelData.BlockColor,
                                    new Vector3I((PixelData.X + x), PixelData.Y, (PixelData.Z + z)),
                                    BlockChangeContext.Drawn,
                                    ref blocks, ref blocksDenied, undoState);
                                blockCount++;
                            }
                        }
                    }
                    break;
                case Direction.two:
                    for (int x = 0; x < img.Width; x++)
                    {
                        for (int z = 0; z < img.Height; z++)
                        {
                            if (img.GetPixel(x, z).ToArgb() != System.Drawing.Color.White.ToArgb())
                            {
                                DrawOneBlock(player, player.World.Map, PixelData.BlockColor,
                                    new Vector3I((PixelData.X - x), PixelData.Y, (PixelData.Z + z)),
                                    BlockChangeContext.Drawn,
                                    ref blocks, ref blocksDenied, undoState);
                                blockCount++;
                            }
                        }
                    }
                    break;
                case Direction.three:
                    for (int y = 0; y < img.Width; y++)
                    {
                        for (int z = 0; z < img.Height; z++)
                        {
                            if (img.GetPixel(y, z).ToArgb() != System.Drawing.Color.White.ToArgb())
                            {
                                DrawOneBlock(player, player.World.Map, PixelData.BlockColor,
                                    new Vector3I(PixelData.X, (PixelData.Y + y), (PixelData.Z + z)),
                                    BlockChangeContext.Drawn,
                                    ref blocks, ref blocksDenied, undoState);
                                blockCount++;
                            }
                        }
                    }
                    break;
                case Direction.four:
                    for (int y = 0; y < img.Width; y++)
                    {
                        for (int z = 0; z < img.Height; z++)
                        {
                            if (img.GetPixel(y, z).ToArgb() != System.Drawing.Color.White.ToArgb())
                            {
                                DrawOneBlock(player, player.World.Map, PixelData.BlockColor,
                                    new Vector3I(PixelData.X, ((PixelData.Y) - y), (PixelData.Z + z)),
                                    BlockChangeContext.Drawn,
                                    ref blocks, ref blocksDenied, undoState);
                                blockCount++;
                            }
                        }
                    }
                    break;
                default:
                    break; //if blockcount = 0, message is shown and returned
            }
        }

        #endregion

        #region Helpers

        public static Bitmap Crop(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            Func<int, bool> allWhiteRow = row =>
            {
                for (int i = 0; i < w; ++i)
                    if (bmp.GetPixel(i, row).R != 255)
                        return false;
                return true;
            };
            Func<int, bool> allWhiteColumn = col =>
            {
                for (int i = 0; i < h; ++i)
                    if (bmp.GetPixel(col, i).R != 255)
                        return false;
                return true;
            };
            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (allWhiteRow(row))
                    topmost = row;
                else break;
            }
            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (allWhiteRow(row))
                    bottommost = row;
                else break;
            }
            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (allWhiteColumn(col))
                    leftmost = col;
                else
                    break;
            }
            for (int col = w - 1; col >= 0; --col)
            {
                if (allWhiteColumn(col))
                    rightmost = col;
                else
                    break;
            }
            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.
            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;
            if (croppedWidth == 0)
            {
// No border on left or right
                leftmost = 0;
                croppedWidth = w;
            }
            if (croppedHeight == 0)
            {
// No border on top or bottom
                topmost = 0;
                croppedHeight = h;
            }
            try
            {
                var target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                        new RectangleF(0, 0, croppedWidth, croppedHeight),
                        new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                        GraphicsUnit.Pixel);
                }
                return target;
            }
            catch
            {
                return bmp; //return original image, I guess
            }
        }

        //Measure the size of the string length using IDisposable. Backport from AtomicCraft Client
        public static SizeF MeasureTextSize(string text, Font font)
        {
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    return g.MeasureString(text, font);
                }
            }
        }

        //stores information needed for each pixel
        public struct PixelData
        {
            public static int X;
            public static int Y;
            public static int Z;
            public static Block BlockColor;
        }

        #region DrawOneBlock

        private static void DrawOneBlock(Player player, Map map, Block drawBlock, Vector3I coord,
            BlockChangeContext context, ref int blocks, ref int blocksDenied, fCraft.Drawing.UndoState undoState)
        {
            if (map == null) return;
            if (player == null) throw new ArgumentNullException("player");

            if (!map.InBounds(coord)) return;
            Block block = map.GetBlock(coord);
            if (block == drawBlock) return;

            if (player.CanPlace(map, coord, drawBlock, context) != CanPlaceResult.Allowed)
            {
                blocksDenied++;
                return;
            }

            map.QueueUpdate(new BlockUpdate(null, coord, drawBlock));
            Player.RaisePlayerPlacedBlockEvent(player, map, coord, block, drawBlock, context);

            if (!undoState.IsTooLargeToUndo)
            {
                if (!undoState.Add(coord, block))
                {
                    player.Message("NOTE: This draw command is too massive to undo.");
                    player.LastDrawOp = null;
                }
            }
            blocks++;
        }

        #endregion

        //stolen from BuildingCommands

        #endregion
    }
}