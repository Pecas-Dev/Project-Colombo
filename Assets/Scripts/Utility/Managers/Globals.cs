
using System.Drawing;
using UnityEngine;

namespace ProjectColombo
{
    public static class GameGlobals
    {
        public enum MusicScale { NONE = 0, MAJOR, MINOR }

        public enum MusicNote { C5, CS5, CS6, C6, A4, A5, AS4, F5, FS5, G5, GS5, E5, DS5, D5 }

        public static int TILESIZE = 31;

        public static Color32 majorColor = new Color32(0xF3, 0xC3, 0x48, 255);
        public static Color32 minorColor = new Color32(0x79, 0x31, 0xDF, 255);
    }
}