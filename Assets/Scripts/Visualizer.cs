using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    private CommandBuffer cb;
    private Camera maincam;
    private ComputeBuffer waveFormSamplesBuffer; // There is 1024 floats in this 

    private Material visualizerMat;



    private bool audioSampleIsDirty;
    private float[] audioSamples;
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
        //----

        waveFormSamplesBuffer = new ComputeBuffer(1024, sizeof(float));

        visualizerMat.SetBuffer("_WaveForm", waveFormSamplesBuffer);
        
    }



    public void OnAudioFrameSampleGotUpdate(float[] data)
    {
        audioSampleIsDirty = true;
        audioSamples = data;
    }


    private void OnDestroy()
    {
        waveFormSamplesBuffer.Release();

        Piano piano = FindObjectOfType<Piano>();
        if (!piano) return;
        piano.OnAudioFrameSampleGotUpdate -= OnAudioFrameSampleGotUpdate;
    }
    // Update is called once per frame
    void Update()
    {


        if (audioSampleIsDirty)
        {
            waveFormSamplesBuffer.SetData(audioSamples);
            audioSampleIsDirty = false;
        }

 
    }
}
