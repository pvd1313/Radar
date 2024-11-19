#if ODIN_VALIDATOR

using System;
using System.Runtime.CompilerServices;
using Beast.Claw.Collections;
using Beast.Claw.Debug;
using Beast.Claw.Editor.Validation;
using Beast.Claw.Reflection;
using Beast.Claw.Text;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;

[assembly: RegisterValidator(typeof(OdinValidator))]

namespace Beast.Claw.Editor.Validation
{
    public class OdinValidator : ValueValidator<object>
    {
        // Odin creates OdinValidator per each validation.
        private static readonly RingPool<StringBuilder> BUILDER_POOL;
        private static readonly string VALIDATE_ENUM;
        private static readonly string VALIDATE_NULL_REFERENCE;
        
        static OdinValidator()
        {
            try
            {
                BUILDER_POOL = new RingPool<StringBuilder>(16, () => new StringBuilder(256));
                VALIDATE_ENUM = typeof(OdinValidator).FullName + "." + nameof(_validateEnum) + "()";
                VALIDATE_NULL_REFERENCE = typeof(OdinValidator).FullName + "." + nameof(_validateNullReference) + "()";
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
        
        protected override void Validate(ValidationResult result)
        {
            if (ValidationManager.CheckAssetTypeIsExcluded(ValueEntry.ParentType))
            {
                return;
            }
            
            if (!_validateNullReference(result))
            {
                return;
            }

            object value = Property.BaseValueEntry.WeakSmartValue;
            Type itemType = Property.BaseValueEntry.TypeOfValue;

            if (!_validateEnum(result, value, itemType))
            {
                return;
            }

            if (!_validateFromFactory(result, value, itemType))
            {
                return;
            }
        }

        private bool _validateNullReference(ValidationResult result)
        {
            if (Property.BaseValueEntry.ValueState == PropertyValueState.NullReference)
            {
                result.ResultType = ValidationResultType.Error;

                StringBuilder builder = BUILDER_POOL.Next().Clear();

                if (_tryParseArrayIndex(Property.NiceName))
                {
                    builder += "Element at index (" + Property.NiceName + ") is null";
                }
                else
                {
                    builder += "Property '" + Property.NiceName + "' is null";
                }
                
                result.Message = builder.ToString();
                
                _addValidationMethodName(result, VALIDATE_NULL_REFERENCE);
                
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _validateEnum(ValidationResult result, object value, Type itemType)
        {
            if (!BEnum.TryGetEnumData(itemType, out EnumData enumData))
            {
                return true;
            }

            long valueLong = Reflect.UnboxAsLong(value, enumData.typeCode);

            if (enumData.isFlags)
            {
                if (valueLong == 0 || (enumData.flagsMask & valueLong) != 0)
                {
                    return true;
                }
            }
            
            if (!enumData.isFlags && enumData.allValues.Contains(valueLong))
            {
                return true;
            }
            
            StringBuilder str = BUILDER_POOL.Next().Clear();

            if (_tryParseArrayIndex(Property.NiceName))
            {
                str = str + "Element at index (" + Property.NiceName + ") equals " + value.Q();
                str = str + " which is invalid " + itemType.Q();
            }
            else
            {
                str = str + "Property '" + Property.NiceName + "' equals " +
                      value.Q() + " which is invalid " + itemType.Q();
            }

            result.ResultType = ValidationResultType.Error;
            result.Message = str.ToString();
                
            _addValidationMethodName(result, VALIDATE_ENUM);
                
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _validateFromFactory(ValidationResult result, object value, Type itemType)
        {
            if (!ValidationManager.TryFindValidator(itemType, out ValidationMethod method))
            {
                return true;
            }
            
            try
            {
                string invokeResult = (string) method.info.Invoke(value, null);
                
                if (invokeResult != null)
                {
                    result.ResultType = ValidationResultType.Error;
                    result.Message = invokeResult;

                    _addValidationMethodName(result, method.fullNameWithMethod);

                    return false;
                }

            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Exception during validation via {method.fullNameWithMethod.Q()}");

                ValidationManager.Forget(itemType);
                
                UnityEngine.Debug.LogException(e);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _tryParseArrayIndex(string propertyNiceName)
        {
            if (int.TryParse(propertyNiceName, out _))
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void _addValidationMethodName(ValidationResult result, string methodName)
        {
            if (ValidationPreference.MethodDisplayEnabled)
            {
                result.Add(new ResultItem(methodName, ValidationResultType.Warning));
            }
        }
    }
}

#endif