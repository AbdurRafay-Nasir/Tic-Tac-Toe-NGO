using UnityEngine;
using UnityEngine.UI;

public class RematchUIManager : MonoBehaviour
{
    [SerializeField] Button _rematchButton;

    #region Unity Callbacks

    private void Awake()
    {
        _rematchButton.onClick.AddListener(OnRematchButtonClicked);

        _rematchButton.gameObject.SetActive(false);
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

        _rematchButton.onClick.RemoveListener(OnRematchButtonClicked);
    }

    #endregion

    private void OnMatchEnd(MatchResult matchResult)
    {
        // Show rematch button on all clients
        _rematchButton.gameObject.SetActive(true);
    }

    private void OnMatchRestarted()
    {
        // Hide rematch button on remote clients

        // Local Client  - You as a player, playing on your own 
        //                 pc are considered local client
        // Remote Client - Other players connected with you are called
        //                 remote clients

        // Local client hides the rematch button when they press it.
        // However this does not hide the rematch button on remote clients
        // Fortunately, when any client clicks on rematch button
        // GameManager's OnMatchRestarted is invoked for all clients
        // this is where we can hide the Rematch button.

        // For example, assume that there are 2 clients; C1 and C2.
        // When C2 clicks on rematch button, the GameManager's 
        // OnMatchRestarted is invoked. C1 is already listening to it
        // and so it can hide its own rematch button.

        // If this still not makes sense, Comment out following code
        // and then test.
        _rematchButton.gameObject.SetActive(false);
    }

    private void OnRematchButtonClicked()
    {
        // Restart the game
        GameManager.Instance.RestartGame();

        // Hide the rematch button as it completed it's job.
        _rematchButton.gameObject.SetActive(false);
    }
}
