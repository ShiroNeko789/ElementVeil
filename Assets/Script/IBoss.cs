/// <summary>
/// Add this interface to all boss classes: RubbishBoss, SmokeBoss, MushroomBoss.
/// ExitWall uses this to check death without caring which boss type it is.
/// </summary>
public interface IBoss
{
    bool IsDead();
}
