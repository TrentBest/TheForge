using System.Collections.Generic;
using System.Linq;
using TheSingularityWorkshop.Builders;

namespace TheSingularityWorkshop.Libraries
{
    public static class BlueprintLibrary
    {
        // The "Database"
        public static Dictionary<string, MotorBuilder> Motors { get; private set; } = new Dictionary<string, MotorBuilder>();
        public static Dictionary<string, DroneBuilder> Drones { get; private set; } = new Dictionary<string, DroneBuilder>();

        static BlueprintLibrary()
        {
            // Seed with defaults for testing
            Register(new MotorBuilder("Standard_100N", 100f));
            Register(new MotorBuilder("Heavy_500N", 500f));
        }

        // --- CRUD Helpers ---

        public static void Register(MotorBuilder builder)
        {
            if (builder == null || string.IsNullOrEmpty(builder.Name)) return;

            // Upsert (Update or Insert)
            if (Motors.ContainsKey(builder.Name))
                Motors[builder.Name] = builder;
            else
                Motors.Add(builder.Name, builder);
        }

        public static void Register(DroneBuilder builder)
        {
            if (builder == null || string.IsNullOrEmpty(builder.DroneName)) return;

            if (Drones.ContainsKey(builder.DroneName))
                Drones[builder.DroneName] = builder;
            else
                Drones.Add(builder.DroneName, builder);
        }

        public static void DeleteMotor(string name)
        {
            if (Motors.ContainsKey(name)) Motors.Remove(name);
        }

        public static void RenameMotor(string oldName, MotorBuilder builder)
        {
            // If the name changed, we need to remove the old key and add the new one
            if (oldName != builder.Name)
            {
                DeleteMotor(oldName);
                Register(builder);
            }
            else
            {
                // Just update the value
                Register(builder);
            }
        }

        public static List<string> GetMotorNames() => Motors.Keys.ToList();
        public static MotorBuilder GetMotor(string name) => Motors.ContainsKey(name) ? Motors[name] : null;
    }
}