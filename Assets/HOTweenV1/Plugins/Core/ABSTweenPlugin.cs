//
// ABSTweenPlugin.cs
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
using System.Reflection;
#if !MICRO
using FastDynamicMemberAccessor;
#endif
using Holoville.HOTween.Core;
using Holoville.HOTween.Core.Easing;
using UnityEngine;

namespace Holoville.HOTween.Plugins.Core
{
    /// <summary>
    /// ABSTRACT base class for all <see cref="ABSTweenPlugin"/> classes.
    /// </summary>
    public abstract class ABSTweenPlugin
    {
        // VARS ///////////////////////////////////////////////////

        /// <summary>
        /// Untyped start value.
        /// </summary>
        protected object _startVal;

        /// <summary>
        /// Untyped end value.
        /// </summary>
        protected object _endVal;

        /// <summary>
        /// Stored so it can be set indipendently in case of speed-based tweens.
        /// </summary>
        protected float _duration;

        bool _initialized;
        bool _easeReversed;

        /// <summary>
        /// Name of the property being tweened. Stored during Init, used by overwrite manager and log messages.
        /// </summary>
        protected string _propName;

        /// <summary>
        /// Stored to be used during recreation of plugin for partial tweens.
        /// </summary>
        internal Type targetType;

        /// <summary>
        /// Ease type.
        /// </summary>
        protected TweenDelegate.EaseFunc ease;

        /// <summary>
        /// Indicates that the end value is relative instead than absolute.
        /// Default: <c>false</c>.
        /// </summary>
        protected bool isRelative;

        /// <summary>
        /// Some plugins (like PlugSetColor) may set this to <c>false</c> when instantiated,
        /// to prevent the creation of a useless valAccessor.
        /// </summary>
        protected bool ignoreAccessor;

        EaseType easeType; // Stored so instance can be cloned and ease can be changed while playing.
        EaseInfo easeInfo;
        EaseCurve easeCurve; // Used when assigning an AnimationCurve to the ease
#if !MICRO
        IMemberAccessor valAccessor;
#endif
        internal bool wasStarted;
        bool speedBasedDurationWasSet;
        int prevCompletedLoops; // Stored only during Incremental loop type.

        // SPECIFIC GETTERS/SETTERS ///////////////////////////////
        // Used in case a specific type of accessor can be used to speed things up

        bool _useSpeedTransformAccessors;
        Transform _transformTarget;
        TweenDelegate.HOAction<Vector3> _setTransformVector3;
        TweenDelegate.HOFunc<Vector3> _getTransformVector3;
        TweenDelegate.HOAction<Quaternion> _setTransformQuaternion;
        TweenDelegate.HOFunc<Quaternion> _getTransformQuaternion; 

        // IOS-ONLY VARS //////////////////////////////////////////

        internal PropertyInfo propInfo;
        internal FieldInfo fieldInfo;

        // REFERENCES /////////////////////////////////////////////

        /// <summary>
        /// Reference to the Tweener controlling this plugin.
        /// </summary>
        protected Tweener tweenObj;

        // GETS/SETS //////////////////////////////////////////////

        /// <summary>
        /// Gets the untyped start value,
        /// sets both the untyped and the typed start value.
        /// </summary>
        protected abstract object startVal { get; set; }

        /// <summary>
        /// Gets the untyped end value,
        /// sets both the untyped and the typed end value.
        /// </summary>
        protected abstract object endVal { get; set; }

        // READ-ONLY GETS /////////////////////////////////////////

        /// <summary>
        /// Used by TweenParms to understand if this plugin was initialized with
        /// another Tweener, and thus clone it.
        /// </summary>
        internal bool initialized
        {
            get
            {
                return _initialized;
            }
        }

        internal float duration
        {
            get
            {
                return _duration;
            }
        }

        internal bool easeReversed
        {
            get
            {
                return _easeReversed;
            }
        }

