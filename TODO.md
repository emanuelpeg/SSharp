<p align="center">
  <img src="./assets/ssharp.jpeg" alt="SSharp Logo" width="130" />
</p>

# SSharp — Roadmap hacia un Lenguaje 100% Funcional Puro (TODO)

## 🌿 Filosofía de Diseño: *Pure Functions & Data First* (Referencia: Haskell)

A diferencia de los lenguajes híbridos orientados a objetos (como Scala o C#), la meta y visión fundamental de **SSharp** es ser un lenguaje **puramente funcional**, tomando a **Haskell** como lenguaje de referencia canónico:

1. **Sin métodos orientados a objetos (`xs.method()`)**:
   - No existen métodos de instancia adjuntos a objetos o estructuras de datos.
   - Los datos son estructuras inmutables pasivas (ADTs, records, tuplas) y **todas las operaciones son funciones libres puras**:
     - ❌ *Orientado a objetos (a erradicar)*: `xs.head`, `xs.tail`, `xs.map(f)`, `xs.filter(p)`, `xs.size`, `opt.getOrElse(default)`, `s.contains(x)`, `m.get(k)`
     - ✅ *100% Funcional Puro*: `head(xs)`, `tail(xs)`, `map(f, xs)`, `filter(p, xs)`, `length(xs)`, `getOrElse(default, opt)`, `contains(x, s)`, `get(k, m)`
2. **`head` y `tail` como Funciones Puras**:
   - En Haskell: `head :: [a] -> a` y `tail :: [a] -> [a]`.
   - En SSharp debe escribirse estrictamente `head(list)` y `tail(list)`, nunca `list.head` ni `list.headValue`.
   - Además, para evitar excepciones en listas vacías, se adoptarán las funciones puras seguras `headOption(xs): Option[T]` y `tailOption(xs): Option[List[T]]` (análogo a `listToMaybe` / `uncons` en Haskell).
3. **Composición y Flujo con Operador Pipe (`|>`)**:
   - Encadenamiento funcional de izquierda a derecha (análogo al operador `&` de Haskell en `Data.Function`):
     ```ssharp
     val resultado = xs
         |> filter(isEven)
         |> map(doubleIt)
         |> take(5)
     ```
4. **Transparencia Referencial y Control de Efectos**:
   - Todas las funciones son deterministas y libres de efectos secundarios ocultos.
   - La entrada/salida y mutabilidad deben estar aisladas y gobernadas mediante tipos monádicos puros (Mónada `IO`), al igual que en Haskell.

---

## 🔍 Auditoría Exhaustiva: Elementos No Funcionales Detectados en el Repositorio

Tras una inspección profunda de todo el código fuente, compilador, runtime, pruebas y ejemplos, se han detectado los siguientes aspectos imperativos u orientados a objetos que deben ser modificados:

### 1. Ejemplos con Sintaxis No Funcional / Métodos Orientados a Objetos (`Sample/`)

- **`Sample/collections.ss` [CRÍTICO - Roto]**:
  - Utiliza profusamente la sintaxis de métodos y propiedades de objetos con punto `.`:
    - Lista: `l.size`, `l.headValue`, `l.tailList`, `l.reverse`, `l.take(3)`, `l.drop(2)`, `l.contains(3)`, `l.forall(...)`, `l.exists(...)`, `l.map(...)`, `l.filter(...)`, `l.foldLeft(...)`, `l.appended(...)`, `l.prepended(...)`, `l.concat(...)`.
    - Set: `s.size`, `s.contains(2)`, `s.incl(10)`, `s.excl(2)`, `s.union(sB)`, `s.intersect(sB)`, `s.diff(sB)`, `s.toList`.
    - Map: `m.size`, `m.contains(...)`, `m.get(...)`, `m.updated(...)`, `m.removed(...)`, `m.keys`, `m.values`, `m.toList`.
  - *Acción requerida*: Modificar completamente `Sample/collections.ss` para usar exclusivamente funciones libres del Prelude: `head(l)`, `tail(l)`, `length(l)`, `take(3, l)`, `drop(2, l)`, `contains(3, l)`, `forall(p, l)`, `exists(p, l)`, `map(f, l)`, `filter(p, l)`, `foldLeft(z, f, l)`, `append(l, 6)`, `prepend(0, l)`, `concat(l, l2)`. Y para Set/Map: `size(s)`, `contains(elem, s)`, `insert(elem, s)`, `delete(elem, s)`, `union(sA, sB)`, `get(k, m)`, `keys(m)`, `values(m)`, etc.

- **`Sample/monads.ss` [CRÍTICO - Roto]**:
  - Invoca métodos de instancia sobre `Option`, `List`, `Set` y `Map`:
    - `opt1.isEmpty`, `opt1.isDefined`, `opt2.getOrElse(...)`, `opt1.flatMap(...)`, `nestedOpt.flatten()`.
    - `l.flatMap(...)`, `nestedList.flatten()`.
    - `s.flatMap(...)`, `nestedSet.flatten()`.
    - `m.flatMap(...)`.
  - *Acción requerida*: Reemplazar todas las invocaciones por funciones libres puras: `isEmpty(opt1)`, `isDefined(opt1)`, `getOrElse(99, opt2)`, `flatMap(f, opt1)`, `flatten(nestedOpt)`, `flatMap(f, l)`, `flatten(nestedList)`, etc.

- **`Sample/variance.ss` [Conceptual - Paradigma OO]**:
  - Demuestra varianza de subtipado de orientación a objetos (`+A` covarianza, `-A` contravarianza) y herencia con `trait Container[+A]`, `trait Printer[-A]` y clases mutables `trait MutableBox[A]`.
  - En Haskell no existe el subtipado ($A <: B$), ni herencia ni varianza; todo el polimorfismo se modela de forma paramétrica ($f :: a \to b$) o ad-hoc mediante Type Classes (`Functor`, `Contravariant`).
  - *Acción requerida*: Eliminar o rediseñar el ejemplo hacia polimorfismo paramétrico puro, Type Classes y tipos algebraicos.

- **`Sample/hello.ss` [Efectos Secundarios Impuros]**:
  - Contiene funciones con efectos secundarios ocultos: `def sideEffect(): Boolean = { println("Side effect executed!"); true }` evaluadas dentro de funciones booleanas puras (`and(false, sideEffect())`).
  - En Haskell, ninguna función pura puede desencadenar efectos colaterales; las operaciones de E/S deben devolver `IO a`.
  - *Acción requerida*: Adecuar el ejemplo al modelo de pureza referencial.

- **`Sample/tuples.ss` [Acceso a Miembros con Punto]**:
  - Accede a los elementos de tuplas con notación punto OO: `persona._1`, `persona._2`, `persona._3`.
  - En Haskell se utiliza pattern matching directo `val (nombre, edad, activo) = persona` o funciones selectoras libres (`fst`, `snd`).
  - *Acción requerida*: Promover pattern matching y proveer funciones selectoras puras (`fst`, `snd`) eliminando la dependencia de la notación punto.

- **`SSharp.Spec/cases/025_simple_trait_and_class/input.ss` [Acceso a Campos OO]**:
  - Accede a los campos del case class con notación punto: `s.name`, `s.age`.
  - En Haskell, los records generan automáticamente funciones selectoras: `name s`, `age s`.
  - *Acción requerida*: Soportar funciones selectoras generadas automáticamente `name(s)` y `age(s)` o deconstrucción por pattern matching.

---

### 2. Funciones de Lista (`head`, `tail`, `init`, `last`) ✅

- **Estado**: **COMPLETADO**
- **Cambios realizados**:
  - ✅ `HeadValue` y `TailList` en `SSharp.Runtime/List.cs` cambiados de `public` a `internal`, forzando el uso de `head(xs)` y `tail(xs)` desde código SSharp.
  - ✅ `head`, `tail`, `reduce`, `init` y `last` en `Predef.cs` reescritos con pattern matching directo sobre `Cons<T>`, sin dependencia de las propiedades internas.
  - ✅ Funciones seguras implementadas y tipadas en `TypeChecker.cs`:
    - `headOption[T](xs: List[T]): Option[T]` (equivalente a `listToMaybe` en Haskell).
    - `tailOption[T](xs: List[T]): Option[List[T]]`.
    - `uncons[T](xs: List[T]): Option[(T, List[T])]` (equivalente a `Data.List.uncons`).
    - `init[T](xs: List[T]): List[T]` y `last[T](xs: List[T]): T`.
  - ✅ Spec case `053_head_and_tail_pure_functions` creado y validado.

---

### 3. Funciones Libres para `Option`, `Set` y `Map` en el Prelude ✅

- **Estado**: **COMPLETADO** (implementadas en `Predef.cs`, tipadas en `TypeChecker.cs` y validadas con tests `054` y `055`).

- **Para `Option[T]` (referencia Haskell: `Maybe a`)**:
  - [x] `isSome[T](opt: Option[T]): Boolean` / `isDefined` (Haskell: `isJust`)
  - [x] `isNone[T](opt: Option[T]): Boolean` / `isEmpty` (Haskell: `isNothing`)
  - [x] `getOrElse[T](defaultVal: T, opt: Option[T]): T` (Haskell: `fromMaybe`)
  - [x] `flatten[T](opt: Option[Option[T]]): Option[T]` (Haskell: `join`)

- **Para `Set[T]` (referencia Haskell: `Data.Set`)**:
  - [x] `size[T](s: Set[T]): Int` (Haskell: `size`)
  - [x] `isEmpty[T](s: Set[T]): Boolean` (Haskell: `null`)
  - [x] `contains[T](elem: T, s: Set[T]): Boolean` (Haskell: `member`)
  - [x] `insert[T](elem: T, s: Set[T]): Set[T]` / `incl`
  - [x] `delete[T](elem: T, s: Set[T]): Set[T]` / `excl`
  - [x] `union[T](s1: Set[T], s2: Set[T]): Set[T]`
  - [x] `intersect[T](s1: Set[T], s2: Set[T]): Set[T]` (Haskell: `intersection`)
  - [x] `diff[T](s1: Set[T], s2: Set[T]): Set[T]` (Haskell: `difference`)
  - [x] `toList[T](s: Set[T]): List[T]` (Haskell: `toList`)
  - [x] `map[T, U](f: T => U, s: Set[T]): Set[U]`
  - [x] `filter[T](p: T => Boolean, s: Set[T]): Set[T]`

- **Para `Map[K, V]` (referencia Haskell: `Data.Map`)**:
  - [x] `size[K, V](m: Map[K, V]): Int`
  - [x] `isEmpty[K, V](m: Map[K, V]): Boolean`
  - [x] `contains[K, V](k: K, m: Map[K, V]): Boolean` (Haskell: `member`)
  - [x] `get[K, V](k: K, m: Map[K, V]): Option[V]` (Haskell: `lookup`)
  - [x] `insert[K, V](k: K, v: V, m: Map[K, V]): Map[K, V]` / `updated`
  - [x] `delete[K, V](k: K, m: Map[K, V]): Map[K, V]` / `removed`
  - [x] `keys[K, V](m: Map[K, V]): Set[K]`
  - [x] `values[K, V](m: Map[K, V]): List[V]` (Haskell: `elems`)
  - [x] `toList[K, V](m: Map[K, V]): List[(K, V)]` (Haskell: `toList`)

---

### 4. Elementos Imperativos en el Runtime (`SSharp.Runtime`)

- **Métodos `Foreach(Action<...>)` [Imperativo - No Funcional]**:
  - En `List.cs`, `Set.cs` y `Map.cs` existe el método `Foreach(Action<T>)`.
  - Este método promueve la iteración imperativa y efectos secundarios por mutación, en lugar de reducciones puras (`foldLeft`/`foldRight`), transformaciones (`map`) o recursión.
  - *Acción requerida*: Eliminar `Foreach` del Runtime de SSharp.
- **Métodos de Instancia Públicos**:
  - En `SSharpList`, `SSharpOption`, `SSharpSet` y `SSharpMap` se conservan métodos de instancia públicos (`Map`, `Filter`, etc.).
  - Deben quedar como detalles internos de implementación en C# o ser reemplazados completamente por la API estática de `Predef`.

---

### 5. Compilador: Notación Punto, Subtipado y ADTs (`SSharp.Compiler`)

- **Operador Punto (`.`) vs. Composición Funcional**:
  - En Haskell, el operador `.` es la composición matemática de funciones: `(f . g) x = f (g x)`.
  - En SSharp, el parser aún tiene `MemberAccessExpr`.
  - *Acción requerida*:
    1. Eliminar soporte sintáctico de llamadas a métodos (`expr.method(...)`).
    2. Redireccionar el acceso a registros `record.field` a funciones selectoras de primera clase `field(record)`.
    3. Liberar el token `.` para que pueda ser utilizado como operador de composición de funciones o implementar `compose(f, g)`.
- **Subtipado y Varianza (`trait`, `class`, `extends`, `+A`, `-A`)**:
  - En SSharp, los ADTs se modelan actualmente usando el patrón orientado a objetos de Scala 2 (`sealed trait Base`, `case class Deriv(...) extends Base`).
  - Esto introduce un sistema de subtipado complejo (`IsSubtype`, `LUB`, `Variance.Covariant`, etc.) que no pertenece a un lenguaje puramente funcional como Haskell.
  - *Acción requerida*: Diseñar e incorporar sintaxis nativa de tipos de datos algebraicos puros (ejemplo: `data Shape = Circle(radius: Double) | Rectangle(width: Double, height: Double)` o `enum Shape`) sin herencia ni `extends`.

---

### 6. Transparencia Referencial y Mónada IO

- **Estado actual**:
  - `print`, `println`, `readLine` devuelven `Unit` o `String` y ejecutan operaciones de E/S directamente en cualquier punto del programa.
  - Cualquier función puede ejecutar efectos secundarios sin estar tipada.
- **Referencia Haskell**:
  - En Haskell, toda operación con efectos de entrada/salida está encapsulada en el tipo monádico `IO a`. Una función con efectos es un valor de tipo `IO[Unit]`, preservando la transparencia referencial en el resto del lenguaje.
- **Acción requerida**:
  - Diseñar el tipo monádico `IO[A]` para encapsular operaciones de entrada/salida.
  - El punto de entrada `main` debe retornar `IO[Unit]` (o desugarear secuenciación monádica en `main`).

---

## 📊 Matriz Comparativa: Haskell vs. SSharp

| Dimensión / Característica | Haskell (Referencia Pura) | SSharp Actual | Meta en SSharp Funcional Puro | Estado |
| :--- | :---: | :---: | :---: | :---: |
| **Paradigma** | 100% Funcional Puro | Híbrido funcional con vestigios OO | **100% Funciones Libres & Datos Puros** | 🔄 En migración |
| **`head` y `tail`** | `head xs`, `tail xs` | `head(xs)`, `tail(xs)` / `l.headValue` en ejemplos | **`head(xs)`, `tail(xs)` obligatorios** | ⚠️ Ejemplos por corregir |
| **`headOption` / `uncons`** | `listToMaybe xs`, `uncons xs` | ❌ Lanza excepción en `Nil` | `headOption(xs)`, `tailOption(xs)` | **Prioridad 1** |
| **Invocación de Colecciones** | `map f xs`, `length xs` | `map(f, xs)` en listas, pero OO en `Sample` | `map(f, xs)`, `filter(p, xs)` en todas | ⚠️ Samples por corregir |
| **Prelude Set & Map** | `Data.Set`, `Data.Map` libres | ❌ Sin funciones libres en `Predef` | Funciones libres completas (`lookup`, `insert`, etc.) | **Prioridad 1** |
| **Operador Pipe / Composición** | `&` (`Data.Function`) / `.` | `\|>` nativo / ❌ Sin operador `.` de composición | `\|>` + composición pura de funciones | ✅ `\|>` / **Prioridad 2** comp. |
| **Notación Punto (`.`)** | Composición de funciones `(f . g)` | Acceso a miembros/métodos OO | Remoción de métodos OO; selectoras como funciones | **Prioridad 1** |
| **Tuplas de Aridad N** | `(a, b, c)` + `fst`, `snd` | `(a, b, c)` + `_1`, `_2` OO | `(a, b, c)` + pattern matching + `fst`/`snd` | 🔄 Tuplas listas, faltan `fst`/`snd` |
| **Pattern Matching & Guards** | `case x of ... \| cond ->` | `case x if cond =>`, `case x @ pat =>` | Guards y As-patterns funcionales | ✅ Completado |
| **Comprensiones Monádicas** | `do` notation / list comp `[x \| ...]` | `for { x <- xs } yield ...` (desugared) | Desugared a `flatMap`/`map`/`filter` | ✅ Completado |
| **Definición de ADTs** | `data Shape = Circle Float \| ...` | `sealed trait` + `case class extends` | ADT sintaxis pura sin herencia OO (`data`) | **Prioridad 2** |
| **Subtipado y Varianza** | ❌ No hay subtipado ni `+T`/`-T` | `+A`, `-A`, jerarquía de subtipos | Paramétrico puro + Type Classes (`Functor`, etc.) | **Prioridad 3** |
| **Type Classes** | `class Functor f where ...` | ❌ No disponible | `trait Functor[F[_]]` + resolución de instancias | **Prioridad 3** |
| **Transparencia Referencial (IO)** | Mónada `IO a` | Efectos impuros en funciones con `Unit` | Mónada `IO[A]` para acotar efectos secundarios | **Prioridad 3** |
| **Combinadores Funcionales** | `id`, `const`, `flip`, `curry` | ❌ No en Prelude | Combinadores estándar en `Predef` | **Prioridad 2** |

---

## 🎯 Plan de Acción y Tareas Detalladas

### Fase 1: Corrección Inmediata de Ejemplos y Funciones de Prelude (Prioridad 1)

#### 🧪 1.1. Modificación de Ejemplos en `Sample/`
- [x] **`Sample/collections.ss`**:
  - Reemplazado `l.headValue` por `head(l)`.
  - Reemplazado `l.tailList` por `tail(l)`.
  - Reemplazado `l.size` por `length(l)` o `size(l)`.
  - Reemplazadas todas las llamadas de métodos `.take(3)`, `.drop(2)`, `.contains(3)`, `.map(...)`, `.filter(...)`, `.foldLeft(...)`, `.concat(...)` por llamadas a funciones libres.
  - Reemplazadas llamadas en `Set`: `size(s)`, `contains(..., s)`, `insert(..., s)`, `delete(..., s)`, `union(sA, sB)`, `toList(s)`.
  - Reemplazadas llamadas en `Map`: `size(m)`, `contains(k, m)`, `get(..., m)`, `insert(..., m)`, `delete(..., m)`, `keys(m)`, `values(m)`, `toList(m)`.
- [x] **`Sample/monads.ss`**:
  - Reemplazado `isEmpty(opt1)`, `isDefined(opt1)`.
  - Reemplazado `getOrElse(99, opt2)`.
  - Reemplazado `flatMap(f, opt1)`, `flatten(nestedOpt)`, `flatMap(f, l)`, `flatten(nestedList)`, `flatMap(f, s)`, `flatten(nestedSet)`, `flatMap(f, m)`.
  - Empleo de funciones libres `fst(t)` y `snd(t)` en tuplas.
- [x] **`Sample/tuples.ss`**:
  - Sustituidos `persona._1`, `persona._2`, `persona._3` por deconstrucción funcional pura con pattern matching y funciones libres selectoras `fst(par)`, `snd(par)`.
- [x] **`Sample/variance.ss`**:
  - Rediseñado hacia polimorfismo paramétrico puro y ADTs (Sum & Product Types), eliminando mutabilidad (`MutableBox`) y conceptos de herencia OO.
- [x] **`Sample/hello.ss`**:
  - Adecuado para demostrar evaluación perezosa pura por nombre (`call-by-name`) sin efectos secundarios ocultos, y funciones libres `head(l)` y `tail(l)`.

#### 📦 1.2. `SSharp.Runtime` (Funciones Libres del Prelude)
- [x] **Funciones de Lista Seguras**:
  - [x] Implementado `headOption<T>(SSharpList<T> xs): SSharpOption<T>` en `Predef.cs`.
  - [x] Implementado `tailOption<T>(SSharpList<T> xs): SSharpOption<SSharpList<T>>` en `Predef.cs`.
  - [x] Implementado `uncons<T>(SSharpList<T> xs): SSharpOption<SSharpTuple2<T, SSharpList<T>>>` en `Predef.cs`.
  - [x] Implementado `init<T>(SSharpList<T> xs): SSharpList<T>` y `last<T>(SSharpList<T> xs): T`.
  - [x] Implementado `forall`, `exists`, `append`, `prepend`, `flatten`.
- [x] **Funciones Libres para `Option`**:
  - [x] `isSome<T>(SSharpOption<T> opt): bool`
  - [x] `isNone<T>(SSharpOption<T> opt): bool`
  - [x] `getOrElse<T>(T defaultVal, SSharpOption<T> opt): T`
  - [x] `flatten<T>(SSharpOption<SSharpOption<T>> opt): SSharpOption<T>`
- [x] **Funciones Libres para `Set`**:
  - [x] `size<T>(SSharpSet<T> s): int`, `isEmpty<T>(SSharpSet<T> s): bool`
  - [x] `contains<T>(T elem, SSharpSet<T> s): bool`
  - [x] `insert<T>(T elem, SSharpSet<T> s): SSharpSet<T>`, `incl`
  - [x] `delete<T>(T elem, SSharpSet<T> s): SSharpSet<T>`, `excl`
  - [x] `union<T>(SSharpSet<T> a, SSharpSet<T> b): SSharpSet<T>`
  - [x] `intersect<T>(SSharpSet<T> a, SSharpSet<T> b): SSharpSet<T>`
  - [x] `diff<T>(SSharpSet<T> a, SSharpSet<T> b): SSharpSet<T>`
  - [x] `toList<T>(SSharpSet<T> s): SSharpList<T>`
  - [x] `map`, `filter`, `flatMap`, `flatten`
- [x] **Funciones Libres para `Map`**:
  - [x] `size<K, V>(SSharpMap<K, V> m): int`, `isEmpty<K, V>(SSharpMap<K, V> m): bool`
  - [x] `contains<K, V>(K k, SSharpMap<K, V> m): bool`
  - [x] `get<K, V>(K k, SSharpMap<K, V> m): SSharpOption<V>`
  - [x] `insert<K, V>(K k, V v, SSharpMap<K, V> m): SSharpMap<K, V>`, `updated`
  - [x] `delete<K, V>(K k, SSharpMap<K, V> m): SSharpMap<K, V>`, `removed`
  - [x] `keys<K, V>(SSharpMap<K, V> m): SSharpSet<K>`
  - [x] `values<K, V>(SSharpMap<K, V> m): SSharpList<V>`
  - [x] `toList<K, V>(SSharpMap<K, V> m): SSharpList<SSharpTuple2<K, V>>`
  - [x] `flatMap`
- [x] **Funciones para Tuplas y Combinadores**:
  - [x] `fst<A, B>(SSharpTuple2<A, B> t): A` y `snd<A, B>(SSharpTuple2<A, B> t): B`
  - [x] `id<T>(T x): T`
  - [x] `constVal<A, B>(A a, B b): A`
- [x] **Limpieza de Métodos Imperativos**:
  - [x] Eliminar `Foreach` en `List.cs`, `Set.cs` y `Map.cs`.

#### 🧠 1.3. `SSharp.Compiler` (TypeChecker & CodeGenerator)
- [x] Registrar en el entorno inicial `_env` de `TypeChecker.cs` todas las funciones libres nuevas de `Option`, `Set`, `Map`, `Tuple` y seguras de lista (`headOption`, `tailOption`, `uncons`).
- [x] Soporte de inferencia de tipos para llamadas a `get`, `insert`, `delete`, `keys`, `values`, `toList`, `flatten`, `fst`, `snd`, `getOrElse`.

---

### Fase 2: Pureza de Datos y Tipos Algebraicos (Prioridad 2)

- ✅ **Sintaxis Nativa de ADTs (`data` / Sum Types)** — COMPLETADO:
  - Palabra clave `data` y separador `|` añadidos al Lexer y Parser.
  - Soporte de tipos suma puramente funcionales sin necesidad de `sealed trait` ni `extends`:
    ```ssharp
    data Shape = Circle(radius: Double) | Rectangle(width: Double, height: Double) | Dot
    ```
  - Soporte de constructores con argumentos y constructores sin parámetros (constantes / case objects).
- ✅ **Funciones Selectoras de Campos** — COMPLETADO:
  - Generación automática de funciones selectoras puras para campos nombrados: `radius(shape)`, `width(shape)`, `height(shape)`.
  - Spec case `058_native_adts` validado con pattern matching y funciones selectoras libres.
- ✅ **Type Aliases (`type`)** — COMPLETADO:
  - Palabra clave `type` añadida al Lexer y Parser.
  - Nodo AST `TypeAliasDecl` con soporte de type params (`type Predicate[A] = A => Boolean`).
  - `TypeChecker.ResolveType` expande aliases de forma transparente con sustitución de variables.
  - `CodeGenerator.MapTypeNode` expande aliases antes de emitir C# (type erasure — no genera código).
  - Spec case `056_type_aliases` validado:
    ```ssharp
    type Predicate[A] = A => Boolean
    type Transform[A, B] = A => B
    type Point = (Double, Double)
    type Name = String
    ```

---

### Fase 3: Transparencia Referencial, Mónada IO y Type Classes (Prioridad 3)

- [ ] **Mónada `IO`**:
  - Diseñar el tipo `IO[A]` para aislar efectos colaterales de consola y E/S.
  - Convertir `println` y `readLine` en acciones puras: `println(msg): IO[Unit]`, `readLine(): IO[String]`.
  - Integrar `IO` en for-comprehensions (`for { line <- readLine(); _ <- println("Hola " + line) } yield ()`).
- [ ] **Sistema de Type Classes**:
  - Modelar polimorfismo ad-hoc sin herencia:
    - `Eq[A]`: comparación de igualdad pura.
    - `Show[A]`: representación textual.
    - `Functor[F[_]]`: mapeo generalizado `fmap`.
    - `Applicative[F[_]]` y `Monad[F[_]]`.
- [ ] **Streams / LazyList Infinitos**:
  - Soporte de estructuras de datos perezosas infinitas al estilo Haskell (`[1..]`, `repeat`, `cycle`).

---

## 🧪 Pruebas de Especificación Asociadas (`SSharp.Spec`)

- [x] `048_predef_free_functions`: `map`, `filter`, `length`, `foldLeft` como funciones libres.
- [x] `049_pipe_operator`: Cadenas de transformación funcional con `|>`.
- [x] `050_tuple_literals_and_matching`: Tuplas de aridad 2 a 8 con pattern matching.
- [x] `051_match_guards_and_as_patterns`: Pattern guards condicionales y as-patterns.
- [x] `052_for_comprehensions`: Comprensiones monádicas desugareadas a funciones libres.
- [x] `053_head_and_tail_pure_functions`: Validación de `head`, `tail`, `last`, `init`, `headOption`, `tailOption`, `uncons` como funciones libres puras. `HeadValue`/`TailList` son `internal`.
- [x] `054_set_and_map_free_functions`: Validación de operaciones libres sobre `Set` y `Map` (`size`, `isEmpty`, `contains`, `insert`, `delete`, `union`, `intersect`, `diff`, `keys`, `values`, `get`).
- [x] `055_option_free_functions`: Validación de `isSome`, `isNone`, `isDefined`, `isEmpty`, `getOrElse`, `map`, `filter`, `flatten` como funciones libres.
- [x] `056_type_aliases`: Aliases simples (`type Name = String`), paramétricos (`type Predicate[A] = A => Boolean`, `type Transform[A, B] = A => B`) y de tupla (`type Point = (Double, Double)`). Expansión transparente en TypeChecker y CodeGenerator.
- [x] `057_either_monad_free_functions`: Validación de `Either` con funciones puras `Left`, `Right`, `isLeft`, `isRight`, `getOrElse`, `mapEither`, `mapLeft`, y deconstrucción con pattern matching.
- [x] `058_native_adts`: Sintaxis nativa de tipos de datos algebraicos (`data Shape = Circle(...) | Rectangle(...) | Dot`), deconstrucción en pattern matching y funciones selectoras puras generadas (`radius`, `width`, `height`).
