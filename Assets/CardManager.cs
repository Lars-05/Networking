using UnityEngine;

public class CardManager : MonoBehaviour
{
    Sprite[] cardSprites;
   
    Sprite GetCard(int number)
    {
        return cardSprites[number];
    }
    
    
    
}
