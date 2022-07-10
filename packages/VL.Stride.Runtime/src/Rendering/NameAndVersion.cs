using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Rendering
{
    internal record struct NameAndVersion(string NamePart, string VersionPart = null)
    {
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(VersionPart))
                return $"{NamePart} ({VersionPart})";
            return NamePart;
        }

        public static implicit operator string(NameAndVersion n) => n.ToString();
    }
}
