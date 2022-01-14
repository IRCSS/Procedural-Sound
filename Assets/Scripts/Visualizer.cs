using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

public class Visualizer : MonoBehaviour
{

    // The visualizer is written for a 49 keys keyboard. It has a key board part and a waveform visualisation part.

    // WAVEFORM Visualusation
    // Simply takes the data passed to the speaker and visualize it. However for legibility reasons, I have *zommed* in 
    // and only showing first 128 of the samples. the first 128 samples correspond to 0.0027 or 2.7 milisecond
    // This means any frequency deeper than 370 hz wont fit in this visualisation, and only part of the wave form can be shown. 
    // This is around FSharp4 (the Fsharp of the middle C octave). Anything lower is still visibile but only partially 

    // PIANO VISUALISATION
    // The 49 key piano begins with the white key C2 and ends with the white key C6, spanning 4 octaves
    // There are a total of 28 white keys and 21 black keys. I will save the sharp and white keys
    // in seperate look up array for easy shader code

    private CommandBuffer cb;
    private Camera maincam;
    private ComputeBuffer waveFormSamplesBuffer; // There is 1024 floats in this 
    private ComputeBuffer whiteKeysPressedBuffer;
    private ComputeBuffer blackKeysPressedBuffer;

    private Material visualizerMat;



    private bool audioSampleIsDirty;
    private bool whiteKeypressedBufferIsDirty;
    private bool blackKeypressedBufferIsDirty;

    private float[] audioSamples;

     KeyNamesToIndicies[] whiteKeysIn49Keyboard = new KeyNamesToIndicies[] { KeyNamesToIndicies.C2, KeyNamesToIndicies.D2, KeyNamesToIndicies.E2, KeyNamesToIndicies.F2, KeyNamesToIndicies.G2, KeyNamesToIndicies.A2, KeyNamesToIndicies.B2,
                                                                             KeyNamesToIndicies.C3, KeyNamesToIndicies.D3, KeyNamesToIndicies.E3, KeyNamesToIndicies.F3, KeyNamesToIndicies.G3, KeyNamesToIndicies.A3, KeyNamesToIndicies.B3,
                                                                             KeyNamesToIndicies.C4, KeyNamesToIndicies.D4, KeyNamesToIndicies.E4, KeyNamesToIndicies.F4, KeyNamesToIndicies.G4, KeyNamesToIndicies.A4, KeyNamesToIndicies.B4,
                                                                             KeyNamesToIndicies.C5, KeyNamesToIndicies.D5, KeyNamesToIndicies.E5, KeyNamesToIndicies.F5, KeyNamesToIndicies.G5, KeyNamesToIndicies.A5, KeyNamesToIndicies.B5};


    KeyNamesToIndicies[] blackKeysIn49Keyboard = new KeyNamesToIndicies[] {  KeyNamesToIndicies.CSharp2, KeyNamesToIndicies.DSharp2,  KeyNamesToIndicies.FSharp2, KeyNamesToIndicies.GSharp2, KeyNamesToIndicies.ASharp2,
                                                                             KeyNamesToIndicies.CSharp3, KeyNamesToIndicies.DSharp3,  KeyNamesToIndicies.FSharp3, KeyNamesToIndicies.GSharp3, KeyNamesToIndicies.ASharp3,
                                                                             KeyNamesToIndicies.CSharp4, KeyNamesToIndicies.DSharp4,  KeyNamesToIndicies.FSharp4, KeyNamesToIndicies.GSharp4, KeyNamesToIndicies.ASharp4,
                                                                             KeyNamesToIndicies.CSharp5, KeyNamesToIndicies.DSharp5,  KeyNamesToIndicies.FSharp5, KeyNamesToIndicies.GSharp5, KeyNamesToIndicies.ASharp5};


    private float[] whiteKeysPressedStateArray;
    private float[] blackKeysPressedStateArray;

    void Start()
    {

        maincam = Camera.main;
        if (!maincam) Debug.LogError("ERROR: no camera in the scene is taged as Main Cam");

        cb = new CommandBuffer()
        {
            name = "Visualizer"
        };

        visualizerMat = new Material(Shader.Find("Unlit/VisualizerShader"));

        cb.Blit(null, BuiltinRenderTextureType.CameraTarget, visualizerMat);

        maincam.AddCommandBuffer(CameraEvent.AfterEverything, cb);

        //----

        Piano piano = FindObjectOfType<Piano>();
        if (!piano) Debug.LogError("ERROR: The visualiser Couldnt find the piano");

        piano.OnAudioFrameSampleGotUpdate += OnAudioFrameSampleGotUpdate;
        piano.OnPianoKeyPressed           += KeyPressed;
        piano.OnPianoKeyReleased          += KeyReleased;
        //----

        waveFormSamplesBuffer = new ComputeBuffer(1024, sizeof(float));

        visualizerMat.SetBuffer("_WaveForm", waveFormSamplesBuffer);

        whiteKeysPressedBuffer = new ComputeBuffer(28, sizeof(float));
        blackKeysPressedBuffer = new ComputeBuffer(21, sizeof(float));

        visualizerMat.SetBuffer("_whiteKeysPressedState", whiteKeysPressedBuffer);
        visualizerMat.SetBuffer("_blackKeysPressedState", blackKeysPressedBuffer);

        whiteKeysPressedStateArray = new float[28];
        blackKeysPressedStateArray = new float[21];

        FillArrayF(whiteKeysPressedStateArray, 0.0f);
        FillArrayF(blackKeysPressedStateArray, 0.0f);

        whiteKeysPressedBuffer.SetData(whiteKeysPressedStateArray);
        blackKeysPressedBuffer.SetData(blackKeysPressedStateArray);

    }


