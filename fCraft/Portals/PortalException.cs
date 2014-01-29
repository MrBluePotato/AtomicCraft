using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fCraft.Portals
{
    internal class PortalException : Exception
    {
        public PortalException(String message)
            : base(message)
        {
            // Do nothing
        }
    }
}