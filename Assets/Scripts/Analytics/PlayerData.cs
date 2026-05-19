using System;
using UnityEngine;

[Serializable]

public class PlayerData
{
    [Serializable]

    public class PlayerDataInfoToArray
    {

        public PlayerDataInfo[] _playerDataInfoArray;


    }
    [Serializable]
    public class PlayerDataInfo
    {
        public string name="Teste";

        public int lives=99;

        public float health=100.0f;


    }

    public static PlayerDataInfoToArray CreateClassFromJson(string json)
    {

        return  JsonUtility.FromJson<PlayerDataInfoToArray>(json);


    }
    public static string CreateJsonFromClass(PlayerDataInfo playerDataInfo)
    {

        return JsonUtility.ToJson(playerDataInfo);

    }
}
