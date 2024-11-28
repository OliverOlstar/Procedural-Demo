namespace ODev.Update
{
    public enum Priority
    {
        First = int.MinValue,
        Input = -2000,
        UI = -1000,
        World = -200,
        Default = 0,
        OnGround = 350,
        CharacterAbility = 375,
        CharacterController = 400,
        ModelController = 500,
        Interactator = 700,
        Camera = 1000,
        PoseAnimator = 5000,
        Last = int.MaxValue
    }
}
