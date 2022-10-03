using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DialogueController : MonoBehaviour
{
    public TextMeshProUGUI DialogueText;
    public float DialogueSpeed;
    public Animator DialogueAnimator;
    public string[] Sentences;
    private int Index = 0;
    private bool StartDialogue = true;

 
    // Update is called once per frame
    void Update()
    {
        //Input for next can be changed

        if(Input.GetKeyDown(KeyCode.V))
        {
            if (StartDialogue)
            {
                DialogueAnimator.SetTrigger("Enter");
                StartDialogue = false;
            }
            else
            {
                NextSentence();
            }
        }



    }

  

    void NextSentence()
    {
        if(Index <= Sentences.Length - 1)
        {
            DialogueText.text = "";
            StartCoroutine(WriteSentence());
        }
        else
        {
            DialogueText.text = "";
            DialogueAnimator.SetTrigger("Exit");
            Index = 0;
        }
    }

    IEnumerator WriteSentence()
    {
        foreach(char Character in Sentences[Index].ToCharArray())
        {
            DialogueText.text += Character;
            yield return new WaitForSeconds(DialogueSpeed);
            StartDialogue = true;
        }
        Index++;
    }
}
