// This file is auto-generated. Modifications are not saved.

namespace UnityConstants
{
    public static class Tags
    {
        public const string Untagged = "Untagged";
        public const string Respawn = "Respawn";
        public const string Finish = "Finish";
        public const string EditorOnly = "EditorOnly";
        public const string MainCamera = "MainCamera";
        public const string Player = "Player";
        public const string GameController = "GameController";
    }

    public static class SortingLayers
    {
        public const int Default = 0;
    }

    public static class Layers
    {
        // Regular layer indices for assigning layers dynamically in code
        public const int Default = 0;
        public const int TransparentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Water = 4;
        public const int UI = 5;
        public const int PlaneXY = 8;
        public const int PlaneZY = 9;

        // Pre-configured layer masks for use with raycasts or other such queries
        public const int DefaultMask = 1 << Default;
        public const int TransparentFXMask = 1 << TransparentFX;
        public const int IgnoreRaycastMask = 1 << IgnoreRaycast;
        public const int WaterMask = 1 << Water;
        public const int UIMask = 1 << UI;
        public const int PlaneXYMask = 1 << PlaneXY;
        public const int PlaneZYMask = 1 << PlaneZY;
    }

    public static class Scenes
    {
    }
}

