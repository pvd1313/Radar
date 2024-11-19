using System.Text;

namespace Beast.Radar.Editor.GoogleSheet
{
    public struct JsonOutput
    {
        public StringBuilder stream;
        public string tab;
        public string space;
        public string lineSeparator;
        public int depth;
    }
}