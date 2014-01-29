using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraft.Events;
using System.Collections.Concurrent;
using System.Threading;

namespace fCraft
{
    public class Football
    {
        private FootballBehavior _footballBehavior = new FootballBehavior();
        private Vector3I _startPos;
        private World _world;

        public Football(Player player, World world, Vector3I FootballPos)
        {
            _world = world;
            Player.Clicked += ClickedFootball;
        }

        public void ResetFootball()
        {
            if (_startPos == null)
            {
                _startPos.X = _world.Map.Bounds.XMax - _world.Map.Bounds.XMin;
                _startPos.Y = _world.Map.Bounds.YMax - _world.Map.Bounds.YMin;
                for (int z = _world.Map.Bounds.ZMax; z > 0; z--)
                {
                    if (_world.Map.GetBlock(_startPos.X, _startPos.Y, z) != Block.Air)
                    {
                        _startPos.Z = z + 1;
                        break;
                    }
                }
            }
            _world.Map.QueueUpdate(new BlockUpdate(null, _startPos, Block.WhiteWool));
        }

        public void ClickedFootball(object sender, PlayerClickedEventArgs e)
        {
            //replace e.coords with player.Pos.toblock() (moving event)
            if (e.Coords == _world.footballPos)
            {
                double ksi = 2.0*Math.PI*(-e.Player.Position.L)/256.0;
                double r = Math.Cos(ksi);
                double phi = 2.0*Math.PI*(e.Player.Position.R - 64)/256.0;
                Vector3F dir = new Vector3F((float) (r*Math.Cos(phi)), (float) (r*Math.Sin(phi)),
                    (float) (Math.Sin(ksi)));
                _world.AddPhysicsTask(
                    new Particle(_world, e.Coords, dir, e.Player, Block.WhiteWool, _footballBehavior), 0);
            }
        }
    }
}