        /// <summary>
        /// Used by <see cref="OverwriteManager"/> to get the property name.
        /// </summary>
        internal string propName { get { return _propName; } }

        /// <summary>
        /// Some plugins might override this to specify a different ID (like PlugVector3X).
        /// Used by <see cref="OverwriteManager"/> to check if two plugins are the same (for overwrite purposes).
        /// Plugins with -1 ids always overwrite and are overwritten.
        /// Plugins with different ids are always overwritten by plugins with -1 ids,
        /// but overwrite only identical ids.
        /// </summary>
        /// <value>
        /// The plugin identifier.
        /// </value>
        internal virtual int pluginId
        {
            get
            {
                return -1;
            }
        }


        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        /// <summary>
        /// Creates a new instance of this plugin with the given options.
        /// Used because easeType can't be null, and otherwise there's no way
        /// to understand if the ease was voluntarily set by the user or not.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="object"/> value to tween to.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        protected ABSTweenPlugin(object p_endVal, bool p_isRelative)
        {
            isRelative = p_isRelative;
            _endVal = p_endVal;
        }
        /// <summary>
        /// Creates a new instance of this plugin with the given options.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="object"/> value to tween to.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        protected ABSTweenPlugin(object p_endVal, EaseType p_easeType, bool p_isRelative)
        {
            isRelative = p_isRelative;
            _endVal = p_endVal;
            easeType = p_easeType;
            easeInfo = EaseInfo.GetEaseInfo(p_easeType);
            ease = easeInfo.ease;
        }
        /// <summary>
        /// Creates a new instance of this plugin with the given options.
        /// </summary>
        /// <param name="p_endVal">
        /// The <see cref="object"/> value to tween to.
        /// </param>
        /// <param name="p_easeAnimCurve">
        /// The <see cref="AnimationCurve"/> to use for easing.
        /// </param>
        /// <param name="p_isRelative">
        /// If <c>true</c>, the given end value is considered relative instead than absolute.
        /// </param>
        protected ABSTweenPlugin(object p_endVal, AnimationCurve p_easeAnimCurve, bool p_isRelative)
        {
            isRelative = p_isRelative;
            _endVal = p_endVal;
            easeType = EaseType.AnimationCurve;
            easeCurve = new EaseCurve(p_easeAnimCurve);
            easeInfo = null;
            ease = easeCurve.Evaluate;
        }

        // ===================================================================================
        // INTERNAL METHODS ------------------------------------------------------------------

