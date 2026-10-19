// =====================================================================
// MESA DE PARTES DIGITAL - COLEGIO
// Curso: Fundamentos de Programación (CIIN1205P)
// Prototipo en C# (consola) para registrar, validar, buscar, ordenar
// y guardar expedientes en un archivo de texto.
// =====================================================================
using System;
using System.IO;
using System.Text;

namespace MesaDePartesDigital
{
    class Program
    {
        // ---------------------------------------------------------
        // DATOS GLOBALES
        // Arreglos paralelos: la posición i de cada arreglo
        // pertenece al MISMO expediente.
        // ---------------------------------------------------------
        const int MAXIMO = 100;                 // capacidad del prototipo
        const string ARCHIVO = "expedientes.txt";

        static string[] codigos = new string[MAXIMO];
        static string[] dnis = new string[MAXIMO];
        static string[] nombres = new string[MAXIMO];
        static string[] tramites = new string[MAXIMO];
        static string[] asuntos = new string[MAXIMO];
        static string[] fechas = new string[MAXIMO];
        static string[] estados = new string[MAXIMO];
        static int cantidad = 0;                // expedientes registrados

        // Listas fijas de opciones (arreglos unidimensionales)
        static string[] TIPOS = {
            "Constancia de estudios",
            "Justificación de inasistencia",
            "Traslado de matrícula",
            "Certificado de notas"
        };
        static string[] ESTADOS = { "Pendiente", "En revisión", "Atendido" };

        // =============================================================
        // PROGRAMA PRINCIPAL
        // =============================================================
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;   // tildes y ñ
            CargarExpedientes();

            int opcion;
            do
            {
                MostrarMenu();
                opcion = LeerEntero("Elija una opción: ");

                switch (opcion)
                {
                    case 1: RegistrarExpediente(); break;
                    case 2: BuscarExpediente(); break;
                    case 3: OrdenarExpedientes(); break;
                    case 4: CambiarEstado(); break;
                    case 5: Console.WriteLine("Cerrando sistema..."); break;
                    default: Console.WriteLine("Opción inválida."); break;
                }
            } while (opcion != 5);
        }

