//
// MemberAccessorException.cs
//
// Author: James Nies
// Licensed under The Code Project Open License (CPOL): http://www.codeproject.com/info/cpol10.aspx

#if !MICRO
using System;

namespace FastDynamicMemberAccessor
{
    /// <summary>
    /// PropertyAccessorException class.
    /// </summary>
    internal class MemberAccessorException : Exception
    {
        internal MemberAccessorException(string message)
            : base(message)
        {
        }
    }
}
#endif
