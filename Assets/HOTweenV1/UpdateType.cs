//
// UpdateType.cs
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

namespace Holoville.HOTween
{
    /// <summary>
    /// Enumeration of types of update that can be applied to a tween.
    /// </summary>
    public enum UpdateType
    {
        /// <summary>
        /// Normal update.
        /// </summary>
        Update,

        /// <summary>
        /// Late update.
        /// </summary>
        LateUpdate,

        /// <summary>
        /// Fixed update (useful for rigidBodies).
        /// </summary>
        FixedUpdate,

        /// <summary>
        /// Timescale independent update.
        /// Contrary to the other types, this one is not subject to changes in Time.timeScale,
        /// thus it's the best way for tweens that happen inside GUI methods
        /// (so that even if the game is paused, the GUI will still have animated tweens).
        /// </summary>
        TimeScaleIndependentUpdate
    }
}
