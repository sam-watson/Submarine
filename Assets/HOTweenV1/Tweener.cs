//
// Tweener.cs
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
using System.Collections.Generic;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using Holoville.HOTween.Plugins.Core;
using UnityEngine;

namespace Holoville.HOTween
{
    /// <summary>
    /// Tween component, created by HOTween for each separate tween.
    /// <para>Author: Daniele Giardini (http://www.holoville.com)</para>
    /// </summary>
    public class Tweener : ABSTweenComponent
    {
        // VARS ///////////////////////////////////////////////////

        float _elapsedDelay;

        internal EaseType _easeType = HOTween.defEaseType;
        internal AnimationCurve _easeAnimationCurve; // Eventual animation curve to use instead of regular ease
        internal float _easeOvershootOrAmplitude = HOTween.defEaseOvershootOrAmplitude;
        internal float _easePeriod = HOTween.defEasePeriod;

        internal bool _pixelPerfect;
        internal bool _speedBased;
        internal float _delay;

        internal float delayCount;

        internal TweenDelegate.TweenCallback onPluginOverwritten;
        internal TweenDelegate.TweenCallbackWParms onPluginOverwrittenWParms;
        internal object[] onPluginOverwrittenParms;

        // REFERENCES /////////////////////////////////////////////

        internal List<ABSTweenPlugin> plugins;
        object _target;

        // SPECIAL VARS ///////////////////////////////////////////

        bool isPartialled;
        EaseType _originalEaseType; // Memorized when partial plugins are created/used
        PlugVector3Path pv3Path; // Reference to eventual plugVector3Path plugin (used for partial paths)

        // GETS/SETS //////////////////////////////////////////////

        /// <summary>
        /// Indicates whether this is a FROM or a TO tween.
        /// </summary>
        public bool isFrom { get; internal set; }

        /// <summary>
        /// Ease type of this tweener
        /// (consider that the plugins you have set might have different ease types).
        /// Setting it will change the ease of all the plugins used by this tweener.
        /// </summary>
        public EaseType easeType {
            get { return _easeType; }
            set {
                _easeType = value;
                // Change ease type of all existing plugins.
                int pluginsCount = plugins.Count;
                for (int i = 0; i < pluginsCount; ++i)
                {
                    plugins[i].SetEase(_easeType);
                }
            }
        }

        /// <summary>
        /// Ease type of this tweener
        /// (consider that the plugins you have set might have different ease types).
        /// Setting it will change the ease of all the plugins used by this tweener.
        /// </summary>
        public AnimationCurve easeAnimationCurve
        {
            get { return _easeAnimationCurve; }
            set
            {
                _easeAnimationCurve = value;
                _easeType = EaseType.AnimationCurve;
                // Change ease type of all existing plugins.
                int pluginsCount = plugins.Count;
                for (int i = 0; i < pluginsCount; ++i) {
                    plugins[i].SetEase(_easeType);
                }
            }
        }

        /// <summary>
        /// Eventual overshoot to use with Back easeTypes.
        /// </summary>
        public float easeOvershootOrAmplitude {
            get { return _easeOvershootOrAmplitude; }
            set { _easeOvershootOrAmplitude = value; }
        }

        /// <summary>
        /// Eventual period to use with Elastic easeTypes.
        /// </summary>
        public float easePeriod
        {
            get { return _easePeriod; }
            set { _easePeriod = value; }
        }

        // READ-ONLY GETS /////////////////////////////////////////

        /// <summary>
        /// Target of this tween.
        /// </summary>
        public object target
        {
            get
            {
                return _target;
            }
        }

        /// <summary>
        /// <c>true</c> if this tween is animated via integers values only.
        /// </summary>
        public bool pixelPerfect
        {
            get
            {
                return _pixelPerfect;
            }
        }

        /// <summary>
        /// <c>true</c> if this tween is animated by speed instead than by duration.
        /// </summary>
        public bool speedBased
        {
            get
            {
                return _speedBased;
            }
        }

        /// <summary>
        /// The delay that was set for this tween.
        /// </summary>
        public float delay
        {
            get
            {
                return _delay;
            }
        }

        /// <summary>
        /// The currently elapsed delay time.
        /// </summary>
        public float elapsedDelay
        {
            get
            {
                return _elapsedDelay;
            }
        }


        // ***********************************************************************************
        // CONSTRUCTOR + PARMS CREATOR
        // ***********************************************************************************