        static void MostrarMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== MESA DE PARTES DIGITAL - COLEGIO =====");
            Console.WriteLine("1. Registrar expediente");
            Console.WriteLine("2. Buscar expediente por código");
            Console.WriteLine("3. Ordenar y listar expedientes");
            Console.WriteLine("4. Cambiar estado de un expediente");
            Console.WriteLine("5. Salir");
        }

        // =============================================================
        // MÓDULO 1: REGISTRAR
        // =============================================================
        static void RegistrarExpediente()
        {
            Console.WriteLine("\n--- REGISTRO DE EXPEDIENTE ---");

            if (cantidad >= MAXIMO)
            {
                Console.WriteLine("Capacidad llena. No se pueden registrar más expedientes.");
                return;
            }

            string dni = LeerDni();
            string nombre = LeerNombre();
            string tramite = ElegirTramite();
            string asunto = LeerAsunto();

            codigos[cantidad] = GenerarCodigo();
            dnis[cantidad] = dni;
            nombres[cantidad] = nombre;
            tramites[cantidad] = tramite;
            asuntos[cantidad] = asunto;
            fechas[cantidad] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            estados[cantidad] = ESTADOS[0];            // inicia "Pendiente"
            cantidad++;

            GuardarExpedientes();
            Console.WriteLine("Expediente registrado. Su código es: " + codigos[cantidad - 1]);
        }

        // Código con ceros a la izquierda: EXP-0001, EXP-0002, ...
        static string GenerarCodigo()
        {
            return "EXP-" + (cantidad + 1).ToString("0000");
        }

        // ---------------- Validaciones ----------------

        static bool DniValido(string dni)
        {
            if (dni.Length != 8) return false;
            for (int i = 0; i < dni.Length; i++)
            {
                if (!char.IsDigit(dni[i])) return false;
            }
            return true;
        }

        static bool NombreValido(string nombre)
        {
            if (nombre.Length < 3) return false;
            for (int i = 0; i < nombre.Length; i++)
            {
                if (!char.IsLetter(nombre[i]) && nombre[i] != ' ') return false;
            }
            return true;
        }

        static string LeerDni()
        {
            string dni;
            do
            {
                dni = LeerTexto("DNI del solicitante (8 dígitos): ");
                if (!DniValido(dni))
                    Console.WriteLine("DNI inválido. Debe tener 8 dígitos numéricos.");
            } while (!DniValido(dni));
            return dni;
        }

        static string LeerNombre()
        {
            string nombre;
            do
            {
                nombre = LeerTexto("Apellidos y nombres: ").ToUpper();
                if (!NombreValido(nombre))
                    Console.WriteLine("Nombre inválido. Use solo letras y espacios (mínimo 3).");
            } while (!NombreValido(nombre));
            return nombre;
        }

        static string ElegirTramite()
        {
            Console.WriteLine("Tipo de trámite:");
            for (int i = 0; i < TIPOS.Length; i++)
            {
                Console.WriteLine("  " + (i + 1) + ". " + TIPOS[i]);
            }

            int opcion;
            do
            {
                opcion = LeerEntero("Elija el tipo (1-" + TIPOS.Length + "): ");
                if (opcion < 1 || opcion > TIPOS.Length)
                    Console.WriteLine("Opción fuera de rango.");
            } while (opcion < 1 || opcion > TIPOS.Length);

            return TIPOS[opcion - 1];
        }

        static string LeerAsunto()
        {
            string asunto;
            do
            {
                asunto = LeerTexto("Asunto (breve): ");
                if (asunto.Length == 0)
                    Console.WriteLine("El asunto no puede estar vacío.");
            } while (asunto.Length == 0);

            asunto = asunto.Replace(";", ",");          // ';' separa campos en el archivo
            if (asunto.Length > 60)
                asunto = asunto.Substring(0, 60);       // máximo 60 caracteres
            return asunto;
        }

        // =============================================================
        // MÓDULO 2: BUSCAR (búsqueda secuencial por código)
        // =============================================================
        static int BuscarPosicion(string codigo)
        {
            for (int i = 0; i < cantidad; i++)
            {
                if (codigos[i] == codigo) return i;
            }
            return -1;                                  // no encontrado
        }

        static void BuscarExpediente()
        {
            Console.WriteLine("\n--- BÚSQUEDA DE EXPEDIENTE ---");
            string codigo = LeerTexto("Ingrese el código (ej. EXP-0001): ").ToUpper();

            int pos = BuscarPosicion(codigo);
            if (pos == -1)
                Console.WriteLine("No se encontró ningún expediente con el código " + codigo + ".");
            else
                MostrarExpediente(pos);
        }

        static void MostrarExpediente(int i)
        {
            Console.WriteLine("Código : " + codigos[i]);
            Console.WriteLine("DNI    : " + dnis[i]);
            Console.WriteLine("Nombre : " + nombres[i]);
            Console.WriteLine("Trámite: " + tramites[i]);
            Console.WriteLine("Asunto : " + asuntos[i]);
            Console.WriteLine("Fecha  : " + fechas[i]);
            Console.WriteLine("Estado : " + estados[i]);
        }

        // =============================================================
        // MÓDULO 3: ORDENAR (método burbuja) Y LISTAR
        // =============================================================
        static void OrdenarExpedientes()
        {
            Console.WriteLine("\n--- ORDENAR EXPEDIENTES ---");
            if (cantidad == 0)
            {
                Console.WriteLine("No hay expedientes registrados.");
                return;
            }

            Console.WriteLine("1. Por código");
            Console.WriteLine("2. Por apellidos y nombres (A-Z)");
            int criterio = LeerEntero("Elija el criterio: ");
            if (criterio != 1 && criterio != 2)
            {
                Console.WriteLine("Criterio inválido.");
                return;
            }

            for (int i = 0; i < cantidad - 1; i++)
            {
                for (int j = 0; j < cantidad - 1 - i; j++)
                {
                    string a, b;
                    if (criterio == 1)
                    {
                        a = codigos[j];
                        b = codigos[j + 1];
                    }
                    else
                    {
                        a = nombres[j];
                        b = nombres[j + 1];
                    }

                    if (string.Compare(a, b) > 0)
                        Intercambiar(j, j + 1);
                }
            }

            ListarExpedientes();
        }

        // Intercambia TODOS los datos de dos posiciones (arreglos paralelos)
        static void Intercambiar(int x, int y)
        {
            string aux;
            aux = codigos[x]; codigos[x] = codigos[y]; codigos[y] = aux;
            aux = dnis[x]; dnis[x] = dnis[y]; dnis[y] = aux;
            aux = nombres[x]; nombres[x] = nombres[y]; nombres[y] = aux;
            aux = tramites[x]; tramites[x] = tramites[y]; tramites[y] = aux;
            aux = asuntos[x]; asuntos[x] = asuntos[y]; asuntos[y] = aux;
            aux = fechas[x]; fechas[x] = fechas[y]; fechas[y] = aux;
            aux = estados[x]; estados[x] = estados[y]; estados[y] = aux;
        }

        static void ListarExpedientes()
        {
            Console.WriteLine("\nCÓDIGO   | DNI      | APELLIDOS Y NOMBRES | TRÁMITE | ESTADO");
            for (int i = 0; i < cantidad; i++)
            {
                Console.WriteLine(codigos[i] + " | " + dnis[i] + " | " + nombres[i]
                                  + " | " + tramites[i] + " | " + estados[i]);
            }
            Console.WriteLine("Total: " + cantidad + " expediente(s).");
        }

        // =============================================================
        // MÓDULO 4: CAMBIAR ESTADO
        // =============================================================
        static void CambiarEstado()
        {
            Console.WriteLine("\n--- CAMBIAR ESTADO ---");
            string codigo = LeerTexto("Código del expediente: ").ToUpper();

            int pos = BuscarPosicion(codigo);
            if (pos == -1)
            {
                Console.WriteLine("No se encontró ningún expediente con el código " + codigo + ".");
                return;
            }

            Console.WriteLine("Estado actual: " + estados[pos]);
            for (int i = 0; i < ESTADOS.Length; i++)
            {
                Console.WriteLine("  " + (i + 1) + ". " + ESTADOS[i]);
            }

            int opcion = LeerEntero("Nuevo estado: ");
            if (opcion < 1 || opcion > ESTADOS.Length)
            {
                Console.WriteLine("Opción inválida. No se hicieron cambios.");
                return;
            }

            estados[pos] = ESTADOS[opcion - 1];
            GuardarExpedientes();
            Console.WriteLine("Estado actualizado correctamente.");
        }

        // =============================================================
        // MÓDULO 5: PERSISTENCIA CON ARCHIVOS DE TEXTO
        // Cada línea = un expediente, campos separados por ';'
        // =============================================================
        static void GuardarExpedientes()
        {
            StreamWriter escritor = new StreamWriter(ARCHIVO);   // abrir
            for (int i = 0; i < cantidad; i++)
            {
                string linea = codigos[i] + ";" + dnis[i] + ";" + nombres[i] + ";"
                             + tramites[i] + ";" + asuntos[i] + ";" + fechas[i] + ";"
                             + estados[i];
                escritor.WriteLine(linea);                       // escribir
            }
            escritor.Close();                                    // cerrar
        }

        static void CargarExpedientes()
        {
            if (!File.Exists(ARCHIVO))
            {
                Console.WriteLine("No hay archivo previo. Se inicia con el sistema vacío.");
                return;
            }

            StreamReader lector = new StreamReader(ARCHIVO);     // abrir
            string linea = lector.ReadLine();                    // leer
            while (linea != null && cantidad < MAXIMO)
            {
                string[] campos = linea.Split(';');
                if (campos.Length == 7)
                {
                    codigos[cantidad] = campos[0];
                    dnis[cantidad] = campos[1];
                    nombres[cantidad] = campos[2];
                    tramites[cantidad] = campos[3];
                    asuntos[cantidad] = campos[4];
                    fechas[cantidad] = campos[5];
                    estados[cantidad] = campos[6];
                    cantidad++;
                }
                linea = lector.ReadLine();
            }
            lector.Close();                                      // cerrar

            Console.WriteLine(cantidad + " expediente(s) cargado(s) desde el archivo.");
        }

        // =============================================================
        // FUNCIONES AUXILIARES DE ENTRADA
        // =============================================================
        static string LeerTexto(string mensaje)
        {
            Console.Write(mensaje);
            string texto = Console.ReadLine();
            if (texto == null) texto = "";
            return texto.Trim();
        }

        // 'out' = parámetro por referencia (devuelve el número leído)
        static int LeerEntero(string mensaje)
        {
            int numero;
            Console.Write(mensaje);
            while (!int.TryParse(Console.ReadLine(), out numero))
            {
                Console.Write("Entrada inválida. " + mensaje);
            }
            return numero;
        }
    }
}
