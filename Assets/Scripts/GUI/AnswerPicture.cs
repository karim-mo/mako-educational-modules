﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class AnswerPicture : MonoBehaviour
{
    public Image buttonImage;
    [HideInInspector]
    public bool Correct;

    public void initButton(Sprite img, bool correct)
    {
        buttonImage.sprite = img;
        Correct = correct;
        gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (Correct)
            {
                // Send positive feedback to robot
                // Update game manager with correct answer to proceed with the questions
                NetworkManager._Manager.sendExpression("hf");
                NetworkManager._Manager.sendServoExpression("right_up");
                Topics._topics.CorrectAnswerTrigger();
                Debug.Log("This is the correct answer");
            }
            else
            {
                // Send Negative feedback to robot
                NetworkManager._Manager.sendExpression("sf");
                Topics._topics.WrongAnswerTrigger();
                Debug.Log("This is the wrong answer");
            }
        });
    }
}
