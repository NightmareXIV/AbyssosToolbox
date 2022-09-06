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
            /*ImGui.InputInt("1", ref a1);
            ImGui.InputInt("2", ref a2);
            ImGui.InputInt("3", ref a3);
            if (P.memory.Addr != 0 && ImGui.Button("Invoke"))
            {
                P.memory.ProcessMapEffectHook.Original(P.memory.Addr, (uint)a1, (ushort)a2, (ushort)a3);
            }*/
            if (ImGui.Button("Test splatoon connection"))
            {
                var el = new Element(0)
                {
                    refX = Svc.ClientState.LocalPlayer.Position.X,
                    refY = Svc.ClientState.LocalPlayer.Position.Z,
                    refZ = Svc.ClientState.LocalPlayer.Position.Y,
                    radius = 5f,
                    color = ImGuiColors.DalamudViolet.ToUint()
                };
                Splatoon.AddDynamicElements("Splatoon test", new Element[] { el }, new long[] { Environment.TickCount64 + 10000, -1, -2 });
            }
        }
    }
}
