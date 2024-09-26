using System;

namespace Beast.Claw.Editor
{
    public struct BeastEditorUIResult
    {
        public bool success;

        public BeastEditorUIResult WhenSuccess(Action action)
        {
            if (success)
            {
                action.Invoke();
            }

            return this;
        }

        public BeastEditorUIResult WhenFailed(Action action)
        {
            if (!success)
            {
                action.Invoke();
            }

            return this;
        }

        public BeastEditorUIResult Then(Action<BeastEditorUIResult> action)
        {
            action.Invoke(this);

            return this;
        }

        public BeastEditorUIResult WhenSuccess(Func<BeastEditorUIResult> action)
        {
            if (success)
            {
                return action.Invoke();
            }

            return this;
        }

        public BeastEditorUIResult WhenFailed(Func<BeastEditorUIResult> action)
        {
            if (!success)
            {
                return action.Invoke();
            }

            return this;
        }
    }
}