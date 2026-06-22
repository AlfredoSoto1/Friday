
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
  server_id   SERIAL       PRIMARY KEY,
  name        VARCHAR(255) NOT NULL UNIQUE,
  server_code VARCHAR(255) NOT NULL UNIQUE,
  created_at  TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
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
  email      VARCHAR(255) NOT NULL UNIQUE,
  fullname   VARCHAR(255) NOT NULL UNIQUE,
  username   VARCHAR(255) NOT NULL UNIQUE,
  created_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE discord.servers_users (
  su_id      SERIAL     PRIMARY KEY,
  server_id  INT        NOT NULL,
  user_id    INT        NOT NULL,
  verified   BOOLEAN    NOT NULL DEFAULT FALSE,
  created_at TIMESTAMP  DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (user_id) REFERENCES discord.users(user_id) ON DELETE CASCADE,
  FOREIGN KEY (server_id) REFERENCES discord.servers(server_id) ON DELETE CASCADE
);

CREATE TABLE discord.permissions (
  permission_id SERIAL       PRIMARY KEY,
  name          VARCHAR(255) NOT NULL UNIQUE,
  created_at    TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE discord.roles (
  role_id    SERIAL       PRIMARY KEY,
  name       VARCHAR(255) NOT NULL UNIQUE,
  created_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
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
  team_id     SERIAL       PRIMARY KEY,
  name        VARCHAR(255) NOT NULL UNIQUE,
  created_at  TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE discord.user_teams (
  su_id INT NOT NULL,
  team_id INT NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

  PRIMARY KEY (su_id, team_id),
  FOREIGN KEY (su_id) REFERENCES discord.servers_users(su_id) ON DELETE CASCADE,
  FOREIGN KEY (team_id) REFERENCES discord.teams(team_id) ON DELETE CASCADE
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
  faculty_id SERIAL       PRIMARY KEY,
  name       VARCHAR(255) NOT NULL UNIQUE,
  created_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE inelicom.buildings (
  building_id SERIAL       PRIMARY KEY,
  name        VARCHAR(255) NOT NULL UNIQUE,
  gpin        TEXT         NOT NULL UNIQUE,
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
  name        VARCHAR(255) NOT NULL UNIQUE,
  description TEXT         NOT NULL UNIQUE,
  created_at  TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE inelicom.organizations (
  organization_id SERIAL       PRIMARY KEY,
  name            VARCHAR(255) NOT NULL UNIQUE,
  description     TEXT         NOT NULL UNIQUE,
  created_at      TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================================
-- Discord Bot Runtime Configuration
-- ============================================================================

CREATE TABLE discord.bot_server_profiles (
  profile_id                 SERIAL       PRIMARY KEY,
  discord_guild_id           BIGINT       NOT NULL UNIQUE,
  name                       VARCHAR(255) NOT NULL,
  enabled                    BOOLEAN      NOT NULL DEFAULT TRUE,
  primary_color              VARCHAR(6)   NOT NULL DEFAULT '2f80ed',
  thumbnail_url              TEXT,
  footer_text                TEXT,
  verification_enabled       BOOLEAN      NOT NULL DEFAULT TRUE,
  verification_title         TEXT         NOT NULL DEFAULT 'Bienvenido al servidor',
  verification_description   TEXT         NOT NULL DEFAULT 'Presiona el boton de verificacion para confirmar tu correo institucional.',
  verification_button_label  VARCHAR(80)  NOT NULL DEFAULT 'Verify',
  verification_channel_id    VARCHAR(32),
  verified_role_id           VARCHAR(32),
  verification_banner_url    TEXT,
  welcome_enabled            BOOLEAN      NOT NULL DEFAULT TRUE,
  welcome_title              TEXT         NOT NULL DEFAULT 'Bienvenido',
  welcome_description        TEXT         NOT NULL DEFAULT 'Gracias por unirte. Completa la verificacion para acceder al servidor.',
  welcome_channel_id         VARCHAR(32),
  welcome_banner_url         TEXT,
  created_at                 TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,
  updated_at                 TIMESTAMP    DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE discord.bot_command_responses (
  response_id   SERIAL       PRIMARY KEY,
  profile_id    INT          NOT NULL,
  command_name  VARCHAR(64)  NOT NULL,
  title         TEXT         NOT NULL,
  description   TEXT         NOT NULL,
  image_url     TEXT,
  url           TEXT,
  color         VARCHAR(6),
  ephemeral     BOOLEAN      NOT NULL DEFAULT TRUE,
  created_at    TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,

  UNIQUE (profile_id, command_name),
  FOREIGN KEY (profile_id) REFERENCES discord.bot_server_profiles(profile_id) ON DELETE CASCADE
);

CREATE TABLE discord.bot_setup_channels (
  setup_channel_id SERIAL       PRIMARY KEY,
  profile_id       INT          NOT NULL,
  name             VARCHAR(100) NOT NULL,
  type             VARCHAR(20)  NOT NULL,
  category         VARCHAR(100),
  position         INT          NOT NULL DEFAULT 0,
  created_at       TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,

  FOREIGN KEY (profile_id) REFERENCES discord.bot_server_profiles(profile_id) ON DELETE CASCADE
);

CREATE TABLE discord.bot_member_verifications (
  verification_id   SERIAL       PRIMARY KEY,
  discord_guild_id  BIGINT       NOT NULL,
  discord_user_id   VARCHAR(32)  NOT NULL,
  discord_username  VARCHAR(255) NOT NULL,
  email             VARCHAR(255) NOT NULL,
  fun_fact          TEXT,
  verified          BOOLEAN      NOT NULL DEFAULT FALSE,
  verified_at       TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,

  UNIQUE (discord_guild_id, discord_user_id)
);

CREATE TABLE discord.bot_member_levels (
  level_id          SERIAL       PRIMARY KEY,
  discord_guild_id  BIGINT       NOT NULL,
  discord_user_id   VARCHAR(32)  NOT NULL,
  discord_username  VARCHAR(255) NOT NULL,
  xp                INT          NOT NULL DEFAULT 0,
  level             INT          NOT NULL DEFAULT 1,
  updated_at        TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,

  UNIQUE (discord_guild_id, discord_user_id)
);
