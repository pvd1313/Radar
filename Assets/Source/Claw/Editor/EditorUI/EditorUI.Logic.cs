using System;

namespace Beast.Claw.Editor
{
    partial struct EditorUI
    {
        public EditorUI WhenSuccess(Action action)
        {
            if (success)
            {
                action.Invoke();
            }

            return this;
        }

        public EditorUI WhenFailed(Action action)
        {
            if (!success)
            {
                action.Invoke();
            }

            return this;
        }

        public EditorUI Then(Action<EditorUI> action)
        {
            action.Invoke(this);

            return this;
        }

        public EditorUI WhenSuccess(Func<EditorUI> action)
        {
            if (success)
            {
                return action.Invoke();
            }

            return this;
        }

        public EditorUI WhenFailed(Func<EditorUI> action)
        {
            if (!success)
            {
                return action.Invoke();
            }

            return this;
        }
    }
}