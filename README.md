# Tic-Tac-Toe Multiplayer
Networked tic-tac-toe made in Unity 6000.0.26f1, using Netcode for Gameobjects.  

> [!IMPORTANT]  
> This is intended to be used by Intermediate to Advanced users to understand Multiplayer game development.  
> It is recommended to first understand unity basics If you are absolute beginner. 

## Contents

- [Packages Used](#packages-used)
- [Game Structure](#game-structure)
- [Multiplayer Concepts](#multiplayer-concepts)
  - [Client, Server, and Host](#multiplayer-concepts)
  - [Network Topologies](#network-topologies)
  - [Dedicated Server](#dedicated-server)
  - [Client-hosted Server](#client-hosted-server)
  - [Difference between Dedicated and Client-hosted Servers](#difference-between-dedicated-and-client-hosted-servers)
  - [Network Variable](#network-variable)
  - [RPC](#rpc)
      - [Server RPC](#server-rpc)
      - [Client RPC](#client-rpc)
- [Texture & Sound Credits](#texture-and-sound-credits)
- [Author](#author)

## Packages Used
- Netcode for Gameobjects 1.x.x
- Multiplayer play mode 1.x.x

## Game Structure
- `GameManager` is the core script of this game. It handles all the gameplay logic.
- `VisualsManager` is responsible for spawning Circles and Crosses on all clients.
- UI Managers are responsible for handling UI of game.
    -  `TurnUIManager` shows which player turn currently is. Also shows either you are playing as circle or cross.
    -  `ScoreUIManager` shows the current score of Circle and Cross.
    -  `ResultUIManager` shows whether local client won or loose.
    -  `RematchUIManager` shows a rematch button to restart the match.
    -  `ButtonsUIManager` shows Host & Client buttons to join as host or client.
- `GridPosition` calls Game Manager's ClickGrid() to tell which grid slot was clicked.
- `MatchResult` is a struct that stores information about winner and start & end grid slot position that made the win.

> [!TIP]
> All scripts depend on `GameManager`. As a rule of thumb, ensure that visuals always depend on gameplay script and not vice versa.

> [!NOTE]  
> You may think that this code is significantly different from Code Monkey's tutorial. This is because, it was made as a challenge after I understood networking conepts.
> 
> If you wish to obtain Code Moneky's project files, [visit Code Monkey's website](https://unitycodemonkey.com/tictactoefreecourse.php).

## Multiplayer Concepts

<table>
  <tr>
    <th>Client</th>
    <td>You, a player playing game on your device is considered a client.</td>
  </tr>
  <tr>
    <th>Server</th>
    <td>The instance of your game to which clients connect to.</td>
  </tr>
  <tr>
    <th>Host</th>
    <td>Both a server and client.</td>
  </tr>
</table>

<table>
  <tr>
    <th>Local Client</th>
    <td>Your player on your machine is considered Local Client.</td>
  </tr>
  <tr>
    <th>Remote Client</th>
    <td>Other players playing with you are considered Remote Clients.</td>
  </tr>
</table>

### Network Topologies
This is a fancy way to tell what kind of structure, connections will use. There are 2 main ones.
- Dedicated server
- Client-hosted server

### Dedicated Server
| ![Dedicated Server Image](https://docs-multiplayer.unity3d.com/assets/images/ded_server-d5369721966357b9b4d5e1fa96b05b22.png) |
|:--:| 
| *Credits: [Unity Documentation](https://docs-multiplayer.unity3d.com/netcode/current/terms-concepts/network-topologies/)* |

- This is what you would use when you want to create a competitve game.
- A Dedicated server is simply a computer running your game often without any graphics (to save resources).
- Clients connect to dedicated server.
- The final word is of server. If server says you died that means you died even if you didn't spawn yet. This is because server is responsible for game state (player's health, kills, deaths, score, k/d etc).
- Cheating is often difficult in games using dedicated servers.
- Example: Battlefield, Call of Duty etc.

### Client-hosted server
| ![Client-hosted Server Image](https://docs-multiplayer.unity3d.com/assets/images/client-hosted-16be0b1c9b5020f21325b1e6a7beca73.png) |
|:--:| 
| *Credits: [Unity Documentation](https://docs-multiplayer.unity3d.com/netcode/current/terms-concepts/network-topologies/)* |

- This is used for games where cheating is not a concern. Like Co-op.
- A Client-hosted server is literally the same thing as dedicated server. The only difference is that it runs on a client's machine.
- The client running the server is called host.
- Since host is server and client at same time. They have final say, meaning they can kill your player without moving an inch.
- Example: Among us

### Difference between Dedicated and Client-hosted servers
| Dedicated | Client-hosted |
| --------- | ------------- |
| Dedicated server usually runs on cloud without any graphics. | Client-hosted servers run on client's machine. |
| Dedicated server always runs. | The match will end if host leaves as there is no server. |
| You can control how may servers should be running the game. | Depends on number of people running as server. | 
| Players can choose to play on server close to them | Players are bound by the host's location |
| Expensive | Cheap |
| Clients don't have the overhead of running server specific code.| Resource intensive as host also has to run server code. |
| Harder to cheat | Easier to cheat |
| Use for competitive games | Use for Co-op games |
| Example: Battlefield, Call of Duty | Example: Among us |

### RPC
RPC stands for Remote Procedure Call. It is a way to execute functions on server or clients. For example, a client can call a server RPC, this
Rpc will be executed on server. Similarly, server can call client RPC, this will be executed on all clients by default. However we can tell on
which client the rpc can run.

> [!IMPORTANT]  
> To make a function an Rpc, add `Rpc` attribute on it, and set the `SendTo` parameter to tell whether you want to send to server or clients.
Additionaly Netcode for Gameobjects requires you to append Rpc at end of function's name.

- Attach given scripts to a game object.
- Unity will ask you to add `NetworkObject` component, add it. If unity doesn't ask add it manually.
- Start a virtual player.
- On main Unity editor connect as Host and on virtual player connect as client  
  (This is my preference, not a requirement, the order can be reversed).

#### Server RPC
Consider following code:
```csharp
using UnityEngine;
using Unity.Netcode;

public class ServerRpcExample : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        SayHelloServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void SayHelloServerRpc()
    {
        Debug.Log("Hello from client. ");
    }
}
```

You will see 2 logs of "Hello from client." on host console and nothing on virtual player console. You may ask why this happened?  
Here is a step-by-step explanation of what happens.

- Assume we have 2 clients C1 & C2. C1 is also server. This means C1 is a host.
- `OnNetworkSpawn()` runs when connection is established between server and client.
- C1 starts the game
- C2 is a virtual player. 
- Since C1 is a client it will call `SayHelloServerRpc()`, this call will be sent to server (which is also C1), and will execute there.
- C2 is a client so it will call `SayHelloServerRpc()`, this call will be sent to C1 (because it is server as well) and will execute there.

#### Client RPC
Consider following code:
```csharp
using UnityEngine;
using Unity.Netcode;

public class ClientRpcExample : NetworkBehaviour
{
    [ContextMenu("Send Message To Clients")]
    public void SendMessageToClients()
    {
        // Only the server should be allowed to call client rpc
        if (IsServer)
        {
            SayHelloClientRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SayHelloClientRpc()
    {
        Debug.Log("Hello from server. ");
    }
}
```
- Server (host in this case) will call `SendMessageToClients()` which will call `SayHelloClientRpc()`, this will be invoked by server but will run on clients.
- Join as host and client on Unity editor and virtual player respectively.
- Call `SendMessageToClients()` on host.
- 'Hello from server. ' will be printed on both clients C1 (server and client) and C2 (only client).
- Since C1 is server it will be able to call client rpc, C1 is also client so the rpc will also run there.
- C2 is just a client, so it won't be able to call the rpc. However when C1 invokes the client rpc. C2 will be notified about that invokation and it will run client rpc.

> [!NOTE]  
> Client Rpc's should only be invoked by the server.
> For some reason the new `Rpc` attribute allows clients to run Client Rpc both on local and remote clients.
> This isn't the case for legacy `ClientRpc` attribute.

### Network Variable
A network variable as the name implies is synchronized across the network. Usually All clients are allowed to read its value, but only server can write to it.

## Texture and Sound Credits
- [Code Monkey](https://www.youtube.com/@CodeMonkeyUnity/)

## Author
- Abdur Rafay Nasir
- [Portfolio](https://arafay.netlify.app)
- [LinkedIn](https://www.linkedin.com/in/abdur-rafay-nasir/)
- [Artstation](https://abdur-rafay.artstation.com/)
