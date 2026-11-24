# Mini-REPL RPN en C# (.NET 9.0)

Evaluador interactivo de consola que procesa expresiones matemáticas y lógicas en
**Notación Polaca Inversa (RPN / postfija)** sobre una pila, desarrollado como
componente práctico de la asignación de Diseño e Implementación de Lenguajes
de Programación.

> **Nota sobre la versión de .NET:** Este proyecto apunta a **.NET 9.0** (`net9.0`).

---

## Estructura del proyecto

```
├── RpnRepl.csproj      # Proyecto de consola (.NET 9.0)
├── Program.cs          # Mini-REPL: bucle de lectura, captura de errores
├── Evaluador.cs        # Motor de evaluación postfija sobre pila
├── Entorno.cs          # Tabla de símbolos con alcance por bloques (scoping)
├── Valor.cs            # Tipo compuesto (tagged union) para Int/Float/Bool
└── Errores.cs          # Excepciones específicas (ErrorDeTipo, ErrorDeEjecucion)
```

---

## Requisitos

- .NET 9.0 SDK (o superior) instalado.
- Un compilador de C# compatible con C# 12 o superior (se usa `switch` expressions
  y target-typed `new()`).

---

## Compilación

Desde la carpeta del proyecto:

```bash
dotnet build
```

---

## Ejecución

Desde la carpeta del proyecto:

```bash
dotnet run
```

---

## Uso del Mini-REPL

Escribe expresiones en notación postfija separando los tokens por espacios.
Los comandos especiales son: `begin`, `end`, `vars`, `exit`.

### Operadores soportados

| Token | Descripción | Ejemplo | Resultado |
|-------|-------------|---------|-----------|
| `+` | Suma | `3 4 +` | `7` |
| `-` | Resta | `10 4 -` | `6` |
| `*` | Multiplicación | `3 4 *` | `12` |
| `/` | División | `10 2 /` | `5` |
| `==` | Igualdad | `3 3 ==` | `true` |
| `!=` | Diferencia | `3 4 !=` | `true` |
| `<` | Menor que | `3 4 <` | `true` |
| `>` | Mayor que | `5 2 >` | `true` |
| `=` | Asignación (postfija) | `x 10 =` | asigna 10 a `x` |

### Comandos

| Comando | Descripción |
|---------|-------------|
| `begin` | Abre un ámbito local temporal (nuevo marco de símbolos) |
| `end` | Cierra el ámbito local y descarta sus variables |
| `vars` | Muestra las variables activas en memoria |
| `exit` / `salir` | Sale del REPL |

### Literales

- Números enteros: `10`, `3`, `-5`
- Números flotantes: `3.14`, `2.5`
- Booleanos: `true`, `false`

---

## Sesión de prueba de ejemplo

```
================================================================
 Mini-REPL en Notación Polaca Inversa (RPN / Postfija)
 Lenguaje anfitrión: C# (.NET 9.0)
================================================================
 Ejemplos:
   3 4 +            => 7
   x 10 =           => asigna 10 a x
   x 5 + 2 *        => (x + 5) * 2
   begin x 99 = end => scope local con shadowing
   vars             => muestra variables activas
   exit             => salir
----------------------------------------------------------------
> 3 4 +
=> 7
> x 10 =
=> 10
> x 5 + 2 *
=> 30
> 10 0 /
error de ejecución: división por cero
> 5 true +
error de tipos: el valor no es un número: true
> 1 2 +
=> 3
> 4 +
error de ejecución: stack underflow: se requieren 2 elementos, hay 1
> xyz
error de ejecución: variable no definida: 'xyz'
> begin
> x 99 =
=> 99
> x
=> 99
> end
> x
=> 10
> vars
  x = 30
> true false ==
=> false
> 3 4 < 5 6 > and
error de ejecución: stack underflow: se requieren 2 elementos, hay 1
> exit
Saliendo del REPL...
```

### Explicación de casos destacados

- `10 0 /` → **división por cero**: capturado por `ErrorDeEjecucion`, el REPL continúa.
- `5 true +` → **error de tipos**: `true` no es un número, `AsFlotante()` lanza `ErrorDeTipo`.
- `4 +` (tras un resultado previo) → **stack underflow**: la pila tiene menos de 2 elementos.
- `begin x 99 = end` → **scoping con shadowing**: `x` vale `99` dentro del bloque, pero al salir con `end` se restaura el valor global `10`.
- `vars` → muestra la tabla de símbolos del entorno global.

---

## Puntos teóricos reflejados en el código (Parte I del reporte)

| Punto del reporte | Cómo se ve en el código |
|-------------------|-------------------------|
| Nombres, binding, alcance | `Entorno.Definir` / `Obtener`; `begin`/`end` crean marcos temporales |
| Sistema de tipos | Host C# estático con `var`; valor embebido con `ValorTipo` etiquetado, chequeo dinámico |
| Subrutinas y paso de parámetros | `Evaluador.Ejecutar(stack, entorno)`; `Valor` inmutable (valor), `Entorno`/`Stack` como referencia |
| Memoria | `Stack<Valor>` y `Dictionary` en heap; gestionados por el GC de .NET |
| Abstracción de datos / POO | `sealed class` para `Valor`, `Entorno`, `Evaluador`, `Program`; encapsulación de estado |

---

## Entregable adicional

El reporte técnico completo (Parte I) se encuentra en el archivo
`ReporteTecnico.md` (convertible a PDF) en la carpeta raíz `proyecto/`.

---
