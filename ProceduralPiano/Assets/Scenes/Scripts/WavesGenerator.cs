using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static Unity.VisualScripting.Member;
using Input = UnityEngine.Input;

public class WavesGenerator : MonoBehaviour
{
    [Header("Source")]
    private AudioSource source;

    SawWave sawAudioWave;
    SquareWave squareAudioWave;
    SinusWave sinusAudioWave;

    SinusWave mainAmplitudeModulationOscillator;
    SinusWave sinusAmplitudeModulationOscillator;
    SinusWave sawAmplitudeModulationOscillator;
    SinusWave squareAmplitudeModulationOscillator;

    SinusWave frequencyModulationOscillator;

    public bool randomPlay;

    [Header("UI")]
    [Header("Main")]
    public Slider mainFreqSlider;
    public Toggle mainAmpModToggle;
    public Slider mainAmpModSlider;
    public Toggle mainFreqModToggle;
    public Slider mainFreqModSlider;

    [Header("Sinus")]
    public Toggle sinusToggle;
    public Slider sinusIntensitySlider;
    public Slider sinusFrequencySlider;
    public Toggle sinusAmpModToggle;
    public Slider sinusAmpModSlider;

    [Header("Saw")]
    public Toggle sawToggle;
    public Slider sawIntensitySlider;
    public Slider sawFrequencySlider;
    public Toggle sawAmpModToggle;
    public Slider sawAmpModSlider;

    [Header("Square")]
    public Toggle squareToggle;
    public Slider squareIntensitySlider;
    public Slider squareFrequencySlider;
    public Toggle squareAmpModToggle;
    public Slider squareAmpModSlider;

    [Header("Volume / Frequency")]
    [Range(0.0f, 1.0f)]
    public float mainVolume = 0.5f;
    [Range(100, 2000)]
    public double mainFrequency = 500;

    [Header("Tone Adjustment")]
    [Header("Sinus")]
    public bool useSinusAudioWave;
    [Range(0.0f, 1.0f)]
    public float sinusAudioWaveIntensity = 0.25f;
    [Range(-100, 2000)]
    public double sinusFrequency = 0;

    [Space(5)]
    public bool useSinusAmplitudeModulation;
    [Range(0.0f, 1.0f)]
    public float sinusAmplitudeModulationRangeOut;
    [Range(0.2f, 30.0f)]
    public float sinusAmplitudeModulationOscillatorFrequency = 1.0f;

    [Header("Square")]
    public bool useSquareAudioWave;
    [Range(0.0f, 1.0f)]
    public float squareAudioWaveIntensity = 0.25f;
    [Range(-100, 2000)]
    public double squareFrequency = 0;

    [Space(5)]
    public bool useSquareAmplitudeModulation;
    [Range(0.0f, 1.0f)]
    public float squareAmplitudeModulationRangeOut;
    [Range(0.2f, 30.0f)]
    public float squareAmplitudeModulationOscillatorFrequency = 1.0f;

    [Header("Saw")]
    public bool useSawAudioWave;
    [Range(0.0f, 1.0f)]
    public float sawAudioWaveIntensity = 0.25f;
    [Range(-100, 2000)]
    public double sawFrequency = 0;

    [Space(5)]
    public bool useSawAmplitudeModulation;
    [Range(0.0f, 1.0f)]
    public float sawAmplitudeModulationRangeOut;
    [Range(0.2f, 30.0f)]
    public float sawAmplitudeModulationOscillatorFrequency = 1.0f;

    [Header("Amplitude Modulation")]
    public bool useAmplitudeModulation;
    [Range(0.2f, 30.0f)]
    public float amplitudeModulationOscillatorFrequency = 1.0f;
    [Header("Frequency Modulation")]
    public bool useFrequencyModulation;
    [Range(0.2f, 30.0f)]
    public float frequencyModulationOscillatorFrequency = 1.0f;
    [Range(1.0f, 100.0f)]
    public float frequencyModulationOscillatorIntensity = 10.0f;

    [Header("Out Values")]
    [Range(0.0f, 1.0f)]
    public float amplitudeModulationRangeOut;
    [Range(0.0f, 1.0f)]
    public float frequencyModulationRangeOut;

    private double dataLen;
    double chunkTime;
    double dspTimeStep;
    double currentDspTime;

    //[Header("Sine")]
    //[SerializeField, Range(0, 1)] private float sineAmplitude = 0.5f;
    //[SerializeField] private float sineFrequency = 261.62f; //middle c
    //private double _sinePhase;

    //[Header("Saw")]
    //[SerializeField, Range(0, 1)] private float sawAmplitude = 0.5f;
    //[SerializeField] private float sawFrequency = 261.62f; //middle c
    //private double _sawPhase;

    private int _sampleRate;

    private bool check;

