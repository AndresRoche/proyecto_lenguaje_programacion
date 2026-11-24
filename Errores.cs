using System;

namespace RpnRepl
{
    /// <summary>
    /// Excepciones específicas del evaluador RPN. Permiten distinguir errores de tipos
    /// de errores de ejecución (underflow, división por cero, token inválido) para
    /// reportarlos controladamente en el REPL sin abortar el programa.
    /// </summary>
    public sealed class ErrorDeTipo : Exception
    {
        public ErrorDeTipo(string mensaje) : base(mensaje) { }
    }

    public sealed class ErrorDeEjecucion : Exception
    {
        public ErrorDeEjecucion(string mensaje) : base(mensaje) { }
    }
}
