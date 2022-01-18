using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piano : MonoBehaviour {

    public delegate void AudioFrameSampleGotUpdate(float[] data);
    public AudioFrameSampleGotUpdate OnAudioFrameSampleGotUpdate;

    public delegate void PianoKeyPressed (KeyNamesToIndicies notePressed);
    public delegate void PianoKeyReleased(KeyNamesToIndicies noteToRelease);

    public PianoKeyPressed  OnPianoKeyPressed;
    public PianoKeyReleased OnPianoKeyReleased;
    // --------------------------------------
    // Public
    public KeyIndeciesToFrequencies fundementalToneFrequencies;
    public  float gain;
    public int firstKey     = 9;   // Grand piano strts at A0
    public int numberOfKeys = 88;  // It has 88 keys and ends at C8
    public ADSR keysADSR;

    public  float[] harmonicStrengths = new float[12];

    private float   samplingFrequency;     // this is the number of samples we use per second,to construct the sound waveforms.
                                           // default is 48,000 samples. This means if your frame rate is 60 fps, in each frame you need to provide 48k/60 samples. 

    private AudioSource ad_source;
    
    private float fundementalToneFrequency;

    private float scaleTimer = 0;
    private ActiveNote[] currentlyBeingPlayed = new ActiveNote[88];


    private float[] perAudioFrameSampleData;

    void Start ()
    {
        ad_source         = gameObject.AddComponent<AudioSource>();
        samplingFrequency = AudioSettings.outputSampleRate;

        for(int i = 0; i< currentlyBeingPlayed.Length; i++)
        {
            int freqIndexOfCurrentKey = firstKey + i;

            currentlyBeingPlayed[i].fundementalFrequency = fundementalToneFrequencies.fundementalFrequencies[freqIndexOfCurrentKey];
        }

        perAudioFrameSampleData = new float[1024]; // Change this if something baout sample rate changes, souldnt be hardcoded, but here we are! 




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
        
        for(int j = 0; j<currentlyBeingPlayed.Length; j++)
        {

            float timeSinceNoteStartedPlaying = (float)(AudioSettings.dspTime - currentlyBeingPlayed[j].startPlayTime);

            float volumeModifier = keysADSR.Sustain;
            if (timeSinceNoteStartedPlaying <= keysADSR.Attack)    // It is in the Attack phase, the sound is still rising form 0 top to maximum (1) 
            {
               
                volumeModifier = Mathf.InverseLerp(0.0f, keysADSR.Attack, timeSinceNoteStartedPlaying);
            } else if(timeSinceNoteStartedPlaying < keysADSR.Decay + keysADSR.Attack)   // The sound is in the decay phase meaning it is going from the maximum to the sustained level
            {
                volumeModifier = Mathf.InverseLerp(keysADSR.Attack, keysADSR.Attack + keysADSR.Decay, timeSinceNoteStartedPlaying);
                volumeModifier = Mathf.Lerp(1.0f, keysADSR.Sustain, volumeModifier);
            }
            

            if (!currentlyBeingPlayed[j].isBeingPlayed )              // The key is not being held any more, this is not a realistic piano as it can hold a note on sustain forever, it only goes to release when you release a key! 
            {

                timeSinceNoteStartedPlaying = (float)(AudioSettings.dspTime - currentlyBeingPlayed[j].releaseTime);
               
                if (timeSinceNoteStartedPlaying > keysADSR.Release) continue;  // Skip the contribution of this piano key if it is not being played and it has already faded to 0
               
                volumeModifier = Mathf.InverseLerp(0.0f, keysADSR.Release, timeSinceNoteStartedPlaying);
                volumeModifier = Mathf.Lerp(keysADSR.Sustain, 0.0f, volumeModifier);
            }

            int currentDataStep = 0;
            fundementalToneFrequency = currentlyBeingPlayed[j].fundementalFrequency;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] += ReturnSuperimposedHarmonicsSeries(currentDataStep, currentlyBeingPlayed[j].startPlayTime) * gain * volumeModifier;
                currentDataStep++;
                if (channels == 2)
                {
                    data[i + 1] = data[i]; // if stereo, copy the one ear to the other, and simple jump over the channel 1 in the next iteration
                    i++;
                }
            }
        }

        // Storing the data for visualisation
        if (perAudioFrameSampleData.Length != data.Length / channels)
        {
            Debug.LogError("ERROR: Unmatching array length in the piano audio thread");
            return;
        }

        for(int i = 0; i < data.Length; i+= channels)
        {
            perAudioFrameSampleData[i / channels] = data[i];
        }
        if(OnAudioFrameSampleGotUpdate != null)
        OnAudioFrameSampleGotUpdate(perAudioFrameSampleData);
    }


    public float ReturnSuperimposedHarmonicsSeries(int dataIndex, double audioTime)
    {
        float superImposed = 0.0f;

        for(int i = 1; i<= 12; i++)
        {
            float harmonicFrequency = fundementalToneFrequency * i;

            float timeAtTheBeginig = (float)((AudioSettings.dspTime - audioTime) % (1.0 / (double)harmonicFrequency)); // very important to deal with percision issue as dspTime gets large

            float exactTime = timeAtTheBeginig + (float)dataIndex / samplingFrequency;


            superImposed += Mathf.Sin(exactTime * harmonicFrequency * 2f * Mathf.PI) * harmonicStrengths[i - 1];
        }
        
        return superImposed;
    }


    public void PressANote(KeyNamesToIndicies noteToPress)
    {
        int noteIndexOnPiano = (int)noteToPress - firstKey;
        if (noteIndexOnPiano < 0) { Debug.LogError("Attempted to play a note lower than what this piano can play"); return; }
        if (noteIndexOnPiano >= firstKey + numberOfKeys){ Debug.Log("Attempted to play a note higher than what this piano can play"); return;}
        PressPianoKeyAtIndex(noteIndexOnPiano);
    }

    public void ReleaseANote(KeyNamesToIndicies noteToRelease)
    {
        int noteIndexOnPiano = (int)noteToRelease - firstKey;
        if (noteIndexOnPiano < 0) { Debug.LogError("Attempted to play a note lower than what this piano can play"); return; }
        if (noteIndexOnPiano >= firstKey + numberOfKeys) { Debug.Log("Attempted to play a note higher than what this piano can play"); return; }
        ReleasePianoKeyAtIndex(noteIndexOnPiano);
    }

    public void PressPianoKeyAtIndex(int PianoKeynoteIndex)
    {
        currentlyBeingPlayed[PianoKeynoteIndex].startPlayTime = AudioSettings.dspTime;
        currentlyBeingPlayed[PianoKeynoteIndex].isBeingPlayed = true;

        if (OnPianoKeyPressed != null) OnPianoKeyPressed((KeyNamesToIndicies) PianoKeynoteIndex);
    }

    public void ReleasePianoKeyAtIndex(int PianoKeynoteIndex)
    {
        currentlyBeingPlayed[PianoKeynoteIndex].isBeingPlayed = false;
        currentlyBeingPlayed[PianoKeynoteIndex].releaseTime   = AudioSettings.dspTime;

        if (OnPianoKeyReleased != null) OnPianoKeyReleased((KeyNamesToIndicies)PianoKeynoteIndex);
    }

    // Update is called once per frame
    void Update () {

    }
}
