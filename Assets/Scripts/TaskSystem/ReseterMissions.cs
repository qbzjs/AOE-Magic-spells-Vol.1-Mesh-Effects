﻿using System.IO;
using UnityEditor;
using UnityEngine;

public class ReseterMissions
{
#if UNITY_EDITOR
    [MenuItem("Tools/Reset missions")]
    private static void ResetMissions()
    {
        Missions.MissionsManager.State state = new Missions.MissionsManager.State();
        string data = JsonUtility.ToJson(state, true);
        File.WriteAllText(Missions.MissionsManager.savePath, data);
    }
#endif
}
