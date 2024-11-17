using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public readonly partial struct FlowUI
    {
        private readonly Flow _flow;
        private readonly RectUI _rectUI;

        public FlowUI(Flow flow, RectUI rectUI)
        {
            _flow = flow;
            _rectUI = rectUI;
        }

        public Rect AllocateRect(float2 size)
        {
            return GUILayoutUtility.GetRect(size.x, size.y);
        }

        public void Record(Object obj, string recordName)
        {
            obj.ValidateNotNullThrowException(nameof(obj));
            recordName.ValidateNotEmptyOrNullThrowException(nameof(recordName));
            
            Undo.RecordObject(obj, recordName);
        }
        
        public void RecordMultiple(Object[] objArray, string recordName)
        {
            objArray.ValidateNotNullThrowException(nameof(objArray));
            recordName.ValidateNotEmptyOrNullThrowException(nameof(recordName));
            
            Undo.RecordObjects(objArray, recordName);
        }
    }
}