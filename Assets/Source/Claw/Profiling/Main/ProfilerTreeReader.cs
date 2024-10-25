using System;
using System.Collections.Generic;
using Beast.Claw.Jobs;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Claw.Profiling
{
    public readonly struct ProfilerTreeReader
    {
        public int NodeCount => _tree.nodeCount;
        
        private readonly ProfilerTree _tree;

        public ProfilerTreeReader(ProfilerTree tree)
        {
            _tree = tree;
        }
        
        public void Read(int offset, int count, List<ProfilerNode> result, out int totalNodeCount)
        {
            _tree.ValidateThrowException(nameof(_tree));
            
            if (offset < 0) throw new ArgumentException($"{nameof(offset)} can not be negative");
            if (count < 0) throw new ArgumentException($"{nameof(count)} can not be negative");
            if (count > _tree.nodeCount) throw new AggregateException("Can not read more nodes than exists");
            if (result is null) throw new ArgumentNullException(nameof(result));
            
            ReadTreeJob job = new ()
            {
                inChilds = _tree.childs,
                inNames = _tree.names,
                inSiblings = _tree.siblings,
                inTimes = _tree.times,
                inReadRange = new int2(offset, offset + count - 1),
                outNodeCount = NativeMemory.CreateTempArray<int>(1),
                outTotalNodeCount = NativeMemory.CreateTempArray<int>(1),
                outNodes = NativeMemory.CreateTempArray<NativeNode>(count)
            };
            job.Run();
            
            int nodeCount = job.outNodeCount[0];
            result.Clear();
            
            for (int i = 0; i < nodeCount; i++)
            {
                NativeNode node = job.outNodes[i];
                
                result.Add(new ProfilerNode
                {
                    name = _tree.text[node.name],
                    deep = node.deep,
                    timeMS = node.time
                });
            }

            totalNodeCount = job.outTotalNodeCount[0];

            job.outNodes.Dispose();
            job.outNodeCount.Dispose();
            job.outTotalNodeCount.Dispose();
        }
    }
}