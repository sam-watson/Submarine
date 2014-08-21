//
// TweenDelegate.cs
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

namespace Holoville.HOTween.Core
{
    /// <summary>
    /// Enum of delegates used by HOTween.
    /// </summary>
    public static class TweenDelegate
    {
        /// <summary>
        /// Delegate used to store OnEvent (OnStart, OnComplete, etc) functions that will accept a <see cref="TweenEvent"/> parameter.
        /// </summary>
        public delegate void TweenCallbackWParms(TweenEvent p_callbackData);

        /// <summary>
        /// Delegate used to store OnEvent (OnStart, OnComplete, etc) functions without parameters.
        /// </summary>
        public delegate void TweenCallback();

        /// <summary>
        /// Delegate used internally for ease functions.
        /// </summary>
        public delegate float EaseFunc(float elapsed, float startValue, float changeValue, float duration, float overshootOrAmplitude, float period);

        internal delegate void FilterFunc(int p_index, bool p_optionalBool);

        /// <summary>
        /// Used in place of <c>System.Func</c>, which is not available in mscorlib.
        /// </summary>
        public delegate T HOFunc<out T>();

        /// <summary>
        /// Used in place of <c>System.Action</c>.
        /// </summary>
        public delegate void HOAction<in T>(T p_newValue);
    }
}
