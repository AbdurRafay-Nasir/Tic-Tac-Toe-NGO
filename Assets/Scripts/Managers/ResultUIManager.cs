using UnityEngine;
using UnityEngine.UI;

public class ResultUIManager : MonoBehaviour
{
    [SerializeField] Text _resultText;
    [SerializeField] GameObject _resultTextParent;

    [SerializeField] Color _winColor;
    [SerializeField] Color _loseColor;

    #region Unity Callbacks

    private void Awake()
    {
        _resultTextParent.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnMatchEnd += OnMatchEnd;
        GameManager.Instance.OnMatchRestarted += OnMatchRestarted;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnMatchEnd -= OnMatchEnd;
        GameManager.Instance.OnMatchRestarted -= OnMatchRestarted;
    }

    #endregion

    private void OnMatchEnd(MatchResult matchResult)
    {
        _resultTextParent.SetActive(true);

        if (matchResult.winner == GameManager.Instance.LocalPlayerType)
        {
            _resultText.text = "WINNER";
            _resultText.color = _winColor;
        }
        else
        {
            _resultText.text = "LOOSER";
            _resultText.color = _loseColor;
        }
    }

    private void OnMatchRestarted()
    {
        _resultTextParent.SetActive(false);
    }
}
