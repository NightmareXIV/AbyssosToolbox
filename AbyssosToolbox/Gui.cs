using ECommons.MathHelpers;
using ECommons.SplatoonAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbyssosToolbox
{
    internal static class Gui
    {

        static int a1 = 0;
        static int a2 = 0;
        static int a3 = 0;

        internal static void Draw()
        {
            ImGuiEx.EzTabBar("ABT",
                ("P6S Tiles", DrawTabSettings, null, true),
                ("Debug", DrawTabDebug, ImGuiColors.DalamudGrey3, true),
                ("Contribute", ECommons.ImGuiMethods.Donation.DonationTabDraw, ImGuiColors.ParsedGold, true)

                );
        }

        internal static void DrawTabDebug()
        {
            ImGuiEx.Text(ImGuiColors.DalamudRed, "Careful!");
            ImGui.InputInt("1", ref a1);
            ImGui.InputInt("2", ref a2);
            ImGui.InputInt("3", ref a3);
            if (P.memory.Addr != 0 && ImGui.Button("Invoke"))
            {
                P.memory.ProcessMapEffectHook.Original(P.memory.Addr, (uint)a1, (ushort)a2, (ushort)a3);
            }
        }

        internal static void DrawTabSettings()
        {
            /**/
            var c = P.config.Color.ToVector4();
            if(ImGui.ColorEdit4("Color", ref c, ImGuiColorEditFlags.NoInputs))
            {
                P.config.Color = c.ToUint();
            }
            ImGuiEx.TextWrapped(ImGuiColors.DalamudOrange, "These settings are subject to Splatoon general settings");
            ImGui.SetNextItemWidth(60);
            ImGui.DragFloat("Thickness", ref P.config.Thickness, 0.01f, 0.01f, 100f);
            ImGui.SetNextItemWidth(60);
            ImGui.DragFloat("Fill step", ref P.config.FillStep, 0.01f, 0.01f, 100f);
            ImGui.Checkbox("Highlight source tiles", ref P.config.HighlightSourceTiles);
            if (ImGui.Button("Test splatoon connection"))
            {
                var pos = Svc.ClientState.LocalPlayer.Position.ToVector2();
                var tiles = new HashSet<Vector2>();
                if (Environment.TickCount % 1000 > 500)
                {
                    foreach (var z in GenerateCardinalUnsafeTiles(pos))
                    {
                        tiles.Add(z);
                    }
                }
                else
                {
                    foreach (var z in GenerateIntercardinalUnsafeTiles(pos))
                    {
                        tiles.Add(z);
                    }
                }
                var elements = new List<Element>();
                foreach (var x in tiles.Where(x => P.config.HighlightSourceTiles || x != pos))
                {
                    elements.Add(CreateElement(x));
                }
                Splatoon.AddDynamicElements("AbyssosToolbox.P6S_Tiles", elements.ToArray(), new long[] { Environment.TickCount64 + 10000, -1, -2 });
            }
        }
    }
}
