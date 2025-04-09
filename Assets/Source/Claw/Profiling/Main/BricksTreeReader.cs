using System;
using System.Collections.Generic;
using Beast.Claw.Jobs;
using Beast.Claw.Validation;
using Unity.Mathematics;

namespace Beast.Claw.Profiling
{
    public readonly struct BricksTreeReader
    {
        public int NodeCount => _tree.nodeCount;
        
        private readonly BricksProfilerTree _tree;

        public BricksTreeReader(BricksProfilerTree tree)
        {
            _tree = tree;
        }
        
        public void Read(int offset, int count, List<BricksManagedNode> result, out int totalNodeCount)
        {
            _tree.ValidateThrowException(nameof(_tree));
            
            if (offset < 0) throw new ArgumentException($"{nameof(offset)} can not be negative");
            if (count < 0) throw new ArgumentException($"{nameof(count)} can not be negative");
            if (count > _tree.nodeCount) throw new AggregateException("Can not read more nodes than exists");
            if (result is null) throw new ArgumentNullException(nameof(result));
            
            BricksReadTreeJob job = new()
            {
                inChilds = _tree.childs,
                inNames = _tree.names,
                inSiblings = _tree.siblings,
                inTimes = _tree.times,
                inReadRange = new int2(offset, offset + count - 1),
                outNodeCount = BNative.TempJobArray<int>(1),
                outTotalNodeCount = BNative.TempJobArray<int>(1),
                outNodes = BNative.TempJobArray<BricksNativeNode>(count)
            };
            job.Invoke();
            
            int nodeCount = job.outNodeCount[0];
            result.Clear();
            
            for (int i = 0; i < nodeCount; i++)
            {
                BricksNativeNode node = job.outNodes[i];
                
                result.Add(new BricksManagedNode
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