        /// <summary>
        /// Initializes the plugin after its instantiation.
        /// Called by Tweener after a property and plugin have been validated, and the plugin has to be set and added.
        /// Virtual because some classes (like PlugVector3Path) override it to avoid isRelative being TRUE.
        /// </summary>
        /// <param name="p_tweenObj">
        /// The <see cref="Tweener"/> to refer to.
        /// </param>
        /// <param name="p_propertyName">
        /// The name of the property to control.
        /// </param>
        /// <param name="p_easeType">
        /// The <see cref="EaseType"/> to use.
        /// </param>
        /// <param name="p_targetType">
        /// Directly passed from TweenParms to speed up MemberAccessor creation.
        /// </param>
        /// <param name="p_propertyInfo">
        /// Directly passed from TweenParms to speed up MemberAccessor creation.
        /// </param>
        /// <param name="p_fieldInfo">
        /// Directly passed from TweenParms to speed up MemberAccessor creation.
        /// </param>
        internal virtual void Init(Tweener p_tweenObj, string p_propertyName, EaseType p_easeType, Type p_targetType, PropertyInfo p_propertyInfo, FieldInfo p_fieldInfo)
        {
            _initialized = true;

            tweenObj = p_tweenObj;
            _propName = p_propertyName;
            targetType = p_targetType;
            if (easeType != EaseType.AnimationCurve && easeInfo == null || tweenObj.speedBased || easeType == EaseType.AnimationCurve && easeCurve == null)
            {
                SetEase(p_easeType);
            }
            _duration = tweenObj.duration;

            if (targetType == typeof(Transform)) {
                // Use specific transform accessors for faster performance
                _transformTarget = p_tweenObj.target as Transform;
                _useSpeedTransformAccessors = true;
                switch (_propName) {
                case "position":
                    _setTransformVector3 = value => _transformTarget.position = value;
                    _getTransformVector3 = () => _transformTarget.position;
                    break;
                case "localPosition":
                    _setTransformVector3 = value => _transformTarget.localPosition = value;
                    _getTransformVector3 = () => _transformTarget.localPosition;
                    break;
                case "localScale":
                    _setTransformVector3 = value => _transformTarget.localScale = value;
                    _getTransformVector3 = () => _transformTarget.localScale;
                    break;
                case "rotation":
                    _setTransformQuaternion = value => _transformTarget.rotation = value;
                    _getTransformQuaternion = () => _transformTarget.rotation;
                    break;
                case "localRotation":
                    _setTransformQuaternion = value => _transformTarget.localRotation = value;
                    _getTransformQuaternion = () => _transformTarget.localRotation;
                    break;
                default:
                    _transformTarget = null; // No valid speed property found
                    _useSpeedTransformAccessors = false;
                    break;
                }
            }
            if (!_useSpeedTransformAccessors) {
#if MICRO
                propInfo = p_propertyInfo;
                fieldInfo = p_fieldInfo;
#else
                if (HOTween.isIOS) {
                    propInfo = p_propertyInfo;
                    fieldInfo = p_fieldInfo;
                } else {
                    if (!ignoreAccessor) {
                        valAccessor = MemberAccessorCacher.Make(p_targetType, p_propertyName, p_propertyInfo, p_fieldInfo);
                    }
                }
#endif
            }
        }

        /// <summary>
        /// Starts up the plugin, getting the actual start and change values.
        /// Called by Tweener right before starting the effective animations.
        /// </summary>
        internal void Startup() { Startup(false); }
        /// <summary>
        /// Starts up the plugin, getting the actual start and change values.
        /// Called by Tweener right before starting the effective animations.
        /// </summary>
        /// <param name="p_onlyCalcSpeedBasedDur">
        /// Set to <c>true</c> by <see cref="ForceSetSpeedBasedDuration"/>,
        /// to calculate only the speed based duration and then reset any startup changes
        /// (so Startup can be called from scratch when truly starting up).
        /// </param>
        internal void Startup(bool p_onlyCalcSpeedBasedDur)
        {
            if (wasStarted)
            {
                TweenWarning.Log("Startup() for plugin " + this + " (target: " + tweenObj.target + ") has already been called. Startup() won't execute twice.");
                return; // Startup can't be executed twice otherwise some typedEndVal (like for PlugColor) will be set incorrectly
            }

            object orStartVal = null, orEndVal = null;
            if (p_onlyCalcSpeedBasedDur)
            {
                if (tweenObj.speedBased && !speedBasedDurationWasSet)
                {
                    // Get original values so they can be reset.
                    orStartVal = _startVal;
                    orEndVal = _endVal;
                }
            }
            else
            {
                wasStarted = true;
            }

            // Manage TO or FROM.
            if (tweenObj.isFrom)
            {
                // Order is fundamental (otherwise setters for isRelative get messed up).
                object prevEndVal = _endVal;
                endVal = GetValue();
                startVal = prevEndVal;
            }
            else
            {
                endVal = _endVal;
                startVal = GetValue();
            }
            // Set changeVal.
            SetChangeVal();

            if (tweenObj.speedBased && !speedBasedDurationWasSet)
            {
                // Get duration based on speed.
                // Can't be done earlier because it needs changeVal to be set.
                _duration = GetSpeedBasedDuration(_duration);
                speedBasedDurationWasSet = true;
                // Reset.
                if (p_onlyCalcSpeedBasedDur)
                {
                    _startVal = orStartVal;
                    _endVal = orEndVal;
                }
            }
        }

