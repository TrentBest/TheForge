using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    /// <summary>
    /// The contract for any object that wants to be edited in the Singularity Hub.
    /// This fulfills the "Concrete Builder-Editor Duality".
    /// </summary>
    public interface IGuiProvider
    {
        /// <summary>
        /// The name to display in the Hub's list view.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Returns a delegate that constructs the UI into a provided root container.
        /// </summary>
        Action<VisualElement> GetGuiBuilder();
        SingularityVersion Version => SingularityVersion.Default;
        Texture2D RepresentationalImage => null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>

        VisualElement CreateGui(GuiContext ctx);

#if UNITY_EDITOR
        // Bake the C# Builder logic into a static UXML file
        void ToUIDocument(string assetPath);
#endif

        // Hydrate the logic from an existing UXML file (Runtime)
        void FromUIDocument(string assetPath);
    }


        /// <summary>
        /// Represents a strict semantic versioning structure for Singularity assets and packages.
        /// </summary>
        [Serializable]
        public struct SingularityVersion : IComparable<SingularityVersion>, IEquatable<SingularityVersion>
        {
            public int Major { get; }
            public int Minor { get; }
            public int Build { get; }

            public SingularityVersion(int major, int minor, int build)
            {
                Major = major;
                Minor = minor;
                Build = build;
            }

            // --- COMPARISON LOGIC FOR DEPENDENCY CRAWLERS ---

            public int CompareTo(SingularityVersion other)
            {
                if (Major != other.Major) return Major.CompareTo(other.Major);
                if (Minor != other.Minor) return Minor.CompareTo(other.Minor);
                return Build.CompareTo(other.Build);
            }

            public bool Equals(SingularityVersion other)
            {
                return Major == other.Major && Minor == other.Minor && Build == other.Build;
            }

            public override bool Equals(object obj)
            {
                return obj is SingularityVersion other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = Major;
                    hashCode = (hashCode * 397) ^ Minor;
                    hashCode = (hashCode * 397) ^ Build;
                    return hashCode;
                }
            }

            // --- OPERATOR OVERLOADS FOR CLEAN SYNTAX ---

            public static bool operator ==(SingularityVersion left, SingularityVersion right) => left.Equals(right);
            public static bool operator !=(SingularityVersion left, SingularityVersion right) => !left.Equals(right);
            public static bool operator <(SingularityVersion left, SingularityVersion right) => left.CompareTo(right) < 0;
            public static bool operator <=(SingularityVersion left, SingularityVersion right) => left.CompareTo(right) <= 0;
            public static bool operator >(SingularityVersion left, SingularityVersion right) => left.CompareTo(right) > 0;
            public static bool operator >=(SingularityVersion left, SingularityVersion right) => left.CompareTo(right) >= 0;

            public override string ToString() => $"{Major}.{Minor}.{Build}";

            // A safe default for uninitialized providers
            public static SingularityVersion Default => new SingularityVersion(1, 0, 0);
        }
    }
