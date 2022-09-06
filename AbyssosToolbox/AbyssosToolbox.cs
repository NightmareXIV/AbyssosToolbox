using Dalamud.Hooking;
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
        

        public AbyssosToolbox(DalamudPluginInterface pi)
        {
            P = this;
            ECommons.ECommons.Init(pi, Module.SplatoonAPI);
            new TickScheduler(delegate
            {
                memory = new();
                EzConfigGui.Init(this.Name, Gui.Draw);
                EzCmd.Add("/abyssos", EzConfigGui.Open, "Open plugin interface");
                ClientState_TerritoryChanged(null, Svc.ClientState.TerritoryType);
            });
            Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;
        }

        private void ClientState_TerritoryChanged(object sender, ushort e)
        {
            if(e == P6S)
            {
                PluginLog.Information("Detected P6S, enabling hooks");
                memory.Enable();
                Svc.Framework.Update += Framework_Update;
            }
            else
            {
                PluginLog.Information("Exiting P6S, disabling hooks");
                memory.Disable();
                Svc.Framework.Update -= Framework_Update;
            }
        }

        private void Framework_Update(Dalamud.Game.Framework framework)
        {
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
                    foreach(var x in tiles.Where(x => x.X.InRange(80, 120) && x.Y.InRange(80, 120) && !IntercardinalTiles.Contains(x) && !CardinalTiles.Contains(x)))
                    {
                        elements.Add(new Element(2)
                        {
                            refX = x.X,
                            refY = x.Y - 5,
                            refZ = 0,
                            offX = x.X,
                            offY = x.Y + 5,
                            offZ = 0,
                            Filled = true,
                            radius = 5,
                            includeRotation = true,
                            color = 0x78F700FF
                        }) ;
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

        internal void ResetMechanic()
        {
            ProcessAt = long.MaxValue;
            Swaps.Clear();
            IntercardinalTiles.Clear();
            CardinalTiles.Clear();
        }

        IEnumerable<Vector2> GenerateCardinalUnsafeTiles(Vector2 source)
        {
            for (var i = -4; i <= 4; i++)
            {
                yield return source + new Vector2(10 * i, 0);
                yield return source + new Vector2(0, 10 * i);
            }
        }

        IEnumerable<Vector2> GenerateIntercardinalUnsafeTiles(Vector2 source)
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
            ECommons.ECommons.Dispose();
            P = null;
        }
    }
}