        /// <summary>
        /// If speed based duration was not already set (meaning Startup has not yet been called),
        /// calculates the duration and then resets the plugin so that Startup will restart from scratch.
        /// Used by <see cref="Tweener.ForceSetSpeedBasedDuration"/>.
        /// </summary>
        internal void ForceSetSpeedBasedDuration()
        {
            if (speedBasedDurationWasSet)
            {
                return;
            }
            Startup(true);
        }

        /// <summary>
        /// Overridden by plugins that need a specific type of target, to check it and validate it.
        /// Returns <c>true</c> if the tween target is valid.
        /// </summary>
        internal virtual bool ValidateTarget(object p_target)
        {
            return true;
        }

        /// <summary>
        /// Updates the tween.
        /// </summary>
        /// <param name="p_totElapsed">
        /// The total elapsed time since startup (loops excluded).
        /// </param>
        internal void Update(float p_totElapsed)
        {
            if (tweenObj.loopType == LoopType.Incremental)
            {
                // prevCompleteLoops is stored only during Incremental loops,
                // so that if the loop type is changed while the tween is running,
                // the tween will change and update correctly.
                if (prevCompletedLoops != tweenObj.completedLoops)
                {
                    int currLoops = tweenObj.completedLoops;
                    if (tweenObj._loops != -1 && currLoops >= tweenObj._loops)
                    {
                        --currLoops; // Avoid to calculate completion loop increment
                    }
                    int diff = currLoops - prevCompletedLoops;
                    if (diff != 0)
                    {
                        SetIncremental(diff);
                        prevCompletedLoops = currLoops;
                    }
                }
            }
            else if (prevCompletedLoops != 0)
            {
                // Readapt to non incremental loop type.
                SetIncremental(-prevCompletedLoops);
                prevCompletedLoops = 0;
            }

            if (p_totElapsed > _duration)
            {
                p_totElapsed = _duration;
            }

            DoUpdate(p_totElapsed);
        }

        /// <summary>
        /// Updates the plugin.
        /// </summary>
        protected abstract void DoUpdate(float p_totElapsed);

        /// <summary>
        /// Rewinds the tween.
        /// Should be overriden by tweens that control only part of the property (like HOTPluginVector3X).
        /// </summary>
        internal virtual void Rewind()
        {
            SetValue(startVal);
        }

        /// <summary>
        /// Completes the tween.
        /// Should be overriden by tweens that control only part of the property (like HOTPluginVector3X).
        /// </summary>
        internal virtual void Complete()
        {
            SetValue(_endVal);
        }

        /// <summary>
        /// Reverses the ease of this plugin.
        /// </summary>
        internal void ReverseEase()
        {
            _easeReversed = !_easeReversed;
            if (easeType == EaseType.AnimationCurve) return; // No reverse ease allowed
            if (easeInfo.inverseEase == null) return; // No inverse for this ease.

            ease = (_easeReversed ? easeInfo.inverseEase : easeInfo.ease);
        }

        /// <summary>
        /// Sets the ease type (called during Init, but can also be called by Tweener to change easeType while playing).
        /// </summary>
        internal void SetEase(EaseType p_easeType)
        {
            easeType = p_easeType;
            if (easeType == EaseType.AnimationCurve) {
                if (tweenObj._easeAnimationCurve != null) {
                    easeCurve = new EaseCurve(tweenObj._easeAnimationCurve);
                    ease = easeCurve.Evaluate;
                } else {
                    // Missing animation curve: set to normal ease
                    easeType = EaseType.EaseOutQuad;
                    easeInfo = EaseInfo.GetEaseInfo(easeType);
                    ease = easeInfo.ease;
                }
            } else {
                easeInfo = EaseInfo.GetEaseInfo(easeType);
                ease = easeInfo.ease;
            }

            if (_easeReversed && easeInfo.inverseEase != null)
            {
                ease = easeInfo.inverseEase;
            }
        }

