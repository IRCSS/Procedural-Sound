using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StaffPlayer
{

    private MusicSheet sheetToPlay;
    private int beatPerMinute = 60;

    private float MeasureDuration;   // How long a measure takes, this is based on bpm, and time signature of the sheet
    private float pieceTimer;
    private float measureTimer;
    private int   currentMeasure;
    private StaffType staffToPlay;


    List<bool> measureNotesHaveBeenPlayed;
    List<int>  SharpsInTheMeasure;
    List<int>  FlatsInTheMeasure;
    List<int>  NaturalsInTheMeasure;

    public StaffPlayer(MusicSheet _sheetToPlay, int bpm, StaffType _staffToPlay)
    {
        sheetToPlay   = _sheetToPlay;
        beatPerMinute = bpm;
        staffToPlay = _staffToPlay;

        measureNotesHaveBeenPlayed = new List<bool>();
        SharpsInTheMeasure         = new List<int>();
        FlatsInTheMeasure          = new List<int>();
        NaturalsInTheMeasure       = new List<int>();

        InitializeMusicSheet();
    }

    public void UpdatePlayer(float deltaTime)
    {
        pieceTimer   += deltaTime;
        measureTimer += measureTimer;

        if (measureTimer >= MeasureDuration)
        {
            int nextMeasure = currentMeasure + 1;
            if (nextMeasure >= GetStaff().measuresInTheSheet.Length) nextMeasure = 0;
            SetMeasure(nextMeasure);
        }

        for(int i = 0; i< GetStaff().measuresInTheSheet[currentMeasure].notesInTheMeasure.Length; i++)
        {
            if (measureNotesHaveBeenPlayed[i] == true) continue;
            SheetNote note = GetStaff().measuresInTheSheet[currentMeasure].notesInTheMeasure[i];
            float notePositionInMeasureTime = (note.noteBegin * MeasureDuration )/ (sheetToPlay.noteValueSingleBeat * (float)sheetToPlay.beatsPerMeasure);
        }
        
        
        
    }

    Staff GetStaff()
    {
        switch (staffToPlay)
        {
            case StaffType.Bass:    return sheetToPlay.Bass;
            case StaffType.Treble:  return sheetToPlay.Treble;

        }
        return null;
    }

    void SetMeasure(int measureIndex)
    {
        measureTimer = Mathf.Max(0.0f, measureTimer - MeasureDuration);
        currentMeasure = measureIndex;
       
        measureNotesHaveBeenPlayed.Clear();
        int numberOfNotesInThisMeasure = GetStaff().measuresInTheSheet[currentMeasure].notesInTheMeasure.Length;
        for(int i = 0; i<numberOfNotesInThisMeasure; i++)
        {
            measureNotesHaveBeenPlayed.Add(false);
        }

        SharpsInTheMeasure  .Clear();
        FlatsInTheMeasure   .Clear();
        NaturalsInTheMeasure.Clear();
    }

    int determineMeasureFromTotalTime()
    {
        return (int)(pieceTimer / MeasureDuration);
    }

    void InitializeMusicSheet()
    {
        if (!sheetToPlay)
        {
            Debug.LogError("No Music sheet assigned to the pianist");
        }

        pieceTimer = 0.0f;
        measureTimer = 0.0f;
        MeasureDuration = sheetToPlay.beatsPerMeasure / beatPerMinute; // The duraton of a beat is 1/beat Per Minute. Since the measure has N beats inside, multiplying them gives the total length of a measure
        currentMeasure = determineMeasureFromTotalTime();


    }
}
public class Pianist : MonoBehaviour {


    [Header("Music Sheet ")]
    public MusicSheet sheetToPlay;
    public int        beatPerMinute = 60;
    

    [Header("Debug Keyboard")]
    public KeyNamesToIndicies firstKey = KeyNamesToIndicies.C4;   //These two are for debug porpuses
    public KeyCode[] notesKeyCodes = new KeyCode[12];             // keep at 12, not many keys on the keyboard and 1 octave is enough for debug


    private Piano piano;


    // Use this for initialization
    void Start () {
        piano = GameObject.FindObjectOfType<Piano>();
        if (!piano) Debug.LogError("The Pianist couldnt find its piano!");
        

    }
	


	// Update is called once per frame
	void Update () {

        PlayWithPCKeyboard();
    }


    // HELPER




    private void PlayWithPCKeyboard()
    {
        for (int i = 0; i < notesKeyCodes.Length; i++)
        {

            KeyCode kc = notesKeyCodes[i];
            KeyNamesToIndicies currentNote = firstKey + i;
            if (Input.GetKeyUp(kc))
            {
                piano.ReleaseANote(currentNote);
            }

            if (Input.GetKeyDown(kc))
            {
                piano.PressANote(currentNote);

            }
        }
    }
}
