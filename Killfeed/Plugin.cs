using BepInEx;
using UnityEngine;
using HoneyLib.Events;
using System.Collections.Generic;

namespace Killfeed
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private class KillFeedEntry
        {
            public string Text;
            public float ExpirationTime;
        }

        private List<KillFeedEntry> killFeedEntries = new List<KillFeedEntry>();
        private string latestTag;

        private const float FeedEntryLifetime = 60f;
        private const int TextSize = 22;

        private GUIStyle redStyle;
        private GUIStyle whiteStyle;
        private GUIStyle cyanStyle;

        void Start()
        {
            Events.InfectionTagEvent += InfectionTagEvent;

            redStyle = new GUIStyle();
            redStyle.normal.textColor = Color.red;
            redStyle.fontSize = TextSize;

            whiteStyle = new GUIStyle();
            whiteStyle.normal.textColor = Color.white;
            whiteStyle.fontSize = TextSize;

            cyanStyle = new GUIStyle();
            cyanStyle.normal.textColor = Color.cyan;
            cyanStyle.fontSize = TextSize;
        }

        void Update()
        {
            RemoveExpiredEntries();
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
                string taggedName = names[1];
                GUILayout.BeginHorizontal();

                GUILayout.Label(taggerName, redStyle);
                GUILayout.Label(" tagged ", whiteStyle);
                GUILayout.Label(taggedName, cyanStyle);

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
    }
}
