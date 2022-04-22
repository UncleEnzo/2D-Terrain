namespace Nevelson.Terrain
{
    public interface IPitfallStates
    {
        /// <summary>
        /// Examples include: 
        ///  -Stopping character movement/controls
        ///  -Making invulnerable
        ///  -Disabling colliders
        ///  -Changing fall animation
        /// </summary>
        void PF_Before();

        /// <summary>
        /// Examples include:
        ///  -Sound effects for hitting water
        ///  -Splash particles and water ripples
        /// </summary>
        void PF_During();

        /// <summary>
        /// Object has respawned. Happens before object becomes visible.
        /// If the object is destroyed on Pitfall, After is called right before it is destroyed
        /// Examples include: 
        ///  -Applying damage
        ///  -Setting respawn location
        ///  -Making respawn poof effect
        ///  -Respawn sounds
        ///  -Hit flash
        ///  -Deducting score
        /// </summary>
        void PF_After();
    }
}