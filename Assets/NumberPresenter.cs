using System;
using TMPro;
using UnityEngine;

public class NumberPresenter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberDisplay;

    public void DisplayNumber(int number)
    {
        numberDisplay.text = number.ToString();
    }

    
}
