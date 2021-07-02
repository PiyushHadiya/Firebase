using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public string PlayerId
    {
        get { return _playerId; }
    }
    private string _playerId;
    private void Awake()
    {
        PlayerName();
    }

    private void PlayerName()
    {
        _playerId = PlayerPrefs.GetString("localPlayerId", "");

        if (_playerId.Length <= 0)
        {
            _playerId = "Geust" + UnityEngine.Random.Range(2020, 4040);
            PlayerPrefs.SetString("localPlayerId", _playerId);
        }
    }

}

