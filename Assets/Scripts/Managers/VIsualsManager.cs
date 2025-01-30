using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VisualsManager : NetworkBehaviour
{
    [SerializeField] GameObject _circlePrefab;
    [SerializeField] GameObject _crossPrefab;
    [SerializeField] GameObject _linePrefab;

    private readonly List<GameObject> _spawnedPrefabs = new();

    private const float GRID_SIZE = 3.1f;

    #region Unity Callbacks

    private void Start()
    {
        GameManager.Instance.OnClickedValidSlot += OnClickedValidSlot;
        GameManager.Instance.OnMatchEnd += OnMatchEnd;
        GameManager.Instance.OnMatchRestarted += OnMatchRestarted;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnClickedValidSlot -= OnClickedValidSlot;
        GameManager.Instance.OnMatchEnd -= OnMatchEnd;
        GameManager.Instance.OnMatchRestarted -= OnMatchRestarted;
    }

    #endregion

    #region Game Manager Callbacks

    private void OnClickedValidSlot(int x, int y)
    {
        SpawnObjectServerRpc(GetGridPosition(x, y), GameManager.Instance.LocalPlayerType);
    }

    private void OnMatchEnd(MatchResult winResult)
    {
        if (winResult.winner == PlayerType.None)
            return;

        Vector2 startPos = GetGridPosition(winResult.startIndex.x, winResult.startIndex.y);
        Vector2 endPos = GetGridPosition(winResult.endIndex.x, winResult.endIndex.y);

        Vector2 linePosition = (startPos + endPos) / 2;

        Vector2 direction = endPos - startPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion lineRotation = Quaternion.Euler(0, 0, angle);

        GameObject line = Instantiate(_linePrefab, linePosition, lineRotation);

        _spawnedPrefabs.Add(line);
    }

    private void OnMatchRestarted()
    {
        // From what I see, if a server deletes a networked object it 
        // is also removed from clients
        foreach (GameObject prefab in _spawnedPrefabs)
        {
            Destroy(prefab);
        }
    }

    #endregion

    [Rpc(SendTo.Server)]
    private void SpawnObjectServerRpc(Vector2 position, PlayerType playerType)
    {
        // Instantiate prefab locally, this means it wont spawn for remote clients
        GameObject prefab = playerType switch
        {
            PlayerType.Cross => Instantiate(_crossPrefab, position, Quaternion.identity),
            _ => Instantiate(_circlePrefab, position, Quaternion.identity),
        };

        // Instantiate prefab on remote clients and server. Note that only
        // server is allowed to run Spawn() code. Since this is a server rpc
        // it will only run on server.

        // You may be wondering why didn't we Instantiate prefab locally and passed
        // the prefab onto the server rpc. This is because we cannot pass managed
        // data through rpc. 
        // Managed means reference types such as classes, arrays
        NetworkObject circleNetworkObject = prefab.GetComponent<NetworkObject>();
        circleNetworkObject.Spawn();

        _spawnedPrefabs.Add(prefab);
    }

    /// <summary>
    /// Helper function to get grid position in worldspace
    /// </summary>
    /// <param name="row">Row index of grid slot</param>
    /// <param name="column">Column index of grid slot</param>
    private Vector2 GetGridPosition(int row, int column)
    {
        return new Vector2(-GRID_SIZE + column * GRID_SIZE, (-GRID_SIZE + row * GRID_SIZE) -0.4f);
    }
}