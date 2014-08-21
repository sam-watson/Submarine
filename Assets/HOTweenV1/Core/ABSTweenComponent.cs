//
// ABSTweenComponent.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween.Plugins.Core;
using UnityEngine;

namespace Holoville.HOTween.Core
{
    /// <summary>
    /// Base class for all HOTween members
    /// (<see cref="Tweener"/> and <see cref="Sequence"/>).
    /// </summary>
    public abstract class ABSTweenComponent : IHOTweenComponent
    {
        // VARS ///////////////////////////////////////////////////

        internal string _id = "";
        internal int _intId = -1;
        internal bool _autoKillOnComplete = true;
        internal bool _enabled = true;
        internal float _timeScale = HOTween.defTimeScale;
        internal int _loops = 1;
        internal LoopType _loopType = HOTween.defLoopType;

        internal UpdateType _updateType = HOTween.defUpdateType;
        internal bool _isPaused;

        /// <summary>
        /// Always set to TRUE by Update(), if isStartupIteration is true,
        /// and reset to FALSE in the last line of Update().
        /// Can also be set to TRUE by Sequence.TweenStartupIteration,
        /// and then immediately reset to FALSE.
        /// </summary>
        internal bool ignoreCallbacks;

        /// <summary>
        /// Used by main Sequences to set an ignoreCallbacks value to all its items/subitems,
        /// which the items/subitmes themselves won't be able to reset.
        /// Necessary during TweenStartupIteration.
        /// </summary>
        internal bool _steadyIgnoreCallbacks;
        // Overridden by sequences to set steadyIgnoreCallbacks value to all times and subitems.
        internal virtual bool steadyIgnoreCallbacks
        {
            get { return _steadyIgnoreCallbacks; }
            set { _steadyIgnoreCallbacks = value; }
        }

        internal Sequence contSequence; // Eventual sequence containing this tween.
        internal bool startupDone;

        internal TweenDelegate.TweenCallback onStart;
        internal TweenDelegate.TweenCallbackWParms onStartWParms;
        internal object[] onStartParms;

        internal TweenDelegate.TweenCallback onUpdate;
        internal TweenDelegate.TweenCallbackWParms onUpdateWParms;
        internal object[] onUpdateParms;

        internal TweenDelegate.TweenCallback onPluginUpdated;
        internal TweenDelegate.TweenCallbackWParms onPluginUpdatedWParms;
        internal object[] onPluginUpdatedParms;

        internal TweenDelegate.TweenCallback onPause;
        internal TweenDelegate.TweenCallbackWParms onPauseWParms;
        internal object[] onPauseParms;

        internal TweenDelegate.TweenCallback onPlay;
        internal TweenDelegate.TweenCallbackWParms onPlayWParms;
        internal object[] onPlayParms;

        internal TweenDelegate.TweenCallback onRewinded;
        internal TweenDelegate.TweenCallbackWParms onRewindedWParms;
        internal object[] onRewindedParms;

        internal TweenDelegate.TweenCallback onStepComplete;
        internal TweenDelegate.TweenCallbackWParms onStepCompleteWParms;
        internal object[] onStepCompleteParms;

        internal TweenDelegate.TweenCallback onComplete;
        internal TweenDelegate.TweenCallbackWParms onCompleteWParms;
        internal object[] onCompleteParms;

        /// <summary>
        /// Completed loops.
        /// </summary>
        protected int _completedLoops;

        /// <summary>
        /// Duration.
        /// </summary>
        protected float _duration;

        /// <summary>
        /// Memorized when a partial tween is applied.
        /// </summary>
        protected float _originalDuration;

        /// <summary>
        /// Memorized when applying speedBased duration.
        /// </summary>
        protected float _originalNonSpeedBasedDuration;

        /// <summary>
        /// Full duration.
        /// </summary>
        protected float _fullDuration;

        /// <summary>
        /// Elapsed.
        /// </summary>
        protected float _elapsed;

        /// <summary>
        /// Full elapsed.
        /// </summary>
        protected float _fullElapsed;

        /// <summary>
        /// Destroyed.
        /// </summary>
        protected bool _destroyed;

        /// <summary>
        /// Is empty.
        /// </summary>
        protected bool _isEmpty = true;

        /// <summary>
        /// Running backwards.
        /// </summary>
        protected bool _isReversed;

        /// <summary>
        /// Yoyo looping back.
        /// </summary>
        protected bool _isLoopingBack;

        /// <summary>
        /// Has started.
        /// </summary>
        protected bool _hasStarted;

        /// <summary>
        /// Is complete.
        /// </summary>
        protected bool _isComplete;

        /// <summary>
        /// Used to determine if OnUpdate callbacks should be called.
        /// Refreshed at the end of each update.
        /// </summary>
        protected float prevFullElapsed;

        /// <summary>
        /// Previously completed loops.
        /// Refrehsed at the end of each update.
        /// </summary>
        protected int prevCompletedLoops;

