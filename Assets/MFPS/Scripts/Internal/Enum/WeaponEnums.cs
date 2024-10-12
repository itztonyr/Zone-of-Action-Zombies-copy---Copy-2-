/// <summary>
/// The different type of weapons supported by the game.
/// </summary>
public enum GunType
{
    Shotgun = 0,
    Machinegun = 1,
    Sniper = 2,
    Pistol = 3,
    //Burst = 4, // Not used anymore
    Grenade = 5,
    Melee = 6,
    Launcher = 7,
    None = 99,
}

/// <summary>
/// How the bullet should be instantiated.
/// </summary>
public enum BulletInstanceMethod
{
    Pooled,
    Instanced,
}

/// <summary>
/// How the weapon should be reloaded.
/// Bullet = Reload one bullet at a time.
/// Magazine = Reload the entire magazine.
/// </summary>
public enum ReloadPer
{
    Bullet,
    Magazine,
}

/// <summary>
/// How calculate the projectile direction.
/// </summary>
public enum ProjectileDirectionMode
{
    /// <summary>
    /// Using the parent direction.
    /// </summary>
    ParentDirection,
    /// <summary>
    /// Using the camera direction.
    /// </summary>
    CameraDirection,
    /// <summary>
    /// Handled by the projectile script.
    /// </summary>
    HandleByProjectile
}

/// <summary>
/// Weapon shell shot options.
/// </summary>
public enum ShellShotOptions
{
    /// <summary>
    /// One single shot.
    /// </summary>
    SingleShot,
    /// <summary>
    /// Multiple shots, e.g. shotguns.
    /// </summary>
    PelletShot,
}

/// <summary>
/// How is calculated the time that takes the quick fire.
/// </summary>
public enum WeaponQuickFireTimeMode
{
    /// <summary>
    /// Based on the quick fire animation.
    /// </summary>
    AnimationTime,
    /// <summary>
    /// A fixed time.
    /// </summary>
    FixedTime
}