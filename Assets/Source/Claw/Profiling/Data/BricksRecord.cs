using System.Runtime.CompilerServices;

namespace Beast.Claw.Profiling
{
    internal struct BricksRecord
    {
        private const uint TIME_SAMPLES_MASK = 0x7FFFFFFF;
        private const uint COMMAND_TYPE_MASK = 0x80000000;
        
        private uint _timeAndCommand;
        private int _nameHash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int timeSamples, bool isBeginSampleCommand, int nameHash)
        {
            _timeAndCommand = (uint) (timeSamples & TIME_SAMPLES_MASK) | (isBeginSampleCommand ? COMMAND_TYPE_MASK : 0);
            _nameHash = nameHash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out int timeSamples, out bool isBeginSampleCommand, out int nameHash)
        {
            isBeginSampleCommand = (_timeAndCommand & COMMAND_TYPE_MASK) != 0;
            timeSamples = (int) (_timeAndCommand & TIME_SAMPLES_MASK);
            nameHash = _nameHash;
        }
    }
}