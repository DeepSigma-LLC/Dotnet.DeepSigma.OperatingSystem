using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSigma.OperatingSystem.Models
{
    public record class ApplicationVersion(int Major, int Minor, int Build, int Patch)
    {
        public bool IsGreaterThan(ApplicationVersion other)
        {
            if (Major > other.Major) return true;
            if (Major < other.Major) return false;

            if (Minor > other.Minor) return true;
            if (Minor < other.Minor) return false;

            if (Build > other.Build) return true;
            if (Build < other.Build) return false;

            if (Patch > other.Patch) return true;
            if (Patch < other.Patch) return false;
            return false;
        }

        public bool IsEqualTo(ApplicationVersion other)
        {
            return this == other;
        }
    }
}