        /// <summary>
        /// Called by HOTween each time a new tween is generated via <c>To</c> or similar methods.
        /// </summary>
        internal Tweener(object p_target, float p_duration, TweenParms p_parms)
        {
            _target = p_target;
            _duration = p_duration;

            p_parms.InitializeObject(this, _target);

            if (plugins != null && plugins.Count > 0) {
                // Valid plugins were added: mark this as not empty anymore.
                _isEmpty = false;
            }

            SetFullDuration();
        }

        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        /// <summary>
        /// Kills this Tweener and cleans it.
        /// </summary>
        /// <param name="p_autoRemoveFromHOTween">
        /// If <c>true</c> also calls <c>HOTween.Kill(this)</c> to remove it from HOTween.
        /// Set internally to <c>false</c> when I already know that HOTween is going to remove it.
        /// </param>
        internal override void Kill(bool p_autoRemoveFromHOTween)
        {
            if (_destroyed) return;

            // Remove tween from OverwriteManager if it was allowed on HOTween's initialization.
            if (HOTween.overwriteManager != null) HOTween.overwriteManager.RemoveTween(this);

            plugins = null;
//            _target = null; // Commented so onComplete can be used to determine the target of a tween

            base.Kill(p_autoRemoveFromHOTween);
        }

        /// <summary>
        /// Resumes this Tweener.
        /// </summary>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        public void Play(bool p_skipDelay)
        {
            if (!_enabled)
            {
                return;
            }
            if (p_skipDelay)
            {
                SkipDelay();
            }
            Play();
        }

        /// <summary>
        /// Resumes this Tweener and plays it forward.
        /// </summary>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        public void PlayForward(bool p_skipDelay)
        {
            if (!_enabled)
            {
                return;
            }
            if (p_skipDelay)
            {
                SkipDelay();
            }
            PlayForward();
        }

        /// <summary>
        /// Rewinds this Tweener (loops and tween delay included), and pauses it.
        /// </summary>
        public override void Rewind()
        {
            Rewind(false);
        }

        /// <summary>
        /// Rewinds this Tweener (loops included), and pauses it.
        /// </summary>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        public void Rewind(bool p_skipDelay)
        {
            Rewind(false, p_skipDelay);
        }

        /// <summary>
        /// Restarts this Tweener from the beginning (loops and tween delay included).
        /// </summary>
        public override void Restart()
        {
            Restart(false);
        }

        /// <summary>
        /// Restarts this Tweener from the beginning (loops and tween delay included).
        /// </summary>
        /// <param name="p_skipDelay">
        /// If <c>true</c> skips any initial delay.
        /// </param>
        public void Restart(bool p_skipDelay)
        {
            if (_fullElapsed == 0)
            {
                PlayForward(p_skipDelay);
            }
            else
            {
                Rewind(true, p_skipDelay);
            }
        }

        /// <summary>
        /// Restarts the tween from position 0, but takes the current target value as start value,
        /// and diffs to find the change value (as if it was a relative tween).
        /// </summary>
        public void RestartIncremental()
        {
            if (plugins == null) return;
            int pluginsCount = plugins.Count;
            for (int i = 0; i < pluginsCount; ++i) {
                plugins[i].ForceSetIncrementalRestart();
            }
            Restart();
        }

        /// <summary>
        /// Completes this Tweener.
        /// Where a loop was involved, the Tweener completes at the position where it would actually be after the set number of loops.
        /// If there were infinite loops, this method will have no effect.
        /// </summary>
        internal override void Complete(bool p_autoRemoveFromHOTween)
        {
            if (!_enabled)
            {
                return;
            }
            if (_loops < 0)
            {
                return;
            }

            _fullElapsed = float.IsPositiveInfinity(_fullDuration) ? _duration : _fullDuration;
            Update(0, true);
            if (_autoKillOnComplete)
            {
                Kill(p_autoRemoveFromHOTween);
            }
        }

