using System.Collections.Generic;
using UnityEngine;

namespace RainMeadow
{
    public class ExpeditionGameMode : OnlineGameMode
    {
        public SlugcatCustomization[] avatarSettings;
        public int avatarCount { get; set; } = 1;

        public ExpeditionGameMode(Lobby lobby) : base(lobby)
        {
            avatarSettings = new SlugcatCustomization[4];
            for (int i = 0; i < avatarSettings.Length; i++)
            {
                avatarSettings[i] = new SlugcatCustomization() { nickname = OnlineManager.mePlayer.id.name };
            }
        }

        public override ProcessManager.ProcessID MenuProcessId()
        {
            return RainMeadow.Ext_ProcessID.ExpeditionMenu;
        }

        public override void AddClientData()
        {
            // You can add specific ExpeditionClientSettings if needed
        }

        public override void PreGameStart()
        {
            base.PreGameStart();
        }

        public override void PostGameStart(RainWorldGame game)
        {
            base.PostGameStart(game);
        }

        public override AbstractCreature SpawnAvatar(RainWorldGame self, WorldCoordinate location)
        {
            AbstractCreature mainAvatar = null;

            for (int i = 0; i < avatarCount; i++)
            {
                AbstractCreature abstractCreature = new AbstractCreature(
                    self.world,
                    StaticWorld.GetCreatureTemplate("Slugcat"),
                    null,
                    location,
                    new EntityID(-1, i)
                );

                abstractCreature.state = new PlayerState(abstractCreature, i, avatarSettings[i].playingAs, false)
                {
                    isPup = avatarSettings[i].fakePup
                };

                self.world.GetAbstractRoom(abstractCreature.pos.room).AddEntity(abstractCreature);
                self.session.AddPlayer(abstractCreature);

                if (i == 0) mainAvatar = abstractCreature;
            }

            return mainAvatar!;
        }

        public override void ConfigureAvatar(OnlineCreature onlineCreature)
        {
            if (onlineCreature.abstractCreature.state is PlayerState playerState)
            {
                onlineCreature.AddData(avatarSettings[playerState.playerNumber]);
            }
        }

        public override void Customize(Creature creature, OnlineCreature oc)
        {
            if (oc.TryGetData<SlugcatCustomization>(out var data))
            {
                RainMeadow.creatureCustomizations.GetValue(creature, _ => data);
            }
        }
    }
}
