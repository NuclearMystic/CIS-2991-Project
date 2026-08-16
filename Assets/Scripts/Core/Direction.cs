namespace CIS2991Project.Core
{
    // Shared by PlayerAnimationController, PlayerWeaponVisual, and Enemy's Animator-driven facing -
    // all three independently used this same 0=Down/1=Up/2=Left/3=Right convention (matching the
    // "Direction" int parameter every character's AnimatorController exposes) before this existed.
    public enum Direction
    {
        Down = 0,
        Up = 1,
        Left = 2,
        Right = 3
    }
}
