using System.ComponentModel;

namespace TEditXna.Editor
{
    public enum PaintMode
    {
        [Description("块/墙")]
        TileAndWall,
        [Description("线")]
        Wire,
        [Description("液体")]
        Liquid,
        [Description("轨道")]
        Track,
    }
    public enum TrackMode
    {
        [Description("轨道")]
        Track,
        [Description("促进器")]
        Booster,
        [Description("压力板")]
        Pressure,
        [Description("锤子")]
        Hammer,
    }
}