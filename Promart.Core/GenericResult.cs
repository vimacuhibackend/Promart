using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Promart.Core
{
    /// <summary>
    /// Clase genérica de resultados con Payload (propiedad 'Data') tipado en base al inicializador
    /// </summary>
    /// <typeparam name="T">Tipo del payload a retornar. Ej. una clase, una lista de strings, un valor entero </typeparam>
    public class GenericResult<T> : GenericResult
    {
        #region Ctos
        public GenericResult()
        {
            Messages = new List<GenericMessage>();
        }
        public GenericResult(MessageType tipo, string mensaje, string codigo = null) : this()
        {
            Messages.Add(new GenericMessage(tipo, mensaje, codigo));
        }
        #endregion

        /// <summary>
        /// Payload de datos del tipo declarado en el inicializador.
        /// </summary>
        public T DataObject { get; set; }
    }

    /// <summary>
    /// Clase genérica de resultados
    /// </summary>
    public class GenericResult
    {
        #region Ctos
        public GenericResult()
        {
            Messages = new List<GenericMessage>();
            int Data = 0;
        }
        public GenericResult(MessageType tipo, string mensaje, string codigo = null) : this()
        {
            Messages.Add(new GenericMessage(tipo, mensaje, codigo));
        }

        public GenericResult(MessageType tipo, List<string> mensajes, string codigo = null) : this()
        {
            foreach (var mensaje in mensajes)
            {
                Messages.Add(new GenericMessage(tipo, mensaje, codigo));
            }
        }
        #endregion

        /// <summary>
        /// Flag que indica si la operación fue exitosa
        /// </summary>
        public bool Success => Messages.All(m => m.Type != MessageType.Error);

        /// <summary>
        /// Flag que indica si se han registrado errores
        /// </summary>
        public bool HasErrors => Messages.Any(m => m.Type == MessageType.Error);
        public bool HasWarnings => Messages.Any(m => m.Type == MessageType.Warning);

        /// <summary>
        /// Lista general de mensajes
        /// </summary>
        public List<GenericMessage> Messages { get; set; }
        public int Data { get; set; }
        public override string ToString()
        {
            var _mensaje = Messages.Select(x => x.Message);
            return string.Join(",", _mensaje);
        }

        #region Adds
        public void AddError(string mensajeError, string codigo = null)
        {
            Messages.Add(new GenericMessage(MessageType.Error, mensajeError, codigo));
        }
        public void AddWarning(string mensajeError, string codigo = null)
        {
            Messages.Add(new GenericMessage(MessageType.Warning, mensajeError, codigo));
        }
        public void AddInfo(string mensajeError, string codigo = null)
        {
            Messages.Add(new GenericMessage(MessageType.Info, mensajeError, codigo));
        }
        #endregion
    }


    public class GenericMessage
    {

        internal GenericMessage()
        {
        }


        /*Agregado para poder serializar*/
        [JsonConstructor]
        public  GenericMessage(MessageType type, string message, string code = null)
        {
            Type = type;
            Message = message;
            Code = code;
        }

        /*modificado para poder serializar*/
        [JsonProperty]
        public MessageType Type { get;  set; }
        [JsonProperty]
        public string Message { get;  set; }
        [JsonProperty]
        public string Code { get;  set; }


    }

    public enum MessageType
    {
        Info = 1,
        Warning = 2,
        Error = 3,
    }
}