        // BEHAVIOURS/GAMEOBJECT MANAGEMENT PARMS

        /// <summary>
        /// True if there are behaviours to manage
        /// </summary>
        internal bool manageBehaviours;
        /// <summary>
        /// True if there are gameObject to manage
        /// </summary>
        internal bool manageGameObjects;
        /// <summary>
        /// Behaviours to activate
        /// </summary>
        internal Behaviour[] managedBehavioursOn;
        /// <summary>
        /// Behaviours to deactivate
        /// </summary>
        internal Behaviour[] managedBehavioursOff;
        /// <summary>
        /// GameObjects to activate
        /// </summary>
        internal GameObject[] managedGameObjectsOn;
        /// <summary>
        /// GameObejcts to deactivate
        /// </summary>
        internal GameObject[] managedGameObjectsOff;
        /// <summary>
        /// True = enabled, False = disabled
        /// </summary>
        internal bool[] managedBehavioursOriginalState;
        /// <summary>
        /// True = active, False = inactive
        /// </summary>
        internal bool[] managedGameObjectsOriginalState;

        // GETS/SETS //////////////////////////////////////////////

        /// <summary>
        /// Eventual string ID of this Tweener/Sequence
        /// (more than one Tweener/Sequence can share the same ID, thus allowing for grouped operations).
        /// You can also use <c>intId</c> instead of <c>id</c> for faster operations.
        /// </summary>
        public string id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Eventual int ID of this Tweener/Sequence
        /// (more than one Tweener/Sequence can share the same intId, thus allowing for grouped operations).
        /// The main difference from <c>id</c> is that while <c>id</c> is more legible, <c>intId</c> allows for faster operations.
        /// </summary>
        public int intId
        {
            get
            {
                return _intId;
            }
            set
            {
                _intId = value;
            }
        }

        /// <summary>
        /// Default is <c>true</c>, which means this Tweener/Sequence will be killed and removed from HOTween as soon as it's completed.
        /// If <c>false</c> doesn't remove this Tweener/Sequence from HOTween when it is completed,
        /// and you will need to call an <c>HOTween.Kill</c> to remove this Tweener/Sequence.
        /// </summary>
        public bool autoKillOnComplete
        {
            get
            {
                return _autoKillOnComplete;
            }
            set
            {
                _autoKillOnComplete = value;
            }
        }

