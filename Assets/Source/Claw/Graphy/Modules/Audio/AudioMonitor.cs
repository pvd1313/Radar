using Beast.Claw.Debug;
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
        
        private int _spectrumSize = 512;

        public AudioMonitor(GraphyConfig config, AudioListener listener)
        {
            _config = config;
            _audioListener = listener;
        }

        public void Initialize()
        {
            _spectrumSize = _config.spectrumSize;
            SpectrumCurrent = new float[_spectrumSize];
            SpectrumPrevious = new float[_spectrumSize];
        }
        
        public void Update()
        {
            if (_audioListener != null)
            {
                // Use this data to calculate the dB value

                AudioListener.GetOutputData(SpectrumCurrent, 0);

                float sum = 0;

                for (int i = 0; i < SpectrumCurrent.Length; i++)
                {
                    sum += SpectrumCurrent[i] * SpectrumCurrent[i]; // sum squared samples
                }
                BDebug.Track(this, "sum", sum);
                
                float rmsValue = Mathf.Sqrt(sum / SpectrumCurrent.Length); // rms = square root of average

                BDebug.Track(this, "rms Value", rmsValue);
                
                MaxDB = 20 * Mathf.Log10(rmsValue / REF_VALUE); // calculate dB

                if (MaxDB < -80) MaxDB = -80; // clamp it to -80dB min

                
                BDebug.Track(this, "MaxDB", MaxDB);
                // Use this data to draw the spectrum in the graphs

                AudioListener.GetSpectrumData(SpectrumCurrent, 0, _config.FFTWindow);

                BDebug.Track(this, "Spectrum length", SpectrumCurrent.Length);
                for (int i = 0; i < SpectrumCurrent.Length; i++)
                {
                    // Update the highest value if its lower than the current one
                    if (SpectrumCurrent[i] > SpectrumPrevious[i])
                    {
                        SpectrumPrevious[i] = SpectrumCurrent[i];
                    }

                    // Slowly lower the value 
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