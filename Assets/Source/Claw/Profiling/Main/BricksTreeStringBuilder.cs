using System.Collections.Generic;
using System.Text;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Math;
using Beast.Claw.Validation;

namespace Beast.Claw.Profiling
{
    public class BricksTreeStringBuilder
    {
        private readonly StringBuilder _builder = new();
        private readonly List<BricksManagedNode> _nodes = new();
        private readonly FloatToStringConverter _floatToString = new("D");

        public string ToString(BricksTreeReader reader, string header)
        {
            BProfiler.BeginSample("ProfilerTreeStringBuilder.ToString()");
            
            header.ValidateNotEmptyOrNullThrowException(nameof(header));
            
            BProfiler.BeginSample("ProfilerTreeReader.Read()");
            _nodes.Capacity = BMath.Max(_nodes.Capacity, reader.NodeCount);
            reader.Read(0, reader.NodeCount, _nodes, out int totalNodeCount);
            BProfiler.EndSample();

            BProfiler.BeginSample("StringBuilderCommands");
            _builder.Clear();
            _builder.Append(header);
            _builder.Append(' ');
            _printTimeMS(_nodes[0].timeMS);
            _builder.Append(':');
            _builder.AppendLine();
            for (int i = 1; i < _nodes.Count; i++)
            {
                BricksManagedNode node = _nodes[i];
                
                _builder.Append(' ', node.deep * 4);
                _builder.Append(node.name);
                _builder.Append(": ");

                _builder.Append(' ');

                _printTimeMS(node.timeMS);

                _builder.AppendLine();
            }
            BProfiler.EndSample();
            
            BProfiler.BeginSample("Builder.ToString()");
            string text = _builder.ToString();
            BProfiler.EndSample();
            
            BProfiler.EndSample();

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