        /// <summary>
        /// Completely resets the Tweener except its target,
        /// and applies a new <see cref="TweenType"/>, duration, and <see cref="TweenParms"/>.
        /// </summary>
        /// <param name="p_tweenType">New tween type (to/from)</param>
        /// <param name="p_newDuration">New duration</param>
        /// <param name="p_newParms">New parameters</param>
        public void ResetAndChangeParms(TweenType p_tweenType, float p_newDuration, TweenParms p_newParms)
        {
            if (_destroyed) {
                TweenWarning.Log("ResetAndChangeParms can't run because the tween was destroyed - set AutoKill or autoKillOnComplete to FALSE if you want to avoid destroying a tween after completion");
                return;
            }
            Reset();
            _duration = p_newDuration;
            if (p_tweenType == TweenType.From) p_newParms = p_newParms.IsFrom();

            p_newParms.InitializeObject(this, _target);

            if (plugins != null && plugins.Count > 0) {
                // Valid plugins were added: mark this as not empty anymore.
                _isEmpty = false;
            }

            SetFullDuration();
        }

        /// <summary>
        /// Completely resets this Tweener, except its target
        /// </summary>
        override protected void Reset()
        {
            base.Reset();

            if (HOTween.overwriteManager != null) HOTween.overwriteManager.RemoveTween(this);
            isFrom = false;
            plugins = null;
            isPartialled = false;
            pv3Path = null;
            _delay = _elapsedDelay = delayCount = 0;
            _pixelPerfect = false;
            _speedBased = false;
            _easeType = HOTween.defEaseType;
            _easeOvershootOrAmplitude = HOTween.defEaseOvershootOrAmplitude;
            _easePeriod = HOTween.defEasePeriod;
            _originalEaseType = HOTween.defEaseType;
            onPluginOverwritten = null;
            onStepCompleteWParms = null;
            onPluginOverwrittenParms = null;
        }

