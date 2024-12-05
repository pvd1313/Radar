using UnityEngine;

namespace Beast.Claw.Graphy.Modules
{
    public class AudioMonitor
    {
        private const float REF_VALUE = 1f;
        
        public float[] SpectrumCurrent { get; private set; }
        public float[] SpectrumPrevious { get; private set; }
        public float MaxDB { get; private set; }
        public bool SpectrumDataAvailable => _audioListener != null;
        
        private readonly GraphyConfig _config;
        private readonly AudioListener _audioListener;
        private readonly int _spectrumSize;
        
        public AudioMonitor(GraphyConfig config, AudioListener listener)
        {
            _config = config;
            _audioListener = listener;
            _spectrumSize = _config.spectrumSize;
        }

        public void Initialize()
        {
            SpectrumCurrent = new float[_spectrumSize];
            SpectrumPrevious = new float[_spectrumSize];
        }
        
        public void Update()
        {
            if (_audioListener != null)
            {
                AudioListener.GetOutputData(SpectrumCurrent, 0);

                float sum = 0;

                for (int i = 0; i < SpectrumCurrent.Length; i++)
                {
                    sum += SpectrumCurrent[i] * SpectrumCurrent[i]; 
                }
                
                float rmsValue = Mathf.Sqrt(sum / SpectrumCurrent.Length);
                
                MaxDB = 20 * Mathf.Log10(rmsValue / REF_VALUE); 

                if (MaxDB < -80) MaxDB = -80;
                
                AudioListener.GetSpectrumData(SpectrumCurrent, 0, _config.FFTWindow);

                for (int i = 0; i < SpectrumCurrent.Length; i++)
                {
                    if (SpectrumCurrent[i] > SpectrumPrevious[i])
                    {
                        SpectrumPrevious[i] = SpectrumCurrent[i];
                    }
                    else
                    {
                        SpectrumPrevious[i] = Mathf.Clamp
                        (
                            value: SpectrumPrevious[i] - SpectrumPrevious[i] * Time.deltaTime * 2,
                            min: 0,
                            max: 1
                        );
                    }
                }
            }
        }

        
        /// <summary>
        /// Converts spectrum values to decibels using logarithms.
        /// </summary>
        /// <param name="linear"></param>
        /// <returns></returns>
        public float Lin2dB(float linear)
        {
            return Mathf.Clamp(Mathf.Log10(linear) * 20.0f, -160.0f, 0.0f);
        }

        /// <summary>
        /// Normalizes a value in decibels between 0-1.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public float DBNormalized(float db)
        {
            return (db + 160f) / 160f;
        }
    }
}