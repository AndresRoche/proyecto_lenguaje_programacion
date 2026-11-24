using System;
using System.Collections.Generic;

namespace RpnRepl
{
    /// <summary>
    /// Evaluador de expresiones en Notación Polaca Inversa (RPN / postfija).
    ///
    /// La pila de evaluación (Stack&lt;Valor&gt;) y el entorno de símbolos (Entorno)
    /// se pasan explícitamente a cada subrutina, lo que permite ilustrar en el reporte
    /// cómo C# maneja el paso de parámetros: los objetos Entorno y Stack se pasan por
    /// referencia de objeto (referencia), mientras que los valores int/double/bool
    /// se pasan por valor (copia). Los objetos Valor son inmutables lógicamente.
    /// </summary>
    public sealed class Evaluador
    {
        /// <summary>
        /// Ejecuta una línea de entrada RPN sobre la pila y el entorno dados.
        /// Los tokens se separan por espacios.
        /// </summary>
        public void Ejecutar(string linea, Stack<Valor> pila, Entorno entorno)
        {
            if (string.IsNullOrWhiteSpace(linea))
                return;

            var tokens = linea.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var tok in tokens)
            {
                switch (tok)
                {
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                        OperacionAritmetica(tok, pila, entorno);
                        break;

                    case "==":
                    case "!=":
                    case "<":
                    case ">":
                        OperacionComparacion(tok, pila, entorno);
                        break;

                    case "=":
                        Asignar(pila, entorno);
                        break;

                    case "begin":
                        entorno.IniciarBloque();
                        break;

                    case "end":
                        entorno.CerrarBloque();
                        break;

                    case "vars":
                        MostrarVariables(entorno);
                        break;

                    case "exit":
                    case "salir":
                        Environment.Exit(0);
                        break;

                    default:
                        EmpujarLiteralOVariable(tok, pila);
                        break;
                }
            }
        }

        #region Operaciones sobre la pila

        private static void Require(Stack<Valor> pila, int cantidad)
        {
            if (pila.Count < cantidad)
                throw new ErrorDeEjecucion($"stack underflow: se requieren {cantidad} elementos, hay {pila.Count}");
        }

        private static Valor Desapilar(Stack<Valor> pila)
        {
            if (pila.Count == 0)
                throw new ErrorDeEjecucion("stack underflow: la pila está vacía");
            return pila.Pop();
        }

        /// <summary>
        /// Resuelve un valor: si es un Nombre de variable, busca su valor en el entorno.
        /// Si ya es un valor concreto, se devuelve tal cual.
        /// </summary>
        private static Valor Resolver(Valor v, Entorno entorno)
        {
            if (v.Tipo == ValorTipo.Nombre)
            {
                string nombre = v.AsNombre();
                return entorno.Obtener(nombre);
            }
            return v;
        }

        #endregion

        #region Aritmética

        private void OperacionAritmetica(string op, Stack<Valor> pila, Entorno entorno)
        {
            Require(pila, 2);
            var b = Resolver(Desapilar(pila), entorno);
            var a = Resolver(Desapilar(pila), entorno);

            // Chequeo de tipos dinámico: 5 true + debe reportar error controlado, no abortar.
            double ra = a.AsFlotante();
            double rb = b.AsFlotante();

            double res = op switch
            {
                "+" => ra + rb,
                "-" => ra - rb,
                "*" => ra * rb,
                "/" => rb == 0
                    ? throw new ErrorDeEjecucion("división por cero")
                    : ra / rb,
                _ => throw new InvalidOperationException($"operador aritmético desconocido: {op}")
            };

            // Normalizar: si el resultado es entero exacto, almacenar como Entero.
            pila.Push(res % 1 == 0 ? Valor.Entero((int)res) : Valor.Flotante(res));
        }

        #endregion

        #region Comparación lógica

        private void OperacionComparacion(string op, Stack<Valor> pila, Entorno entorno)
        {
            Require(pila, 2);
            var b = Resolver(Desapilar(pila), entorno);
            var a = Resolver(Desapilar(pila), entorno);

            // Las comparaciones operan sobre números; resultan en Booleano.
            double ra = a.AsFlotante();
            double rb = b.AsFlotante();

            bool resultado = op switch
            {
                "==" => ra == rb,
                "!=" => ra != rb,
                "<"  => ra < rb,
                ">"  => ra > rb,
                _    => throw new InvalidOperationException($"operador de comparación desconocido: {op}")
            };

            pila.Push(Valor.Booleano(resultado));
        }

        #endregion

        #region Asignación y variables

        private void Asignar(Stack<Valor> pila, Entorno entorno)
        {
            // Formato postfijo: x 10 =  o  x 5 2 + =
            // El tope de la pila es el valor, y debajo debe estar el nombre (ValorTipo.Nombre).
            var valor = Desapilar(pila);
            var nombreToken = Desapilar(pila);

            if (nombreToken.Tipo != ValorTipo.Nombre)
                throw new ErrorDeEjecucion("se esperaba un nombre de variable a la izquierda de '='");

            string nombre = nombreToken.AsNombre();
            entorno.Definir(nombre, valor);
            pila.Push(valor); // la asignación devuelve el valor asignado (útil para encadenar)
        }

        private void EmpujarLiteralOVariable(string tok, Stack<Valor> pila)
        {
            // Intentar parsear como número entero
            if (int.TryParse(tok, out var entero))
            {
                pila.Push(Valor.Entero(entero));
                return;
            }

            // Intentar parsear como número flotante
            if (double.TryParse(tok, out var flotante))
            {
                pila.Push(Valor.Flotante(flotante));
                return;
            }

            // Literales lógicos
            if (string.Equals(tok, "true", StringComparison.OrdinalIgnoreCase))
            {
                pila.Push(Valor.Booleano(true));
                return;
            }
            if (string.Equals(tok, "false", StringComparison.OrdinalIgnoreCase))
            {
                pila.Push(Valor.Booleano(false));
                return;
            }

            // Cualquier otro identificador se trata como un nombre de variable.
            // Se empuja como ValorTipo.Nombre; se resolverá al usarse en una operación
            // o se consumirá como destino de asignación en '='.
            pila.Push(Valor.Nombre(tok));
        }

        #endregion

        private static void MostrarVariables(Entorno entorno)
        {
            var globales = entorno.ObtenerGlobales();
            if (globales.Count == 0)
            {
                Console.WriteLine("(sin variables)");
                return;
            }
            foreach (var kvp in globales)
            {
                Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
            }
        }
    }
}
