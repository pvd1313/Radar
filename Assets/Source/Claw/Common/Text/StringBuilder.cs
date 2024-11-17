using System;
using System.Diagnostics;

namespace Beast.Claw.Common.Text
{
    public readonly struct StringBuilder
    {
        private readonly System.Text.StringBuilder _builder;
        private readonly bool _isCreated;

        public StringBuilder(int capacity)
        {
            _builder = new System.Text.StringBuilder(capacity);
            _isCreated = true;
        }
        
        public StringBuilder Clear()
        {
            _ensureCreated();
            
            _builder.Clear();

            return this;
        }

        public StringBuilder Add(string text)
        {
            _ensureCreated();
            
            _builder.Append(text);

            return this;
        }

        public StringBuilder Add(char chr)
        {
            _ensureCreated();
            
            _builder.Append(chr);

            return this;
        }

        public StringBuilder Line()
        {
            _ensureCreated();
            
            _builder.AppendLine();

            return this;
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        [Conditional("DEVELOPMENT")]
        private void _ensureCreated()
        {
            if (!_isCreated)
            {
                throw new ObjectDisposedException($"{GetType().FullName.Q()} is not created.");
            }
        }

        public static StringBuilder operator +(StringBuilder a, string text)
        {
            a.Add(text);

            return a;
        }
    }
}