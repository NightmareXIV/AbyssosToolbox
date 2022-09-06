using Dalamud;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.GameFunctions;
using ECommons.SplatoonAPI;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Reloaded.Hooks.Definitions.X64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbyssosToolbox
{
    internal unsafe class Memory : IDisposable
    {
        internal delegate long ProcessMapEffect(long a1, uint a2, ushort a3, ushort a4);
        [Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 41 0F B7 E8", DetourName = nameof(ProcessMapEffectDetour), Fallibility =Fallibility.Infallible)]
        internal Hook<ProcessMapEffect> ProcessMapEffectHook = null;
        internal long Addr = 0;

        delegate long ProcessTether(Character* a1, byte a2, byte a3, long targetOID, byte a5);
        [Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 20 0F B6 6C 24", DetourName =nameof(ProcessTetherDetour))]
        Hook<ProcessTether> ProcessTetherHook;


        internal Memory()
        {
            SignatureHelper.Initialise(this);
        }

        internal void Enable() 
        {
            if(!ProcessMapEffectHook.IsEnabled) ProcessMapEffectHook.Enable();
            if(!ProcessTetherHook.IsEnabled) ProcessTetherHook.Enable();
        }

        internal void Disable()
        {
            if (ProcessMapEffectHook.IsEnabled) ProcessMapEffectHook.Disable();
            if (ProcessTetherHook.IsEnabled) ProcessTetherHook.Disable();
        }

        long ProcessTetherDetour(Character* a1, byte a2, byte a3, long targetOID, byte a5)
        {
            if (a1 != null && a1->NameID == 0x6C)
            {
                DuoLog.Information($"Swaps: {(IntPtr)a1:X16}, {a2}, {a3}, {targetOID:X16}, {a5}");
                P.Swaps.Add(a1->GameObject.ObjectID);
                P.Swaps.Add((uint)targetOID);
                P.ProcessAt = Environment.TickCount64 + 500;
            }
            return ProcessTetherHook.Original(a1, a2, a3, targetOID, a5);
        }


        internal long ProcessMapEffectDetour(long a1, uint a2, ushort a3, ushort a4)
        {
            Addr = a1;
            if (a2 > 0)
            {
                DuoLog.Information($"MapEffect: {a2}, {a3}, {a4}");
                {
                    if (a3 == 1 && a4 == 2 && Data.MapEffects.TryGetValue((int)a2, out var v))
                    {
                        P.CardinalTiles.Add(v);
                        DuoLog.Information($"Cardinal tile: {v}");
                        P.ProcessAt = Environment.TickCount64 + 500;
                    }
                }
                {
                    if (a3 == 32 && a4 == 64 && Data.MapEffects.TryGetValue((int)a2, out var v))
                    {
                        P.IntercardinalTiles.Add(v);
                        DuoLog.Information($"Intercardinal tile: {v}");
                        P.ProcessAt = Environment.TickCount64 + 500;
                    }
                }
            }
            else
            {
                DuoLog.Information("Mechanic start/end");
                P.ResetMechanic();
                Splatoon.RemoveDynamicElements("AbyssosToolbox.P6S_Tiles");
            }
            return ProcessMapEffectHook.Original(a1, a2, a3, a4);
        }

        public void Dispose()
        {
            this.Disable();
            ProcessMapEffectHook.Dispose();
            ProcessTetherHook.Dispose();
        }
    }
}
