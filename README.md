# 💧 AguaExpress

Sistema de pedidos y reparto de sifones de agua. Android (.NET MAUI) + API (ASP.NET Core) + PostgreSQL en Neon.

```
AguaExpress.slnx
├── AguaExpress.Api/          API REST (.NET 10)
│   ├── Dominio/              Entidades y enums
│   ├── Datos/                DbContext (EF Core + Npgsql) y semilla
│   ├── Dtos/                 Contratos de entrada/salida
│   ├── Servicios/            JWT y notificaciones FCM
│   └── Controladores/        Auth, Usuarios, Sifones/Inventario, Pedidos, Panel
├── AguaExpress.App/          App Android (.NET MAUI, MVVM)
│   ├── Modelos/ Servicios/ ViewModels/ Vistas/ Platforms/Android/
└── BaseDeDatos/esquema.sql   Esquema en español (referencia)
```

## 1. Base de datos (Neon)
1. En https://neon.tech crea un proyecto y copia la cadena de conexión.
2. En `AguaExpress.Api/appsettings.json` pon los datos en `ConnectionStrings:Neon`
   (formato Npgsql: `Host=...;Database=neondb;Username=...;Password=...;SSL Mode=Require`).
3. Cambia `Jwt:Clave` por una clave larga y secreta.
4. Al arrancar, la API crea las tablas (`usuarios`, `tipos_sifon`, `inventario`, `pedidos`,
   `detalles_pedido`), los sifones de 1/3/5/10/15 L y el administrador inicial:
   **admin@aguaexpress.com / Admin123!** (cámbialo en `AdministradorInicial`).

## 2. Notificaciones (Firebase Cloud Messaging)
1. Crea un proyecto en https://console.firebase.google.com y agrega una app Android con el
   id `com.aguaexpress.app`.
2. Descarga `google-services.json` y colócalo en `AguaExpress.App/` (el .csproj ya lo incluye).
3. En Firebase → Configuración → Cuentas de servicio → "Generar nueva clave privada".
   Pega TODO el contenido del JSON en `Firebase:CredencialesJson` de la API
   (en producción, usa una variable de entorno `Firebase__CredencialesJson`).
   Sin esto la app funciona igual, solo que sin push.

## 3. Ejecutar
- API: abre la solución en Visual Studio 2026, proyecto de inicio `AguaExpress.Api` (escucha en `http://0.0.0.0:5000`).
- App: en `AguaExpress.App/Configuracion/AjustesApi.cs` deja `http://10.0.2.2:5000/` para el emulador.
  Con teléfono real usa la IP de tu PC (`http://192.168.x.x:5000/`) o la URL pública de tu API.
- Ejecuta `AguaExpress.App` en un emulador/dispositivo Android (necesita la carga de trabajo
  "Desarrollo de interfaz de usuario multiplataforma .NET (MAUI)").

## 4. Flujo de roles
- **Cliente**: se registra desde la app, arma su pedido (litros 1–15 L), marca su ubicación con GPS y confirma. Puede cancelar mientras esté Pendiente/Asignado.
- **Administrador**: Panel (ingresos del día, sifones entregados, pendientes, stock bajo), Pedidos (asignar/reasignar vendedor, cancelar), Inventario (stock, precios, tipos), Trabajadores (alta de vendedores/admins, activar/desactivar).
- **Vendedor**: ve sus entregas. **🧭 Ir** abre Google Maps con la ruta (Deep Link `google.navigation:`, con respaldo a `geo:` y web). **🔔 Notificar** envía la push "tu pedido va en camino". **✅ Entregado** descuenta inventario. **⚠️ No entregado** registra el motivo y avisa al cliente y al administrador.

## 5. Publicar la API
Cualquier hosting .NET sirve (Azure App Service, Render, Railway, Fly.io). Usa HTTPS y quita
`usesCleartextTraffic` del `AndroidManifest.xml` cuando tengas la URL pública.
