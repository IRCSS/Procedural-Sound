using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SemitoneAttachments
{
    None, Sharp, Flat, Natural
}

[System.Serializable]
public class SheetNote
{
    public float               noteBegin;         // At which beat does the note begin in a measure. For example if the time signature is 4/4 and it is on the second beat, this will be 2
    public float               noteDuration;      // Again in beat units.No time used anywhere in the sheet.
    public int                 notePositionInKey; // This is the note position on the staff, for example in the treble G Major, the G is the 2. I did it like this instead of scientific pitch
                                                  // because it is easier and faster to input music sheets. Also it is easy to switch keys on the same notes and it automaticly takes care of the sharps
    public SemitoneAttachments semiToneSymbol;    // this can push the note up or down by half the step, for example F to F#. It is a global effect, after this any note which has this symbol will be a
                                                  // sharp until a natural pops up or the measure ends
}

[System.Serializable]
public class Measure // A measure is a unit including a series of beats, for example 4 beats
{
    public SheetNote[] notesInTheMeasure;
}

[System.Serializable]
public class Staff
{
    public KeyScale  Key;
    public Measure[] measuresInTheSheet;
}

public enum KeyScale
{
    GMajor
}

public enum StaffType
{
    Treble, Bass
}
[CreateAssetMenu(fileName = "AMusicSheet", menuName = "ScriptableObjects/MusicSheet", order = 1)]
public class MusicSheet : ScriptableObject {

    // For translating a music sheet to this format, here are some helpers to help along. 
    // You need to take the notes from the treble and bass cleff and turn them to the midi indice/ scientific pitch notation
    // The middle C (C4) is at key 48. The middle C is one ledger below the first line  of the treble cleff and one ledger above the 
    // last line of the bass cleff. 
    // First line of the Bass Cleff is a G, in 2 octaves below the middle C, so G2, which is the key 31
    // Last line of the Bass Cleff is an A, two notes under the middle C, so key 45, or A3
    // The First Line of the treble cleff is an E, 2 notes above a middle C, so at 52, E4
    // The last line of the treble cleff is a F, 2 octaves above the middle C, so F5, with key index of 65

    public int beatsPerMeasure;
    public float noteValueSingleBeat;

    public Staff Treble;
    public Staff Bass;

}
