/* ---------------------------------------
 * Author:          Started by David Mkrtchyan, modified by Martin Pane (martintayx@gmail.com) (@martinTayx)
 * Contributors:    https://github.com/Tayx94/graphy/graphs/contributors
 * Project:         Graphy - Ultimate Stats Monitor
 * Date:            18-May-18
 * Studio:          Tayx
 *
 * Git repo:        https://github.com/Tayx94/graphy
 *
 * This project is released under the MIT license.
 * Attribution is not required, but it is always welcomed!
 * -------------------------------------*/

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Tayx.Graphy.Utils.NumString
{
    public static class G_IntString
    {
        private static string[] _negativeBuffer = Array.Empty<string>();
        private static string[] _positiveBuffer = Array.Empty<string>();

        private static bool _allocationReported;

        public static void EnsureRange(int minNegativeValue, int maxPositiveValue)
        {
            if (_getMinValue() > minNegativeValue && minNegativeValue <= 0)
            {
                int length = Mathf.Abs(minNegativeValue);

                _negativeBuffer = new string[length];

                for (int i = 0; i < length; i++)
                {
                    _negativeBuffer[i] = (-i - 1).ToString();
                }
            }

            if (_positiveBuffer.Length < maxPositiveValue)
            {
                _positiveBuffer = new string[maxPositiveValue + 1];

                for (int i = 0; i < maxPositiveValue + 1; i++)
                {
                    _positiveBuffer[i] = i.ToString();
                }
            }
        }

        private static int _getMinValue()
        {
            return -(_negativeBuffer.Length - 1);
        }

        public static void Dispose()
        {
            _negativeBuffer = Array.Empty<string>();
            _positiveBuffer = Array.Empty<string>();
        }

        public static string ToIntStringNonAlloc(this float value)
        {
            if (float.IsNaN(value))
            {
                return "NaN";
            }

            return ToStringNonAlloc((int)value);
        }
        
        public static string ToIntStringNonAlloc(this double value)
        {
            if (double.IsNaN(value))
            {
                return "NaN";
            }

            return ToStringNonAlloc((int)value);
        }
        
        public static string ToStringNonAlloc(this short value)
        {
            return ToIntStringNonAlloc(value);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static string ToStringNonAlloc(this int value)
        {
            if (value < 0 && -value <= _negativeBuffer.Length)
            {
                return _negativeBuffer[value * -1 - 1];
            }

            if (value >= 0 && value < _positiveBuffer.Length)
            {
                return _positiveBuffer[ value];
            }

            if (!_allocationReported)
            {
                _allocationReported = true;
                
                Debug.LogError("ToStringNonAlloc() Failed. Not withing range: " + value);
            }
            
            return value.ToString();
        }
    }
}