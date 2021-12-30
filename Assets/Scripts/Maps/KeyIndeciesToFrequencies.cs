using UnityEngine;


public enum KeyNamesToIndicies
{    /*    +0    +1     +2   +3      +4  +5    +6     +7    +8    +9    +10    +11   */
/*  0*/    C0, CSharp0, D0, DSharp0, E0, F0, FSharp0, G0, GSharp0, A0, ASharp0, B0, // sub contra
/* 12*/    C1, CSharp1, D1, DSharp1, E1, F1, FSharp1, G1, GSharp1, A1, ASharp1, B1, // contra
/* 24*/    C2, CSharp2, D2, DSharp2, E2, F2, FSharp2, G2, GSharp2, A2, ASharp2, B2, // great
/* 36*/    C3, CSharp3, D3, DSharp3, E3, F3, FSharp3, G3, GSharp3, A3, ASharp3, B3, // small
/* 48*/    C4, CSharp4, D4, DSharp4, E4, F4, FSharp4, G4, GSharp4, A4, ASharp4, B4, // one line
/* 60*/    C5, CSharp5, D5, DSharp5, E5, F5, FSharp5, G5, GSharp5, A5, ASharp5, B5, // second line
/* 72*/    C6, CSharp6, D6, DSharp6, E6, F6, FSharp6, G6, GSharp6, A6, ASharp6, B6, // third line
/* 84*/    C7, CSharp7, D7, DSharp7, E7, F7, FSharp7, G7, GSharp7, A7, ASharp7, B7, // fourth line
/* 96*/    C8, CSharp8, D8, DSharp8, E8, F8, FSharp8, G8, GSharp8, A8, ASharp8, B8  // fith line
}

public enum OctavesName
{
    sub_contra, contra, great, small, one_line, second_line, third_line, fourth_line, fith_line
}

public enum KeyNameInOctave
{
    C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B
}

[CreateAssetMenu(fileName = "KeyIndexToFrequencyMapping", menuName = "ScriptableObjects/KeyIndeciesToFrequencies", order = 1)]
public class KeyIndeciesToFrequencies : ScriptableObject
{
    public float[]   fundementalFrequencies;

    public KeyIndeciesToFrequencies()
    {
        fundementalFrequencies = new float[]
        {
              16.35f,   17.32f,   18.35f,   19.45f,   20.60f,   21.83f,   23.12f,   24.50f,   25.96f,   27.50f,   29.14f,   30.87f,
              32.70f,   34.65f,   36.71f,   38.89f,   41.20f,   43.65f,   46.25f,   49.00f,   51.91f,   55.00f,   58.27f,   61.74f,
              65.41f,   69.30f,   73.42f,   77.78f,   82.41f,   87.31f,   92.50f,   98.00f,  103.83f,  110.00f,  116.54f,  123.47f,
             130.81f,  138.59f,  146.83f,  155.56f,  164.81f,  174.61f,  185.00f,  196.00f,  207.65f,  220.00f,  233.08f,  246.94f,
             261.63f,  277.18f,  293.66f,  311.13f,  329.63f,  349.23f,  369.99f,  392.00f,  415.30f,  440.00f,  466.16f,  493.88f,
             523.25f,  554.37f,  587.33f,  622.25f,  659.25f,  698.46f,  739.99f,  783.99f,  830.61f,  880.00f,  932.33f,  987.77f,
            1046.50f, 1108.73f, 1174.66f, 1244.51f, 1318.51f, 1396.91f, 1479.98f, 1567.98f, 1661.22f, 1760.00f, 1864.66f, 1975.53f,
            2093.00f, 2217.46f, 2349.32f, 2489.02f, 2637.02f, 2793.83f, 2959.96f, 3135.96f, 3322.44f, 3520.00f, 3729.31f, 3951.07f,
            4186.01f, 4434.92f, 4698.63f, 4978.03f, 5274.04f, 5587.65f, 5919.91f, 6271.93f, 6648.88f, 7040.00f, 7458.62f, 7902.14f

        };
    }

    public static KeyNamesToIndicies GetKeyIndexName(int index) { return (KeyNamesToIndicies) index; }
    public static OctavesName GetOctaveName(int keyIndex) { return (OctavesName)(keyIndex / 12); }
    public static KeyNameInOctave GetKeyName(int index) { return (KeyNameInOctave)(index % 12); }
}