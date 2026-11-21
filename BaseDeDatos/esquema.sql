-- Esquema PostgreSQL (Neon) de AguaExpress. Opcional: la API lo crea sola con EnsureCreated.
CREATE TABLE usuarios (
  id SERIAL PRIMARY KEY,
  nombre_completo TEXT NOT NULL,
  correo VARCHAR(150) NOT NULL UNIQUE,
  telefono TEXT NOT NULL,
  contrasena_hash TEXT NOT NULL,
  rol VARCHAR(20) NOT NULL,            -- Administrador | Vendedor | Cliente
  activo BOOLEAN NOT NULL DEFAULT TRUE,
  token_fcm TEXT,
  fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE tipos_sifon (
  id SERIAL PRIMARY KEY,
  nombre TEXT NOT NULL,
  capacidad_litros INT NOT NULL,
  precio NUMERIC(10,2) NOT NULL,
  activo BOOLEAN NOT NULL DEFAULT TRUE
);
CREATE TABLE inventario (
  id SERIAL PRIMARY KEY,
  tipo_sifon_id INT NOT NULL UNIQUE REFERENCES tipos_sifon(id) ON DELETE CASCADE,
  stock_disponible INT NOT NULL DEFAULT 0,
  stock_minimo INT NOT NULL DEFAULT 5,
  fecha_actualizacion TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE pedidos (
  id SERIAL PRIMARY KEY,
  cliente_id INT NOT NULL REFERENCES usuarios(id),
  repartidor_id INT REFERENCES usuarios(id),
  direccion TEXT NOT NULL,
  referencia TEXT,
  latitud DOUBLE PRECISION NOT NULL,
  longitud DOUBLE PRECISION NOT NULL,
  estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente', -- Pendiente|Asignado|EnCamino|Entregado|NoEntregado|Cancelado
  total NUMERIC(10,2) NOT NULL,
  notas TEXT,
  motivo_no_entrega TEXT,
  fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT now(),
  fecha_asignacion TIMESTAMPTZ,
  fecha_entrega TIMESTAMPTZ
);
CREATE INDEX ix_pedidos_estado_fecha ON pedidos (estado, fecha_creacion);
CREATE TABLE detalles_pedido (
  id SERIAL PRIMARY KEY,
  pedido_id INT NOT NULL REFERENCES pedidos(id) ON DELETE CASCADE,
  tipo_sifon_id INT NOT NULL REFERENCES tipos_sifon(id),
  cantidad INT NOT NULL,
  precio_unitario NUMERIC(10,2) NOT NULL
);
