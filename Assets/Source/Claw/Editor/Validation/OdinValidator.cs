#if ODIN_VALIDATOR

using System;
using System.Runtime.CompilerServices;
using Beast.Claw.Collection;
using Beast.Claw.Common.Text;
using Beast.Claw.Debug;
using Beast.Claw.Editor.Validation;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;

[assembly: RegisterValidator(typeof(OdinValidator))]

namespace Beast.Claw.Editor.Validation
{
    public class OdinValidator : ValueValidator<object>
    {
        // Odin creates OdinValidator per each validation.
        private static readonly RingPool<StringBuilder> BUILDER_POOL;
        private static readonly string THIS_METHOD_NAME;
        
        static OdinValidator()
        {
            try
            {
                BUILDER_POOL = new RingPool<StringBuilder>(16, () => new StringBuilder(256));

                OdinValidator _;
                THIS_METHOD_NAME = typeof(OdinValidator).FullName + "." + nameof(_.Validate) + "()";
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
        
        protected override void Validate(ValidationResult result)
        {
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

                if (_isArrayIndex(Property.NiceName))
                {
                    builder += "Element at index (" + Property.NiceName + ") is null";
                }
                else
                {
                    builder += "Property " + Property.NiceName.Q() + " is null";
                }
                
                result.Message = builder.ToString();
                
                _addValidationMethodName(result, THIS_METHOD_NAME);
                
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _validateEnum(ValidationResult result, object value, Type itemType)
        {
            if (itemType.IsEnum && !Enum.IsDefined(itemType, value))
            {
                result.ResultType = ValidationResultType.Error;
                
                StringBuilder builder = BUILDER_POOL.Next().Clear();

                if (_isArrayIndex(Property.NiceName))
                {
                    builder += "Element at index (" + Property.NiceName + ") equals " + value.Q();
                    builder += " which is invalid " + itemType.Q();
                }
                else
                {
                    builder += "Property " + Property.NiceName.Q() + " equals " + 
                               value.Q() + "which is invalid " + itemType.Q();
                }
                
                result.Message = builder.ToString();
                
                _addValidationMethodName(result, THIS_METHOD_NAME);
                
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _validateFromFactory(ValidationResult result, object value, Type itemType)
        {
            if (!ValidationMethodStorage.TryFindValidator(itemType, out ValidationMethod method))
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

                    _addValidationMethodName(result, method.name);

                    return false;
                }

            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Exception during validation via {method.name.Q()}");

                ValidationMethodStorage.Forget(itemType);
                
                UnityEngine.Debug.LogException(e);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _isArrayIndex(string propertyNiceName)
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
            result.Add(new ResultItem(methodName, ValidationResultType.Warning));
        }
    }
}

#endif