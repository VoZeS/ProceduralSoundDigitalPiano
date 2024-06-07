using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    SinusWave amplitudeModulationOscillator;
    SinusWave frequencyModulationOscillator;

    public bool randomPlay;

    [Header("Volume / Frequency")]
    [Range(0.0f, 1.0f)]
    public float mainVolume = 0.5f;
    [Range(100, 2000)]
    public double mainFrequency = 500;

    [Header("Tone Adjustment")]
    public bool useSinusAudioWave;
    [Range(0.0f, 1.0f)]
    public float sinusAudioWaveIntensity = 0.25f;

    public bool useSinusAmplitudeModulation;
    [Range(0.0f, 1.0f)]
    public float sinusAmplitudeModulationRangeOut;
    [Range(0.2f, 30.0f)]
    public float sinusAmplitudeModulationOscillatorFrequency = 1.0f;

    [Space(5)]
    public bool useSquareAudioWave;
    [Range(0.0f, 1.0f)]
    public float squareAudioWaveIntensity = 0.25f;
    [Space(5)]
    public bool useSawAudioWave;
    [Range(0.0f, 1.0f)]
    public float sawAudioWaveIntensity = 0.25f;

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

    float mainFrequencyPreviousValue;
    private System.Random RandomNumber = new System.Random();

    private double dataLen;
    double chunkTime;
    double dspTimeStep;
    double currentDspTime;

    [Header("Sine")]
    [SerializeField, Range(0, 1)] private float sineAmplitude = 0.5f;
    [SerializeField] private float sineFrequency = 261.62f; //middle c
    private double _sinePhase;

    [Header("Saw")]
    [SerializeField, Range(0, 1)] private float sawAmplitude = 0.5f;
    [SerializeField] private float sawFrequency = 261.62f; //middle c
    private double _sawPhase;

    private int _sampleRate;

    float value = 0;

    private bool playingSine;
    private bool playingSaw;

    private void Awake()
    {
        source = GetComponent<AudioSource>();

        sawAudioWave = new SawWave();
        squareAudioWave = new SquareWave();
        sinusAudioWave = new SinusWave();

        amplitudeModulationOscillator = new SinusWave();
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
            double signalValue = 0.0;
            double currentFreq = mainFrequency;

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
                signalValue += sinusAudioWaveIntensity * sinusAudioWave.CalculateSignalValue(preciseDspTime, currentFreq);

                if (useSinusAmplitudeModulation)
                {
                    signalValue *= MapValueD(amplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, sinusAmplitudeModulationOscillatorFrequency), -1.0, 1.0, 0.0, 1.0);
                    sinusAmplitudeModulationRangeOut = (float)amplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, sinusAmplitudeModulationOscillatorFrequency) * 0.5f + 0.5f;
                }

            }
            if (useSawAudioWave)
            {
                signalValue += sawAudioWaveIntensity * sawAudioWave.calculateSignalValue(preciseDspTime, currentFreq);
            }
            if (useSquareAudioWave)
            {
                signalValue += squareAudioWaveIntensity * squareAudioWave.calculateSignalValue(preciseDspTime, currentFreq);
            }

            if (useAmplitudeModulation)
            {
                signalValue *= MapValueD(amplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, amplitudeModulationOscillatorFrequency), -1.0, 1.0, 0.0, 1.0);
                amplitudeModulationRangeOut = (float)amplitudeModulationOscillator.CalculateSignalValue(preciseDspTime, amplitudeModulationOscillatorFrequency) * 0.5f + 0.5f;
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

    float MapValue(float referenceValue, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (referenceValue - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    double MapValueD(double referenceValue, double fromMin, double fromMax, double toMin, double toMax)
    {
        return toMin + (referenceValue - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    private void Update()
    {
        if (randomPlay)
        {
            if (!useSinusAudioWave)
            {
                useSinusAudioWave = true;
            }
            if (!useSquareAudioWave)
            {
                useSquareAudioWave = true;
            }
            if (!useSawAudioWave)
            {
                useSawAudioWave = true;
            }
            if (!useAmplitudeModulation)
            {
                useAmplitudeModulation = true;
            }
            if (!useFrequencyModulation)
            {
                useFrequencyModulation = true;
            }

            mainFrequency = Mathf.PingPong(Time.time * 200.0f, 1900.0f) + 100.0f;

            sinusAudioWaveIntensity = Mathf.PingPong(Time.time * 0.5f, 1.0f);

            squareAudioWaveIntensity = Mathf.PingPong(Time.time * 0.6f, 1.0f);

            sawAudioWaveIntensity = Mathf.PingPong(Time.time * 0.7f, 1.0f);

            amplitudeModulationOscillatorFrequency = Mathf.PingPong(Time.time * 3.0f, 30.0f);

            frequencyModulationOscillatorFrequency = Mathf.PingPong(Time.time * 4.0f, 30.0f);

            frequencyModulationOscillatorIntensity = Mathf.PingPong(Time.time * 10.0f, 100.0f);
        }

    }

    // ------------------------------------------------- SINE WAVE
    private static float SineWave(double input)
    {
        return Mathf.Sin((float)input * 2 * Mathf.PI);
    }
    public void PlaySine(bool playing)
    {
        playingSine = playing;

    }

    public void SetSineAmplitude(float value)
    {
        sineAmplitude = value;
    }


    public void SetSineFrequency(float value)
    {
        sineFrequency = value;
    }

    // ------------------------------------------------- SAW WAVE
    private static float sawWave(double input)
    {
        return ((((float)input + 0.5f) % 1) - 0.5f) * 2f;
    }

    public void PlaySaw(bool playing)
    {
        playingSaw = playing;

    }

    public void SetSawAmplitude(float value)
    {
        sawAmplitude = value;
    }

    public void SetSawFrequency(float value)
    {
        sawFrequency = value;
    }

}
