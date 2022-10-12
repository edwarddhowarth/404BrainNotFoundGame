using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertStateVisual : MonoBehaviour
{
    public Sprite suspicious;
    public Sprite alert;
    private SpriteRenderer ren;
    private AIStateController aisc;
    // Start is called before the first frame update
    void Start()
    {
        ren = GetComponent<SpriteRenderer>();
        aisc = transform.parent.parent.GetComponent<AIStateController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(aisc.currentAlertState == AIStateController.AIAlertState.Suspicious)
        {
            ren.sprite = suspicious;
        }
        else if(aisc.currentAlertState == AIStateController.AIAlertState.Aware || aisc.currentAlertState == AIStateController.AIAlertState.InCombat)
        {
            ren.sprite = alert;
        }
        else
        {
            ren.sprite = null;
        }
        
    }
}
