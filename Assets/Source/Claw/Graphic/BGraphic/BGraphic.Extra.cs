using System.Collections.Generic;
using System.Text;
using Beast.Claw.Reflection;
using UnityEngine.Experimental.Rendering;

namespace Beast.Claw.Graphic
{
    public static partial class BGraphic
    {
        private static readonly StringBuilder _textBuilder;
        private static readonly Dictionary<FormatUsage, string> _formatUsage2String;
        private static readonly Dictionary<GraphicsFormat, string> _graphicsFormat2String;

        public static string GetSupportedGraphicsFormats()
        {
            _textBuilder.Clear();

            _textBuilder.AppendLine("Supported Graphics Formats: ");

            foreach (FormatUsage formatUsage in EnumStorage<FormatUsage>.Values)
            {
                _textBuilder.Append("- ");
                _textBuilder.Append(_formatUsage2String[formatUsage]);
                _textBuilder.Append(": ");

                foreach (GraphicsFormat graphicsFormat in EnumStorage<GraphicsFormat>.Values)
                {
                    _textBuilder.Append(_graphicsFormat2String[graphicsFormat]);
                    _textBuilder.Append(", ");
                }

                _textBuilder.AppendLine();
                _textBuilder.AppendLine();
            }

            return _textBuilder.ToString();
        }
    }
}