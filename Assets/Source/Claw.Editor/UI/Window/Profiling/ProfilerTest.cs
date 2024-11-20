using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Beast.Claw.Profiling;
using NUnit.Framework;

namespace Beast.Claw.Editor.UI.Profiling
{
    public class ProfilerTest
    {
        private Claw.Profiling.Profiler _manager;
        private ProfilerConfig _config;
        private List<ProfilerNode> _nodes;
        private int _totalNodeCount;

        [SetUp]
        public void SetUp()
        {
            _nodes = new List<ProfilerNode>();
            _config = new ProfilerConfig(true, 2, 2);
            _manager = new Claw.Profiling.Profiler(_config);
            _totalNodeCount = 0;
            _manager.Initialize();
        }
        
        [Test]
        public void GetHandle_WithNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.GetHandle(null));
        }

        [Test]
        public void OnBeginSample_WithNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.GetHandle(this).BeginSample(null));
        }

        [Test]
        public void OnEndSample_WithNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.GetHandle(this).EndSample(null));
        }

        [Test]
        public void OnUpdate_WithoutSamples_HasRootOnly()
        {
            UpdateManager_ReadResults();
            Assert.That(_totalNodeCount, Is.EqualTo(1));
        }
        
        [Test]
        public void OnUpdate_WithoutSamples_RootDeepIsZero()
        {
            UpdateManager_ReadResults();
            Assert.That(_nodes[0].deep, Is.EqualTo(0));
        }
        
        [Test]
        public void OnUpdate_WithoutSamples_RootNameIsCorrect()
        {
            UpdateManager_ReadResults(); 
            Assert.That(_nodes[0].name, Is.EqualTo("_root"));
        }

        [Test]
        public void OnUpdate_WithDummy_HasRootAndDummyOnly()
        {
            BeginEndDummySample_UpdateManager_ReadResults(); 
            Assert.That(_totalNodeCount, Is.EqualTo(2));
        }
        
        [Test]
        public void OnUpdate_WithDummy_DummyDeepIsOne()
        {
            BeginEndDummySample_UpdateManager_ReadResults(); 
            Assert.That(_nodes[1].deep, Is.EqualTo(1));
        }
        
        [Test]
        public void OnUpdate_WithDummy_DummyNameIsCorrect()
        {
            BeginEndDummySample_UpdateManager_ReadResults();
            Assert.That(_nodes[1].name, Is.EqualTo("TestSample"));
        }

        [Test]
        public void OnBeginSample_WithRecordLimit_ThrowsException()
        {
            for (int i = 0; i < _config.maxRecordCount; i++)
            {
                _manager.GetHandle(this).BeginSample("TestSample");
            }
            Assert.Throws<OutOfMemoryException>(() => _manager.GetHandle(this).BeginSample("TestSample"));
        }

        [Test]
        public void OnEndSample_WithRecordLimit_ThrowsException()
        {
            for (int i = 0; i < _config.maxRecordCount; i++)
            {
                _manager.GetHandle(this).EndSample("TestSample");
            }
            Assert.Throws<OutOfMemoryException>(() => _manager.GetHandle(this).EndSample("TestSample"));
        }

        [Test]
        public void OnBeginSample_WithoutOnEndSample_WhenUpdate_ThrowsException_WithDummyPath()
        {
            _manager.GetHandle(this).BeginSample("TestSample");
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => _manager.RebuildTree());
            Assert.That(exception.Message, Does.Contain("_root/TestSample"));
        }

        [Test]
        public void OnBeginSample_WithOverflow_WhenUpdate_ThrowsException_WithPath()
        {
            _manager.GetHandle(this).BeginSample("SampleA");
            _manager.GetHandle(this).BeginSample("SampleB");
            StackOverflowException exception = Assert.Throws<StackOverflowException>(() => _manager.RebuildTree());
            Assert.That(exception.Message, Does.Contain("_root/SampleA/SampleB"));
        }

        [Test]
        public void OnEndSample_WithoutOnBeginSample_WhenUpdate_ThrowsException_WithPath()
        {
            _manager.GetHandle(this).EndSample("TestSample");
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => _manager.RebuildTree());
            Assert.That(exception.Message, Does.Contain("_root/TestSample"));
        }

        [Test]
        public void OnBeginSample_OnEndSample_WithDifferentNames_WhenUpdate_ThrowsException_WithPaths()
        {
            _manager.GetHandle(this).BeginSample("SampleA");
            _manager.GetHandle(this).EndSample("SampleB");
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => _manager.RebuildTree());
            Assert.That(exception.Message, Does.Contain("_root/SampleA").And.Contain("_root/SampleB"));
        }

        [Test]
        public void OnBeginSample_OnEndSample_WithTimeDelay_TimeIsCorrect()
        {
            Stopwatch timer = new Stopwatch();
            
            timer.Start();
            _manager.GetHandle(this).BeginSample("TestSample");
            Thread.SpinWait((int)(Stopwatch.Frequency * 0.001f));
            _manager.GetHandle(this).EndSample("TestSample");
            timer.Stop();
            
            _manager.RebuildTree();
            _manager.Reader.Read(0, 2, _nodes, out _totalNodeCount);

            float actualTime = _nodes[1].timeMS;
            float expectedTime = timer.ElapsedTicks / (float) Stopwatch.Frequency * 1000;
            Assert.That(actualTime, Is.EqualTo(expectedTime).Within(actualTime * 0.01f));
        }

        private void BeginEndDummySample_UpdateManager_ReadResults()
        {
            _manager.GetHandle(this).BeginSample("TestSample");
            _manager.GetHandle(this).EndSample("TestSample");
            _manager.RebuildTree();

            _manager.Reader.Read(0, 2, _nodes, out _totalNodeCount);
        }

        private void UpdateManager_ReadResults()
        {
            _manager.RebuildTree();
            
            _manager.Reader.Read(0, 1, _nodes, out _totalNodeCount);
        }

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
        }
    }
}