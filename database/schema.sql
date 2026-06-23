
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
  email      VARCHAR(255) NOT NULL UNIQUE,
  fullname   VARCHAR(255) NOT NULL UNIQUE,
  username   VARCHAR(255) NOT NULL UNIQUE,
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
  name       VARCHAR(255) NOT NULL,
  created_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP    DEFAULT CURRENT_TIMESTAMP,

  UNIQUE (server_id, name),
  FOREIGN KEY (server_id) REFERENCES discord.servers(server_id) ON DELETE CASCADE
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
-- Dummy Data
-- ============================================================================

INSERT INTO discord.servers
  (name, guild_id, enabled, verif_channel_id, verif_role_id, welcome_channel_id)
VALUES
  ('Friday Test Server 1', '100000000000000001', TRUE, '333', '111', '444'),
  ('Friday Test Server 2', '100000000000000002', FALSE, '1333', '1111', '1444');

INSERT INTO discord.users (email, fullname, username) VALUES
  ('student@upr.edu', 'Friday Student', 'student'),
  ('mentor@upr.edu', 'Friday Mentor', 'mentor'),
  ('staff@upr.edu', 'Friday Staff', 'staff');

INSERT INTO discord.servers_users (server_id, user_id, discord_user_id, verified, funfact, xp, level) VALUES
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'),
    (SELECT user_id FROM discord.users WHERE email = 'student@upr.edu'),
    '1234',
    FALSE,
    'I like software.',
    24,
    1
  ),
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'),
    (SELECT user_id FROM discord.users WHERE email = 'mentor@upr.edu'),
    '5678',
    TRUE,
    'I help new students.',
    75,
    2
  ),
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000002'),
    (SELECT user_id FROM discord.users WHERE email = 'staff@upr.edu'),
    '9012',
    TRUE,
    'I keep the server organized.',
    10,
    1
  );

INSERT INTO discord.permissions (name) VALUES
  ('view_channels'),
  ('verified_access'),
  ('manage_server');

INSERT INTO discord.roles
  (server_id, discord_role_id, name, color, position, managed, mentionable, hoisted)
VALUES
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'),
    '111',
    'verified',
    5793266,
    1,
    FALSE,
    FALSE,
    FALSE
  ),
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'),
    '112',
    'mentor',
    3447003,
    2,
    FALSE,
    TRUE,
    TRUE
  ),
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'),
    '113',
    'admin',
    15158332,
    3,
    FALSE,
    FALSE,
    TRUE
  ),
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000002'),
    '1111',
    'verified',
    5793266,
    1,
    FALSE,
    FALSE,
    FALSE
  );

INSERT INTO discord.role_permissions (role_id, permission_id) VALUES
  (
    (SELECT role_id FROM discord.roles WHERE discord_role_id = '111'),
    (SELECT permission_id FROM discord.permissions WHERE name = 'verified_access')
  ),
  (
    (SELECT role_id FROM discord.roles WHERE discord_role_id = '112'),
    (SELECT permission_id FROM discord.permissions WHERE name = 'view_channels')
  ),
  (
    (SELECT role_id FROM discord.roles WHERE discord_role_id = '113'),
    (SELECT permission_id FROM discord.permissions WHERE name = 'manage_server')
  );

INSERT INTO discord.user_roles (su_id, role_id) VALUES
  (
    (SELECT su_id FROM discord.servers_users WHERE discord_user_id = '1234'),
    (SELECT role_id FROM discord.roles WHERE discord_role_id = '111')
  ),
  (
    (SELECT su_id FROM discord.servers_users WHERE discord_user_id = '5678'),
    (SELECT role_id FROM discord.roles WHERE discord_role_id = '111')
  ),
  (
    (SELECT su_id FROM discord.servers_users WHERE discord_user_id = '5678'),
    (SELECT role_id FROM discord.roles WHERE discord_role_id = '112')
  );

