import type { FieldConfig } from "@/components/custom/record-fields";

export interface ResourceConfig {
  key: string;
  label: string;
  idKey: string;
  fields: FieldConfig[];
  columns: string[];
}

export const inelicomResources: ResourceConfig[] = [
  {
    key: "contacts",
    label: "Contacts",
    idKey: "contact_id",
    fields: [
      { key: "name", label: "Name" },
      { key: "email", label: "Email" },
      { key: "phone", label: "Phone" },
      { key: "website", label: "Website" },
    ],
    columns: ["contact_id", "name", "email", "phone", "website"],
  },
  {
    key: "faculties",
    label: "Faculties",
    idKey: "faculty_id",
    fields: [{ key: "name", label: "Name" }],
    columns: ["faculty_id", "name"],
  },
  {
    key: "buildings",
    label: "Buildings",
    idKey: "building_id",
    fields: [
      { key: "name", label: "Name" },
      { key: "gpin", label: "GPIN" },
    ],
    columns: ["building_id", "name", "gpin"],
  },
  {
    key: "departments",
    label: "Departments",
    idKey: "department_id",
    fields: [
      { key: "name", label: "Name" },
      { key: "faculty_id", label: "Faculty ID", type: "number" },
      { key: "building_id", label: "Building ID", type: "number" },
    ],
    columns: ["department_id", "name", "faculty_id", "building_id"],
  },
  {
    key: "rooms",
    label: "Rooms",
    idKey: "room_id",
    fields: [
      { key: "code", label: "Code" },
      { key: "name", label: "Name" },
      { key: "building_id", label: "Building ID", type: "number" },
      { key: "department_id", label: "Department ID", type: "number" },
    ],
    columns: ["room_id", "code", "name", "building_id", "department_id"],
  },
  {
    key: "projects",
    label: "Projects",
    idKey: "project_id",
    fields: [
      { key: "name", label: "Name" },
      { key: "description", label: "Description", type: "textarea" },
    ],
    columns: ["project_id", "name", "description"],
  },
  {
    key: "organizations",
    label: "Organizations",
    idKey: "organization_id",
    fields: [
      { key: "name", label: "Name" },
      { key: "description", label: "Description", type: "textarea" },
    ],
    columns: ["organization_id", "name", "description"],
  },
];

export const userFields: FieldConfig[] = [
  { key: "email", label: "Email" },
  { key: "fullname", label: "Full name" },
  { key: "username", label: "Username" },
];

export const roleFields: FieldConfig[] = [
  { key: "discord_role_id", label: "Discord role ID" },
  { key: "name", label: "Name" },
  { key: "color", label: "Color", type: "number" },
  { key: "position", label: "Position", type: "number" },
  { key: "managed", label: "Managed", type: "checkbox" },
  { key: "mentionable", label: "Mentionable", type: "checkbox" },
  { key: "hoisted", label: "Hoisted", type: "checkbox" },
];
