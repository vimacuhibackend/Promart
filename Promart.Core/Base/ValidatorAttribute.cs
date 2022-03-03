using System;

namespace Promart.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class ValidatorAttribute : Attribute
    {
        public abstract bool Validate(object input, out string mensaje);
    }
}
