namespace Assets.Scripts.Swarmy
{
    [System.Serializable]
    public class ThrustData
    {
        // Holds references to the 4 physical motor components
        public MotorThrust FWD_RT;
        public MotorThrust FWD_LT;
        public MotorThrust AFT_RT;
        public MotorThrust AFT_LT;
    }
}