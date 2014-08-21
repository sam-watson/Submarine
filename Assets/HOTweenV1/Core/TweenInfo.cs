// Author: Daniele Giardini
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
// Created: 2012/07/07 13:08
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

using System.Collections.Generic;

namespace Holoville.HOTween.Core
{
    /// <summary>
    /// Used by <see cref="HOTween.GetTweenInfos"/> and HOTweenInspector,
    /// to store info about tweens that can be displayed.
    /// </summary>
    public class TweenInfo
    {
        /// <summary>
        /// Tween.
        /// </summary>
        public ABSTweenComponent tween;
        /// <summary>
        /// Is sequence.
        /// </summary>
        public bool isSequence;
        /// <summary>
        /// Targets.
        /// </summary>
        public List<object> targets;

        /// <summary>
        /// Is paused.
        /// </summary>
        public bool isPaused { get { return tween.isPaused; } }
        /// <summary>
        /// Is complete.
        /// </summary>
        public bool isComplete { get { return tween.isComplete; } }
        /// <summary>
        /// Is enabled.
        /// </summary>
        public bool isEnabled { get { return tween.enabled; } }

        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        /// <summary>
        /// Creates a new TweenInfo object.
        /// </summary>
        public TweenInfo(ABSTweenComponent tween)
        {
            this.tween = tween;
            isSequence = (tween is Sequence);
            targets = tween.GetTweenTargets();
        }
    }
}