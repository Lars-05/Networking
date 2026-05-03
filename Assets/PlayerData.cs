using UnityEngine;

public static class PlayerData
{
    public static bool idSet; 
    public static int id;

    public static void SetPlayerID(int pId)
    {
        id = pId;
        idSet = true;
    }
}