        /// <summary>
        /// Assigns the given callback to this Tweener/Sequence,
        /// overwriting any existing callbacks of the same type.
        /// </summary>
        protected override void ApplyCallback(bool p_wParms, CallbackType p_callbackType, TweenDelegate.TweenCallback p_callback, TweenDelegate.TweenCallbackWParms p_callbackWParms, params object[] p_callbackParms)
        {
            switch (p_callbackType) {
                case CallbackType.OnPluginOverwritten:
                    onPluginOverwritten = p_callback;
                    onPluginOverwrittenWParms = p_callbackWParms;
                    onPluginOverwrittenParms = p_callbackParms;
                    break;
                default:
                    base.ApplyCallback(p_wParms, p_callbackType, p_callback, p_callbackWParms, p_callbackParms);
                    break;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the given target and this Tweener target are the same, and the Tweener is running.
        /// Returns <c>false</c> both if the given target is not the same as this Tweener's, than if this Tweener is paused.
        /// This method is here to uniform <see cref="Tweener"/> with <see cref="Sequence"/>.
        /// </summary>
        /// <param name="p_target">
        /// The target to check.
        /// </param>
        public override bool IsTweening(object p_target)
        {
            if (!_enabled) return false;
            if (p_target == _target) return isSequenced ? !contSequence.isPaused : !_isPaused;
            return false;
        }
        /// <summary>
        /// Returns <c>true</c> if the tween with the given string id is currently involved in a running tween or sequence.
        /// This method is here to uniform <see cref="Tweener"/> with <see cref="Sequence"/>.
        /// </summary>
        /// <param name="p_id">
        /// The id to check for.
        /// </param>
        public override bool IsTweening(string p_id)
        {
            if (!_enabled) return false;
            if (id == p_id) return isSequenced ? !contSequence.isPaused : !_isPaused;
            return false;
        }
        /// <summary>
        /// Returns <c>true</c> if the tween with the given int id is currently involved in a running tween or sequence.
        /// This method is here to uniform <see cref="Tweener"/> with <see cref="Sequence"/>.
        /// </summary>
        /// <param name="p_id">
        /// The id to check for.
        /// </param>
        public override bool IsTweening(int p_id)
        {
            if (!_enabled) return false;
            if (intId == p_id) return isSequenced ? !contSequence.isPaused : !_isPaused;
            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the given target and this Tweener target are the same.
        /// This method is here to uniform <see cref="Tweener"/> with <see cref="Sequence"/>.
        /// </summary>
        /// <param name="p_target">
        /// The target to check.
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the given target and this Tweener target are the same.
        /// </returns>
        public override bool IsLinkedTo(object p_target)
        {
            return (p_target == _target);
        }

        /// <summary>
        /// Returns a list containing the target of this tween.
        /// </summary>
        /// <returns>A list containing the target of this tween.</returns>
        public override List<object> GetTweenTargets()
        {
            return new List<object>() {target};
        }

        /// <summary>
        /// Returns a list containing this tween if the id is the same as the given one 
        /// (or and empty list if no tweens were found).
        /// </summary>
        internal override List<IHOTweenComponent> GetTweensById(string p_id)
        {
            List<IHOTweenComponent> res = new List<IHOTweenComponent>();
            if (id == p_id) res.Add(this);
            return res;
        }

        /// <summary>
        /// Returns a list containing this tween if the id is the same as the given one 
        /// (or and empty list if no tweens were found).
        /// </summary>
        internal override List<IHOTweenComponent> GetTweensByIntId(int p_intId)
        {
            List<IHOTweenComponent> res = new List<IHOTweenComponent>();
            if (intId == p_intId) res.Add(this);
            return res;
        }

        // ===================================================================================
        // PLUGINS SPECIFIC METHODS ----------------------------------------------------------

        /// <summary>
        /// If this Tweener contains a <see cref="PlugVector3Path"/> tween,
        /// returns a point on the path at the given percentage (0 to 1).
        /// Returns a <c>zero Vector</c> if there's no path tween associated with this tween.
        /// Note that, if the tween wasn't started, the OnStart callback will be called
        /// the first time you call this method, because the tween needs to be initialized.
        /// </summary>
        /// <param name="t">The percentage (0 to 1) at which to get the point</param>
        public Vector3 GetPointOnPath(float t)
        {
            PlugVector3Path plugVector3Path = GetPlugVector3PathPlugin();
            if (plugVector3Path == null) return Vector3.zero;

            Startup(); // Ensure startup - if not already executed - to store the path data.
            return plugVector3Path.GetConstPointOnPath(t);
        }

        /// <summary>
        /// If this Tweener contains a <see cref="PlugVector3Path"/> tween returns the length of the path,
        /// otherwise returns -1</summary>
        public float GetPathLength()
        {
            PlugVector3Path plugVector3Path = GetPlugVector3PathPlugin();
            if (plugVector3Path == null) return -1;

            Startup(); // Ensure startup - if not already executed - to store the path data.
            return plugVector3Path.path.pathLength;
        }

        /// <summary>
        /// If this Tweener contains a <see cref="PlugVector3Path"/> tween,
        /// defines a portion of that path to use and re-adapt to (easing included),
        /// also re-adapting the duration to the correct partial,
        /// and rewinds/restarts the tween in its partial form (depending if it was paused or not).
        /// </summary>
        /// <param name="p_waypointId0">
        /// Id of the new starting waypoint on the current path.
        /// If you want to be sure you're targeting the first point in the path, pass -1
        /// (this is because the first waypoint of the path might be different from the first waypoint you passed,
        /// in case the target Transform was not already on the starting position, and thus needed to reach it).
        /// </param>
        /// <param name="p_waypointId1">Id of the new ending waypoint on the current path</param>
        public Tweener UsePartialPath(int p_waypointId0, int p_waypointId1)
        {
            EaseType orEaseType = !isPartialled ? _easeType : _originalEaseType;
            return UsePartialPath(p_waypointId0, p_waypointId1, -1, orEaseType);
        }
        /// <summary>
        /// If this Tweener contains a <see cref="PlugVector3Path"/> tween,
        /// defines a portion of that path to use and re-adapt to (easing included),
        /// and rewinds/restarts the tween in its partial form (depending if it was paused or not).
        /// </summary>
        /// <param name="p_waypointId0">
        /// Id of the new starting waypoint on the current path.
        /// If you want to be sure you're targeting the first point in the path, pass -1
        /// (this is because the first waypoint of the path might be different from the first waypoint you passed,
        /// in case the target Transform was not already on the starting position, and thus needed to reach it).
        /// </param>
        /// <param name="p_waypointId1">Id of the new ending waypoint on the current path</param>
        /// <param name="p_newDuration">
        /// Tween duration of the partial path (if -1 auto-calculates the correct partial based on the original duration)
        /// </param>
        public Tweener UsePartialPath(int p_waypointId0, int p_waypointId1, float p_newDuration)
        {
            EaseType orEaseType = isPartialled ? _originalEaseType : _easeType;
            return UsePartialPath(p_waypointId0, p_waypointId1, p_newDuration, orEaseType);
        }
        /// <summary>
        /// If this Tweener contains a <see cref="PlugVector3Path"/> tween,
        /// defines a portion of that path to use and re-adapt to (easing included),
        /// and rewinds/restarts the tween in its partial form (depending if it was paused or not).
        /// </summary>
        /// <param name="p_waypointId0">
        /// Id of the new starting waypoint on the current path.
        /// If you want to be sure you're targeting the first point in the path, pass -1
        /// (this is because the first waypoint of the path might be different from the first waypoint you passed,
        /// in case the target Transform was not already on the starting position, and thus needed to reach it).
        /// </param>
        /// <param name="p_waypointId1">Id of the new ending waypoint on the current path</param>
        /// <param name="p_newEaseType">New EaseType to apply</param>
        public Tweener UsePartialPath(int p_waypointId0, int p_waypointId1, EaseType p_newEaseType)
        {
            return UsePartialPath(p_waypointId0, p_waypointId1, -1, p_newEaseType);
        }
        /// <summary>
        /// If this Tweener contains a <see cref="PlugVector3Path"/> tween,
        /// defines a portion of that path to use and re-adapt to,
        /// and rewinds/restarts the tween in its partial form (depending if it was paused or not).
        /// </summary>
        /// <param name="p_waypointId0">
        /// Id of the new starting waypoint on the current path.
        /// If you want to be sure you're targeting the first point in the path, pass -1
        /// (this is because the first waypoint of the path might be different from the first waypoint you passed,
        /// in case the target Transform was not already on the starting position, and thus needed to reach it).
        /// </param>
        /// <param name="p_waypointId1">
        /// Id of the new ending waypoint on the current path 
        /// (-1 in case you want to target the ending waypoint of a closed path)
        /// </param>
        /// <param name="p_newDuration">
        /// Tween duration of the partial path (if -1 auto-calculates the correct partial based on the original duration)
        /// </param>
        /// <param name="p_newEaseType">New EaseType to apply</param>
        public Tweener UsePartialPath(int p_waypointId0, int p_waypointId1, float p_newDuration, EaseType p_newEaseType)
        {
            if (plugins.Count > 1) {
                TweenWarning.Log("Applying a partial path on a Tweener (" + _target + ") with more than one plugin/property being tweened is not allowed");
                return this;
            }

            if (pv3Path == null) {
                // Get eventual plugVector3Path plugin
                pv3Path = GetPlugVector3PathPlugin();
                if (pv3Path == null) {
                    TweenWarning.Log("Tweener for " + _target + " contains no PlugVector3Path plugin");
                    return this;
                }
            }

            // Startup the tween (if not already started) to store the path data.
            Startup();
            // Store original duration and easeType (if not already stored).
            if (!isPartialled) {
                isPartialled = true;
                _originalDuration = _duration;
                _originalEaseType = _easeType;
            }
            // Convert waypoints ids to path ids
            int pathWaypointId0 = ConvertWaypointIdToPathId(pv3Path, p_waypointId0, true);
            int pathWaypointId1 = ConvertWaypointIdToPathId(pv3Path, p_waypointId1, false);
            // Get waypoints length percentage (needed for auto-duration and calculation of lookAhed)
            float partialPerc = pv3Path.GetWaypointsLengthPercentage(pathWaypointId0, pathWaypointId1);
            float partialStartPerc = pathWaypointId0 == 0 ? 0 : pv3Path.GetWaypointsLengthPercentage(0, pathWaypointId0);
            // Assign new duration and ease
            _duration = p_newDuration >= 0 ? p_newDuration : _originalDuration * partialPerc;
            _easeType = p_newEaseType;

            // Convert path to partial
            pv3Path.SwitchToPartialPath(_duration, p_newEaseType, partialStartPerc, partialPerc);

            // Re-Startup and restart
            Startup(true);
            if (!_isPaused)
                Restart(true);
            else {
                Rewind(true, true);
            }

            return this; // Returns this so it can be directly used with WaitForCompletion coroutines
        }

        /// <summary>
        /// If this Tweener contains a <see cref="PlugVector3Path"/> tween
        /// that had been partialized, returns it to its original size, easing, and duration,
        /// and rewinds/restarts the tween in its partial form (depending if it was paused or not).
        /// </summary>
        public void ResetPath()
        {
            // Reset original values
            isPartialled = false;
            _duration = speedBased ? _originalNonSpeedBasedDuration : _originalDuration;
            _easeType = _originalEaseType;
            pv3Path.ResetToFullPath(_duration, _easeType);
            // Re-startup and restart
            Startup(true);
            if (!_isPaused)
                Restart(true);
            else {
                Rewind(true);
            }
        }

        /// <summary>
        /// If this Tweener contains a <see cref="PlugVector3Path"/>, returns it.
        /// Otherwise returns null.
        /// </summary>
        /// <returns></returns>
        private PlugVector3Path GetPlugVector3PathPlugin()
        {
            if (plugins == null) return null;
            int pluginsCount = plugins.Count;
            for (int i = 0; i < pluginsCount; ++i) {
                ABSTweenPlugin plug = plugins[i];
                PlugVector3Path plugVector3Path = plug as PlugVector3Path;
                if (plugVector3Path != null) return plugVector3Path;
            }
            return null;
        }

        // ===================================================================================
        // INTERNAL METHODS ------------------------------------------------------------------

        internal override bool Update(float p_shortElapsed, bool p_forceUpdate, bool p_isStartupIteration, bool p_ignoreCallbacks)
        {
            return Update(p_shortElapsed, p_forceUpdate, p_isStartupIteration, p_ignoreCallbacks, false);
        }
        /// <summary>
        /// Updates the Tweener by the given elapsed time,
        /// and returns a value of <c>true</c> if the Tweener is complete.
        /// </summary>
        /// <param name="p_shortElapsed">
        /// The elapsed time since the last update.
        /// </param>
        /// <param name="p_forceUpdate">
        /// If <c>true</c> forces the update even if the Tweener is complete or paused,
        /// but ignores onUpdate, and sends onComplete and onStepComplete calls only if the Tweener wasn't complete before this call.
        /// </param>
        /// <param name="p_isStartupIteration">
        /// If <c>true</c> means the update is due to a startup iteration (managed by Sequence Startup or HOTween.From),
        /// and all callbacks will be ignored.
        /// </param>
        /// <param name="p_ignoreCallbacks">
        /// If <c>true</c> doesn't call any callback method.
        /// </param>
        /// <param name="p_ignoreDelay">
        /// If <c>true</c> uses p_shortElapsed fully ignoring the delay
        /// (useful when setting the initial FROM state).
        /// </param>
        /// <returns>
        /// A value of <c>true</c> if the Tweener is not reversed and is complete (or the tween target doesn't exist anymore), otherwise <c>false</c>.
        /// </returns>
        internal bool Update(float p_shortElapsed, bool p_forceUpdate, bool p_isStartupIteration, bool p_ignoreCallbacks, bool p_ignoreDelay = false)
        {
            if (_destroyed)
            {
                return true;
            }
            if (_target == null || _target.Equals(null))
            {
                Kill(false);
                return true;
            }
            if (!_enabled)
            {
                return false;
            }
            if (_isComplete && !_isReversed && !p_forceUpdate)
            {
                return true;
            }
            if (_fullElapsed == 0 && _isReversed && !p_forceUpdate)
            {
                return false;
            }
            if (_isPaused && !p_forceUpdate)
            {
                return false;
            }

            ignoreCallbacks = p_isStartupIteration || p_ignoreCallbacks;

            if (p_ignoreDelay || delayCount == 0)
            {
                Startup();
                if (!_hasStarted)
                {
                    OnStart();
                }
                if (!_isReversed)
                {
                    _fullElapsed += p_shortElapsed;
                    _elapsed += p_shortElapsed;
                }
                else
                {
                    _fullElapsed -= p_shortElapsed;
                    _elapsed -= p_shortElapsed;
                }
                if (_fullElapsed > _fullDuration)
                {
                    _fullElapsed = _fullDuration;
                }
                else if (_fullElapsed < 0)
                {
                    _fullElapsed = 0;
                }
            }
            else
            {
                // Manage delay (delay doesn't go backwards).
                if (_timeScale != 0)
                {
                    _elapsedDelay += p_shortElapsed/_timeScale; // Calculate delay independently of timeScale
                }
                if (_elapsedDelay < delayCount)
                {
                    return false;
                }
                if (_isReversed)
                {
                    _fullElapsed = _elapsed = 0;
                }
                else
                {
                    _fullElapsed = _elapsed = _elapsedDelay - delayCount;
                    if (_fullElapsed > _fullDuration)
                    {
                        _fullElapsed = _fullDuration;
                    }
                }
                _elapsedDelay = delayCount;
                delayCount = 0;
                Startup();
                if (!_hasStarted)
                {
                    OnStart();
                }
            }

            // Set all elapsed and loops values.
            bool wasComplete = _isComplete;
            bool stepComplete = (!_isReversed && !wasComplete && _elapsed >= _duration);
            SetLoops();
            SetElapsed();
            _isComplete = (!_isReversed && _loops >= 0 && _completedLoops >= _loops);
            bool complete = (!wasComplete && _isComplete);

            // Update the plugins.
            float plugElapsed = (!_isLoopingBack ? _elapsed : _duration - _elapsed);
            int pluginsCount = plugins.Count;
            for (int i = 0; i < pluginsCount; ++i)
            {
                ABSTweenPlugin plug = plugins[i];
                if (!_isLoopingBack && plug.easeReversed || _isLoopingBack && _loopType == LoopType.YoyoInverse && !plug.easeReversed)
                {
                    plug.ReverseEase();
                }
                if (_duration > 0) {
                    plug.Update(plugElapsed);
                    OnPluginUpdated(plug);  // do callback
                }
                else {
                    // 0 duration tweens
                    OnPluginUpdated(plug);  // do callback
                    plug.Complete();
                    if (!wasComplete) complete = true;
                }
            }

            // Manage eventual pause, complete, update, rewinded, and stepComplete.
            if (_fullElapsed != prevFullElapsed)
            {
                OnUpdate();
                if (_fullElapsed == 0)
                {
                    if (!_isPaused) {
                        _isPaused = true;
                        OnPause();
                    }
                    OnRewinded();
                }
            }
            if (complete) {
                if (!_isPaused) {
                    _isPaused = true;
                    OnPause();
                }
                OnComplete();
            }
            else if (stepComplete)
            {
                OnStepComplete();
            }

            ignoreCallbacks = false;
            prevFullElapsed = _fullElapsed;

            return complete;
        }

        /// <summary>
        /// Sets the correct values in case of Incremental loop type.
        /// Also called by Tweener.ApplySequenceIncrement (used by Sequences during Incremental loops).
        /// </summary>
        /// <param name="p_diffIncr">
        /// The difference from the previous loop increment.
        /// </param>
        internal override void SetIncremental(int p_diffIncr)
        {
            if (plugins == null) return;
            int pluginsCount = plugins.Count;
            for (int i = 0; i < pluginsCount; ++i) {
                plugins[i].ForceSetIncremental(p_diffIncr);
            }
        }

        /// <summary>
        /// If speed based duration was not already set (meaning OnStart has not yet been called),
        /// calculates the duration and then resets the tween so that OnStart can be called from scratch.
        /// Used by Sequences when Appending/Prepending/Inserting speed based tweens.
        /// </summary>
        internal void ForceSetSpeedBasedDuration()
        {
            if (!_speedBased || plugins == null) return;

            int pluginsCount = plugins.Count;
            for (int i = 0; i < pluginsCount; ++i) {
                plugins[i].ForceSetSpeedBasedDuration();
            }

            _duration = 0;
            for (int i = 0; i < pluginsCount; ++i) {
                ABSTweenPlugin plug = plugins[i];
                if (plug.duration > _duration) _duration = plug.duration;
            }
            SetFullDuration();
        }

        // ===================================================================================
        // PRIVATE METHODS -------------------------------------------------------------------

        /// <summary>
        /// Sends the tween to the given time (taking also loops into account) and eventually plays it.
        /// If the time is bigger than the total tween duration, it goes to the end.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the tween reached its end and was completed.
        /// </returns>
        protected override bool GoTo(float p_time, bool p_play, bool p_forceUpdate, bool p_ignoreCallbacks)
        {
            if (!_enabled) return false;

            if (p_time > _fullDuration) {
                p_time = _fullDuration;
            } else if (p_time < 0) {
                p_time = 0;
            }
            if (!p_forceUpdate && _fullElapsed == p_time) {
                if (!_isComplete && p_play) Play();
                return _isComplete;
            }

            _fullElapsed = p_time;
            delayCount = 0;
            _elapsedDelay = _delay;
            Update(0, true, false, p_ignoreCallbacks);
            if (!_isComplete && p_play) {
                Play();
            }

            return _isComplete;
        }

        void Rewind(bool p_play, bool p_skipDelay)
        {
            if (!_enabled)
            {
                return;
            }

            Startup();
            if (!_hasStarted)
            {
                OnStart();
            }

            _isComplete = false;
            _isLoopingBack = false;
            delayCount = (p_skipDelay ? 0 : _delay);
            _elapsedDelay = (p_skipDelay ? _delay : 0);
            _completedLoops = 0;
            _fullElapsed = _elapsed = 0;

            int pluginsCount = plugins.Count;
            for (int i = 0; i < pluginsCount; ++i)
            {
                ABSTweenPlugin plug = plugins[i];
                if (plug.easeReversed)
                {
                    plug.ReverseEase();
                }
                plug.Rewind();
            }

            // Manage OnUpdate and OnRewinded.
            if (_fullElapsed != prevFullElapsed)
            {
                OnUpdate();
                if (_fullElapsed == 0)
                {
                    OnRewinded();
                }
            }
            prevFullElapsed = _fullElapsed;

            if (p_play)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        void SkipDelay()
        {
            if (delayCount > 0)
            {
                delayCount = 0;
                _elapsedDelay = _delay;
                _elapsed = _fullElapsed = 0;
            }
        }

        /// <summary>
        /// Startup this tween
        /// (might or might not call OnStart, depending if the tween is in a Sequence or not).
        /// Can be executed only once per tween.
        /// </summary>
        protected override void Startup() { Startup(false); }
        /// <summary>
        /// Startup this tween
        /// (might or might not call OnStart, depending if the tween is in a Sequence or not).
        /// Can be executed only once per tween.
        /// </summary>
        /// <param name="p_force">If TRUE forces startup even if it had already been executed</param>
        void Startup(bool p_force)
        {
            if (!p_force && startupDone) return;

            int pluginsCount = plugins.Count;
            for (int i = 0; i < pluginsCount; ++i) {
                ABSTweenPlugin plug = plugins[i];
                if (!plug.wasStarted) plug.Startup();
            }
            if (_speedBased) {
                // Reset duration based on value changes and speed.
                // Can't be done sooner because it needs to startup the plugins first.
                _originalNonSpeedBasedDuration = _duration;
                _duration = 0;
                for (int i = 0; i < pluginsCount; ++i) {
                    ABSTweenPlugin plug = plugins[i];
                    if (plug.duration > _duration) _duration = plug.duration;
                }
                SetFullDuration();
            } else if (p_force) {
                SetFullDuration(); // Reset full duration
            }

            base.Startup();
        }

        /// <summary>
        /// Manages on first start behaviour.
        /// </summary>
        protected override void OnStart()
        {
            if (steadyIgnoreCallbacks || ignoreCallbacks)
            {
                return;
            }

            // Add tween to OverwriteManager if it was allowed on HOTween's initialization.
            if (HOTween.overwriteManager != null)
            {
                HOTween.overwriteManager.AddTween(this);
            }

            base.OnStart();
        }

        /// <summary>
        /// Manages on play behaviour
        /// </summary>
        protected override void OnPlay()
        {
            if (_delay > 0 && _elapsedDelay <= 0) Update(0, true, true, false, true);
        }

        // ===================================================================================
        // HELPERS ---------------------------------------------------------------------------

        /// <summary>
        /// Fills the given list with all the plugins inside this tween.
        /// Used by <c>HOTween.GetPlugins</c>.
        /// </summary>
        internal override void FillPluginsList(List<ABSTweenPlugin> p_plugs)
        {
            if (plugins == null)
            {
                return;
            }

            int pluginsCount = plugins.Count;
            for (int i = 0; i < pluginsCount; ++i)
            {
                p_plugs.Add(plugins[i]);
            }
        }

        /// <summary>
        /// Returns the correct id of the given waypoint, converted to path id.
        /// </summary>
        /// <param name="p_plugVector3Path">Vector3 path plugin to use</param>
        /// <param name="p_waypointId">Waypoint to convert</param>
        /// <param name="p_isStartingWp">If TRUE indicates that the given waypoint is the starting one,
        /// otherwise it's the ending one</param>
        /// <returns></returns>
        static int ConvertWaypointIdToPathId(PlugVector3Path p_plugVector3Path, int p_waypointId, bool p_isStartingWp)
        {
            if (p_waypointId == -1) return p_isStartingWp ? 1 : p_plugVector3Path.path.path.Length - 2;
            if (p_plugVector3Path.hasAdditionalStartingP) return p_waypointId + 2;
            return p_waypointId + 1;
        }
    }
}
