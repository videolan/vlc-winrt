/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;

namespace VLC_WINRT.Utility.IoC
{
    /// <summary>
    ///     Naive IoC pattern currently lacking any kind of scope control or thread safety.
    /// </summary>
    public class IoC
    {
        private static readonly Dictionary<Type, object> Objects = new Dictionary<Type, object>();

        private static readonly Dictionary<Type, Func<object>> NonConstructedObjects =
            new Dictionary<Type, Func<object>>();

        private static List<Type> _objectsToImmediatelyConstructs = new List<Type>();

        public static T GetInstance<T>()
            where T : class
        {
            CheckAndConstructWaitingObjects();

            Type key = typeof (T);
            if (Objects.ContainsKey(key))
            {
                return GetObject<T>(key);
            }

            throw new ResolutionException("Could not resolve class: " + typeof (T));
        }

        private static T GetObject<T>(Type key) where T : class
        {
            T result;
            if (Objects[key] == null)
            {
                result = (T) NonConstructedObjects[typeof (T)]();
                Objects[key] = result;
            }
            else
            {
                result = (T) Objects[key];
            }
            return result;
        }

        private static void CheckAndConstructWaitingObjects()
        {
            List<Type> toBeConstructed = _objectsToImmediatelyConstructs;
            _objectsToImmediatelyConstructs = null;

            if (toBeConstructed != null)
            {
                foreach (Type objectType in toBeConstructed)
                {
                    object result = NonConstructedObjects[objectType]();
                    Objects[objectType] = result;
                }
            }
        }

        public static void Register<T>()
            where T : class, new()
        {
            Register<T>(false);
        }

        public static void Register<T>(bool constructImmediately)
            where T : class, new()
        {
            if (constructImmediately)
            {
                _objectsToImmediatelyConstructs.Add(typeof (T));
                Objects.Add(typeof (T), null);
                NonConstructedObjects.Add(typeof (T), () => new T());
            }
            else
            {
                Objects.Add(typeof (T), null);
                NonConstructedObjects.Add(typeof (T), () => new T());
            }
        }

        public static void Register<T1, T2>()
            where T2 : class, new()
        {
            Objects.Add(typeof (T1), new T2());
        }
    }
}
