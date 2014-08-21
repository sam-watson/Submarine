// Author: Daniele Giardini
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
// Created: 2012/07/25 11:26
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

using Holoville.HOTween.Core;

namespace Holoville.HOTween
{
    /// <summary>
    /// Only used with <see cref="ABSTweenComponent"/> ApplyCallback method.
    /// </summary>
    public enum CallbackType
    {
        /// <summary>
        /// Called when the tween is starting
        /// </summary>
        OnStart,
        /// <summary>
        /// Called each time the tween is updated
        /// </summary>
        OnUpdate,
        /// <summary>
        /// Called each time a single loop is completed
        /// </summary>
        OnStepComplete,
        /// <summary>
        /// Called when the whole tween (loops included) is complete
        /// </summary>
        OnComplete,
        /// <summary>
        /// Called when the tween is paused
        /// </summary>
        OnPause,
        /// <summary>
        /// Called when the tween is played
        /// </summary>
        OnPlay,
        /// <summary>
        /// Called when the tween is rewinded
        /// </summary>
        OnRewinded,
        /// <summary>
        /// Works only with Tweeners, and not with Sequences.
        /// Called when a plugin of the Tweens is overwritten by the OverwriteManager.
        /// </summary>
        OnPluginOverwritten
    }
}