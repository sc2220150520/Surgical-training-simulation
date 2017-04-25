using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

namespace Devdog.InventorySystem
{
    public static class ComponentExtension
    {
        public static void CopyValuesFrom(this Component to, Component from, System.Type type)
        {
            if (type == typeof(UnityEngine.MonoBehaviour) || type == null)
                return;

            // Copied fields can be restricted with BindingFlags
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                field.SetValue(to, field.GetValue(from));
            }

            // Keep going untill we hit UnityEngine.MonoBehaviour type.
            CopyValuesFrom(to, from, type.BaseType);
        }
    }
}