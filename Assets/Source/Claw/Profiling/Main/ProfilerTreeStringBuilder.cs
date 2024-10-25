using System.Collections.Generic;
using System.Text;
using Beast.Claw.Mathematics;

namespace Beast.Claw.Profiling
{
    public class ProfilerTreeStringBuilder
    {
        private readonly StringBuilder _builder = new();
        private readonly List<ProfilerNode> _nodes = new ();
        private readonly FloatToStringConverter _floatToString = new ("D");

        public string ToString(ProfilerTreeReader reader, string header)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ProfilerTreeStringBuilder.ToString()");
            
            header.ValidateNotEmptyOrNullThrowException(nameof(header));
            
            UnityEngine.Profiling.Profiler.BeginSample("ProfilerTreeReader.Read()");
            _nodes.Capacity = BMath.Max(_nodes.Capacity, reader.NodeCount);
            reader.Read(0, reader.NodeCount, _nodes, out int totalNodeCount);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("StringBuilderCommands");
            
            _builder.Clear();

            _builder.Append(header);

            _builder.Append(' ');

            _printTimeMS(_nodes[0].timeMS);

            _builder.Append(':');

            _builder.AppendLine();

            for (int i = 1; i < _nodes.Count; i++)
            {
                ProfilerNode node = _nodes[i];
                
                _builder.Append(' ', node.deep * 4);
                _builder.Append(node.name);
                _builder.Append(": ");

                _builder.Append(' ');

                _printTimeMS(node.timeMS);

                _builder.AppendLine();
            }
            
            UnityEngine.Profiling.Profiler.EndSample();
            
            UnityEngine.Profiling.Profiler.BeginSample("Builder.ToString()");
            string text = _builder.ToString();
            UnityEngine.Profiling.Profiler.EndSample();
            
            UnityEngine.Profiling.Profiler.EndSample();

            return text;
        }

        private void _printTimeMS(float timeMS)
        {
            _builder.Append('[');

            if (timeMS < 100)
            {
                _floatToString.ToString(timeMS, out string exponent, out string fractionWithDot);

                _builder.Append(exponent);
                _builder.Append(fractionWithDot);
            }
            else
            {
                _builder.Append(BMath.CeilToInt(timeMS));
            }

            _builder.Append("ms");

            _builder.Append(']');
        }
    }
}