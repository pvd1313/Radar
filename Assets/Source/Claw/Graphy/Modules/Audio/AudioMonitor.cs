using UnityEngine;

namespace Beast.Claw.Graphy.Modules
{
    public class AudioMonitor
    {
        private const float REF_VALUE = 1f;
        
        /// <summary>
        /// Current audio spectrum from the specified AudioListener.
        /// </summary>
        public float[] Spectrum { get; private set; }

        /// <summary>
        /// Highest audio spectrum from the specified AudioListener in the last few seconds.
        /// </summary>
        public float[] SpectrumHighestValues { get; private set; }
        
        public float MaxDB { get; private set; }
        
        public bool SpectrumDataAvailable => _audioListener != null;
        
        private readonly GraphyConfig _config;
        
        private AudioListener _audioListener;
        private int _spectrumSize = 512;

        public AudioMonitor(GraphyConfig config, AudioListener listener)
        {
            _config = config;
            _audioListener = listener;
        }

        public void Initialize()
        {
            _spectrumSize = _config.spectrumSize;
            Spectrum = new float[_spectrumSize];
            SpectrumHighestValues = new float[_spectrumSize];
        }
        
        public void Update()
        {
            if (_audioListener != null)
            {
                // Use this data to calculate the dB value

                AudioListener.GetOutputData(Spectrum, 0);

                float sum = 0;

                for (int i = 0; i < Spectrum.Length; i++)
                {
                    sum += Spectrum[i] * Spectrum[i]; // sum squared samples
                }

                float rmsValue = Mathf.Sqrt(sum / Spectrum.Length); // rms = square root of average

                MaxDB = 20 * Mathf.Log10(rmsValue / REF_VALUE); // calculate dB

                if (MaxDB < -80) MaxDB = -80; // clamp it to -80dB min

                // Use this data to draw the spectrum in the graphs

                AudioListener.GetSpectrumData(Spectrum, 0, _config.FFTWindow);

                for (int i = 0; i < Spectrum.Length; i++)
                {
                    // Update the highest value if its lower than the current one
                    if (Spectrum[i] > SpectrumHighestValues[i])
                    {
                        SpectrumHighestValues[i] = Spectrum[i];
                    }

                    // Slowly lower the value 
                    else
                    {
                        SpectrumHighestValues[i] = Mathf.Clamp
                        (
                            value: SpectrumHighestValues[i] - SpectrumHighestValues[i] * Time.deltaTime * 2,
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
        public float lin2dB(float linear)
        {
            return Mathf.Clamp(Mathf.Log10(linear) * 20.0f, -160.0f, 0.0f);
        }

        /// <summary>
        /// Normalizes a value in decibels between 0-1.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public float dBNormalized(float db)
        {
            return (db + 160f) / 160f;
        }
    }
}