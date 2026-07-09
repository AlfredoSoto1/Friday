
DROP SCHEMA IF EXISTS public CASCADE;
DROP SCHEMA IF EXISTS discord CASCADE;
DROP SCHEMA IF EXISTS inelicom CASCADE;


CREATE SCHEMA public;
CREATE SCHEMA discord;
CREATE SCHEMA inelicom;

-- ============================================================================
-- Discord Schema
-- ============================================================================

CREATE TABLE discord.servers (
  server_id          SERIAL       PRIMARY KEY,
  name               VARCHAR(255) NOT NULL UNIQUE,
  guild_id           VARCHAR(255) NOT NULL UNIQUE,
  enabled            BOOLEAN      NOT NULL DEFAULT FALSE,
  department_profile VARCHAR(16)  CHECK (department_profile IN ('INEL_ICOM', 'INSO_CIIC')),
  primary_color      VARCHAR(7)   DEFAULT '#5865F2',
  thumbnail_url      VARCHAR(255),
  footer_text        VARCHAR(255) DEFAULT 'Friday',
  verif_title        VARCHAR(255) DEFAULT 'Verify Yourself',
  verif_desc         TEXT         DEFAULT 'Click the button below to verify yourself and gain access to the server.',
  verif_button_id    VARCHAR(255) DEFAULT 'Verify',
  verif_channel_id   VARCHAR(255),
  verif_role_id      VARCHAR(255),
  verif_banner_url   VARCHAR(255),
  welcome_title      VARCHAR(255) DEFAULT 'Welcome to the Server!',
  welcome_desc       TEXT         DEFAULT 'We are glad to have you here! Please read the rules and enjoy your stay.',
  welcome_channel_id VARCHAR(255),
  welcome_banner_url VARCHAR(255),
  created_at         TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- Semana de Orientación (SO) Schema
-- 
-- This schema contains tables related to users, roles, permissions, and teams.
-- These users, roles and teams are used to manage access control and organization 
-- within discord and the web application.
-- ============================================================================
CREATE TABLE discord.users (
  user_id    SERIAL       PRIMARY KEY,
  email            VARCHAR(255) NOT NULL UNIQUE,
  first_name       VARCHAR(255),
  first_last_name  VARCHAR(255),
  second_last_name VARCHAR(255),
  initial          VARCHAR(10),
  program          VARCHAR(4),
  created_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE discord.servers_users (
  su_id           SERIAL      PRIMARY KEY,
  server_id       INT         NOT NULL,
  user_id         INT         NOT NULL,
  discord_user_id VARCHAR(32) NOT NULL DEFAULT '-',
  verified        BOOLEAN     NOT NULL DEFAULT FALSE,
  funfact         TEXT,
  xp              INT         NOT NULL DEFAULT 0,
  level           INT         NOT NULL DEFAULT 1,
  updated_at      TIMESTAMP   DEFAULT CURRENT_TIMESTAMP,
  created_at      TIMESTAMP   DEFAULT CURRENT_TIMESTAMP,

  UNIQUE (server_id, user_id),
  FOREIGN KEY (user_id) REFERENCES discord.users(user_id) ON DELETE CASCADE,
  FOREIGN KEY (server_id) REFERENCES discord.servers(server_id) ON DELETE CASCADE
);

CREATE TABLE discord.permissions (
  permission_id SERIAL       PRIMARY KEY,
  name          VARCHAR(255) NOT NULL UNIQUE,
  created_at    TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE discord.roles (
  role_id         SERIAL       PRIMARY KEY,
  server_id       INT          NOT NULL,
  discord_role_id VARCHAR(32),
  name            VARCHAR(255) NOT NULL,
  color           INT,
  position        INT,
  managed         BOOLEAN      NOT NULL DEFAULT FALSE,
  mentionable     BOOLEAN      NOT NULL DEFAULT FALSE,
  hoisted         BOOLEAN      NOT NULL DEFAULT FALSE,
  created_at      TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,
  updated_at      TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,

  UNIQUE (server_id, name),
  UNIQUE (server_id, discord_role_id),
  FOREIGN KEY (server_id) REFERENCES discord.servers(server_id) ON DELETE CASCADE
);

CREATE TABLE discord.role_permissions (
  role_id       INT NOT NULL,
  permission_id INT NOT NULL,
  created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

  PRIMARY KEY (role_id, permission_id),
  FOREIGN KEY (role_id) REFERENCES discord.roles(role_id) ON DELETE CASCADE,
  FOREIGN KEY (permission_id) REFERENCES discord.permissions(permission_id) ON DELETE CASCADE
);

CREATE TABLE discord.user_roles (
  su_id       INT NOT NULL,
  role_id     INT NOT NULL,
  created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

  PRIMARY KEY (su_id, role_id),
  FOREIGN KEY (su_id) REFERENCES discord.servers_users(su_id) ON DELETE CASCADE,
  FOREIGN KEY (role_id) REFERENCES discord.roles(role_id) ON DELETE CASCADE
);

CREATE TABLE discord.teams (
  team_id    SERIAL       PRIMARY KEY,
  server_id  INT          NOT NULL,
  role_id    INT,
  position   INT          NOT NULL,
  name       VARCHAR(255) NOT NULL,
  created_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,

  UNIQUE (server_id, position),
  FOREIGN KEY (server_id) REFERENCES discord.servers(server_id) ON DELETE CASCADE,
  FOREIGN KEY (role_id) REFERENCES discord.roles(role_id) ON DELETE SET NULL
);

CREATE TABLE discord.user_teams (
  su_id INT NOT NULL,
  team_id INT NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

  PRIMARY KEY (su_id, team_id),
  FOREIGN KEY (su_id) REFERENCES discord.servers_users(su_id) ON DELETE CASCADE,
  FOREIGN KEY (team_id) REFERENCES discord.teams(team_id) ON DELETE CASCADE
);

CREATE TABLE discord.channels (
  channel_id         SERIAL       PRIMARY KEY,
  server_id          INT          NOT NULL,
  discord_channel_id VARCHAR(32)  NOT NULL,
  parent_channel_id  VARCHAR(32),
  name               VARCHAR(255) NOT NULL,
  type               VARCHAR(50)  NOT NULL,
  position           INT,
  topic              TEXT,
  nsfw               BOOLEAN      NOT NULL DEFAULT FALSE,
  created_at         TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,
  updated_at         TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,

  UNIQUE (server_id, discord_channel_id),
  FOREIGN KEY (server_id) REFERENCES discord.servers(server_id) ON DELETE CASCADE
);

CREATE TABLE discord.server_syncs (
  sync_id             SERIAL    PRIMARY KEY,
  server_id           INT       NOT NULL,
  role_count          INT       NOT NULL DEFAULT 0,
  channel_count       INT       NOT NULL DEFAULT 0,
  category_count      INT       NOT NULL DEFAULT 0,
  synced_by_discord_id VARCHAR(32),
  synced_at           TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (server_id) REFERENCES discord.servers(server_id) ON DELETE CASCADE
);

-- ============================================================================
-- Inelicom Schema
-- 
-- This schema contains tables related to buildings, departments, rooms, faculties,
-- projects and organizations. These entities are used to manage the physical and
-- organizational structure of the university, as well as the projects and organizations 
-- that are part of the university community.
-- ============================================================================

CREATE TABLE inelicom.contacts (
  contact_id SERIAL       PRIMARY KEY,
  name       VARCHAR(255) NOT NULL,
  email      VARCHAR(255) NOT NULL,
  phone      VARCHAR(20)  NOT NULL,
  website    TEXT         NOT NULL,
  created_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE inelicom.faculties (
  faculty_id      SERIAL       PRIMARY KEY,
  name            VARCHAR(255) NOT NULL UNIQUE,
  extension       VARCHAR(32),
  web             TEXT,
  phone           VARCHAR(32),
  facebook        TEXT,
  email           VARCHAR(255),
  office          VARCHAR(255),
  job_entitlement VARCHAR(255),
  description     TEXT,
  abbreviation    VARCHAR(32),
  instagram       TEXT,
  created_at      TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE inelicom.buildings (
  building_id SERIAL       PRIMARY KEY,
  code        VARCHAR(32) UNIQUE,
  name        VARCHAR(255) NOT NULL UNIQUE,
  gpin        TEXT         NOT NULL,
  created_at  TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE inelicom.departments (
  department_id SERIAL       PRIMARY KEY,
  name          VARCHAR(255) NOT NULL UNIQUE,
  faculty_id    INT          NOT NULL,
  building_id   INT          NOT NULL,
  created_at    TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (faculty_id) REFERENCES inelicom.faculties(faculty_id) ON DELETE CASCADE,
  FOREIGN KEY (building_id) REFERENCES inelicom.buildings(building_id) ON DELETE CASCADE
);

CREATE TABLE inelicom.rooms (
  room_id       SERIAL       PRIMARY KEY,
  code          VARCHAR(255) NOT NULL UNIQUE,
  name          VARCHAR(255) NOT NULL UNIQUE,
  building_id   INT          NOT NULL,
  department_id INT          NOT NULL,
  created_at    TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (building_id) REFERENCES inelicom.buildings(building_id) ON DELETE CASCADE,
  FOREIGN KEY (department_id) REFERENCES inelicom.departments(department_id) ON DELETE CASCADE
);

CREATE TABLE inelicom.projects (
  project_id  SERIAL       PRIMARY KEY,
  web         TEXT,
  facebook    TEXT,
  instagram   TEXT,
  email       VARCHAR(255),
  name        VARCHAR(255) NOT NULL UNIQUE,
  description TEXT         NOT NULL UNIQUE,
  created_at  TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE inelicom.organizations (
  organization_id SERIAL       PRIMARY KEY,
  email           VARCHAR(255),
  facebook        TEXT,
  instagram       TEXT,
  twitter_x       TEXT,
  web             TEXT,
  name            VARCHAR(255) NOT NULL UNIQUE,
  description     TEXT         NOT NULL UNIQUE,
  created_at      TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);
