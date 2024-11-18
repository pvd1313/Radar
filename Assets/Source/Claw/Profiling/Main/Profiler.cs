using System;
using System.Diagnostics;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Jobs;
using Beast.Claw.Validation;
using Unity.Collections;
using Unity.Jobs;

namespace Beast.Claw.Profiling
{
    public partial class Profiler : IProfiler, IDisposable
    {
        public ProfilerTreeReader Reader => new (_profilingTree);
        
        private readonly ProfilerConfig _config;

        private bool _enableClawProfiling;
        private bool _enableUnityProfiling;
        private NativeArray<ProfilerRecord> _records;
        private int _recordCount;
        private int _maxRecordCount;
        private int _nameCount;
        private string[] _namesActive;
        private string[] _namesPassive;
        private Stopwatch _samplesTimer;
        private Stopwatch _buildTreeJobTimer;
        private ProfilerTree _profilingTree;
        private ExceptionHandler _exceptionHandler;

        public Profiler(ProfilerConfig config)
        {
            config.ValidateThrowException(nameof(config));
            
            _config = config;
        }

        public void Initialize()
        {
            _enableClawProfiling = _config.customProfilingEnabled;
            _buildTreeJobTimer = new Stopwatch();
            _maxRecordCount = _config.maxRecordCount;
            _records = new NativeArray<ProfilerRecord>(_maxRecordCount, Allocator.Persistent);
            _recordCount = 0;
            _nameCount = 1;
            _namesActive = new string[_maxRecordCount + 1];
            _namesPassive = new string[_maxRecordCount + 1];
            _namesActive[0] = "_root";
            _namesPassive[0] = "_root";
            _samplesTimer = new Stopwatch();
            _samplesTimer.Start();
            _profilingTree = new ProfilerTree
            {
                childs = Native.PermArray<ushort>(1),
                names = Native.PermArray<ushort>(1),
                siblings = Native.PermArray<ushort>(1),
                times = Native.PermArray<float>(1),
                text = _namesActive,
                nodeCount = 1
            };
            _profilingTree.times[0] = 0;
            _profilingTree.childs[0] = 0;
            _profilingTree.names[0] = 0;
            _profilingTree.siblings[0] = 0;
        }

        public void FetchConfig()
        {
            _enableClawProfiling = _config.customProfilingEnabled;
            _enableUnityProfiling = UnityEngine.Profiling.Profiler.enabled;
        }

        public void RebuildTree()
        {
            _profilingTree.Dispose();

            int nodeCount = _recordCount / 2 + _recordCount % 2 + 1;

            BuildTreeJob job = new ()
            {
                inRecords = _records,
                inRecordCount = _recordCount,
                inFrequency = Stopwatch.Frequency,
                outChilds = Native.PermArray<ushort>(nodeCount),
                outNames = Native.PermArray<ushort>(nodeCount),
                outSiblings = Native.PermArray<ushort>(nodeCount),
                outTimes = Native.PermArray<float>(nodeCount),
                outState = Native.CreateTempArray<TreeBuildState>(1),
                parentStack = Native.CreateTempArray<ushort>(_config.stackSize),
                siblingStack = Native.CreateTempArray<ushort>(_config.stackSize),
                timeStack = Native.CreateTempArray<int>(_config.stackSize),
                nameHashStack = Native.CreateTempArray<int>(_config.stackSize)
            };
            
            UnityEngine.Profiling.Profiler.BeginSample("ProfilingManager.BuildTreeJob");
            _buildTreeJobTimer.Restart();
            job.Run();
            _buildTreeJobTimer.Stop();
            UnityEngine.Profiling.Profiler.EndSample();

            float elapsedMS = _buildTreeJobTimer.ElapsedMillisecondsFloat();
            BDebug.Track(this, "BuildTreeJobMS", elapsedMS);

            try
            {
                _exceptionHandler.HandleJobState(_namesActive, job);
            }
            finally
            {
                job.parentStack.Dispose();
                job.siblingStack.Dispose();
                job.timeStack.Dispose();
                job.nameHashStack.Dispose();
                job.outState.Dispose();

                _profilingTree = new ProfilerTree
                {
                    childs = job.outChilds,
                    names = job.outNames,
                    siblings = job.outSiblings,
                    times = job.outTimes,
                    text = _namesActive,
                    nodeCount = _recordCount + 1
                };
                
                _recordCount = 0;
                
                (_namesActive, _namesPassive) = (_namesPassive, _namesActive);
                
                _nameCount = 1;

                _samplesTimer.Restart();
            }
        }

        public Handle GetHandle(object owner)
        {
            if (owner is null) throw new ArgumentNullException(nameof(owner));

            return new Handle(this, _enableUnityProfiling || _enableClawProfiling);
        }

        private void _beginSample(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            if (_enableUnityProfiling)
            {
                UnityEngine.Profiling.Profiler.BeginSample(name);
            }

            if (!_enableClawProfiling)
            {
                return;
            }

            if (_recordCount >= _maxRecordCount)
            {
                string message = $"Too many records ({_maxRecordCount}). Try adjusting record count in the config.";
                throw new OutOfMemoryException(message);
            }

            ProfilerRecord record = new ();
            record.Write((int) _samplesTimer.ElapsedTicks, true, name.GetHashCode());
            _records[_recordCount++] = record;
            _namesActive[_nameCount++] = name;
        }

        private void _endSample(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            if (_enableUnityProfiling)
            {
                UnityEngine.Profiling.Profiler.EndSample();
            }

            if (!_enableClawProfiling)
            {
                return;
            }

            if (_recordCount >= _maxRecordCount)
            {
                string message = $"Too many records ({_maxRecordCount}). Adjust record count in the config.";
                throw new OutOfMemoryException(message);
            }

            ProfilerRecord record = new ProfilerRecord();
            record.Write((int) _samplesTimer.ElapsedTicks, false, name.GetHashCode());
            _records[_recordCount++] = record;
            _namesActive[_nameCount++] = name;
        }

        public void Dispose()
        {
            _records.Dispose();
            _profilingTree.Dispose();
        }
    }
}