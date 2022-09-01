using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core : MonoBehaviour
{
    public static int playersCount;

    public static LayerMask layerMaskCard = 1 << 8;

    void Awake()
    {
        playersCount = 2;
    }
}

public enum TurnType
{
    First,
    Attack,
    Defence
}
