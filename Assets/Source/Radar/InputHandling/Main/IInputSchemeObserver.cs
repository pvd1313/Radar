using System;

namespace Beast.Radar.InputHandling
{
    public interface IInputSchemeObserver
    {
        event Action<EInputScheme> Changed;
        
        EInputScheme Current { get; }
    }
}