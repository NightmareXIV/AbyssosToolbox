﻿using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.MathHelpers;
using ECommons.SimpleGui;
using ECommons.SplatoonAPI;
using System;

namespace AbyssosToolbox
{
    public class AbyssosToolbox : IDalamudPlugin
    {
        internal static AbyssosToolbox P;
        public string Name => "AbyssosToolbox";
        internal Memory memory;
        internal const uint P6S = 1084;
        internal long ProcessAt = long.MaxValue;
        internal List<Vector2> CardinalTiles = new();
        internal List<Vector2> IntercardinalTiles = new();
        internal List<uint> Swaps = new();
        internal Config config;
        internal bool Combat = false;
        

        public AbyssosToolbox(DalamudPluginInterface pi)
        {
            P = this;
            ECommonsMain.Init(pi, this, Module.SplatoonAPI);
            new TickScheduler(delegate
            {
                config = Svc.PluginInterface.GetPluginConfig() as Config ?? new();
                memory = new();
                EzConfigGui.Init(Gui.Draw, config);
                EzCmd.Add("/abyssos", EzConfigGui.Open, "Open plugin interface");
                ClientState_TerritoryChanged( Svc.ClientState.TerritoryType);
            });
            Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;
        }

        internal void ClientState_TerritoryChanged(ushort e)
        {
            if(e == P6S || P.config.AlwaysEnable)
            {
                Combat = Svc.Condition[ConditionFlag.InCombat];
                PluginLog.Information($"Detected zone: {e}, enabling hooks");
                memory.Enable();
                Svc.Framework.Update += Framework_Update;
            }
            else
            {
                PluginLog.Information($"Detected zone: {e}, disabling hooks");
                memory.Disable();
                Svc.Framework.Update -= Framework_Update;
            }
        }

        private void Framework_Update(object framework)
        {
            if (Svc.Condition[ConditionFlag.InCombat] != Combat)
            {
                Combat = Svc.Condition[ConditionFlag.InCombat];
                if (Combat)
                {
                    PluginLog.Information("Entering combat");
                }
                else
                {
                    PluginLog.Information("Exiting combat");
                }
                ResetMechanic();
                Splatoon.RemoveDynamicElements("AbyssosToolbox.P6S_Tiles");
                PluginLog.Information("Clearing tiles");
            }
            if(Environment.TickCount64 > ProcessAt)
            {
                try
                {
                    foreach (var x in Svc.Objects)
                    {
                        if (Swaps.Contains(x.ObjectId))
                        {
                            if (CardinalTiles.TryGetFirst(z => Vector2.Distance(z, x.Position.ToVector2()) < 1, out var swapped))
                            {
                                CardinalTiles.Remove(swapped);
                                IntercardinalTiles.Add(swapped);
                                PluginLog.Information($"Swapping +->X {swapped}");
                            }
                            else if (IntercardinalTiles.TryGetFirst(z => Vector2.Distance(z, x.Position.ToVector2()) < 1, out var swapped2))
                            {
                                IntercardinalTiles.Remove(swapped2);
                                CardinalTiles.Add(swapped2);
                                PluginLog.Information($"Swapping X->+ {swapped2}");
                            }
                        }
                    }
                    var tiles = new HashSet<Vector2>();
                    foreach (var x in CardinalTiles)
                    {
                        foreach (var z in GenerateCardinalUnsafeTiles(x))
                        {
                            tiles.Add(z);
                        }
                    }
                    foreach (var x in IntercardinalTiles)
                    {
                        foreach (var z in GenerateIntercardinalUnsafeTiles(x))
                        {
                            tiles.Add(z);
                        }
                    }
                    var elements = new List<Element>();
                    foreach(var el in tiles.Where(x => x.X.InRange(80, 120) && x.Y.InRange(80, 120) && 
                    (P.config.HighlightSourceTiles || (!IntercardinalTiles.Contains(x) && !CardinalTiles.Contains(x)))
                    ))
                    {
                        elements.Add(CreateElement(el)) ;
                    }
                    Splatoon.AddDynamicElements("AbyssosToolbox.P6S_Tiles", elements.ToArray(), new long[] { -1, -2 });
                }
                catch(Exception e)
                {
                    e.Log();
                }
                ResetMechanic();
            }
        }

        internal static Element CreateElement(Vector2 x, float z = 0)
        {
            return new Element(ElementType.LineBetweenTwoFixedCoordinates)
            {
                refX = x.X,
                refY = x.Y - 5,
                refZ = z,
                offX = x.X,
                offY = x.Y + 5,
                offZ = z,
                Filled = true,
                radius = 5,
                includeRotation = true,
                color = P.config.Color,
                FillStep = P.config.FillStep,
                thicc = P.config.Thickness,
            };
        }

        internal void ResetMechanic()
        {
            PluginLog.Information($"Performing reset");
            ProcessAt = long.MaxValue;
            Swaps.Clear();
            IntercardinalTiles.Clear();
            CardinalTiles.Clear();
        }

        internal static IEnumerable<Vector2> GenerateCardinalUnsafeTiles(Vector2 source)
        {
            for (var i = -4; i <= 4; i++)
            {
                yield return source + new Vector2(10 * i, 0);
                yield return source + new Vector2(0, 10 * i);
            }
        }

        internal static IEnumerable<Vector2> GenerateIntercardinalUnsafeTiles(Vector2 source)
        {
            for (var i = -4; i <= 4; i++)
            {
                yield return source + new Vector2(-10 * i, -10 * i);
                yield return source + new Vector2(-10 * i, 10 * i);
            }
        }

        public void Dispose()
        {
            Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
            Svc.Framework.Update -= Framework_Update;
            memory.Dispose();
            ECommonsMain.Dispose();
            P = null;
        }
    }
}