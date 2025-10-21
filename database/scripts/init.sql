--
-- PostgreSQL database dump
--

-- Dumped from database version 16.3
-- Dumped by pg_dump version 17.2

CREATE USER jstg
with password 'password';

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', FALSE);
SET check_function_bodies = FALSE;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: gender; Type: TYPE; Schema: public; Owner: jstg
--

CREATE TYPE public.gender AS ENUM (
    'male',
    'female',
    'other'
);


ALTER TYPE public.gender OWNER TO jstg;

--
-- Name: image_type; Type: TYPE; Schema: public; Owner: jstg
--

CREATE TYPE public.image_type AS ENUM (
    'signature'
);


ALTER TYPE public.image_type OWNER TO jstg;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: assessment_types; Type: TABLE; Schema: public; Owner: jstg
--

CREATE TABLE public.assessment_types (
    id integer NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    name text,
    common_name text
);


ALTER TABLE public.assessment_types OWNER TO jstg;

--
-- Name: assessment_types_id_seq; Type: SEQUENCE; Schema: public; Owner: jstg
--

CREATE SEQUENCE public.assessment_types_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.assessment_types_id_seq OWNER TO jstg;

--
-- Name: assessment_types_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: jstg
--

ALTER SEQUENCE public.assessment_types_id_seq OWNED BY public.assessment_types.id;


--
-- Name: assessors; Type: TABLE; Schema: public; Owner: jstg
--

CREATE TABLE public.assessors (
    registration_id character(8) NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    first_name text NOT NULL,
    last_name text NOT NULL,
    gender public.gender NOT NULL,
    email text NOT NULL,
    qualifications_paragraph text NOT NULL
);


ALTER TABLE public.assessors OWNER TO jstg;

--
-- Name: documents; Type: TABLE; Schema: public; Owner: jstg
--

CREATE TABLE public.documents (
    id integer NOT NULL,
    name text NOT NULL,
    user_friendly_name text NOT NULL,
    common_name text NOT NULL,
    file_name text NOT NULL
);


ALTER TABLE public.documents OWNER TO jstg;

--
-- Name: documents_id_seq; Type: SEQUENCE; Schema: public; Owner: jstg
--

CREATE SEQUENCE public.documents_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.documents_id_seq OWNER TO jstg;

--
-- Name: documents_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: jstg
--

ALTER SEQUENCE public.documents_id_seq OWNED BY public.documents.id;


--
-- Name: images; Type: TABLE; Schema: public; Owner: jstg
--

CREATE TABLE public.images (
    id integer NOT NULL,
    assessor_id character varying(8),
    image_type public.image_type NOT NULL,
    file_name text NOT NULL
);


ALTER TABLE public.images OWNER TO jstg;

--
-- Name: images_id_seq; Type: SEQUENCE; Schema: public; Owner: jstg
--

CREATE SEQUENCE public.images_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.images_id_seq OWNER TO jstg;

--
-- Name: images_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: jstg
--

ALTER SEQUENCE public.images_id_seq OWNED BY public.images.id;


--
-- Name: referral_companies; Type: TABLE; Schema: public; Owner: jstg
--

CREATE TABLE public.referral_companies (
    id integer NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    name text NOT NULL,
    common_name text NOT NULL,
    street_address text NOT NULL,
    unit text NOT NULL,
    city text NOT NULL,
    province text NOT NULL,
    postal_code text NOT NULL,
    country text DEFAULT 'Canada'::text NOT NULL,
    phone text NOT NULL,
    fax text NOT NULL,
    email text NOT NULL
);


ALTER TABLE public.referral_companies OWNER TO jstg;

--
-- Name: referral_companies_id_seq; Type: SEQUENCE; Schema: public; Owner: jstg
--

CREATE SEQUENCE public.referral_companies_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.referral_companies_id_seq OWNER TO jstg;

--
-- Name: referral_companies_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: jstg
--

ALTER SEQUENCE public.referral_companies_id_seq OWNED BY public.referral_companies.id;


