using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using QFSW.QC;


public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;
    private string playerName = "Test Name";
    
    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            print($"Signed in player: {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerName += UnityEngine.Random.Range(0, 100);
        print($"Random assigned name = {playerName}");
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPullForUpdates();
    }
    private async void HandleLobbyHeartbeat()
    {
        if(hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0f)
            {
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    [Command]
    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    [Command]
    private async void CreateLobby()
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public,"CaptureTheFlag")},
                    //This would be how to attach the tag to a dataObject index option to allow it to be referenced and searched and filtered elsewhere
                    //{"GameMode", new DataObject(DataObject.VisibilityOptions.Public,"CaptureTheFlag",DataObject.IndexOptions.S1)}
                    {"Map", new DataObject(DataObject.VisibilityOptions.Public,"Dust2")}
                }                
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("MyLobby", 4,options);
            print($"Created Lobby {lobby.Name} with {lobby.MaxPlayers} max players. ID: {lobby.Id}  Code: {lobby.LobbyCode}");
            hostLobby = lobby;
            joinedLobby = lobby;

            PrintPlayers(lobby);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    [Command]
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0",QueryFilter.OpOptions.GT),
                    //How you would filter to only have capture the flag mode (or any other string tag)
                    //new QueryFilter(QueryFilter.FieldOptions.S1, "CaptureTheFlag",QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder> 
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }

            };

            QueryResponse qr = await Lobbies.Instance.QueryLobbiesAsync();

            print($"Found {qr.Results.Count} lobbies");
            foreach (Lobby l in qr.Results)
            {
                print($"Found Lobby {l.Name} of mode {l.Data["GameMode"].Value} with {l.MaxPlayers - l.AvailableSlots} out of {l.MaxPlayers} players");
            }
        }
        catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    [Command]
    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode,options);
            joinedLobby = lobby;
            print($"Joined lobby with code {lobbyCode}");
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }


    [Command]
    private async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobby = lobby;
        }
        catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
        
    }

    private void PrintPlayers(Lobby lobby)
    {
        print($"Players in Lobby {lobby.Name} of Mode {lobby.Data["GameMode"].Value} on map {lobby.Data["Map"].Value}:");
        foreach (Player p in lobby.Players)
        {
            print($"Player {p.Id} with name {p.Data["Playername"].Value}");
        }
    }

    private Player GetPlayer()
    {
        Player p = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"Playername",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
            }
        };

        return p;
    }

    [Command]
    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                {"Gamemode", new DataObject(DataObject.VisibilityOptions.Public,gameMode)}
            }
            });
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
        
    }
    private float lobbyUpdateTimer;
    private async void HandleLobbyPullForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer += Time.deltaTime;
            if (lobbyUpdateTimer > 1.5f)
            {
                lobbyUpdateTimer = 0;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }

    private async void UpdatePlayerName(string newPlayername)
    {
        try
        {
            playerName = newPlayername;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                {"Playername",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
            },
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
        
    }

    [Command]
    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    [Command]
    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    [Command]
    private async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    [Command]
    private async void DeleteLobby()
    {
        await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
    }
}
