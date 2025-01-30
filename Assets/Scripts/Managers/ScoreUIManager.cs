using UnityEngine;
using UnityEngine.UI;

public class ScoreUIManager : MonoBehaviour
{
    [SerializeField] Text _circleScoreText;
    [SerializeField] Text _crossScoreText;

    #region Unity Callbacks

    private void Start()
    {
        GameManager.Instance.OnMatchEnd += OnMatchEnd;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnMatchEnd -= OnMatchEnd;
    }

    #endregion

    private void OnMatchEnd(MatchResult matchResult)
    {
        _circleScoreText.text = GameManager.Instance.CircleScore.ToString();
        _crossScoreText.text = GameManager.Instance.CrossScore.ToString();
    }
}