--
-- Name: assessment_types id; Type: DEFAULT; Schema: public; Owner: jstg
--

ALTER TABLE ONLY public.assessment_types ALTER COLUMN id SET DEFAULT nextval('public.assessment_types_id_seq'::regclass);


--
-- Name: documents id; Type: DEFAULT; Schema: public; Owner: jstg
--

ALTER TABLE ONLY public.documents ALTER COLUMN id SET DEFAULT nextval('public.documents_id_seq'::regclass);


--
-- Name: images id; Type: DEFAULT; Schema: public; Owner: jstg
--

ALTER TABLE ONLY public.images ALTER COLUMN id SET DEFAULT nextval('public.images_id_seq'::regclass);


--
-- Name: referral_companies id; Type: DEFAULT; Schema: public; Owner: jstg
--

ALTER TABLE ONLY public.referral_companies ALTER COLUMN id SET DEFAULT nextval('public.referral_companies_id_seq'::regclass);


--
-- Data for Name: assessment_types; Type: TABLE DATA; Schema: public; Owner: jstg
--

INSERT INTO public.assessment_types (id, name, common_name) VALUES (1, 'Assessment type 1', 'at1');
INSERT INTO public.assessment_types (id, name, common_name) VALUES (2, 'Assessment type 2', 'at2');
INSERT INTO public.assessment_types (id, name, common_name) VALUES (3, 'Assessment type 3', 'at3');


--
-- Data for Name: assessors; Type: TABLE DATA; Schema: public; Owner: jstg
--

INSERT INTO public.assessors (registration_id, first_name, last_name, gender, email, qualifications_paragraph) VALUES ('G1234569', 'Frodo', 'Baggins', 'male', 'frodo@lotr.com', 'Ring Bearer');
INSERT INTO public.assessors (registration_id, first_name, last_name, gender, email, qualifications_paragraph) VALUES ('G1234559', 'Bilbo', 'Baggins', 'male', 'bilbo@lotr.com', 'Ring Bearer');
INSERT INTO public.assessors (registration_id, is_active, first_name, last_name, gender, email, qualifications_paragraph) VALUES ('G6543219', FALSE, 'Tom', 'Bombadil', 'male', 'tom@lotr.com', 'Yellow Boots');
INSERT INTO public.assessors (registration_id, first_name, last_name, gender, email, qualifications_paragraph) VALUES ('G1597539', 'Goldberry', 'River-daughter', 'female', 'goldberry@lotr.com', 'lilies');
INSERT INTO public.assessors (registration_id, is_active, first_name, last_name, gender, email, qualifications_paragraph) VALUES ('G1597535', FALSE, 'Treebeard', 'Fangorn', 'other', 'treebeard@lotr.com', 'Fangorn Forest');


--
-- Data for Name: documents; Type: TABLE DATA; Schema: public; Owner: jstg
--

INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (1, 'Test Document 1', 'AC', 'AC', 'TD1.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (2, 'Test Document 2', 'AC MRB', 'AC MRB', 'TD2.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (3, 'Test Document 3', 'AC MRB NEB', 'AC MRB NEB', 'TD3.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (4, 'Test Document 4', 'AC NEB', 'AC NEB', 'TD4.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (5, 'Test Document 5', 'CAT', 'CAT', 'TD5.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (6, 'Test Document 5', 'CAT AC', 'CAT AC', 'TD5.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (7, 'Test Document 5', 'CAT AC MRB', 'CAT AC MRB', 'TD5.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (8, 'Test Document 5', 'CAT CAT_GOSE', 'CAT CAT_GOSE', 'TD5.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (9, 'Test Document 5', 'CAT CAT_GOSE MRB', 'CAT CAT_GOSE MRB', 'TD5.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (10, 'Test Document 5', 'CAT_GOSE', 'CAT_GOSE', 'TD5.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (11, 'Test Document 5', 'MRB', 'MRB', 'TD5.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (12, 'Test Document 5', 'MRB NEB', 'MRB NEB', 'TD5.docx');
INSERT INTO public.documents (id, name, user_friendly_name, common_name, file_name) VALUES (13, 'Test Document NEB', 'NEB', 'NEB', 'NEB.dotx');


--
-- Data for Name: images; Type: TABLE DATA; Schema: public; Owner: jstg
--

INSERT INTO public.images (id, assessor_id, image_type, file_name) VALUES (1, 'G1234569', 'signature', 'aa.png');
INSERT INTO public.images (id, assessor_id, image_type, file_name) VALUES (2, 'G1234559', 'signature', 'bb.png');
INSERT INTO public.images (id, assessor_id, image_type, file_name) VALUES (3, 'G6543219', 'signature', 'cc.png');
INSERT INTO public.images (id, assessor_id, image_type, file_name) VALUES (4, 'G1597539', 'signature', 'dd.png');
INSERT INTO public.images (id, assessor_id, image_type, file_name) VALUES (5, 'G1597535', 'signature', 'ee.png');

--
-- Data for Name: referral_companies; Type: TABLE DATA; Schema: public; Owner: jstg
--

INSERT INTO public.referral_companies (id, name, common_name, street_address, unit, city, province, postal_code, country, phone, fax, email) VALUES (1, 'Nalgene', 'Nalgene', '123 Water Street', 'Suite 1500', 'Toronto', 'Ontario', 'M1M 1M1', 'Canada', '999-999-9999', '999-999-9999', 'info@nalgene.com');
INSERT INTO public.referral_companies (id, name, common_name, street_address, unit, city, province, postal_code, country, phone, fax, email) VALUES (2, 'Excel', 'Execel', '123 Gum Street', '', 'Toronto', 'Ontario', 'M9M 9M9', 'Canada', '111-111-1111', '111-111-1111', 'info@excel.com');


--
-- Name: assessment_types_id_seq; Type: SEQUENCE SET; Schema: public; Owner: jstg
--

SELECT pg_catalog.setval('public.assessment_types_id_seq', 3, TRUE);


--
-- Name: documents_id_seq; Type: SEQUENCE SET; Schema: public; Owner: jstg
--

SELECT pg_catalog.setval('public.documents_id_seq', 13, TRUE);


--
-- Name: images_id_seq; Type: SEQUENCE SET; Schema: public; Owner: jstg
--

SELECT pg_catalog.setval('public.images_id_seq', 1, TRUE);


--
-- Name: referral_companies_id_seq; Type: SEQUENCE SET; Schema: public; Owner: jstg
--

SELECT pg_catalog.setval('public.referral_companies_id_seq', 2, TRUE);


--
-- Name: assessment_types assessment_types_pkey; Type: CONSTRAINT; Schema: public; Owner: jstg
--

ALTER TABLE ONLY public.assessment_types
    ADD CONSTRAINT assessment_types_pkey PRIMARY KEY (id);


--
-- Name: assessors assessors_pkey; Type: CONSTRAINT; Schema: public; Owner: jstg
--

ALTER TABLE ONLY public.assessors
    ADD CONSTRAINT assessors_pkey PRIMARY KEY (registration_id);


--
-- Name: documents documents_pkey; Type: CONSTRAINT; Schema: public; Owner: jstg
--

ALTER TABLE ONLY public.documents
    ADD CONSTRAINT documents_pkey PRIMARY KEY (id);


--
-- Name: images images_pkey; Type: CONSTRAINT; Schema: public; Owner: jstg
--

ALTER TABLE ONLY public.images
    ADD CONSTRAINT images_pkey PRIMARY KEY (id);


--
-- Name: referral_companies referral_companies_pkey; Type: CONSTRAINT; Schema: public; Owner: jstg
--

ALTER TABLE ONLY public.referral_companies
    ADD CONSTRAINT referral_companies_pkey PRIMARY KEY (id);


--
-- Name: images images_assessor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: jstg
--

ALTER TABLE ONLY public.images
    ADD CONSTRAINT images_assessor_id_fkey FOREIGN KEY (assessor_id) REFERENCES public.assessors(registration_id);


--
-- PostgreSQL database dump complete
--

