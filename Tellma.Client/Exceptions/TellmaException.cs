using System;

namespace Tellma.Client
{
    public class TellmaException : Exception
    {
        public TellmaException(string msg) : base(msg)
        {
        }
    }
}
