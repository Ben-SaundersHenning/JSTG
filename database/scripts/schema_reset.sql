-- Drop everything and start fresh
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO jstg;

CREATE TYPE public.gender AS ENUM (
    'male',
    'female',
    'other'
);

CREATE TYPE public.image_type AS ENUM (
    'signature'
);

create table public.assessment_base_types (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name TEXT NOT NULL
);

create table public.document_templates (
   id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
   label TEXT NOT NULL,
   file_path TEXT NOT NULL,
   is_active BOOLEAN NOT NULL DEFAULT TRUE
);

create table public.document_type_members (
    combination_id INTEGER REFERENCES document_templates(id),
    type_id INTEGER REFERENCES assessment_base_types(id),
    PRIMARY KEY (combination_id, type_id)
);

CREATE TABLE public.assessors (
    registration_id character(8) PRIMARY KEY,
    is_active boolean NOT NULL DEFAULT TRUE,
    first_name text NOT NULL,
    last_name text NOT NULL,
    gender public.gender NOT NULL,
    email text NOT NULL,
    qualifications_paragraph text NOT NULL
);

CREATE TABLE public.images (
   id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
   assessor_id character(8) REFERENCES assessors(registration_id),
   image_type public.image_type NOT NULL,
   file_path text NOT NULL
);

CREATE TABLE public.referral_companies (
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    is_active boolean NOT NULL DEFAULT TRUE,
    name text NOT NULL,
    common_name text NOT NULL,
    phone text NOT NULL,
    fax text NOT NULL,
    email text NOT NULL
);

CREATE TABLE public.referral_company_addresses (
  id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  company_id INTEGER REFERENCES referral_companies(id),
  address_type TEXT NOT NULL,
  street_address TEXT NOT NULL,
  unit TEXT,
  city TEXT NOT NULL,
  province TEXT NOT NULL DEFAULT 'Ontario',
  country TEXT NOT NULL DEFAULT 'Canada',
  postal_code TEXT NOT NULL
);

GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO jstg;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO jstg;
