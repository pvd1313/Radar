using System;

namespace Beast.Claw.Editor
{
    partial struct BeastEditorUI
    {
        public BeastEditorUI WhenSuccess(Action action)
        {
            if (success)
            {
                action.Invoke();
            }

            return this;
        }

        public BeastEditorUI WhenFailed(Action action)
        {
            if (!success)
            {
                action.Invoke();
            }

            return this;
        }

        public BeastEditorUI Then(Action<BeastEditorUI> action)
        {
            action.Invoke(this);

            return this;
        }

        public BeastEditorUI WhenSuccess(Func<BeastEditorUI> action)
        {
            if (success)
            {
                return action.Invoke();
            }

            return this;
        }

        public BeastEditorUI WhenFailed(Func<BeastEditorUI> action)
        {
            if (!success)
            {
                return action.Invoke();
            }

            return this;
        }
    }
}