        /// <summary>
        /// Returns the speed-based duration based on the given speed.
        /// </summary>
        protected abstract float GetSpeedBasedDuration(float p_speed);

        /// <summary>
        /// Returns a clone of the basic plugin
        /// (as it was at construction, without anything that was set during Init).
        /// </summary>
        internal ABSTweenPlugin CloneBasic()
        {
            // OPTIMIZE incredibly slow. Possible solutions...
            // - http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects (but requires Linq, and thus System.Core, which I'd prefer to avoid)
            // - http://ayende.com/blog/3167/creating-objects-perf-implications (has to know the class to create, thus is useless)
            return Activator.CreateInstance(GetType(), (tweenObj != null && tweenObj.isFrom ? _startVal : _endVal), easeType, isRelative) as ABSTweenPlugin;
        }

        // ===================================================================================
        // PRIVATE METHODS -------------------------------------------------------------------

        /// <summary>
        /// Sets the typed changeVal based on the current startVal and endVal.
        /// Can only be called once, otherwise some typedEndVal (like HOTPluginColor) will be set incorrectly.
        /// </summary>
        protected abstract void SetChangeVal();

        /// <summary>
        /// Used by Tweeners to force SetIncremental
        /// (SetIncremental can't be made internal since
        /// it needs to be overridden outside of HOTweem for custom plugin).
        /// </summary>
        internal void ForceSetIncremental(int p_diffIncr)
        { SetIncremental(p_diffIncr); }
        /// <summary>
        /// Sets the correct values in case of Incremental loop type.
        /// Also called by Tweener.ApplySequenceIncrement (used by Sequences during Incremental loops).
        /// </summary>
        /// <param name="p_diffIncr">
        /// The difference from the previous loop increment.
        /// </param>
        protected abstract void SetIncremental(int p_diffIncr);

        internal void ForceSetIncrementalRestart()
        { SetIncrementalRestart(); }
        /// <summary>
        /// Sets the correct values in case of RestartIncremental.
        /// </summary>
        protected abstract void SetIncrementalRestart();

        /// <summary>
        /// Sets the value of the controlled property.
        /// Some plugins (like PlugSetColor or PlugQuaterion) might override this to get values from different properties.
        /// </summary>
        /// <param name="p_value">
        /// The new value.
        /// </param>
        protected virtual void SetValue(Vector3 p_value)
        {
            if (_useSpeedTransformAccessors) {
                // Use specific accessors
                if (_setTransformVector3 != null) _setTransformVector3(p_value);
            } else SetUndefinedValue(p_value);
        }
        protected virtual void SetValue(Quaternion p_value)
        {
            if (_useSpeedTransformAccessors) {
                // Use specific accessors
                if (_setTransformQuaternion != null) _setTransformQuaternion(p_value);
            } else SetUndefinedValue(p_value);
        }
        protected virtual void SetValue(object p_value)
        {
            SetUndefinedValue(p_value);
        }
        protected virtual void SetValue(Vector2 p_value)
        {
            SetUndefinedValue(p_value);
        }
        protected virtual void SetValue(Vector4 p_value)
        {
            SetUndefinedValue(p_value);
        }
        protected virtual void SetValue(Color p_value)
        {
            SetUndefinedValue(p_value);
        }
        protected virtual void SetValue(Color32 p_value)
        {
            SetUndefinedValue(p_value);
        }

