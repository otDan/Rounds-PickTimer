﻿using System;
using System.Collections;
using Photon.Pun;
using PickTimer.Asset;
using PickTimer.Util;
using TMPro;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = System.Collections.Hashtable;

namespace PickTimer.Network
{
    public class LobbyMonitor : MonoBehaviourPunCallbacks, IGameStartHookHandler
    {
        public static LobbyMonitor instance { get; private set; }
        private static bool _enabled;
        private static TextMeshProUGUI _lobbyTimerText;
        internal static GameObject LobbyTimerCanvas;
        private static GameObject _lobbyTimerUi;
        // private static GameObject _lobbyTimerParticles;
        private static Button _minusButton;
        private static Button _plusButton;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            GameHook.instance.RegisterHooks(this);
        }

        private void Update()
        {
            if (!_enabled) return;
            _lobbyTimerText.text = PickTimer.PickTimerTime.ToString();
        }

        public override void OnCreatedRoom()
        {
            if (PhotonNetwork.IsMasterClient)
            {
            }
        }

        public override void OnJoinedRoom()
        {
            if (!PickTimer.PickTimerEnabled) return;

            if (_lobbyTimerUi == null)
            {
                InitializeLobbyTimerUi();
            }

            _lobbyTimerUi.SetActive(false);
            if (PhotonNetwork.OfflineMode) return;

            _lobbyTimerUi.SetActive(true);

            if (PhotonNetwork.IsMasterClient) return;
            _minusButton.gameObject.SetActive(false);
            _plusButton.gameObject.SetActive(false);
        }

        public override void OnLeftRoom()
        {
            Destroy(_lobbyTimerUi);
            _enabled = false;
        }

        private static void IncrementPickTimerValue()
        {
            if (PickTimer.PickTimerTime + 1 <= 60)
            {
                PickTimer.PickTimerTime += 1;
            }

            PickTimer.SyncTimer();
        }

        private static void DecrementPickTimerValue()
        {
            if (PickTimer.PickTimerTime - 1 >= 5)
            {
                PickTimer.PickTimerTime -= 1;
            }

            PickTimer.SyncTimer();
        }

        private static void InitializeLobbyTimerUi()
        {
            LobbyTimerCanvas = GameObject.Find("/Game/UI/UI_Game/Canvas/");

            _lobbyTimerUi = Instantiate(AssetManager.TimerLobbyUI, LobbyTimerCanvas.transform, true);
            _lobbyTimerUi.AddComponent<BringBgToTop>();

            _lobbyTimerText = _lobbyTimerUi.GetComponentInChildren<TextMeshProUGUI>();
            _lobbyTimerText.text = PickTimer.PickTimerTime.ToString();
            _lobbyTimerText.enableWordWrapping = false;
            _lobbyTimerText.overflowMode = TextOverflowModes.Overflow;
            _lobbyTimerText.alignment = TextAlignmentOptions.Center;

            RectTransform canvasRectTransform = LobbyTimerCanvas.GetComponent<RectTransform>();

            RectTransform lobbyTimerUiRect = _lobbyTimerUi.GetComponent<RectTransform>();
            lobbyTimerUiRect.localScale = new Vector3(0.8f, 0.8f, 1);
            lobbyTimerUiRect.anchorMin = new Vector2(0, 0);
            lobbyTimerUiRect.anchorMax = new Vector2(1, 1);
            lobbyTimerUiRect.pivot = new Vector2(1f, 1f);
            lobbyTimerUiRect.offsetMin = new Vector2(0, 0);
            lobbyTimerUiRect.offsetMax = new Vector2(0, 0);
            var lobbyTimerX = canvasRectTransform.rect.width / 2 - lobbyTimerUiRect.rect.width / 2;
            lobbyTimerUiRect.localPosition = new Vector3(lobbyTimerX, 0, 0);

            // _lobbyTimerParticles = Instantiate(AssetManager.TimerParticles, _lobbyTimerUi.transform, true);
            // RectTransform lobbyTimerParticlesRect = _lobbyTimerParticles.GetComponent<RectTransform>();
            // lobbyTimerParticlesRect.localScale = new Vector3(0.1f, 0.1f, 1);
            // lobbyTimerParticlesRect.anchorMin = new Vector2(0, 0);
            // lobbyTimerParticlesRect.anchorMax = new Vector2(1, 1);
            // lobbyTimerParticlesRect.pivot = new Vector2(1f, 1f);
            // lobbyTimerParticlesRect.offsetMin = new Vector2(0, 0);
            // lobbyTimerParticlesRect.offsetMax = new Vector2(0, 0);
            // lobbyTimerParticlesRect.localPosition = new Vector3(0, 0, 0);

            foreach (var button in _lobbyTimerUi.gameObject.GetComponentsInChildren<Button>())
            {
                switch (button.gameObject.name)
                {
                    case "Minus":
                        _minusButton = button;
                        _minusButton.onClick.AddListener(DecrementPickTimerValue);
                        break;
                    case "Plus":
                        _plusButton = button;
                        _plusButton.onClick.AddListener(IncrementPickTimerValue);
                        break;
                }
            }

            _lobbyTimerUi.SetActive(false);
            _enabled = true;
        }

        private class BringBgToTop : MonoBehaviour
        {
            private void Update()
            {
                if (_lobbyTimerUi.transform.GetSiblingIndex() != 21)
                    _lobbyTimerUi.transform.SetSiblingIndex(21);
            }
        }

        public void OnGameStart()
        {
            _lobbyTimerUi.SetActive(false);
        }
        
        public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
        }

        private void OnDestroy()
        {
            GameHook.instance.RemoveHooks(this);
        }
    }
}
