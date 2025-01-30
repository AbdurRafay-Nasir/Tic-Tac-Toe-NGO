using UnityEngine;

public class TurnUIManager : MonoBehaviour
{
    [SerializeField] GameObject circleYouTextGameObject;
    [SerializeField] GameObject crossYouTextGameObject;
    [SerializeField] GameObject circleArrowGameObject;
    [SerializeField] GameObject crossArrowGameObject;

    #region Unity Callbacks

    private void Awake()
    {
        circleYouTextGameObject.SetActive(false);
        crossYouTextGameObject.SetActive(false);
        circleArrowGameObject.SetActive(false);
        crossArrowGameObject.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnMatchBegin += OnMatchStart;
        GameManager.Instance.OnPlayablePlayerTypeChange += OnPlayablePlayerTypeChange;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnMatchBegin -= OnMatchStart;
        GameManager.Instance.OnPlayablePlayerTypeChange -= OnPlayablePlayerTypeChange;
    }

    #endregion

    #region Game Manager Callbacks

    private void OnMatchStart()
    {
        switch (GameManager.Instance.LocalPlayerType)
        {
            case PlayerType.Circle:
                circleYouTextGameObject.SetActive(true);
                break;

            case PlayerType.Cross:
                crossYouTextGameObject.SetActive(true);
                break;
        }

        switch (GameManager.Instance.CurrentPlayablePlayerType)
        {
            case PlayerType.Circle:
                circleArrowGameObject.SetActive(true);
                break;

            case PlayerType.Cross:
                crossArrowGameObject.SetActive(true);
                break;
        }

        GameManager.Instance.OnMatchBegin -= OnMatchStart;
    }

    private void OnPlayablePlayerTypeChange(PlayerType newPlayablePlayerType)
    {
        switch (GameManager.Instance.CurrentPlayablePlayerType)
        {
            case PlayerType.Circle:

                circleArrowGameObject.SetActive(true);
                crossArrowGameObject.SetActive(false);
                break;

            case PlayerType.Cross:

                crossArrowGameObject.SetActive(true);
                circleArrowGameObject.SetActive(false);
                break;
        }
    }

    #endregion
}