        void SetUndefinedValue(object p_value)
        {
            if (_useSpeedTransformAccessors) {
                // Use specific accessors
                // Required for internal functions that use SetValue with untyped _startVal variable
                if (_setTransformVector3 != null) _setTransformVector3((Vector3)p_value);
                else _setTransformQuaternion((Quaternion)p_value);
            } else {
#if MICRO
                if (propInfo != null)
                {
                    try
                    {
                        propInfo.SetValue(tweenObj.target, p_value, null);
                    }
                    catch (InvalidCastException)
                    {
                        // This happens only if a float is being assigned to an int.
                        propInfo.SetValue(tweenObj.target, (int)Math.Floor((double)(float)p_value), null);
                    }
                    catch (ArgumentException)
                    {
                        // This happens only on iOS if a float is being assigned to an int.
                        propInfo.SetValue(tweenObj.target, (int)Math.Floor((double)(float)p_value), null);
                    }
                }
                else
                {
                    try
                    {
                        fieldInfo.SetValue(tweenObj.target, p_value);
                    }
                    catch (InvalidCastException)
                    {
                        // This happens only if a float is being assigned to an int.
                        fieldInfo.SetValue(tweenObj.target, (int)Math.Floor((double)(float)p_value));
                    }
                    catch (ArgumentException)
                    {
                        // This happens only on iOS if a float is being assigned to an int.
                        fieldInfo.SetValue(tweenObj.target, (int)Math.Floor((double)(float)p_value));
                    }
                }
#else
                if (HOTween.isIOS) {
                    if (propInfo != null) {
                        try {
                            propInfo.SetValue(tweenObj.target, p_value, null);
                        } catch (InvalidCastException) {
                            // This happens only if a float is being assigned to an int.
                            propInfo.SetValue(tweenObj.target, (int)Math.Floor((double)(float)p_value), null);
                        } catch (ArgumentException) {
                            // This happens only on iOS if a float is being assigned to an int.
                            propInfo.SetValue(tweenObj.target, (int)Math.Floor((double)(float)p_value), null);
                        }
                    } else {
                        try {
                            fieldInfo.SetValue(tweenObj.target, p_value);
                        } catch (InvalidCastException) {
                            // This happens only if a float is being assigned to an int.
                            fieldInfo.SetValue(tweenObj.target, (int)Math.Floor((double)(float)p_value));
                        } catch (ArgumentException) {
                            // This happens only on iOS if a float is being assigned to an int.
                            fieldInfo.SetValue(tweenObj.target, (int)Math.Floor((double)(float)p_value));
                        }
                    }
                } else {
                    try {
                        valAccessor.Set(tweenObj.target, p_value);
                    } catch (InvalidCastException) {
                        // This happens only if a float is being assigned to an int.
                        valAccessor.Set(tweenObj.target, (int)Math.Floor((double)(float)p_value)); // OPTIMIZE store if it's int prior to this, so valAccessor doesn't even have to run to catch the error?
                    } catch (ArgumentException) {
                        // This happens only on iOS if a float is being assigned to an int, but is also here just to be sure.
                        valAccessor.Set(tweenObj.target, (int)Math.Floor((double)(float)p_value));
                    }
                }
#endif
            }
        }

        /// <summary>
        /// Gets the current value of the controlled property.
        /// Some plugins (like PlugSetColor) might override this to set values on different properties.
        /// </summary>
        protected virtual object GetValue()
        {
            if (_useSpeedTransformAccessors) {
                // Use specific accessors
                if (_getTransformVector3 != null)
                    return _getTransformVector3();
                else
                    return _getTransformQuaternion();
            } else {
#if MICRO
                if (propInfo != null)
                {
                    //return propInfo.GetValue(tweenObj.target, null);
                    return propInfo.GetGetMethod().Invoke(tweenObj.target, null); // HACK for reflection bug with some generics on iOS
                }
                return fieldInfo.GetValue(tweenObj.target);
#else
                if (HOTween.isIOS) {
                    if (propInfo != null) {
                        //return propInfo.GetValue(tweenObj.target, null);
                        return propInfo.GetGetMethod().Invoke(tweenObj.target, null); // HACK for reflection bug with some generics on iOS
                    }
                    return fieldInfo.GetValue(tweenObj.target);
                }
                return valAccessor.Get(tweenObj.target);
#endif
            }
        }
    }
}
