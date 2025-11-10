# webProductos

Aplicación de gestión de usuarios y productos con autenticación JWT y autorización por roles. Proyecto estructurado en 4 capas: **Api**, **Application**, **Domain**, **Infrastructure**.

---

## Estructura de la solución

- **webProductos.Api**: Controladores, configuración de JWT y endpoints.
- **webProductos.Application**: Lógica de negocio, servicios, interfaces, DTOs, mapeo con AutoMapper.
- **webProductos.Domain**: Entidades y relaciones principales (User, Role, Product).
- **webProductos.Infrastructure**: Implementación de repositorios, EF Core, acceso a base de datos y seeders.
- **webProductos.Application.Tests**: Pruebas unitarias con xUnit y Moq.

---

## Tecnologías utilizadas

- .NET 8
- C#
- Entity Framework Core + Pomelo MySQL
- AutoMapper
- JWT para autenticación
- xUnit y Moq para pruebas
- Docker & Docker Compose

---

## Flujo de desarrollo

### 1. Inicialización
- Crear la solución y proyectos por capas.
- Inicializar Git, configurar `.gitignore` y subir a GitHub.

### 2. Modelado de entidades y EF Core
- Entidades: `User`, `Role`, `Product`.
- Configuración de `AppDbContext` y migraciones.
- Seed de datos iniciales.
- Relación 1:N `Role-User` y 1:N `User-Product`.

### 3. Capa Application
- Creación de DTOs:
  - `UserDto`, `RegisterUserDto`, `LoginUserDto`, `AuthResponseDto`
  - `ProductDto`
- Interfaces de servicios y repositorios.
- Implementación de servicios y repositorios.
- Lógica de login, registro y hash de contraseñas con BCrypt.
- Autenticación JWT con roles incluidos en el token.
- Pruebas unitarias básicas de creación de usuario/producto y login.

### 4. Capa API
- Controladores:
  - `UsersController`: CRUD de usuarios (protegido por roles)
  - `ProductsController`: CRUD de productos (protegido por roles)
  - `AuthController`: registro y login con JWT
- Configuración de JWT y autorización por roles en endpoints.
- Ejemplo de login POST body:
```json
{
  "email": "admin@example.com",
  "password": "admin123"
}

Endpoints
Usuarios
Método	Endpoint	Rol requerido	Descripción
POST	/api/auth/register	-	Registrar usuario
POST	/api/auth/login	-	Login y generar JWT
GET	/api/users	Admin	Listar usuarios
GET	/api/users/{id}	Admin,Vendedor,User	Obtener usuario por ID
PUT	/api/users/{id}	Admin	Actualizar usuario
DELETE	/api/users/{id}	Admin	Eliminar usuario
Productos
Método	Endpoint	Rol requerido	Descripción
POST	/api/products	Admin,Vendedor	Crear producto
GET	/api/products	Admin,Vendedor,User	Listar productos
GET	/api/products/{id}	Admin,Vendedor,User	Obtener producto por ID
PUT	/api/products/{id}	Admin,Vendedor	Actualizar producto
DELETE	/api/products/{id}	Admin,Vendedor	Eliminar producto
Autenticación

JWT incluido en encabezado Authorization: Bearer <token>

Roles: Admin, Seller, User

El token expira pasada 1 hora.

Pruebas

Servicios y controladores testeados con xUnit y Moq

Ejemplos:

Crear usuario

Login y validación de JWT

Creación de producto

Dockerización

Contiene Dockerfile y docker-compose.yml para levantar:

API

MySQL

Adminer

Variables de entorno configurables para conexión a DB y JWT.

Despliegue

Subir rama feature/api-controllers-jwt a GitHub.

Crear Pull Request a develop o main.

Configurar servidor (Render/Railway) y variables de entorno:

Jwt:Key

ConnectionStrings:DefaultConnection

Aplicar migraciones en arranque.

Levantar contenedores con Docker Compose (opcional).

Documentación API

Se recomienda usar Postman:

Crear colección con todos los endpoints.

Guardar JWT en variable de entorno para pruebas de endpoints protegidos.

Autor

Daniel Ariza
