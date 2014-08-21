//
// MemberAccessorCacher.cs
//
// Author: Daniele Giardini
//
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if !MICRO
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FastDynamicMemberAccessor
{
    /// <summary>
    /// Cache manager for James Nies' MemberAccessor classes
    /// </summary>
    internal static class MemberAccessorCacher
    {
        // VARS ///////////////////////////////////////////////////

        static Dictionary<Type, Dictionary<string, MemberAccessor>> dcMemberAccessors;


        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the cached memberAccessor if it alread exists,
        /// or calls MemberAccessor.Make and caches and returns the newly created MemberAccessor.
        /// </summary>
        internal static MemberAccessor Make(Type p_targetType, string p_propName, PropertyInfo p_propertyInfo, FieldInfo p_fieldInfo)
        {
            if (dcMemberAccessors != null && dcMemberAccessors.ContainsKey(p_targetType) && dcMemberAccessors[p_targetType].ContainsKey(p_propName))
            {
                return dcMemberAccessors[p_targetType][p_propName];
            }

            if (dcMemberAccessors == null)
            {
                dcMemberAccessors = new Dictionary<Type, Dictionary<string, MemberAccessor>>();
            }
            if (!dcMemberAccessors.ContainsKey(p_targetType))
            {
                dcMemberAccessors.Add(p_targetType, new Dictionary<string, MemberAccessor>());
            }
            Dictionary<string, MemberAccessor> dcFinal = dcMemberAccessors[p_targetType];

            MemberAccessor ma = MemberAccessor.Make(p_propertyInfo, p_fieldInfo);
            dcFinal.Add(p_propName, ma);

            return ma;
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        internal static void Clear()
        {
            dcMemberAccessors = null;
        }
    }
}
#endif