    private void Awake()
    {
        source = GetComponent<AudioSource>();

        sawAudioWave = new SawWave();
        squareAudioWave = new SquareWave();
        sinusAudioWave = new SinusWave();

        mainAmplitudeModulationOscillator = new SinusWave();
        sinusAmplitudeModulationOscillator = new SinusWave();
        sawAmplitudeModulationOscillator = new SinusWave();
        squareAmplitudeModulationOscillator = new SinusWave();

        frequencyModulationOscillator = new SinusWave();

        _sampleRate = AudioSettings.outputSampleRate;

    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        currentDspTime = AudioSettings.dspTime;
        dataLen = data.Length / channels;
        chunkTime = dataLen / _sampleRate;
        dspTimeStep = chunkTime / dataLen;

        double preciseDspTime;
        for (int i = 0; i < dataLen; i++)
        {
            preciseDspTime = currentDspTime + i * dspTimeStep;
            double sinusSignal = 0.0;
            double sawSignal = 0.0;
            double squareSignal = 0.0;

            double currentFreq = mainFrequency;
            double sinusFreq = sinusFrequency;
            double sawFreq = sawFrequency;
            double squareFreq = squareFrequency;

            if (useFrequencyModulation)
            {
                double freqOffset = (frequencyModulationOscillatorIntensity * mainFrequency * 0.75) / 100.0;
                currentFreq += MapValueD(frequencyModulationOscillator.CalculateSignalValue(preciseDspTime, frequencyModulationOscillatorFrequency), -1.0, 1.0, -freqOffset, freqOffset);
                frequencyModulationRangeOut = (float)frequencyModulationOscillator.CalculateSignalValue(preciseDspTime, frequencyModulationOscillatorFrequency) * 0.5f + 0.5f;
            }
            else
            {
                frequencyModulationRangeOut = 0.0f;
            }

            if (useSinusAudioWave)
            {
                sinusSignal = sinusAudioWaveIntensity * sinusAudioWave.CalculateSignalValue(preciseDspTime, currentFreq + sinusFreq);

                if (useSinusAmplitudeModulation)
                {
                    sinusSignal *= MapValueD(sinusAmplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, sinusAmplitudeModulationOscillatorFrequency), -1.0, 1.0, 0.0, 1.0);
                    sinusAmplitudeModulationRangeOut = (float)sinusAmplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, sinusAmplitudeModulationOscillatorFrequency) * 0.5f + 0.5f;
                }
            }
            if (useSawAudioWave)
            {
                sawSignal = sawAudioWaveIntensity * sawAudioWave.calculateSignalValue(preciseDspTime, currentFreq + sawFreq);

                if (useSawAmplitudeModulation)
                {
                    sawSignal *= MapValueD(sawAmplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, sawAmplitudeModulationOscillatorFrequency), -1.0, 1.0, 0.0, 1.0);
                    sawAmplitudeModulationRangeOut = (float)sawAmplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, sawAmplitudeModulationOscillatorFrequency) * 0.5f + 0.5f;
                }
            }
            if (useSquareAudioWave)
            {
                squareSignal = squareAudioWaveIntensity * squareAudioWave.calculateSignalValue(preciseDspTime, currentFreq + squareFreq);

                if (useSquareAmplitudeModulation)
                {
                    squareSignal *= MapValueD(squareAmplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, squareAmplitudeModulationOscillatorFrequency), -1.0, 1.0, 0.0, 1.0);
                    squareAmplitudeModulationRangeOut = (float)squareAmplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, squareAmplitudeModulationOscillatorFrequency) * 0.5f + 0.5f;
                }
            }

            double signalValue = sinusSignal + sawSignal + squareSignal;

            if (useAmplitudeModulation)
            {
                signalValue *= MapValueD(mainAmplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, amplitudeModulationOscillatorFrequency), -1.0, 1.0, 0.0, 1.0);
                amplitudeModulationRangeOut = (float)mainAmplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, amplitudeModulationOscillatorFrequency) * 0.5f + 0.5f;
            }
            else
            {
                amplitudeModulationRangeOut = 0.0f;
            }

            float x = mainVolume * 0.5f * (float)signalValue;

            for (int j = 0; j < channels; j++)
            {
                data[i * channels + j] = x;
            }
        }
    }


    double MapValueD(double referenceValue, double fromMin, double fromMax, double toMin, double toMax)
    {
        return toMin + (referenceValue - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    private void Update()
    {
        if (randomPlay)
        {
            check = false;

            if (!useSinusAudioWave)
            {
                useSinusAudioWave = true;
                sinusToggle.isOn = true;
            }
            if (!useSquareAudioWave)
            {
                useSquareAudioWave = true;
                squareToggle.isOn = true;
            }
            if (!useSawAudioWave)
            {
                useSawAudioWave = true;
                sawToggle.isOn = true;
            }
            if (!useAmplitudeModulation)
            {
                useAmplitudeModulation = true;
                mainAmpModToggle.isOn = true;
            }
            if (!useFrequencyModulation)
            {
                useFrequencyModulation = true;
                mainFreqModToggle.isOn = true;
            }

            mainFrequency = Mathf.PingPong(Time.time * 200.0f, 1900.0f) + 100.0f;
            mainFreqSlider.value = (float)mainFrequency;

            sinusAudioWaveIntensity = Mathf.PingPong(Time.time * 0.5f, 1.0f);
            sinusIntensitySlider.value = (float)sinusAudioWaveIntensity;

            squareAudioWaveIntensity = Mathf.PingPong(Time.time * 0.6f, 1.0f);
            squareIntensitySlider.value = (float)squareAudioWaveIntensity;

            sawAudioWaveIntensity = Mathf.PingPong(Time.time * 0.7f, 1.0f);
            sawIntensitySlider.value = (float)sawAudioWaveIntensity;

            amplitudeModulationOscillatorFrequency = Mathf.PingPong(Time.time * 3.0f, 30.0f);
            mainAmpModSlider.value = (float)amplitudeModulationOscillatorFrequency;

            frequencyModulationOscillatorFrequency = Mathf.PingPong(Time.time * 4.0f, 30.0f);
            mainFreqModSlider.value = (float)frequencyModulationOscillatorFrequency;

            frequencyModulationOscillatorIntensity = Mathf.PingPong(Time.time * 10.0f, 100.0f);
        }
        else if(!randomPlay && !check)
        {
            check = true;

            sinusToggle.isOn = false;
            squareToggle.isOn = false;
            sawToggle.isOn = false;
            mainAmpModToggle.isOn = false;
            mainFreqModToggle.isOn = false;

        }

    }

    // ------------------------------------------------- SINUS WAVE
    private static float SineWave(double input)
    {
        return Mathf.Sin((float)input * 2 * Mathf.PI);
    }

    public void PlaySinus(bool playing)
    {
        useSinusAudioWave = playing;

    }  
    
    public void SetSinusWaveIntensity(float intensity)
    {
        sinusAudioWaveIntensity = intensity;

    }

    public void SetSinusFreq(float freq)
    {
        sinusFrequency = freq;

    }

    public void SetSinusAmplitudeMod(bool amp)
    {
        useSinusAmplitudeModulation = amp;

    }

    public void SetSinusAmplitudeMod(float ampFreq)
    {
        sinusAmplitudeModulationOscillatorFrequency = ampFreq;

    }

    // ------------------------------------------------- SAW WAVE
    private static float SawWave(double input)
    {
        return ((((float)input + 0.5f) % 1) - 0.5f) * 2f;
    }

    public void PlaySaw(bool playing)
    {
        useSawAudioWave = playing;

    }

    public void SetSawWaveIntensity(float intensity)
    {
        sawAudioWaveIntensity = intensity;

    }

    public void SetSawFreq(float freq)
    {
        sawFrequency = freq;

    }

    public void SetSawAmplitudeMod(bool amp)
    {
        useSawAmplitudeModulation = amp;

    }

    public void SetSawAmplitudeMod(float ampFreq)
    {
        sawAmplitudeModulationOscillatorFrequency = ampFreq;

    }

    // ------------------------------------------------- SQUARE WAVE

    public void PlaySquare(bool playing)
    {
        useSquareAudioWave = playing;

    }

    public void SetSquareWaveIntensity(float intensity)
    {
        squareAudioWaveIntensity = intensity;

    }

    public void SetSquareFreq(float freq)
    {
        squareFrequency = freq;

    }

    public void SetSquareAmplitudeMod(bool amp)
    {
        useSquareAmplitudeModulation = amp;

    }

    public void SetSquareAmplitudeMod(float ampFreq)
    {
        squareAmplitudeModulationOscillatorFrequency = ampFreq;

    }

    // ------------------------------------------------- ALL WAVES
    public void SetMainVolume(float vol)
    {
        mainVolume = vol;
    }

    public void SetMainFrequency(float freq)
    {
        mainFrequency = freq;
    }

    public void ToggleMainAmplitudeMod(bool toggle)
    {
        useAmplitudeModulation = toggle;
    }

    public void SetMainAmplitudeModIntensity(float intensity)
    {
        amplitudeModulationOscillatorFrequency = intensity;
    }

    public void ToggleMainFrequencyMod(bool toggle)
    {
        useFrequencyModulation = toggle;
    }

    public void SetMainFrequencyModIntensity(float intensity)
    {
        frequencyModulationOscillatorFrequency = intensity;
    }

    public void RandomToggle(bool toggle)
    {
        randomPlay = toggle;
    }

}
