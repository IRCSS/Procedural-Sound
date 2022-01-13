using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StaffPlayer
{


    public class NoteBeingPlayed
    {
        public float noteTimer;
        public float noteDuration;
        public KeyNamesToIndicies theNote;

        public NoteBeingPlayed(float _noteDuration, KeyNamesToIndicies note)
        {
            noteDuration = _noteDuration;
            theNote = note;
        }
    }

 
    private MusicSheet sheetToPlay;
    private int beatPerMinute = 60;

    private float MeasureDuration;   // How long a measure takes, this is based on bpm, and time signature of the sheet
    private float pieceTimer;
    private float measureTimer;
    private int   currentMeasure;
    private StaffType staffToPlay;
    private Piano ref_Piano;

    List<bool> measureNotesHaveBeenPlayed;
    List<int>  SharpsInTheMeasure;
    List<int>  FlatsInTheMeasure;
    List<int>  NaturalsInTheMeasure;

    List<NoteBeingPlayed> notesCurrentlyPlaying;
    public StaffPlayer(MusicSheet _sheetToPlay, int bpm, StaffType _staffToPlay, Piano piano)
    {
        sheetToPlay   = _sheetToPlay;
        beatPerMinute = bpm;
        staffToPlay = _staffToPlay;
        ref_Piano = piano;

        measureNotesHaveBeenPlayed = new List<bool>();
        SharpsInTheMeasure         = new List<int>();
        FlatsInTheMeasure          = new List<int>();
        NaturalsInTheMeasure       = new List<int>();
        

        notesCurrentlyPlaying = new List<NoteBeingPlayed>();

        InitializeMusicSheet();
    }

    

    public void UpdatePlayer(float deltaTime)
    {
        pieceTimer   += deltaTime;
        measureTimer += deltaTime;
        UpdateCurrentlyBeingPlayedNotes(deltaTime);
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
            if (notePositionInMeasureTime <= measureTimer)
            {
                measureNotesHaveBeenPlayed[i] = true;
                PlayANote(note);
            }

        }
        
    }

    void UpdateCurrentlyBeingPlayedNotes(float deltaTime)
    {
        List<int> toRemove = new List<int>();

        for(int i = 0; i<notesCurrentlyPlaying.Count; i++)
        {
            notesCurrentlyPlaying[i].noteTimer += deltaTime;
            if(notesCurrentlyPlaying[i].noteTimer> notesCurrentlyPlaying[i].noteDuration)
            {
                ref_Piano.ReleaseANote(notesCurrentlyPlaying[i].theNote);
                toRemove.Add(i);
            }
        }

        for(int i = toRemove.Count-1; i>=0; i--)
        {
            notesCurrentlyPlaying.RemoveAt(i);
        }
        

    }

    void RemoveSemiToneIfExists(int indexOnTheKey)
    {
        SharpsInTheMeasure  .Remove(indexOnTheKey);
        FlatsInTheMeasure   .Remove(indexOnTheKey);
        NaturalsInTheMeasure.Remove(indexOnTheKey);
    }

    int ReturnNoteOffsetFromKeyToScientific(int keyIndexOffset)
    {
        int absIndex     = keyIndexOffset;
        int wholeOctaves = absIndex / 7;
        int noteInKey    = absIndex % 7;

        switch (staffToPlay)
        {
            case StaffType.Treble:
                
                // Treble starts from E4, so the pattern would be 
                // E4, F4, FSharp4, G4, GSharp4, A4, ASharp4, B4, C5, CSharp5, D5, DSharp5
                
                int[] semiToneIncludedOffset = new int[] { 0, 0, 1, 2, 3, 3, 4 };
                
                return wholeOctaves * 12 + semiToneIncludedOffset[noteInKey]  +noteInKey;
                
            case StaffType.Bass:
                // The bass starts at G2, so the pattern is
                //  G2, GSharp2, A2, ASharp2, B2, C3, CSharp3, D3, DSharp3, E3, F3, FSharp3
                int[] semiToneIncludedOffsetBass = new int[] { 0, 1, 2, 3, 3, 4, 5 };
                return wholeOctaves * 12 + semiToneIncludedOffsetBass[noteInKey] + noteInKey;
               
        }
        return 0;
    }

    void PlayANote(SheetNote note)
    {
        
        float noteDurationInSeconds = (note.noteDuration * MeasureDuration )  / (sheetToPlay.noteValueSingleBeat * (float)sheetToPlay.beatsPerMeasure);
        
        switch (note.semiToneSymbol)  // Take care of semi tone annotations. 
        {
            case SemitoneAttachments.Flat:
                RemoveSemiToneIfExists(note.notePositionInKey);
                FlatsInTheMeasure.Add(note.notePositionInKey);
                break;
            case SemitoneAttachments.Sharp:
                RemoveSemiToneIfExists(note.notePositionInKey);
                SharpsInTheMeasure.Add(note.notePositionInKey);
                break;
            case SemitoneAttachments.Natural:
                RemoveSemiToneIfExists(note.notePositionInKey);
                NaturalsInTheMeasure.Add(note.notePositionInKey);
                break;
        }

        KeyNamesToIndicies noteAsScientificPitch = (KeyNamesToIndicies)(ReturnNoteOffsetFromKeyToScientific(note.notePositionInKey) + GetStaffFirstNoteAsSP());
        KeyNameInOctave noteInOctave = KeyIndeciesToFrequencies.GetKeyName((int)noteAsScientificPitch);

        int semiToneOffset = 0;

        switch (GetStaff().Key)          // Take care of turning tones to semi tones on major or minor scales
        {
            case KeyScale.GMajor:
                if (noteInOctave == KeyNameInOctave.F && !NaturalsInTheMeasure.Contains(note.notePositionInKey)) semiToneOffset++;
                break;
        }

        // Take care of turning tones to semi tones through the marks in the music sheet

        if (SharpsInTheMeasure.Contains(note.notePositionInKey)) semiToneOffset++;
        if (FlatsInTheMeasure.Contains(note.notePositionInKey))  semiToneOffset--;

        noteAsScientificPitch = (KeyNamesToIndicies) ((int)noteAsScientificPitch + semiToneOffset);

        NoteBeingPlayed nbp = new NoteBeingPlayed(noteDurationInSeconds, noteAsScientificPitch);
        if(staffToPlay == StaffType.Treble) 
        Debug.Log("The Treble note is: " + noteAsScientificPitch + ", which has index: " + (int)(noteAsScientificPitch));

        ref_Piano.PressANote(noteAsScientificPitch);
        notesCurrentlyPlaying.Add(nbp);
    }

    int GetStaffFirstNoteAsSP()
    {
        switch (staffToPlay)
        {
            case StaffType.Bass:    return (int)KeyNamesToIndicies.G2;
            case StaffType.Treble:  return (int)KeyNamesToIndicies.E4;
        }
        return 0;
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
        MeasureDuration = (float)sheetToPlay.beatsPerMeasure * 60.0f / beatPerMinute; // The duraton of a beat is 1/beat Per Minute. Since the measure has N beats inside, multiplying them gives the total length of a measure
        
        currentMeasure = determineMeasureFromTotalTime();
        SetMeasure(currentMeasure);

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

    public bool PlaySongLeftHand  = true;
    public bool PlaySongRightHand = true;

    StaffPlayer leftHand;
    StaffPlayer rightHand;


    // Use this for initialization
    void Start () {
        piano = GameObject.FindObjectOfType<Piano>();
        if (!piano) Debug.LogError("The Pianist couldnt find its piano!");

        leftHand  = new StaffPlayer(sheetToPlay, beatPerMinute, StaffType.Bass, piano);
        rightHand = new StaffPlayer(sheetToPlay, beatPerMinute, StaffType.Treble, piano);


    }
	


	// Update is called once per frame
	void Update () {
        PlayWithPCKeyboard();
        if(PlaySongLeftHand)  leftHand.UpdatePlayer(Time.deltaTime);
        if(PlaySongRightHand) rightHand.UpdatePlayer(Time.deltaTime);
      
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
