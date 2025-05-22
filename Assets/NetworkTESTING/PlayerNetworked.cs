using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetworked : NetworkBehaviour
{
    PlayerInputControls controls;
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(new MyCustomData { _int = 56, _bool = true,message = "Less than 129 char" }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }


    public Transform SpawnObjectPrefab;


    private void Awake()
    {
        controls = new PlayerInputControls();
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            print($"Owner: {OwnerClientId} || Value: {newValue._int} || Bool: {newValue._bool} || Message: {newValue.message}");
        };
    }
    public override void OnNetworkDespawn()
    {
        
    }
    Transform spawned;
    private void Update()
    {
        
        if (!IsOwner) return;
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            //TestServerRpc();
            //TestClientRpc();

            spawned = Instantiate(SpawnObjectPrefab);
            spawned.GetComponent<NetworkObject>().Spawn(true);

            //randomNumber.Value = new MyCustomData {_int = Random.Range(0, 100), _bool = Random.Range(0,2) > 0, message = $"Gotta be careful with space"};
        }
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            spawned.GetComponent<NetworkObject>().Despawn(true);
            Destroy(spawned.gameObject);
        }
        Vector3 moveDir = Vector3.zero;
        Vector2 input = controls.Player.Move.ReadValue<Vector2>();
        float moveSpeed = 5f;
        moveDir.z = input.y;
        moveDir.x = input.x;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    [ServerRpc]
    private void TestServerRpc()
    {
        print($"Test server RPC {OwnerClientId}");
    }

    [ClientRpc]
    private void TestClientRpc()
    {
        print($"Test Client RPC");
    }
}