        /// <summary>
        /// Default is <c>true</c>.
        /// If set to <c>false</c>, this Tweener/Sequence will not be updated,
        /// and any use of animation methods (Play/Pause/Rewind/etc) will be ignored
        /// (both if called directly via this instance, than if using HOTween.Play/Pause/Rewind/etc.).
        /// </summary>
        public bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }
        }

        /// <summary>
        /// Time scale that will be used by this Tweener/Sequence.
        /// </summary>
        public float timeScale
        {
            get
            {
                return _timeScale;
            }
            set
            {
                _timeScale = value;
            }
        }

        /// <summary>
        /// Number of times the Tweener/Sequence will run (<c>-1</c> means the tween has infinite loops).
        /// </summary>
        public int loops
        {
            get
            {
                return _loops;
            }
            set
            {
                _loops = value;
                SetFullDuration();
            }
        }

        /// <summary>
        /// Type of loop for this Tweener/Sequence, in case <see cref="loops"/> is greater than 1 (or infinite).
        /// </summary>
        public LoopType loopType
        {
            get
            {
                return _loopType;
            }
            set
            {
                _loopType = value;
            }
        }

        /// <summary>
        /// Gets and sets the time position of the Tweener/Sequence (loops are included when not infinite, delay is not).
        /// </summary>
        public float position
        {
            get
            {
                return (_loops < 1 ? _elapsed : _fullElapsed);
            }
            set
            {
                GoTo(value, !_isPaused);
            }
        }

        // READ-ONLY GETS /////////////////////////////////////////

        /// <summary>
        /// Duration of this Tweener/Sequence, loops and tween delay excluded.
        /// </summary>
        public float duration
        {
            get
            {
                return _duration;
            }
        }

        /// <summary>
        /// Full duration of this Tweener/Sequence, loops included (when not infinite) but tween delay excluded.
        /// </summary>
        public float fullDuration
        {
            get
            {
                return _fullDuration;
            }
        }

        /// <summary>
        /// Elapsed time within the current loop (tween delay excluded).
        /// </summary>
        public float elapsed
        {
            get
            {
                return _elapsed;
            }
        }

        /// <summary>
        /// Full elapsed time including loops (but without considering tween delay).
        /// </summary>
        public float fullElapsed
        {
            get
            {
                return _fullElapsed;
            }
        }

        /// <summary>
        /// The update type for this Tweener/Sequence.
        /// </summary>
        public UpdateType updateType
        {
            get
            {
                return _updateType;
            }
        }

        /// <summary>
        /// Number of loops that have been executed.
        /// </summary>
        public int completedLoops
        {
            get
            {
                return _completedLoops;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> if this Tweener/Sequence was destroyed
        /// (either because it was manually destroyed, because it was completed, or because its target was destroyed).
        /// </summary>
        public bool destroyed
        {
            get
            {
                return _destroyed;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> if this Tweener/Sequence contains no tweens
        /// (if this is a tween, it means that no valid property to tween was set;
        /// if this is a sequence, it means no valid <see cref="Tweener"/> was yet added).
        /// </summary>
        public bool isEmpty
        {
            get
            {
                return _isEmpty;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> if this Tweener/Sequence is set to go backwards (because of a call to <c>Reverse</c>.
        /// </summary>
        public bool isReversed
        {
            get
            {
                return _isReversed;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> when this Tweener/Sequence is in the "going backwards" part of a Yoyo loop.
        /// </summary>
        public bool isLoopingBack
        {
            get
            {
                return _isLoopingBack;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> if this Tweener/Sequence is paused.
        /// </summary>
        public bool isPaused
        {
            get
            {
                return _isPaused;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> after this Tweener/Sequence was started the first time,
        /// or if a call to <c>GoTo</c> or <c>GoToAndPlay</c> was executed.
        /// </summary>
        public bool hasStarted
        {
            get
            {
                return _hasStarted;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> when this Tweener/Sequence is complete.
        /// </summary>
        public bool isComplete
        {
            get
            {
                return _isComplete;
            }
        }

        /// <summary>
        /// Returns a value of <c>true</c> if this Tweener/Sequence was added to a Sequence.
        /// </summary>
        public bool isSequenced
        {
            get
            {
                return contSequence != null;
            }
        }


        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        /// <summary>
        /// Kills this Tweener/Sequence, removes it from HOTween, and cleans it.
        /// </summary>
        public void Kill()
        {
            Kill(true);
        }

        /// <summary>
        /// Kills this Tweener/Sequence and cleans it.
        /// </summary>
        /// <param name="p_autoRemoveFromHOTween">
        /// If <c>true</c> also calls <c>HOTween.Kill(this)</c> to remove it from HOTween.
        /// Set internally to <c>false</c> when I already know that HOTween is going to remove it.
        /// </param>
        internal virtual void Kill(bool p_autoRemoveFromHOTween)
        {
            if (_destroyed) return;

            _destroyed = _isEmpty = true;
            if (p_autoRemoveFromHOTween) HOTween.Kill(this);
        }

        /// <summary>
        /// Resumes this Tweener/Sequence (tween delay included).
        /// </summary>
        public void Play()
        {
            if (_enabled) {
                PlayIfPaused();
            }
        }

        void PlayIfPaused()
        {
            if (_isPaused && (!_isReversed && !_isComplete || _isReversed && _fullElapsed > 0)) {
                _isPaused = false;
                OnPlay();
            }
        }

        /// <summary>
        /// Resumes this Tweener/Sequence (tween delay included) and plays it forward.
        /// </summary>
        public void PlayForward()
        {
            if (_enabled) {
                _isReversed = false;
                PlayIfPaused();
            }
        }

        /// <summary>
        /// Resumes this Tweener/Sequence and plays it backwards.
        /// </summary>
        public void PlayBackwards()
        {
            if (_enabled) {
                _isReversed = true;
                PlayIfPaused();
            }
        }

        /// <summary>
        /// Pauses this Tweener/Sequence.
        /// </summary>
        public void Pause()
        {
            if (_enabled && !_isPaused) {
                _isPaused = true;
                OnPause();
            }
        }

        /// <summary>
        /// Rewinds this Tweener/Sequence (loops and tween delay included), and pauses it.
        /// </summary>
        public abstract void Rewind();

        /// <summary>
        /// Restarts this Tweener/Sequence from the beginning (loops and tween delay included).
        /// </summary>
        public abstract void Restart();

        /// <summary>
        /// Reverses this Tweener/Sequence,
        /// animating it backwards from its curren position.
        /// </summary>
        /// <param name="p_forcePlay">
        /// If TRUE, the tween will also start playing in case it was paused,
        /// otherwise it will maintain its current play/pause state (default).
        /// </param>
        public void Reverse(bool p_forcePlay = false)
        {
            if (_enabled) {
                _isReversed = !_isReversed;
                if (p_forcePlay) Play();
            }
        }

        /// <summary>
        /// Completes this Tweener/Sequence.
        /// Where a loop was involved, the Tweener/Sequence completes at the position where it would actually be after the set number of loops.
        /// If there were infinite loops, this method will have no effect.
        /// </summary>
        public void Complete()
        {
            Complete(true);
        }

        /// <summary>
        /// Sends the Tweener/Sequence to the given time (taking also loops into account).
        /// If the time is bigger than the total Tweener/Sequence duration, it goes to the end.
        /// </summary>
        /// <param name="p_time">
        /// The time where the Tweener/Sequence should be sent.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the Tweener/Sequence reached its end and was completed.
        /// </returns>
        public bool GoTo(float p_time)
        {
            return GoTo(p_time, false, false, false);
        }
        /// <summary>
        /// Sends the Tweener/Sequence to the given time (taking also loops into account).
        /// If the time is bigger than the total Tweener/Sequence duration, it goes to the end.
        /// </summary>
        /// <param name="p_time">
        /// The time where the Tweener/Sequence should be sent.
        /// </param>
        /// <param name="p_forceUpdate">
        /// By default, if a Tweener/Sequence is already at the exact given time, it will not be refreshed.
        /// Setting this to <c>true</c> will force it to refresh
        /// (useful only if you want to be sure that any changes you made to the tweened property,
        /// outside of HOTween, are reset).
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the Tweener/Sequence reached its end and was completed.
        /// </returns>
        public bool GoTo(float p_time, bool p_forceUpdate)
        {
            return GoTo(p_time, false, p_forceUpdate, false);
        }
        internal bool GoTo(float p_time, bool p_forceUpdate, bool p_ignoreCallbacks)
        {
            return GoTo(p_time, false, p_forceUpdate, p_ignoreCallbacks);
        }

        /// <summary>
        /// Sends the Tweener/Sequence to the given time (taking also loops into account) and plays it.
        /// If the time is bigger than the total Tweener/Sequence duration, it goes to the end.
        /// </summary>
        /// <param name="p_time">
        /// The time where the Tweener/Sequence should be sent.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the Tweener/Sequence reached its end and was completed.
        /// </returns>
        public bool GoToAndPlay(float p_time)
        {
            return GoTo(p_time, true, false, false);
        }
        /// <summary>
        /// Sends the Tweener/Sequence to the given time (taking also loops into account) and plays it.
        /// If the time is bigger than the total Tweener/Sequence duration, it goes to the end.
        /// </summary>
        /// <param name="p_time">
        /// The time where the Tweener/Sequence should be sent.
        /// </param>
        /// <param name="p_forceUpdate">
        /// By default, if a Tweener/Sequence is already at the exact given time, it will not be refreshed.
        /// Setting this to <c>true</c> will force it to refresh
        /// (useful only if you want to be sure that any changes you made to the tweened property,
        /// outside of HOTween, are reset).
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the Tweener/Sequence reached its end and was completed.
        /// </returns>
        public bool GoToAndPlay(float p_time, bool p_forceUpdate)
        {
            return GoTo(p_time, true, p_forceUpdate, false);
        }
        internal bool GoToAndPlay(float p_time, bool p_forceUpdate, bool p_ignoreCallbacks)
        {
            return GoTo(p_time, true, p_forceUpdate, p_ignoreCallbacks);
        }

        /// <summary>
        /// A coroutine that waits until the Tweener/Sequence is complete (delays and loops included).
        /// You can use it inside a coroutine as a yield. Ex:
        /// yield return StartCoroutine( myTweenComponent.WaitForCompletion() );
        /// </summary>
        public IEnumerator WaitForCompletion()
        {
            while (!_destroyed && !_isComplete) {
                yield return null;
            }
            yield break;
        }

        /// <summary>
        /// A coroutine that waits until the Tweener/Sequence is rewinded (loops included).
        /// You can use it inside a coroutine as a yield. Ex:
        /// yield return StartCoroutine( myTweenComponent.WaitForRewind() );
        /// </summary>
        public IEnumerator WaitForRewind()
        {
            while (!destroyed && _fullElapsed > 0) {
                yield return 0;
            }
            yield break;
        }

        /// <summary>
        /// Completely resets this tween, except its target (in case of Tweeners).
        /// </summary>
        protected virtual void Reset()
        {
            _id = "";
            _intId = -1;
            _autoKillOnComplete = true;
            _enabled = true;
            _timeScale = HOTween.defTimeScale;
            _loops = 1;
            _loopType = HOTween.defLoopType;
            _updateType = HOTween.defUpdateType;
            _isPaused = false;
            _completedLoops = 0;
            _duration = _originalDuration = _originalNonSpeedBasedDuration = _fullDuration = 0;
            _elapsed = _fullElapsed = 0;
            _isEmpty = true;
            _isReversed = _isLoopingBack = _hasStarted = _isComplete = false;
            startupDone = false;
            onStart = null;
            onStartWParms = null;
            onStartParms = null;
            onUpdate = null;
            onUpdateWParms = null;
            onUpdateParms = null;
            onPluginUpdated = null;
            onPluginUpdatedWParms = null;
            onPluginUpdatedParms = null;
            onStepComplete = null;
            onStepCompleteWParms = null;
            onStepCompleteParms = null;
            onComplete = null;
            onCompleteWParms = null;
            onCompleteParms = null;
            onPause = null;
            onPauseWParms = null;
            onPauseParms = null;
            onPlay = null;
            onPlayWParms = null;
            onPlayParms = null;
            onRewinded = null;
            onRewindedWParms = null;
            onRewindedParms = null;

            manageBehaviours = false;
            managedBehavioursOff = null;
            managedBehavioursOn = null;
            managedBehavioursOriginalState = null;
            manageGameObjects = false;
            managedGameObjectsOff = null;
            managedGameObjectsOn = null;
            managedGameObjectsOriginalState = null;
        }

        /// <summary>
        /// Assigns the given callback to this Tweener/Sequence,
        /// overwriting any existing callbacks of the same type.
        /// </summary>
        /// <param name="p_callbackType">The type of callback to apply</param>
        /// <param name="p_callback">The function to call, who must return <c>void</c> and accept no parameters</param>
        public void ApplyCallback(CallbackType p_callbackType, TweenDelegate.TweenCallback p_callback)
        {
            ApplyCallback(false, p_callbackType, p_callback, null, null);
        }
        /// <summary>
        /// Assigns the given callback to this Tweener/Sequence,
        /// overwriting any existing callbacks of the same type.
        /// </summary>
        /// <param name="p_callbackType">The type of callback to apply</param>
        /// <param name="p_callback">The function to call.
        /// It must return <c>void</c> and has to accept a single parameter of type <see cref="TweenEvent"/></param>
        /// <param name="p_callbackParms">Additional comma separated parameters to pass to the function</param>
        public void ApplyCallback(CallbackType p_callbackType, TweenDelegate.TweenCallbackWParms p_callback, params object[] p_callbackParms)
        {
            ApplyCallback(true, p_callbackType, null, p_callback, p_callbackParms);
        }
        /// <summary>
        /// Assigns the given callback to this Tweener/Sequence,
        /// overwriting any existing callbacks of the same type.
        /// This overload will use sendMessage to call the method named p_methodName 
        /// on every MonoBehaviour in the p_sendMessageTarget GameObject.
        /// </summary>
        /// <param name="p_callbackType">The type of callback to apply</param>
        /// <param name="p_sendMessageTarget">GameObject to target for sendMessage</param>
        /// <param name="p_methodName">Name of the method to call</param>
        /// <param name="p_value">Eventual additional parameter</param>
        /// <param name="p_options">SendMessageOptions</param>
        public void ApplyCallback(CallbackType p_callbackType, GameObject p_sendMessageTarget, string p_methodName, object p_value, SendMessageOptions p_options = SendMessageOptions.RequireReceiver)
        {
            TweenDelegate.TweenCallbackWParms cb = HOTween.DoSendMessage;
            object[] cbParms = new object[] {
                p_sendMessageTarget,
                p_methodName,
                p_value,
                p_options
            };
            ApplyCallback(true, p_callbackType, null, cb, cbParms);
        }
        /// <summary>
        /// Assigns the given callback to this Tweener/Sequence,
        /// overwriting any existing callbacks of the same type.
        /// </summary>
        protected virtual void ApplyCallback(bool p_wParms, CallbackType p_callbackType, TweenDelegate.TweenCallback p_callback, TweenDelegate.TweenCallbackWParms p_callbackWParms, params object[] p_callbackParms)
        {
            switch (p_callbackType) {
                case CallbackType.OnStart:
                    onStart = p_callback;
                    onStartWParms = p_callbackWParms;
                    onStartParms = p_callbackParms;
                    break;
                case CallbackType.OnUpdate:
                    onUpdate = p_callback;
                    onUpdateWParms = p_callbackWParms;
                    onUpdateParms = p_callbackParms;
                    break;
                case CallbackType.OnStepComplete:
                    onStepComplete = p_callback;
                    onStepCompleteWParms = p_callbackWParms;
                    onStepCompleteParms = p_callbackParms;
                    break;
                case CallbackType.OnComplete:
                    onComplete = p_callback;
                    onCompleteWParms = p_callbackWParms;
                    onCompleteParms = p_callbackParms;
                    break;
                case CallbackType.OnPlay:
                    onPlay = p_callback;
                    onPlayWParms = p_callbackWParms;
                    onPlayParms = p_callbackParms;
                    break;
                case CallbackType.OnPause:
                    onPause = p_callback;
                    onPauseWParms = p_callbackWParms;
                    onPauseParms = p_callbackParms;
                    break;
                case CallbackType.OnRewinded:
                    onRewinded = p_callback;
                    onRewindedWParms = p_callbackWParms;
                    onRewindedParms = p_callbackParms;
                    break;
                case CallbackType.OnPluginOverwritten:
                    TweenWarning.Log("ApplyCallback > OnPluginOverwritten type is available only with Tweeners and not with Sequences");
                    break;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the given target is currently involved in a running tween or sequence.
        /// Returns <c>false</c> both if the given target is not inside a tween, than if the relative tween is paused.
        /// To simply check if the target is attached to a tween or sequence, use <c>IsLinkedTo( target )</c> instead.
        /// </summary>
        /// <param name="p_target">
        /// The target to check.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the given target is currently involved in a running tween or sequence.
        /// </returns>
        public abstract bool IsTweening(object p_target);
        /// <summary>
        /// Returns <c>true</c> if the tween with the given string id is currently involved in a running tween or sequence.
        /// </summary>
        /// <param name="p_id">
        /// The id to check for.
        /// </param>
        public abstract bool IsTweening(string p_id);
        /// <summary>
        /// Returns <c>true</c> if the tween with the given int id is currently involved in a running tween or sequence.
        /// </summary>
        /// <param name="p_id">
        /// The id to check for.
        /// </param>
        public abstract bool IsTweening(int p_id);

        /// <summary>
        /// Returns <c>true</c> if the given target is linked to a tween or sequence (running or not).
        /// </summary>
        /// <param name="p_target">
        /// The target to check.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the given target is linked to a tween or sequence (running or not).
        /// </returns>
        public abstract bool IsLinkedTo(object p_target);

        /// <summary>
        /// Returns a list of all the targets of this tween, or NULL if there are none.
        /// </summary>
        /// <returns>A list of all the targets of this tween, or NULL if there are none.</returns>
        public abstract List<object> GetTweenTargets();

        // ===================================================================================
        // INTERNAL METHODS ------------------------------------------------------------------

        /// <summary>
        /// Returns a list of the eventual existing tweens with the given Id within this ABSTweenComponent,
        /// nested tweens included (or an empty list if no tweens were found).
        /// </summary>
        internal abstract List<IHOTweenComponent> GetTweensById(string p_id);

        /// <summary>
        /// Returns a list of the eventual existing tweens with the given intId within this ABSTweenComponent,
        /// nested tweens included (or an empty list if no tweens were found).
        /// </summary>
        internal abstract List<IHOTweenComponent> GetTweensByIntId(int p_intId);

        /// <summary>
        /// Used internally by HOTween, to avoid having the tween calling a kill while HOTween will already be killing it.
        /// </summary>
        internal abstract void Complete(bool p_doAutoKill);

        /// <summary>
        /// Updates the Tweener/Sequence by the given elapsed time,
        /// and returns a value of <c>true</c> if the Tweener/Sequence is complete.
        /// </summary>
        /// <param name="p_elapsed">
        /// The elapsed time since the last update.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the tween is not reversed and is complete (or the tween target doesn't exist anymore), otherwise <c>false</c>.
        /// </returns>
        internal bool Update(float p_elapsed)
        {
            return Update(p_elapsed, false, false, false);
        }
        /// <summary>
        /// Updates the Tweener/Sequence by the given elapsed time,
        /// and returns a value of <c>true</c> if the Tweener/Sequence is complete.
        /// </summary>
        /// <param name="p_elapsed">
        /// The elapsed time since the last update.
        /// </param>
        /// <param name="p_forceUpdate">
        /// If <c>true</c> forces the update even if the Tweener/Sequence is complete or paused,
        /// but ignores onUpdate, and sends onComplete and onStepComplete calls only if the Tweener/Sequence wasn't complete before this call.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the tween is not reversed and complete (or the tween target doesn't exist anymore), otherwise <c>false</c>.
        /// </returns>
        internal bool Update(float p_elapsed, bool p_forceUpdate)
        {
            return Update(p_elapsed, p_forceUpdate, false, false);
        }
        /// <summary>
        /// Updates the Tweener/Sequence by the given elapsed time,
        /// and returns a value of <c>true</c> if the Tweener/Sequence is complete.
        /// </summary>
        /// <param name="p_elapsed">
        /// The elapsed time since the last update.
        /// </param>
        /// <param name="p_forceUpdate">
        /// If <c>true</c> forces the update even if the Tweener/Sequence is complete or paused,
        /// but ignores onUpdate, and sends onComplete and onStepComplete calls only if the Tweener/Sequence wasn't complete before this call.
        /// </param>
        /// <param name="p_isStartupIteration">
        /// If <c>true</c> means the update is due to a startup iteration (managed by Sequence Startup),
        /// and all callbacks will be ignored.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the tween is not reversed and complete (or the tween target doesn't exist anymore), otherwise <c>false</c>.
        /// </returns>
        internal bool Update(float p_elapsed, bool p_forceUpdate, bool p_isStartupIteration)
        {
            return Update(p_elapsed, p_forceUpdate, p_isStartupIteration, false);
        }
        /// <summary>
        /// Updates the Tweener/Sequence by the given elapsed time,
        /// and returns a value of <c>true</c> if the Tweener/Sequence is complete.
        /// </summary>
        /// <param name="p_elapsed">
        /// The elapsed time since the last update.
        /// </param>
        /// <param name="p_forceUpdate">
        /// If <c>true</c> forces the update even if the Tweener/Sequence is complete or paused,
        /// but ignores onUpdate, and sends onComplete and onStepComplete calls only if the Tweener/Sequence wasn't complete before this call.
        /// </param>
        /// <param name="p_isStartupIteration">
        /// If <c>true</c> means the update is due to a startup iteration (managed by Sequence Startup),
        /// and all callbacks will be ignored.
        /// </param>
        /// <param name="p_ignoreCallbacks">
        /// If <c>true</c> doesn't call any callback method.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the tween is not reversed and complete (or the tween target doesn't exist anymore), otherwise <c>false</c>.
        /// </returns>
        internal abstract bool Update(float p_elapsed, bool p_forceUpdate, bool p_isStartupIteration, bool p_ignoreCallbacks);

        /// <summary>
        /// Applies the correct Incremental Sequence loop value.
        /// Called by Sequences when they need to change the increment value of nested Sequences/Tweeners.
        /// </summary>
        /// <param name="p_diffIncr">
        /// The difference from the previous main Sequence loop increment.
        /// </param>
        internal abstract void SetIncremental(int p_diffIncr);

        // ===================================================================================
        // PRIVATE METHODS -------------------------------------------------------------------

        /// <summary>
        /// Sends the Tweener/Sequence to the given time (taking also loops into account) and plays it.
        /// If the time is bigger than the total Tweener/Sequence duration, it goes to the end.
        /// </summary>
        protected abstract bool GoTo(float p_time, bool p_play, bool p_forceUpdate, bool p_ignoreCallbacks);

        /// <summary>
        /// Startup this tween
        /// (might or might not call OnStart, depending if the tween is in a Sequence or not).
        /// Can be executed only once per tween.
        /// </summary>
        protected virtual void Startup()
        {
            startupDone = true;
        }

        /// <summary>
        /// Manages on first start behaviour.
        /// </summary>
        protected virtual void OnStart()
        {
            if (steadyIgnoreCallbacks || ignoreCallbacks) return;

            _hasStarted = true;
            if (onStart != null) {
                onStart();
            } else if (onStartWParms != null) {
                onStartWParms(new TweenEvent(this, onStartParms));
            }
            OnPlay();
        }

        /// <summary>
        /// Manages on update behaviour.
        /// </summary>
        protected void OnUpdate()
        {
            if (steadyIgnoreCallbacks || ignoreCallbacks) {
                return;
            }
            if (onUpdate != null) {
                onUpdate();
            } else if (onUpdateWParms != null) {
                onUpdateWParms(new TweenEvent(this, onUpdateParms));
            }
        }

        /// <summary>
        /// Manages on plugin results behaviour.
        /// </summary>
        protected void OnPluginUpdated(ABSTweenPlugin p_plugin)
        {
            if (steadyIgnoreCallbacks || ignoreCallbacks) {
                return;
            }
            if (onPluginUpdated != null) {
                onPluginUpdated();
            } else if (onPluginUpdatedWParms != null) {
                onPluginUpdatedWParms(new TweenEvent(this, onPluginUpdatedParms, p_plugin));
            }
        }

        /// <summary>
        /// Manages on pause behaviour.
        /// </summary>
        protected void OnPause()
        {
            if (steadyIgnoreCallbacks || ignoreCallbacks) return;

            ManageObjects(false);
            if (onPause != null) {
                onPause();
            } else if (onPauseWParms != null) {
                onPauseWParms(new TweenEvent(this, onPauseParms));
            }
        }

        /// <summary>
        /// Manages on resume behaviour (also called when the tween starts).
        /// </summary>
        protected virtual void OnPlay()
        {
            if (steadyIgnoreCallbacks || ignoreCallbacks) return;

            ManageObjects(true);
            if (onPlay != null) {
                onPlay();
            } else if (onPlayWParms != null) {
                onPlayWParms(new TweenEvent(this, onPlayParms));
            }
        }

        /// <summary>
        /// Manages on rewinded behaviour.
        /// </summary>
        protected void OnRewinded()
        {
            if (steadyIgnoreCallbacks || ignoreCallbacks) return;

            if (onRewinded != null) {
                onRewinded();
            } else if (onRewindedWParms != null) {
                onRewindedWParms(new TweenEvent(this, onRewindedParms));
            }
        }

        /// <summary>
        /// Manages step on complete behaviour.
        /// </summary>
        protected void OnStepComplete()
        {
            if (steadyIgnoreCallbacks || ignoreCallbacks) {
                return;
            }
            if (onStepComplete != null) {
                onStepComplete();
            } else if (onStepCompleteWParms != null) {
                onStepCompleteWParms(new TweenEvent(this, onStepCompleteParms));
            }
        }

        /// <summary>
        /// Manages on complete behaviour.
        /// </summary>
        protected void OnComplete()
        {
            _isComplete = true;
            OnStepComplete();
            if (steadyIgnoreCallbacks || ignoreCallbacks) return;

            if (onComplete != null || onCompleteWParms != null) {
                if (HOTween.isUpdateLoop) {
                    // delegate to HOTween which will call OnCompleteDispatch after this tween is eventually destroyed.
                    HOTween.onCompletes.Add(this);
                } else {
                    // Call onComplete immediately
                    OnCompleteDispatch();
                }
            }
        }

        /// <summary>
        /// Called by HOTween if this tween was placed inside its onCompletes list during this.OnComplete().
        /// </summary>
        internal void OnCompleteDispatch()
        {
            if (onComplete != null) {
                onComplete();
            } else if (onCompleteWParms != null) {
                onCompleteWParms(new TweenEvent(this, onCompleteParms));
            }
        }

        /// <summary>
        /// Sets the current <c>fullDuration</c>, based on the current <c>duration</c> and <c>loops</c> values.
        /// Remember to call this method each time you change the duration or loops of a tween.
        /// </summary>
        protected void SetFullDuration()
        {
            _fullDuration = (_loops < 0 ? float.PositiveInfinity : _duration * _loops);
        }

        /// <summary>
        /// Sets the current <c>elapsed</c> time, based on the current <c>fullElapsed</c> and <c>completedLoops</c> values.
        /// Remember to call this method each time you set fullElapsed (after changing the eventual loops count where needed).
        /// </summary>
        protected void SetElapsed()
        {
            if (_duration == 0 || _loops >= 0 && _completedLoops >= _loops) {
                _elapsed = _duration;
            } else if (_fullElapsed < _duration) {
                _elapsed = _fullElapsed;
            } else {
                _elapsed = _fullElapsed % _duration;
            }
        }

        /// <summary>
        /// Sets <c>completedLoops</c> and <c>isLoopingBack</c>, based on the current <c>fullElapsed</c> value.
        /// </summary>
        protected void SetLoops()
        {
            // Take care of floating points imprecision,
            // to avoid things like "2/2 = 0.99999" from happening
            if (_duration == 0) {
                _completedLoops = 1;
            } else {
                float div = _fullElapsed / _duration;
                int ceil = (int)Math.Ceiling(div);
                if (ceil - div < 0.0000001f) {
//                if (ceil - div < 0.000001f) {
                    _completedLoops = ceil;
                } else {
                    _completedLoops = ceil - 1;
                }
            }
            _isLoopingBack = (_loopType != LoopType.Restart && _loopType != LoopType.Incremental &&
                              (_loops > 0 && (_completedLoops < _loops && _completedLoops % 2 != 0 || _completedLoops >= _loops && _completedLoops % 2 == 0)
                               || _loops < 0 && _completedLoops % 2 != 0));
        }

        /// <summary>
        /// Manages the components/gameObjects that should be activated/deactivated.
        /// </summary>
        protected void ManageObjects(bool isPlay)
        {
            // Behaviours/GameObjects management
            if (manageBehaviours) {
                int offLen = 0;
                if (managedBehavioursOn != null) {
                    offLen = managedBehavioursOn.Length;
                    for (int i = 0; i < managedBehavioursOn.Length; ++i) {
                        Behaviour behaviour = managedBehavioursOn[i];
                        if (behaviour == null) continue;
                        if (isPlay) {
                            managedBehavioursOriginalState[i] = behaviour.enabled;
                            behaviour.enabled = true;
                        } else behaviour.enabled = managedBehavioursOriginalState[i];
                    }
                }
                if (managedBehavioursOff != null) {
                    for (int i = 0; i < managedBehavioursOff.Length; ++i) {
                        Behaviour behaviour = managedBehavioursOff[i];
                        if (behaviour == null) continue;
                        if (isPlay) {
                            managedBehavioursOriginalState[offLen + i] = behaviour.enabled;
                            behaviour.enabled = false;
                        } else behaviour.enabled = managedBehavioursOriginalState[i + offLen];
                    }
                }
            }
            if (manageGameObjects) {
                int offLen = 0;
                if (managedGameObjectsOn != null) {
                    offLen = managedGameObjectsOn.Length;
                    for (int i = 0; i < managedGameObjectsOn.Length; ++i) {
                        GameObject go = managedGameObjectsOn[i];
                        if (go == null) continue;
                        if (isPlay) {
                            managedGameObjectsOriginalState[i] = go.active;
                            go.active = true;
                        } else go.active = managedGameObjectsOriginalState[i];
                    }
                }
                if (managedGameObjectsOff != null) {
                    for (int i = 0; i < managedGameObjectsOff.Length; ++i) {
                        GameObject go = managedGameObjectsOff[i];
                        if (go == null) continue;
                        if (isPlay) {
                            managedGameObjectsOriginalState[offLen + i] = go.active;
                            go.active = false;
                        } else go.active = managedGameObjectsOriginalState[i + offLen];
                    }
                }
            }
        }

        // ===================================================================================
        // HELPERS ---------------------------------------------------------------------------

        /// <summary>
        /// Fills the given list with all the plugins inside this sequence tween,
        /// while also looking for them recursively through inner sequences.
        /// Used by <c>HOTween.GetPlugins</c>.
        /// </summary>
        internal abstract void FillPluginsList(List<ABSTweenPlugin> p_plugs);
    }
}
