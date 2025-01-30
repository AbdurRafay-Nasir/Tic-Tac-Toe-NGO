using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsUIManager : MonoBehaviour
{
    [SerializeField] GameObject _backgroundGO;

    [Header("Buttons")]
    [SerializeField] Button _hostButton;
    [SerializeField] Button _clientButton;

    private void Awake()
    {
        _hostButton.onClick.AddListener(OnHostButtonClicked);
        _clientButton.onClick.AddListener(OnClientButtonClicked);
    }

    private void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();

        _hostButton.onClick.RemoveListener(OnHostButtonClicked);
        _clientButton.onClick.RemoveListener(OnClientButtonClicked);

        _backgroundGO.SetActive(false);
        _hostButton.gameObject.SetActive(false);
        _clientButton.gameObject.SetActive(false);
    }

    private void OnClientButtonClicked()
    {
        NetworkManager.Singleton.StartClient();

        _hostButton.onClick.RemoveListener(OnHostButtonClicked);
        _clientButton.onClick.RemoveListener(OnClientButtonClicked);

        _backgroundGO.SetActive(false);
        _hostButton.gameObject.SetActive(false);
        _clientButton.gameObject.SetActive(false);
    }

}
