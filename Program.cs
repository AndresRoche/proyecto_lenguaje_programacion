using System;
using System.Collections.Generic;

namespace RpnRepl
{
    public sealed class Program
    {
        private static readonly Evaluador Evaluador = new();
        private static readonly Stack<Valor> Pila = new();
        private static readonly Entorno Entorno = new();

        public static void Main(string[] args)
        {
            Console.Title = "Mini-REPL RPN (C# / .NET 9.0)";
            Console.WriteLine("================================================================");
            Console.WriteLine(" Mini-REPL en Notación Polaca Inversa (RPN / Postfija)");
            Console.WriteLine(" Lenguaje anfitrión: C# (.NET 9.0)");
            Console.WriteLine("================================================================");
            Console.WriteLine(" Ejemplos:");
            Console.WriteLine("   3 4 +            => 7");
            Console.WriteLine("   x 10 =           => asigna 10 a x");
            Console.WriteLine("   x 5 + 2 *        => (x + 5) * 2");
            Console.WriteLine("   begin x 99 = end => scope local con shadowing");
            Console.WriteLine("   vars             => muestra variables activas");
            Console.WriteLine("   exit             => salir");
            Console.WriteLine("----------------------------------------------------------------");

            while (true)
            {
                try
                {
                    Console.Write("> ");
                    var linea = Console.ReadLine();

                    if (linea == null)
                    {
                        // Ctrl+Z o EOF
                        Console.WriteLine();
                        break;
                    }

                    var entrada = linea.Trim();
                    if (string.IsNullOrEmpty(entrada))
                        continue;

                    if (string.Equals(entrada, "exit", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(entrada, "salir", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    Evaluador.Ejecutar(entrada, Pila, Entorno);

                    if (Pila.Count > 0)
                    {
                        Console.WriteLine($"=> {Pila.Peek()}");
                    }
                }
                catch (ErrorDeTipo ex)
                {
                    Console.WriteLine($"error de tipos: {ex.Message}");
                }
                catch (ErrorDeEjecucion ex)
                {
                    Console.WriteLine($"error de ejecución: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error inesperado: {ex.Message}");
                }
            }

            Console.WriteLine("Saliendo del REPL...");
        }
    }
}
