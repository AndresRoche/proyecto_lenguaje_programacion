using System;
using System.Collections.Generic;

namespace RpnRepl
{
    /// <summary>
    /// Tabla de símbolos con alcance por bloques (scoping).
    /// Implementa un stack de marcos (frames): cada 'begin' apila un nuevo marco local,
    /// cada 'end' lo descarta. La lectura busca desde el marco más interno hacia el
    /// global (shadowing). La escritura afecta únicamente al marco actual.
    /// </summary>
    public sealed class Entorno
    {
        private readonly Stack<Dictionary<string, Valor>> _marcos = new();

        public Entorno()
        {
            _marcos.Push(new Dictionary<string, Valor>()); // marco global
        }

        /// <summary>
        /// Abre un ámbito local temporal (begin).
        /// </summary>
        public void IniciarBloque()
        {
            _marcos.Push(new Dictionary<string, Valor>());
        }

        /// <summary>
        /// Cierra el ámbito local más interno (end).
        /// Las variables definidas dentro se descartan y no sobrescriben el global.
        /// </summary>
        public void CerrarBloque()
        {
            if (_marcos.Count <= 1)
                throw new ErrorDeEjecucion("no hay ningún bloque 'begin' abierto para cerrar");
            _marcos.Pop();
        }

        /// <summary>
        /// Define o actualiza una variable en el marco actual (scope local o global).
        /// Paso por referencia del objeto Entorno; la mutación persiste en el mismo objeto.
        /// </summary>
        public void Definir(string nombre, Valor valor)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("nombre de variable inválido", nameof(nombre));
            _marcos.Peek()[nombre] = valor;
        }

        /// <summary>
        /// Obtiene el valor de una variable buscando desde el marco más interno al externo.
        /// Implementa shadowing: una variable local oculta a la homónima global.
        /// </summary>
        public Valor Obtener(string nombre)
        {
            foreach (var marco in _marcos)
            {
                if (marco.TryGetValue(nombre, out var valor))
                    return valor;
            }
            throw new KeyNotFoundException($"variable no definida: '{nombre}'");
        }

        /// <summary>
        /// Devuelve una copia de todas las variables del marco global (para el comando vars).
        /// </summary>
        public IReadOnlyDictionary<string, Valor> ObtenerGlobales()
        {
            // El marco global es el último en el stack (el primero que se pusó).
            // Reconstruimos el diccionario global recorriendo todos los marcos y tomando
            // solo las que no fueron sombreadas en marcos superiores: para simplicidad
            // del REPL mostramos el contenido del marco global base.
            var global = new Dictionary<string, Valor>();
            foreach (var marco in _marcos)
            {
                foreach (var kvp in marco)
                {
                    if (!global.ContainsKey(kvp.Key))
                        global[kvp.Key] = kvp.Value;
                }
            }
            return global;
        }

        public int Profundidad => _marcos.Count;
    }
}
