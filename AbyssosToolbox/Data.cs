using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbyssosToolbox
{
    internal static class Data
    {
        internal static Dictionary<int, Vector2> MapEffects = new()
        {
            { 1, new(85, 85) },
            { 2, new(95, 85)},
            { 3, new(105,85)},
            { 4, new(115,85)},
            { 5, new(95,95)},
            { 6, new(105,95)},
            { 7, new(95, 105)},
            { 8, new(105,105)},
            { 9, new(85,115)},
            { 10, new(95,115)},
            { 11, new(105,115)},
            { 12, new(115,115)},
            { 13, new(85,105)},
            { 14, new(115,105)},
            { 15, new(85,95)},
            { 16, new(115,95)},
        };
    }
}