INSERT INTO discord.teams (server_id, name) VALUES
  ((SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'), 'Orientation'),
  ((SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'), 'Support'),
  ((SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000002'), 'Archive');

INSERT INTO discord.user_teams (su_id, team_id) VALUES
  (
    (SELECT su_id FROM discord.servers_users WHERE discord_user_id = '1234'),
    (SELECT team_id FROM discord.teams WHERE name = 'Orientation')
  ),
  (
    (SELECT su_id FROM discord.servers_users WHERE discord_user_id = '5678'),
    (SELECT team_id FROM discord.teams WHERE name = 'Support')
  ),
  (
    (SELECT su_id FROM discord.servers_users WHERE discord_user_id = '9012'),
    (SELECT team_id FROM discord.teams WHERE name = 'Archive')
  );

INSERT INTO discord.channels
  (server_id, discord_channel_id, parent_channel_id, name, type, position, topic, nsfw)
VALUES
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'),
    '222',
    NULL,
    'general',
    'text',
    1,
    'General chat',
    FALSE
  ),
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'),
    '333',
    NULL,
    'verification',
    'text',
    2,
    'Member verification',
    FALSE
  ),
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'),
    '444',
    NULL,
    'welcome',
    'text',
    3,
    'Welcome messages',
    FALSE
  ),
  (
    (SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000002'),
    '1222',
    NULL,
    'general',
    'text',
    1,
    'Archived general chat',
    FALSE
  );

INSERT INTO discord.server_syncs
  (server_id, role_count, channel_count, category_count, synced_by_discord_id)
VALUES
  ((SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000001'), 3, 3, 0, '5678'),
  ((SELECT server_id FROM discord.servers WHERE guild_id = '100000000000000002'), 1, 1, 0, '9012');

INSERT INTO inelicom.contacts (name, email, phone, website) VALUES
  ('DCSP Office', 'dcsp@uprm.edu', '787-832-4040', 'https://www.uprm.edu/cse'),
  ('Decanato de Estudiantes', 'decanato.estudiantes@uprm.edu', '787-832-4040', 'https://www.uprm.edu/decanatoestudiantes'),
  ('Guardia Universitaria', 'guardia@uprm.edu', '787-265-3872', 'https://www.uprm.edu/guardiauniversitaria'),
  ('Asesoria Academica', 'asesoria.academica@uprm.edu', '787-832-4040', 'https://www.uprm.edu/asesoriaacademica'),
  ('Asistencia Economica', 'asistencia.economica@uprm.edu', '787-832-4040', 'https://www.uprm.edu/asistenciaeconomica');

INSERT INTO inelicom.faculties (name) VALUES
  ('Engineering'),
  ('Arts and Sciences'),
  ('Business Administration');

INSERT INTO inelicom.buildings (name, gpin) VALUES
  ('Stefani Building', 'GPIN-STEFANI'),
  ('Monzon Building', 'GPIN-MONZON'),
  ('Business Administration Building', 'GPIN-AE');

INSERT INTO inelicom.departments (name, faculty_id, building_id) VALUES
  (
    'Department of Computer Science and Engineering',
    (SELECT faculty_id FROM inelicom.faculties WHERE name = 'Engineering'),
    (SELECT building_id FROM inelicom.buildings WHERE name = 'Stefani Building')
  ),
  (
    'Department of Electrical and Computer Engineering',
    (SELECT faculty_id FROM inelicom.faculties WHERE name = 'Engineering'),
    (SELECT building_id FROM inelicom.buildings WHERE name = 'Stefani Building')
  ),
  (
    'Department of Mathematical Sciences',
    (SELECT faculty_id FROM inelicom.faculties WHERE name = 'Arts and Sciences'),
    (SELECT building_id FROM inelicom.buildings WHERE name = 'Monzon Building')
  );

INSERT INTO inelicom.rooms (code, name, building_id, department_id) VALUES
  (
    'S-113',
    'Software Engineering Lab',
    (SELECT building_id FROM inelicom.buildings WHERE name = 'Stefani Building'),
    (SELECT department_id FROM inelicom.departments WHERE name = 'Department of Computer Science and Engineering')
  ),
  (
    'S-201',
    'Computer Networks Lab',
    (SELECT building_id FROM inelicom.buildings WHERE name = 'Stefani Building'),
    (SELECT department_id FROM inelicom.departments WHERE name = 'Department of Computer Science and Engineering')
  ),
  (
    'M-107',
    'General Classroom',
    (SELECT building_id FROM inelicom.buildings WHERE name = 'Monzon Building'),
    (SELECT department_id FROM inelicom.departments WHERE name = 'Department of Mathematical Sciences')
  );

INSERT INTO inelicom.projects (name, description) VALUES
  ('Friday Assistant', 'Backend and bot services for campus Discord workflows.'),
  ('Inelicom Catalog', 'Searchable source of departments, rooms, contacts, projects, and organizations.'),
  ('Student Services Directory', 'Reference data for recurring student support questions.');

INSERT INTO inelicom.organizations (name, description) VALUES
  ('Computer Science Student Association', 'Student organization for computing talks, workshops, and peer support.'),
  ('IEEE UPRM Student Branch', 'Engineering student branch focused on technical and professional development.'),
  ('INELICOM Volunteers', 'Volunteer group supporting departmental activities and events.');

