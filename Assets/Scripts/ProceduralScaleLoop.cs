using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralScaleLoop : MonoBehaviour {

// --------------------------------------
    // Public
    public KeyIndeciesToFrequencies fundementalToneFrequencies;
    public  float gain;
    public    int beginingNote;
    
    public  float[] harmonicStrengths = new float[12];

    private float   samplingFrequency;     // this is the number of samples we use per second,to construct the sound waveforms.
                                           // default is 48,000 samples. This means if your frame rate is 60 fps, in each frame you need to provide 48k/60 samples. 

    private AudioSource ad_source;

    private int currentNote = 0;
    private float[] phase = new float[12];
    private float fundementalToneFrequency;

    private float scaleTimer = 0;

    void Start ()
    {
        ad_source         = gameObject.AddComponent<AudioSource>();
        samplingFrequency = AudioSettings.outputSampleRate;
        currentNote = beginingNote;
        fundementalToneFrequency = fundementalToneFrequencies.fundementalFrequencies[currentNote];
        
    }

    // This function is called every time the audio stream info is updated. IMPORTANT: the function 
    // is not called in the same frequency as Update or FixedUpdate. To measure its delta time, you can
    // use dpsTime. Default, it is called every 0.021333... seconds, so around 46.8751 fps. Knowing this info
    // is important to populate your data block and match the sample Rate set in the project. 

    // data: all the info the system needs to reconstruct the sound wave form. Its members go between -1 and 1
    // and represent the displacement of the speaker surface. (which creates the wave front). 
    // Its length is: number of samples per function call * number of channels (the channels are for example 2 for stereo, 1 for mono)
    // The way it is laid in memory is like this: 1st member channel 0 - 1st member channel 1 - 2nd member channel0 - 2nd memeber channel 1 etc.
    // The numbers here need to match sample rate. Default the length of the array is 2048, with two channels, so 1024 per channel
    // As stated default this function is called 46.8751 times per second, 1024 * 46.8751 = 48k, which is our sample rate
    void OnAudioFilterRead(float[] data, int channels)
    {
        
        if ((float)AudioSettings.dspTime - scaleTimer  > 1.0f)
        {
            currentNote++;
            if (currentNote >= fundementalToneFrequencies.fundementalFrequencies.Length) currentNote = beginingNote;
            fundementalToneFrequency = fundementalToneFrequencies.fundementalFrequencies[currentNote];
            scaleTimer = (float)AudioSettings.dspTime;
        };
        



        int currentSampleIndex = 0;
        for (int i = 0; i< data.Length; i++)
        {




           
            data[i] = ReturnSuperimposedHarmonicsSeries()* gain;

            currentSampleIndex++;

            if (channels == 2)
            {
                data[i + 1] = data[i]; // if stereo, copy the one ear to the other, and simple jump over the channel 1 in the next iteration
                i++;
            }
        }
    }


    public float ReturnSuperimposedHarmonicsSeries()
    {
        float superImposed = 0.0f;

        for(int i = 1; i<= 12; i++)
        {
            float harmonicFrequency = fundementalToneFrequency * i;
            float increment = harmonicFrequency * 2f * Mathf.PI / samplingFrequency;

            phase[i-1] = phase[i - 1] + increment;
            if (phase[i - 1] > 2.0f * Mathf.PI) phase[i - 1] = 0;

            superImposed += Mathf.Sin(phase[i - 1]) * harmonicStrengths[i - 1];
        }
        


        return superImposed;
    }

        // Update is called once per frame
        void Update () {
	}
}
