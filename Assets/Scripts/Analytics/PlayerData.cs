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
        public string name;

        public string name_fish;

        public string fish_rarity;


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