    private void OnDestroy()
    {
        waveFormSamplesBuffer.Release();

        Piano piano = FindObjectOfType<Piano>();
        if (!piano) return;
        piano.OnAudioFrameSampleGotUpdate -= OnAudioFrameSampleGotUpdate;
        piano.OnPianoKeyPressed           -= KeyPressed;
        piano.OnPianoKeyReleased          -= KeyReleased;
    }
    // Update is called once per frame
    void Update()
    {


        if (audioSampleIsDirty)
        {
            waveFormSamplesBuffer.SetData(audioSamples);
            audioSampleIsDirty = false;
        }

        if (whiteKeypressedBufferIsDirty)
        {
            whiteKeysPressedBuffer.SetData(whiteKeysPressedStateArray);
            whiteKeypressedBufferIsDirty = false;
        }

        if (blackKeypressedBufferIsDirty)
        {
            blackKeysPressedBuffer.SetData(blackKeysPressedStateArray);
            blackKeypressedBufferIsDirty = false;
        }
        



    }

    // __________________________________________________________________________________________

    public void OnAudioFrameSampleGotUpdate(float[] data)
    {
        audioSampleIsDirty = true;
        audioSamples       = data;
    }

    public void KeyPressed(KeyNamesToIndicies notePressed)
    {
        if (IsWhiteKey(notePressed))
        {
           int indexForDrawing =  System.Array.BinarySearch(whiteKeysIn49Keyboard, notePressed);

            if(indexForDrawing<0 || indexForDrawing>= whiteKeysPressedStateArray.Length)
            {
                Debug.LogWarning("WARNING: Visualizer attempted to visualze notes that dont exist in the 49 keys piano");
                return;
            }

            whiteKeysPressedStateArray[indexForDrawing] = 1.0f;
            whiteKeypressedBufferIsDirty                = true;

        } else
        {
            int indexForDrawing = System.Array.BinarySearch(blackKeysIn49Keyboard, notePressed);
            if (indexForDrawing < 0 || indexForDrawing >= blackKeysPressedStateArray.Length)
            {
                Debug.LogWarning("WARNING: Visualizer attempted to visualze notes that dont exist in the 49 keys piano");
                return;
            }

            blackKeysPressedStateArray[indexForDrawing] = 1.0f;
            blackKeypressedBufferIsDirty                = true;

        }
    }
    public void KeyReleased(KeyNamesToIndicies notePressed)
    {
        if (IsWhiteKey(notePressed))
        {
           int indexForDrawing =  System.Array.BinarySearch(whiteKeysIn49Keyboard, notePressed);

            if(indexForDrawing<0 || indexForDrawing>= whiteKeysPressedStateArray.Length)
            {
                Debug.LogWarning("WARNING: Visualizer attempted to visualze notes that dont exist in the 49 keys piano");
                return;
            }

            whiteKeysPressedStateArray[indexForDrawing] = 0.0f;
            whiteKeypressedBufferIsDirty                = true;

        } else
        {
            int indexForDrawing = System.Array.BinarySearch(blackKeysIn49Keyboard, notePressed);
            if (indexForDrawing < 0 || indexForDrawing >= blackKeysPressedStateArray.Length)
            {
                Debug.LogWarning("WARNING: Visualizer attempted to visualze notes that dont exist in the 49 keys piano");
                return;
            }

            blackKeysPressedStateArray[indexForDrawing] = 0.0f;
            blackKeypressedBufferIsDirty                = true;

        }

    }
    private bool IsWhiteKey(KeyNamesToIndicies keyToAsses)
    {
        KeyNameInOctave keyName = KeyIndeciesToFrequencies.GetKeyName((int)keyToAsses);

        if(keyName == KeyNameInOctave.CSharp || 
           keyName == KeyNameInOctave.DSharp || 
           keyName == KeyNameInOctave.FSharp || 
           keyName == KeyNameInOctave.GSharp ||
           keyName == KeyNameInOctave.ASharp) return false;

        return true;

    }

    void FillArrayF(float[] toFill, float value)
    {
        for(int i = 0; i<toFill.Length; i++)
        {
            toFill[i] = value;
        }

    }


}
