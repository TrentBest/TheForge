using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.GURPS
{
    public class GURPS_ActorContext : IStateContext
    {
        // Metadata not needed on the GPU
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;
        public string SourceBook { get; set; }

        // The "Brain" for high-level decision making
        public FSMHandle Brain { get; private set; }

        // Reference to the raw data (or an index into a Global ComputeBuffer)
        public GURPS_ActorData RawData;

        public GURPS_ActorContext(string name, GURPS_ActorData initialData)
        {
            Name = name;
            RawData = initialData;
        }

        // --- API Methods (Logic that modifies the struct) ---

        public void TakeDamage(float amount)
        {
            // Logic abstracted from the raw memory
            RawData.CurrentHealth -= amount;

            // If health is critical, we can trigger an FSM evaluation
            if (RawData.CurrentHealth <= 0)
                Brain?.TransitionTo("Unconscious");
        }

        public void SetAttributes(int st, int dx, int iq, int ht)
        {
            RawData.ST = st;
            RawData.DX = dx;
            RawData.IQ = iq;
            RawData.HT = ht;
        }
    }
}