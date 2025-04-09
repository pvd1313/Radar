using System;
using System.Diagnostics;
using Beast.Claw.Debug;
using Beast.Claw.Jobs;
using Beast.Claw.Validation;
using Unity.Collections;

namespace Beast.Claw.Profiling
{
    public partial class BricksProfiler : IBricksProfiler, IDisposable
    {
        public BricksTreeReader Reader => new (_tree);
        
        private readonly BricksConfig _config;

        private bool _enableBricksProfiling;
        private bool _enableUnityProfiling;
        private NativeArray<BricksRecord> _records;
        private int _recordCount;
        private int _maxRecordCount;
        private int _nameCount;
        private string[] _namesActive;
        private string[] _namesPassive;
        private Stopwatch _samplesTimer;
        private Stopwatch _buildTreeJobTimer;
        private BricksProfilerTree _tree;
        private ExceptionHandler _exceptionHandler;

        public BricksProfiler(BricksConfig config)
        {
            config.ValidateThrowException(nameof(config));
            
            _config = config;
        }

        public void Initialize()
        {
            _buildTreeJobTimer = new Stopwatch();
            _maxRecordCount = _config.maxRecordCount;
            _records = new NativeArray<BricksRecord>(_maxRecordCount, Allocator.Persistent);
            _recordCount = 0;
            _nameCount = 1;
            _namesActive = new string[_maxRecordCount + 1];
            _namesPassive = new string[_maxRecordCount + 1];
            _namesActive[0] = "_root";
            _namesPassive[0] = "_root";
            _samplesTimer = new Stopwatch();
            _samplesTimer.Start();
            _tree = new BricksProfilerTree
            {
                childs = BNative.PermArray<ushort>(1),
                names = BNative.PermArray<ushort>(1),
                siblings = BNative.PermArray<ushort>(1),
                times = BNative.PermArray<float>(1),
                text = _namesActive,
                nodeCount = 1
            };
            _tree.times[0] = 0;
            _tree.childs[0] = 0;
            _tree.names[0] = 0;
            _tree.siblings[0] = 0;
        }

        public void SetActive(bool isBricksProfilerActive, bool isUnityProfilerActive)
        {
            _enableBricksProfiling = isBricksProfilerActive;
            _enableUnityProfiling = isUnityProfilerActive;
        }

        public void RebuildTree()
        {
            _tree.Dispose();

            int nodeCount = _recordCount / 2 + _recordCount % 2 + 1;

            BricksBuildTreeJob job = new()
            {
                inRecords = _records,
                inRecordCount = _recordCount,
                inFrequency = Stopwatch.Frequency,
                outChilds = BNative.PermArray<ushort>(nodeCount),
                outNames = BNative.PermArray<ushort>(nodeCount),
                outSiblings = BNative.PermArray<ushort>(nodeCount),
                outTimes = BNative.PermArray<float>(nodeCount),
                outState = BNative.TempJobArray<BricksTreeBuildState>(1),
                parentStack = BNative.TempJobArray<ushort>(_config.stackSize),
                siblingStack = BNative.TempJobArray<ushort>(_config.stackSize),
                timeStack = BNative.TempJobArray<int>(_config.stackSize),
                nameHashStack = BNative.TempJobArray<int>(_config.stackSize)
            };
            
            _buildTreeJobTimer.Restart();
            job.Invoke();
            _buildTreeJobTimer.Stop();

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

                _tree = new BricksProfilerTree
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

            return new Handle(this, _enableUnityProfiling || _enableBricksProfiling);
        }

        private void _beginSample(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            if (_enableUnityProfiling)
            {
                BProfiler.BeginSample(name);
            }

            if (!_enableBricksProfiling)
            {
                return;
            }

            if (_recordCount >= _maxRecordCount)
            {
                string message = $"Too many records ({_maxRecordCount}). Try adjusting record count in the config.";
                throw new OutOfMemoryException(message);
            }

            BricksRecord record = default;
            record.Write((int) _samplesTimer.ElapsedTicks, true, name.GetHashCode());
            _records[_recordCount++] = record;
            _namesActive[_nameCount++] = name;
        }

        private void _endSample(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            if (_enableUnityProfiling)
            {
                BProfiler.EndSample();
            }

            if (!_enableBricksProfiling)
            {
                return;
            }

            if (_recordCount >= _maxRecordCount)
            {
                string message = $"Too many records ({_maxRecordCount}). Adjust record count in the config.";
                throw new OutOfMemoryException(message);
            }

            BricksRecord record = default;
            record.Write((int) _samplesTimer.ElapsedTicks, false, name.GetHashCode());
            _records[_recordCount++] = record;
            _namesActive[_nameCount++] = name;
        }

        public void Dispose()
        {
            _records.Dispose();
            _tree.Dispose();
        }
    }
}