using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefsHandler : MonoBehaviour
{
    public static PrefsHandler instance;
    private void Awake()
    {
        instance = this;
    }

    public static string totalCoins = "totalCoins";
}
