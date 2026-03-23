using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScorePresenter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreTextMesh;
    [SerializeField] private TextMeshProUGUI enemyScoreTextMesh ;


    void OnEnable()
    {
        EventBus.OnRecieveResult += SetPlayerScores;
    }

    void OnDisable()
    {
        EventBus.OnRecieveResult -= SetPlayerScores;
    }
    
    void SetPlayerScores(int enemyScore, int playerScore)
    {
        Debug.Log("SetPlayerScores");
        scoreTextMesh.text = "Score:" + playerScore.ToString();
        enemyScoreTextMesh.text = "Enemy Score: " + enemyScore.ToString();
    }
}
