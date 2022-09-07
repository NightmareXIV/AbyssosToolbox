using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbyssosToolbox
{
    internal class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        public uint Color = 0x78F700FF;
        public float FillStep = 0.5f;
        public float Thickness = 2f;
        public bool HighlightSourceTiles = true;
    }
}
