﻿using Society.Music;

using UnityEngine;
using UnityEngine.UI;

namespace Society.Menu
{
    internal sealed class MusicToggle : MonoBehaviour
    {
        private LocationMusic locationMusic;
        private void Start()
        {
            locationMusic = FindObjectOfType<LocationMusic>();
            GetComponent<Toggle>().onValueChanged.AddListener(OnChangeValue);
        }
        private void OnChangeValue(bool v)
        {
            locationMusic.SetEnabledMusic(v);
        }
        private void OnDestroy()
        {
            GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        }
    }
}