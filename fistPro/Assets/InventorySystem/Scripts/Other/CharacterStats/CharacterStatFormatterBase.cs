using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Devdog.InventorySystem
{
    /// <summary>
    /// Formats stats, extends monobehaviour to serialize it through editor.
    /// </summary>
    public abstract class CharacterStatFormatterBase : UnityEngine.MonoBehaviour
    {
        public abstract string FormatStat(IEnumerable<float> stats);
    }
}