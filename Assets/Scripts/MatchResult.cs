using Unity.Netcode;
using UnityEngine;

public struct MatchResult : INetworkSerializable
{
    public PlayerType winner;

    public Vector2Int startIndex;
    public Vector2Int endIndex;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref winner);

        serializer.SerializeValue(ref startIndex);
        serializer.SerializeValue(ref endIndex);
    }
}
