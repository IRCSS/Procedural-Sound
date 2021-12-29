using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTone : MonoBehaviour {

    // --------------------------------------
    // Public
    public  float toneFrequency; 

    private float samplingFrequency;       // this is the number of samples we use per second,to construct the sound waveforms.
                                           // default is 48,000 samples. This means if your frame rate is 60 fps, in each frame you need to provide 48k/60 samples. 
                                           
    private AudioSource ad_source;

    private float phase;
	// Use this for initialization
	void Start () {
        ad_source         = gameObject.GetComponent<AudioSource>();
        samplingFrequency = AudioSettings.outputSampleRate;
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

        float timeAtTheBeginig = (float)(AudioSettings.dspTime%(1.0 / (double)toneFrequency)); // very important to deal with percision issue as dspTime gets large

        float increment = toneFrequency * 2f * Mathf.PI / samplingFrequency ;

        int currentSampleIndex = 0;
        for (int i = 0; i< data.Length; i++)
        {

            float exactTime = timeAtTheBeginig + (float)currentSampleIndex / samplingFrequency;
             data[i] = Mathf.Sin((exactTime * toneFrequency * 2f * Mathf.PI )) * 0.8f;

            // phase = phase + increment;                            // If you count your own phase (kind of like a timer), you dont have to deal with percision issue of float, however if you are playing several frequencies, each would require its own phase, which can get annoying
            // if (phase > 2 * Mathf.PI) phase = 0;
            // data[i] = Mathf.Sin(phase)*0.8f;

            currentSampleIndex++;

            if (channels == 2)
            {
                data[i + 1] = data[i]; // if stereo, copy the one ear to the other, and simple jump over the channel 1 in the next iteration
                i++;
            }
        }
    }

        // Update is called once per frame
        void Update () {
	}
}
