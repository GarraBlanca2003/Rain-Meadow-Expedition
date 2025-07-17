using HarmonyLib;
using Menu;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainMeadow
{
    public static class Ext_ProcessID
    {
        public static readonly ProcessManager.ProcessID ExpeditionMenu = new("ExpeditionMenu", true);
    }

    public class ExpeditionOnlineMenu : Menu.Menu, IChatSubscriber
    {
        private ExpeditionGameMode expeditionGameMode;
        private MenuLabel lobbyLabel;
        private SimpleButton startButton;
        private CheckBox EfriendlyFire;
        private ChatTextBox chatTextBox;
        private Vector2 chatTextBoxPos;
        private bool isChatToggled = false;
        private List<MenuObject> chatSubObjects = new();
        private List<(string, string)> chatLog = new();
        private int currentLogIndex = 0;
        private const int maxVisibleMessages = 13;
        public bool EsaveToDisk = false;
        public bool EfriendlyFireb = false;


        public ExpeditionOnlineMenu(ProcessManager manager) : base(manager, Ext_ProcessID.ExpeditionMenu)
        {
            ID = OnlineManager.lobby.gameMode.MenuProcessId();
            expeditionGameMode = (ExpeditionGameMode)OnlineManager.lobby.gameMode;

            if (OnlineManager.lobby.isOwner)
            {
                //expeditionGameMode.EsaveToDisk = true;
            }

            SetupUI();
            MatchmakingManager.OnPlayerListReceived += UpdatePlayerList;
            ChatTextBox.OnShutDownRequest += ResetChatInput;
            ChatLogManager.Subscribe(this);
        }

        private void SetupUI()
        {
            pages.Add(new Page(this, null, "main", 0));
            lobbyLabel = new MenuLabel(this, pages[0], "LOBBY", new Vector2(200, 550), new Vector2(110, 30), true);
            pages[0].subObjects.Add(lobbyLabel);

            startButton = new SimpleButton(this, pages[0], "Start Expedition", "START_EXPEDITION", new Vector2(200, 100), new Vector2(200, 40));
            pages[0].subObjects.Add(startButton);

            EfriendlyFire = new CheckBox(this, pages[0], null, new Vector2(200, 160), 150f, "Friendly Fire", "EXPEDFRIENDLYFIRE", false);
            if (!OnlineManager.lobby.isOwner)
            {
                EfriendlyFire.buttonBehav.greyedOut = true;
            }
            pages[0].subObjects.Add(EfriendlyFire);

            var invite = new SimplerButton(this, pages[0], "Invite Friends", new Vector2(420, 100), new Vector2(150, 35));
            invite.OnClick += (_) => MatchmakingManager.currentInstance.OpenInvitationOverlay();
            pages[0].subObjects.Add(invite);

            chatTextBoxPos = new Vector2(manager.rainWorld.options.ScreenSize.x * 0.001f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 0);
            var toggleChat = new SimplerSymbolButton(this, pages[0], "Kill_Slugcat", "", chatTextBoxPos);
            toggleChat.OnClick += (_) => ToggleChat(!isChatToggled);
            pages[0].subObjects.Add(toggleChat);
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            if (message == "START_EXPEDITION")
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            expeditionGameMode.avatarCount = 1; // For now, no jolly coop
            for (int i = 1; i < 4; i++)
            {
                manager.rainWorld.DeactivatePlayer(i);
            }

            manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New; // TODO: Verify this is fine for Expedition
            manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
        }

        public override void Update()
        {
            base.Update();
            //expeditionGameMode.EfriendlyFire = EfriendlyFire.Checked;
        }

        public override void ShutDownProcess()
        {
            isChatToggled = false;
            ResetChatInput();
            ChatTextBox.OnShutDownRequest -= ResetChatInput;
            ChatLogManager.Unsubscribe(this);
            if (manager.upcomingProcess != ProcessManager.ProcessID.Game)
            {
                OnlineManager.LeaveLobby();
            }
            RainMeadow.rainMeadowOptions._SaveConfigFile();
            base.ShutDownProcess();
        }

        public void AddMessage(string user, string message)
        {
            if (OnlineManager.lobby == null || OnlineManager.lobby.gameMode.mutedPlayers.Contains(user)) return;
            chatLog.Add((user, message));
            UpdateLogDisplay();
        }

        public void ToggleChat(bool toggled)
        {
            isChatToggled = toggled;
            ResetChatInput();
            UpdateLogDisplay();
        }

        internal void ResetChatInput()
        {
            chatTextBox?.DelayedUnload(0.1f);
            pages[0].ClearMenuObject(ref chatTextBox);
            if (isChatToggled && chatTextBox is null)
            {
                chatTextBox = new ChatTextBox(this, pages[0], "", new Vector2(chatTextBoxPos.x + 24, 0), new Vector2(575, 30));
                pages[0].subObjects.Add(chatTextBox);
            }
        }

        internal void UpdateLogDisplay()
        {
            foreach (var e in chatSubObjects)
            {
                e.RemoveSprites();
                pages[0].RemoveSubObject(e);
            }
            chatSubObjects.Clear();

            if (!isChatToggled || chatLog.Count == 0) return;

            int startIndex = Mathf.Clamp(chatLog.Count - maxVisibleMessages - currentLogIndex, 0, chatLog.Count - maxVisibleMessages);
            float yOffSet = 0;
            var visibleLog = chatLog.Skip(startIndex).Take(maxVisibleMessages);
            foreach (var (username, message) in visibleLog)
            {
                var usernameLabel = new MenuLabel(this, pages[0], username, new Vector2(100, 330f - yOffSet), new Vector2(400, 30f), false);
                usernameLabel.label.alignment = FLabelAlignment.Left;
                usernameLabel.label.color = ChatLogManager.GetDisplayPlayerColor(username);
                chatSubObjects.Add(usernameLabel);
                pages[0].subObjects.Add(usernameLabel);

                var messageLabel = new MenuLabel(this, pages[0], $": {message}", new Vector2(100 + LabelTest.GetWidth(usernameLabel.label.text) + 2f, 330f - yOffSet), new Vector2(600, 30f), false);
                messageLabel.label.alignment = FLabelAlignment.Left;
                chatSubObjects.Add(messageLabel);
                pages[0].subObjects.Add(messageLabel);

                yOffSet += 20f;
            }
        }

        private void UpdatePlayerList(PlayerInfo[] players)
        {
            // TODO: Implement if you want player UI
        }
    }
} 