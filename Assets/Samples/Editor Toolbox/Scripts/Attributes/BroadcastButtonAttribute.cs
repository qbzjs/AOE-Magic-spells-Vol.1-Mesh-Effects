﻿using System;

namespace UnityEngine
{
    [Obsolete("Use EditorButtonAttribute instead.")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class BroadcastButtonAttribute : ButtonAttribute
    {
        public BroadcastButtonAttribute(string methodName, string label = null, ButtonActivityType type = ButtonActivityType.Everything)
            : base(methodName, label, type)
        { }
    }
}