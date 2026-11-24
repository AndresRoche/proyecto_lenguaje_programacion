using System;

namespace RpnRepl
{
    /// <summary>
    /// Tipo compuesto (tagged union) que representa los valores del lenguaje embebido (RPN).
    /// El anfitrión C# aplica tipado estático fuerte; el lenguaje embebido realiza chequeo
    /// de tipos dinámico en tiempo de ejecución inspeccionando el discriminador Tipo.
    /// </summary>
    public sealed class Valor
    {
        public ValorTipo Tipo { get; }
        public object Dato { get; }

        private Valor(ValorTipo tipo, object dato)
        {
            Tipo = tipo;
            Dato = dato;
        }

        public static Valor Entero(int v) => new Valor(ValorTipo.Entero, v);
        public static Valor Flotante(double v) => new Valor(ValorTipo.Flotante, v);
        public static Valor Booleano(bool v) => new Valor(ValorTipo.Booleano, v);
        public static Valor Nombre(string nombre) => new Valor(ValorTipo.Nombre, nombre);

        public int AsEntero()
        {
            if (Dato is int i) return i;
            if (Dato is double d) return (int)d;
            throw new ErrorDeTipo($"el valor no es un entero: {Dato}");
        }

        public double AsFlotante()
        {
            if (Dato is double d) return d;
            if (Dato is int i) return i;
            throw new ErrorDeTipo($"el valor no es un número: {Dato}");
        }

        public bool AsBooleano()
        {
            if (Dato is bool b) return b;
            throw new ErrorDeTipo($"el valor no es un booleano: {Dato}");
        }

        public string AsNombre()
        {
            if (Dato is string s) return s;
            throw new ErrorDeTipo($"el valor no es un nombre de variable: {Dato}");
        }

        public override string ToString()
        {
            return Tipo switch
            {
                ValorTipo.Entero   => Dato.ToString(),
                ValorTipo.Flotante => Dato.ToString(),
                ValorTipo.Booleano => Dato.ToString().ToLower(),
                ValorTipo.Nombre   => $"<nombre:{Dato}>",
                _                  => Dato?.ToString() ?? "null"
            };
        }
    }

    public enum ValorTipo
    {
        Entero,
        Flotante,
        Booleano,
        Nombre
    }
}
