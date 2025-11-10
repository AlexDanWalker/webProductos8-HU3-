Recomendaciones para levantar el proyecto:

Clonar el repo
- git clone https://github.com/AlexDanWalker/webProductos8-HU3-.git
- cd webProductos8-HU3-

 Restaurar paquetes NuGet:

dotnet restore

Esto descarga todas las dependencias necesarias.

---

 Crear la base de datos (si no existe)

Asegúrate de que tu cadena de conexión en appsettings.json esté correcta. Luego:

dotnet ef database update


Esto aplicará las migraciones que existan.

---

Si quieres crear la base desde cero con migraciones manuales, primero debes crear una migración:

dotnet ef migrations add InitialCreate
dotnet ef database update

Nota: Si el repo ignora las migraciones, solo haz esto localmente.
---

 Ejecutar el proyecto
dotnet run --project src/webProductos.Api


Dockerizacion:

docker compose down

docker compose up --build
