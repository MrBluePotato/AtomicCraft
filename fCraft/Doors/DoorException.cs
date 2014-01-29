using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fCraft.Doors
{
    internal class DoorException : Exception
    {
        public DoorException(String message)
            : base(message)
        {
            // Do nothing
        }
    }
}