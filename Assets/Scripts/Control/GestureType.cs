namespace GestureControl.Data
{
    /// <summary>
    /// 手势类型枚举
    /// 用于标识系统支持的所有手势类型
    /// </summary>
    public enum GestureType
    {
        None,           // 无手势状态
        LeftFist,       // 左手握拳手势
        RightFist,      // 右手握拳手势
        DoubleFist,     // 双手同时握拳手势
        LeftPinch,      // 左手捏合手势（拇指与食指接触）
        RightPinch,     // 右手捏合手势
        DoublePinch,    // 双手同时捏合手势
        LeftSwipe,      // 左手滑动手势（水平方向）
        RightSwipe,    // 右手滑动手势
    }
}