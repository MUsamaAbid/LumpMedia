using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Manager : MonoBehaviour
{
    [SerializeField] GameObject PlayerPrefab;

    private void Start()
    {
        SpawnPlayer(); 
    }
    void SpawnPlayer()
    {
        int r = Random.Range(-10, 10);
        Vector3 pos = new Vector3(PlayerPrefab.transform.position.x + r, PlayerPrefab.transform.position.y, PlayerPrefab.transform.position.z);
        PhotonNetwork.Instantiate(PlayerPrefab.name, pos, PlayerPrefab.transform.rotation);
    }
}
