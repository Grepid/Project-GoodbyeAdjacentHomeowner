using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    [Command]
    private async void CreateRelay()
    {
        try
        {
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);



            RelayServerData rsd = new RelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);
            NetworkManager.Singleton.StartHost();

            print($"Relay Join Code: {joinCode}");

            //copies to clipboard
            TextEditor te = new TextEditor();
            te.text = joinCode;
            te.SelectAll();
            te.Copy();
        }
        catch(RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }



    [Command]
    private async void JoinRelay(string joinCode)
    {
        try
        {
            print($"Joining Relay with code {joinCode}");
            JoinAllocation ja = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData rsd = new RelayServerData(ja, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);
            NetworkManager.Singleton.StartClient();
            print($"Successfully Joined Relay {joinCode}");
        }
        catch(RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }
}
