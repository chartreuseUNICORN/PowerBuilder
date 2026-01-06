using System;

namespace PowerBuilder.Exceptions
{
    public class MissingBindingException : Exception
    {
        public MissingBindingException(string message) : base(message){}
    }
}