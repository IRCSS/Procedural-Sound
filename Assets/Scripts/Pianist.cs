using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pianist : MonoBehaviour {


    
    public KeyNamesToIndicies firstKey = KeyNamesToIndicies.C4;   //These two are for debug porpuses
    public KeyCode[] notesKeyCodes = new KeyCode[12];             // keep at 12, not many keys on the keyboard and 1 octave is enough for debug


    private Piano piano;
    // Use this for initialization
    void Start () {
        piano = GameObject.FindObjectOfType<Piano>();
        if (!piano) Debug.LogError("The Pianist couldnt find its piano!");
	}
	

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

	// Update is called once per frame
	void Update () {

      
    }
}
