using System.Collections.Generic;

namespace Promart.Core
{
    public abstract class BaseValidator
    {

        protected EntidadContextModel EntidadEducativa { get; set; }

        public HashSet<string> hsSiNo { protected get; set; }
        public HashSet<string> hsSexo { protected get; set; }
        public HashSet<string> hsTipoDocumentoIdentidad { protected get; set; }

        public BaseValidator()
        {

        }

        public BaseValidator(EntidadContextModel entidadEducativa)
        {
            EntidadEducativa = entidadEducativa;
        }

    }
}
