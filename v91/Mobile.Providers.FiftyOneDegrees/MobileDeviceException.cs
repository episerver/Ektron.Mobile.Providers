using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobile.Providers.FiftyOneDegrees
{
    public class MobileDeviceException : Exception
    {
        public MobileDeviceException()
        {
        }

        public MobileDeviceException(string message)
        : base(message)
        {
        }

        public MobileDeviceException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
