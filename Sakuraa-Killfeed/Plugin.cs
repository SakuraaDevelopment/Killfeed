using BepInEx;
using UnityEngine;
using HoneyLib.Events;
using System.Collections.Generic;
using Photon.Pun;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Net.Http;

namespace Sakuraa_Killfeed
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private VideoPlayer videoPlayer;
        private class KillFeedEntry
        {
            public string Text;
            public float ExpirationTime;
        }

        private List<KillFeedEntry> killFeedEntries = new List<KillFeedEntry>();
        private string latestTag;

        private const float FeedEntryLifetime = 60f;
        private const int TextSize = 22;

        private GUIStyle whiteStyle;
        private Dictionary<string, Color> playerColors = new Dictionary<string, Color>();

        private float countdown;
        private bool LA = false;
        private float LAcountdown = 30f;

        void Start()
        {
            Events.InfectionTagEvent += InfectionTagEvent;

            whiteStyle = new GUIStyle();
            whiteStyle.normal.textColor = Color.white;
            whiteStyle.fontSize = TextSize;
        }

        async void Update()
        {
            if (countdown <= 0f)
            {
                Debug.Log("Countdown over");
                countdown = 30f;
                GorillaTagManager GTM = GameObject.FindObjectOfType<GorillaTagManager>();
                foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
                {
                    VRRig rig = GTM.FindPlayerVRRig(player);
                    Color playerColor = rig.mainSkin.material.color;
                    if (playerColor == new Color(1f, 1f, 1f, 1f))
                    {
                        continue;
                    }
                    playerColors[player.NickName] = playerColor;
                }
            }
        }

        void OnGUI()
        {
            float feedWidth = 400f;
            float feedHeight = 200f;
            float margin = 1f;

            float xPos = Screen.width - feedWidth - margin;
            float yPos = margin;

            GUILayout.BeginArea(new Rect(xPos, yPos, feedWidth, feedHeight));
            GUILayout.FlexibleSpace();
             
            foreach (KillFeedEntry entry in killFeedEntries)
            {
                string[] names = entry.Text.Split(new[] { " tagged " }, System.StringSplitOptions.None);
                string taggerName = names[0];
                GUIStyle taggerStyle = GetPlayerNameStyle(taggerName);

                string taggedName = names[1];
                GUIStyle taggedStyle = GetPlayerNameStyle(taggedName);

                GUILayout.BeginHorizontal();

                GUILayout.Label(taggerName, taggerStyle);
                GUILayout.Label(" tagged ", whiteStyle);
                GUILayout.Label(taggedName, taggedStyle);

                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }

        void InfectionTagEvent(object sender, InfectionTagEventArgs e)
        {
            latestTag = e.taggingPlayer.NickName + " tagged " + e.taggedPlayer.NickName;
            killFeedEntries.Add(new KillFeedEntry { Text = latestTag, ExpirationTime = Time.time + FeedEntryLifetime });
        }

        private void RemoveExpiredEntries()
        {
            killFeedEntries.RemoveAll(entry => entry.ExpirationTime <= Time.time);
        }

        private GUIStyle GetPlayerNameStyle(string playerName)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = GetPlayerColor(playerName);
            style.fontSize = TextSize;
            return style;
        }

        private Color GetPlayerColor(string playerName)
        {
            if (playerColors.ContainsKey(playerName))
            {
                return playerColors[playerName];
            }

            GorillaTagManager GTM = GameObject.FindObjectOfType<GorillaTagManager>();
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if (player.NickName == playerName)
                {
                    VRRig rig = GTM.FindPlayerVRRig(player);
                    Color playerColor = rig.mainSkin.material.color;
                    playerColors[playerName] = playerColor;
                    return playerColor;
                }
            }
            return Color.white;
        }
    }
}
