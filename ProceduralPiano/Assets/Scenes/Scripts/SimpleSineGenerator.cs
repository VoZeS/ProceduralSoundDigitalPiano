using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using static Unity.VisualScripting.Member;

public class SimpleSineGenerator : MonoBehaviour
{
    [Header("Source")]
    private AudioSource source;

    [Header("Volume LFO")]
    public bool volLFOactive;
    public float volSpeed = 0.1f;
    [SerializeField, Range(0, 1)] private float volAmplitude = 0.5f;
    [SerializeField] private float volFrequency = 0.5f;

    [Header("Pitch LFO")]
    public bool pitchLFOactive;
    public float pitchSpeed = 0.1f;
    [SerializeField, Range(0, 1)] private float pitchAmplitude = 0.5f;
    [SerializeField] private float pitchFrequency = 0.5f;

    private float LFO_Index = 0.0f;

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

        _sampleRate = AudioSettings.outputSampleRate;
        playingSine = false;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        double sinePhaseIncrement = sineFrequency / _sampleRate;
        double sawPhaseIncrement = sawFrequency / _sampleRate;

        for (int sample = 0; sample < data.Length; sample += channels)
        {
            //float value = Mathf.Sin((float)_phase * 2 * Mathf.PI) * amplitude;

            if (playingSine)
            {
                value = sineWave(_sinePhase) * sineAmplitude;
                _sinePhase = (_sinePhase + sinePhaseIncrement % 1);

            }

            if (playingSaw)
            {
                value = sawWave(_sawPhase) * sawAmplitude;
                _sawPhase = (_sawPhase + sawPhaseIncrement % 1);

            }


            for (int channel = 0; channel < channels; channel++)
            {
                data[sample + channel] = value;
            }
        }

    }

    private void Update()
    {
        VolLFO(volLFOactive, volSpeed);
        PitchLFO(pitchLFOactive, pitchSpeed);
    }
    private void VolLFO(bool active, float speed)
    {
        LFO_Index = LFO_Index + speed;

        if (active)
            source.volume = volAmplitude + Mathf.Sin(LFO_Index) * volFrequency;
        else
            source.volume = 1;
    }
    private void PitchLFO(bool active, float speed)
    {
        LFO_Index = (LFO_Index + speed) * Time.deltaTime;

        if (active)
            source.pitch = pitchAmplitude + Mathf.Sin(LFO_Index) * pitchFrequency;
        else
            source.pitch = 1;
    }


    // ------------------------------------------------- SINE WAVE
    private static float sineWave(double input)
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
