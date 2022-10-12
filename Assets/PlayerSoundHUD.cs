using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSoundHUD : MonoBehaviour
{
    float sound;
    List<float> averageSound;
    Slider soundSlider; 
    // Start is called before the first frame update
    void Start()
    {
        EventManager.StartListening(EventManager.EventType.PlayerSoundLevel, PlayerSoundLevel);
        soundSlider = GetComponent<Slider>();
        averageSound = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        if (averageSound.Count <= 50)
        {
            averageSound.Insert(0, sound);
        }
        else
        {
            averageSound.RemoveAt(averageSound.Count - 1);
            averageSound.Insert(0, sound);
        }
        foreach (float i in averageSound)
        {
            sound += i;
        }

        soundSlider.value = (sound / averageSound.Count) / 2f;
    }

    void PlayerSoundLevel(Dictionary<string, object> message)
    {
        if (message["soundLevel"] is float)
        {
            sound = (float)message["soundLevel"];
        }

    }
}
