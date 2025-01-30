using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Events

    /// <summary>
    /// Invoked when 2 players join the game. Only invoked once during a session.
    /// </summary>
    /// <remarks>This is <b>Synchronized across network</b>.</remarks>
    public event Action OnMatchBegin;

    /// <summary>
    /// Invoked when one client wins. Tells about winner and indices that made that win.
    /// </summary>
    /// <remarks>This is <b>Synchronized across network</b>.</remarks>
    public event Action<MatchResult> OnMatchEnd;

    /// <summary>
    /// Invoked when match restarts.
    /// </summary>
    /// <remarks>This is <b>Synchronized across network</b>.</remarks>
    public event Action OnMatchRestarted;

    /// <summary>
    /// Invoked when all of these conditions meet<br/>
    /// - users clicks on grid <br/>
    /// - it is current players turn <br/>
    /// - slot is empty
    /// </summary>
    /// <remarks>
    /// 'Whose turn is next' is calculated after this event is invoked <br/>
    /// This is <b>NOT synchronized across network</b>.
    /// </remarks>
    public event Action<int, int> OnClickedValidSlot;

    /// <summary>
    /// Invoked When turn switches. Provides updated playable player type
    /// </summary>
    /// <remarks>This is <b>Synchronized across network</b>.</remarks>
    public event Action<PlayerType> OnPlayablePlayerTypeChange;

    #endregion

    #region Public Properties

    /// <summary>
    /// The Local Client's Type (Host is Circle, Client is Cross)
    /// </summary>
    /// <remarks>This is <b>Synchronized across network</b>.</remarks>
    public PlayerType LocalPlayerType { get; private set; }

    /// <summary>
    /// Whose turn it is currently. 
    /// </summary>
    /// <remarks>This is <b>Synchronized across network</b></remarks>
    public PlayerType CurrentPlayablePlayerType { get; private set; }

    /// <summary>
    /// The current circle player score. 
    /// </summary>
    /// <remarks>This is <b>Synchronized across network</b></remarks>
    public int CircleScore { get; private set; }

    /// <summary>
    /// The current cross player score. 
    /// </summary>
    /// <remarks>This is <b>Synchronized across network</b></remarks>
    public int CrossScore { get; private set; }

    #endregion

    #region Private Fields

    /// <summary>
    /// Each player's turn is stored in this array
    /// </summary>
    /// <remarks>This is <b>Synchronized across network</b></remarks>
    private PlayerType[,] _gridSlotPlayerType;

    private int _filledSlotsCount;
    private bool _isMatchEnded;

    private const string LOG_PREFIX = "<color=green>Game Manager:</color>";

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        Instance = this;
        _gridSlotPlayerType = new PlayerType[3, 3];
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    #endregion

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.ConnectedClientsList.Count != 2)
            return;

        StartGameClientRpc();
        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartGameClientRpc()
    {
        LocalPlayerType = NetworkManager.LocalClientId == 0 ? PlayerType.Circle : PlayerType.Cross;

        // First to join, gets to play first
        CurrentPlayablePlayerType = PlayerType.Circle;

        OnMatchBegin?.Invoke();
        Debug.Log($"{LOG_PREFIX} Game Started");
    }

    // Called externally by GridPosition script
    public void ClickGrid(int x, int y)
    {
        if (_isMatchEnded)
        {
            Debug.Log($"{LOG_PREFIX} Game has ended.");
            return;
        }

        if (CurrentPlayablePlayerType != LocalPlayerType)
        {
            Debug.Log($"{LOG_PREFIX} You are {LocalPlayerType}. Current turn: {CurrentPlayablePlayerType}.");
            return;
        }

        if (IsSlotFilled(x, y))
        {
            Debug.Log($"{LOG_PREFIX} Slot ({x}, {y}) already filled with {_gridSlotPlayerType[x, y]}.");
            return;
        }

        UpdateGridStateServerRpc(x, y);

        CheckForWinnerServerRpc();

        UpdateCurrentPlayablePlayerTypeServerRpc();

        OnClickedValidSlot?.Invoke(x, y);
    }

    // Called externally by Rematch Button when it is clicked
    public void RestartGame()
    {
        RestartGameServerRpc();
    }

    #region Helper Functions

    private bool IsSlotFilled(int x, int y)
    {
        return _gridSlotPlayerType[x, y] != PlayerType.None;
    }

    private MatchResult GetMatchResult(PlayerType[,] grid)
    {
        for (int i = 0; i < 3; i++)
        {
            // Check rows
            if (grid[i, 0] != PlayerType.None && grid[i, 0] == grid[i, 1] && grid[i, 1] == grid[i, 2])
            {
                return new MatchResult
                {
                    winner = grid[i, 0],
                    startIndex = new Vector2Int(i, 0),
                    endIndex = new Vector2Int(i, 2)
                };
            }

            // Check columns
            if (grid[0, i] != PlayerType.None && grid[0, i] == grid[1, i] && grid[1, i] == grid[2, i])
            {
                return new MatchResult
                {
                    winner = grid[0, i],
                    startIndex = new Vector2Int(0, i),
                    endIndex = new Vector2Int(2, i)
                };
            }
        }

        // Check diagonals
        if (grid[0, 0] != PlayerType.None && grid[0, 0] == grid[1, 1] && grid[1, 1] == grid[2, 2])
        {
            return new MatchResult
            {
                winner = grid[0, 0],
                startIndex = new Vector2Int(0, 0),
                endIndex = new Vector2Int(2, 2)
            };
        }

        if (grid[0, 2] != PlayerType.None && grid[0, 2] == grid[1, 1] && grid[1, 1] == grid[2, 0])
        {
            return new MatchResult
            {
                winner = grid[0, 2],
                startIndex = new Vector2Int(0, 2),
                endIndex = new Vector2Int(2, 0)
            };
        }

        // No winner
        return new MatchResult
        {
            winner = PlayerType.None,
            startIndex = new Vector2Int(-1, -1),
            endIndex = new Vector2Int(-1, -1)
        };
    }

    #endregion

    #region Restart Game Rpc

    [Rpc(SendTo.Server)]
    private void RestartGameServerRpc()
    {
        RestartGameClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RestartGameClientRpc()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                _gridSlotPlayerType[i, j] = PlayerType.None;
            }
        }

        _filledSlotsCount = 0;
        _isMatchEnded = false;

        // UpdateCurrentPlayablePlayerTypeServerRpc();

        OnMatchRestarted?.Invoke();

        Debug.Log($"{LOG_PREFIX} Match Restarted. ");
    }

    #endregion

    #region Winner Rpc's

    [Rpc(SendTo.Server)]
    private void CheckForWinnerServerRpc()
    {
        MatchResult matchResult = GetMatchResult(_gridSlotPlayerType);

        if (matchResult.winner != PlayerType.None)
        {
            if (matchResult.winner == PlayerType.Circle)
                CircleScore++;
            else
                CrossScore++;

            AnnounceWinnerClientRpc(matchResult, CircleScore, CrossScore);
        }
        else if (++_filledSlotsCount >= 9)
        {
            AnnounceTieClientRpc();
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void AnnounceWinnerClientRpc(MatchResult matchResult, int circleScore, int crossScore)
    {
        Debug.Log(matchResult.winner == LocalPlayerType ? $"{LOG_PREFIX} You WIN!" : $"{LOG_PREFIX} You LOSE.");

        _isMatchEnded = true;

        CircleScore = circleScore;
        CrossScore = crossScore;

        OnMatchEnd?.Invoke(matchResult);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AnnounceTieClientRpc()
    {
        Debug.Log($"{LOG_PREFIX} Game Tied.");

        _isMatchEnded = true;

        OnMatchEnd?.Invoke(new MatchResult());
    }

    #endregion

    #region Update Grid State RPC

    [Rpc(SendTo.Server)]
    private void UpdateGridStateServerRpc(int x, int y)
    {
        _gridSlotPlayerType[x, y] = CurrentPlayablePlayerType;
        UpdateGridStateClientRpc(x, y);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateGridStateClientRpc(int x, int y)
    {
        Debug.Log($"{LOG_PREFIX} Turn taken by {CurrentPlayablePlayerType}.");

        _gridSlotPlayerType[x, y] = CurrentPlayablePlayerType;
    }

    #endregion

    #region Update Current Playable Player Type Rpc

    [Rpc(SendTo.Server)]
    private void UpdateCurrentPlayablePlayerTypeServerRpc()
    {
        UpdateCurrentPlayablePlayerTypeClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateCurrentPlayablePlayerTypeClientRpc()
    {
        CurrentPlayablePlayerType = CurrentPlayablePlayerType == PlayerType.Circle ? PlayerType.Cross : PlayerType.Circle;
        OnPlayablePlayerTypeChange?.Invoke(CurrentPlayablePlayerType);
    }

    #endregion
}

public enum PlayerType
{
    None, Circle, Cross
}