﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using InventorySystem.Items.Usables.Scp1344;
using Mirror;
using UnityEngine;

namespace Mandragora
{
    public static class Extensions
    {
        public static bool IsDisconnected(this Player player)
        {
            return player?.Connection?.identity == null || string.IsNullOrEmpty(player.UserId);
        }

        public static void SendDetectionMessageTo(this Player sender, Player msgReceiver)
        {
            msgReceiver.Connection.Send(new Scp1344DetectionMessage(sender.NetId));
        }

        public static void ResyncEffectTo(this Player effectOwner, Player target, EffectType effect)
        {
            effectOwner.SendFakeEffectTo(target, effect, effectOwner.GetEffect(effect).Intensity);
        }

        public static void SendFakeEffectTo(this Player effectOwner, Player target, EffectType effect, byte intensity)
        {
            MirrorExtensions.SendFakeSyncObject(target, effectOwner.NetworkIdentity, typeof(PlayerEffectsController), (writer) =>
            {
                try
                {
                    const ulong InitSyncObjectDirtyBit = 0b0001;
                    const uint ChangesCount = 1;
                    const byte OperationId = (byte)SyncList<byte>.Operation.OP_SET;

                    var foundEffect = effectOwner.GetEffect(effect);
                    uint indexOfChange = (uint)effectOwner.ReferenceHub.playerEffectsController.GetIndexOf(foundEffect);

                    var newIntensity = intensity;

                    writer.WriteULong(InitSyncObjectDirtyBit);
                    writer.WriteUInt(ChangesCount);
                    writer.WriteByte(OperationId);
                    writer.WriteUInt(indexOfChange);
                    writer.WriteByte(newIntensity);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            });
        }

        public static int GetIndexOf(this PlayerEffectsController controller, StatusEffectBase effect)
        {
            for (int i = 0; i < controller.EffectsLength; i++)
            {
                if (controller.AllEffects[i] == effect)
                    return i;
            }

            throw new IndexOutOfRangeException("effect wasn't found");
        }

        public static bool IsKillswitched(this PluginFeature feature)
        {
            if (EntryPoint.Instance == null)
                return true;

            if (!EntryPoint.Instance.Config.FeaturesKillswitch.TryGetValue(feature, out var isKillswitched))
                return false;

            return isKillswitched;
        }

        public static Vector3 GetWorldPositionFrom(this Vector3 offset, RoomType room)
        {
            if (room == RoomType.Surface)
                return offset;

            var foundRoom = Room.Get(room);
            if (foundRoom == null)
            {
                Log.Warn($"null room: {offset}, {room}");
                return offset;
            }

            return foundRoom.WorldPosition(offset);
        }

        public static Color SetBrightness(this Color color, float brightness)
        {
            color *= brightness;
            color.a = 0.1f;
            return color;
        }
    }
}
