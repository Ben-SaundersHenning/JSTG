--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 18.3

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: public; Type: SCHEMA; Schema: -; Owner: postgres
--

-- *not* creating schema, since initdb creates it


ALTER SCHEMA public OWNER TO postgres;

--
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON SCHEMA public IS '';


--
-- Name: gender; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.gender AS ENUM (
    'male',
    'female',
    'other'
);


ALTER TYPE public.gender OWNER TO postgres;

--
-- Name: image_type; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.image_type AS ENUM (
    'signature'
);


ALTER TYPE public.image_type OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: assessment_base_types; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.assessment_base_types (
    id integer NOT NULL,
    name text NOT NULL
);


ALTER TABLE public.assessment_base_types OWNER TO postgres;

--
-- Name: assessment_base_types_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.assessment_base_types ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.assessment_base_types_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: assessors; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.assessors (
    registration_id character(8) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    first_name text NOT NULL,
    last_name text NOT NULL,
    gender public.gender NOT NULL,
    email text NOT NULL,
    qualifications_paragraph text NOT NULL
);


ALTER TABLE public.assessors OWNER TO postgres;

--
-- Name: document_templates; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.document_templates (
    id integer NOT NULL,
    label text NOT NULL,
    file_path text NOT NULL,
    is_active boolean DEFAULT true NOT NULL
);


ALTER TABLE public.document_templates OWNER TO postgres;

--
-- Name: document_templates_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.document_templates ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.document_templates_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: document_type_members; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.document_type_members (
    combination_id integer NOT NULL,
    type_id integer NOT NULL
);


ALTER TABLE public.document_type_members OWNER TO postgres;

--
-- Name: images; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.images (
    id integer NOT NULL,
    assessor_id character(8),
    image_type public.image_type NOT NULL,
    file_path text NOT NULL
);


ALTER TABLE public.images OWNER TO postgres;

--
-- Name: images_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.images ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.images_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: referral_companies; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.referral_companies (
    id integer NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    name text NOT NULL,
    common_name text NOT NULL,
    phone text NOT NULL,
    fax text NOT NULL,
    email text NOT NULL
);


ALTER TABLE public.referral_companies OWNER TO postgres;

--
-- Name: referral_companies_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.referral_companies ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.referral_companies_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: referral_company_addresses; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.referral_company_addresses (
    id integer NOT NULL,
    company_id integer,
    address_type text NOT NULL,
    street_address text NOT NULL,
    unit text,
    city text NOT NULL,
    province text DEFAULT 'Ontario'::text NOT NULL,
    country text DEFAULT 'Canada'::text NOT NULL,
    postal_code text NOT NULL
);


ALTER TABLE public.referral_company_addresses OWNER TO postgres;

--
-- Name: referral_company_addresses_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.referral_company_addresses ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.referral_company_addresses_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: assessment_base_types; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.assessment_base_types (id, name) FROM stdin;
1	AC
2	MRB
3	NEB
4	CAT
5	CAT_GOSE
\.


--
-- Data for Name: assessors; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.assessors (registration_id, is_active, first_name, last_name, gender, email, qualifications_paragraph) FROM stdin;
G1234567	t	Frodo	Baggins	male	frodo@lotr.com	Ring Bearer
G7654321	f	Samwise	Gamgee	male	sam@lotr.com	Gardener
\.


--
-- Data for Name: document_templates; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.document_templates (id, label, file_path, is_active) FROM stdin;
1	AC	templates/AC.dotx	t
2	MRB	templates/MRB.dotx	t
3	NEB	templates/NEB.dotx	t
4	CAT	templates/CAT.dotx	t
5	CAT GOSE	templates/CATGOSE.dotx	t
6	AC/NEB	templates/AC_NEB.dotx	t
7	MRB/NEB	templates/MRB_NEB.dotx	t
8	CAT/CAT GOSE	templates/CAT_CATGOSE.dotx	t
\.


--
-- Data for Name: document_type_members; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.document_type_members (combination_id, type_id) FROM stdin;
1	1
2	2
3	3
4	4
5	5
6	1
6	3
7	2
7	3
8	4
8	5
\.


--
-- Data for Name: images; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.images (id, assessor_id, image_type, file_path) FROM stdin;
1	G1234567	signature	images/G1234567.png
2	G7654321	signature	images/G7654321.png
\.


--
-- Data for Name: referral_companies; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.referral_companies (id, is_active, name, common_name, phone, fax, email) FROM stdin;
1	t	Viewpoint Medical Assessments	Viewpoint	999-999-9999	888-888-8888	info@viewpoint.com
2	f	HVE Medical Assessments	HVE	777-777-7777	666-666-6666	info@hve.com
\.


--
-- Data for Name: referral_company_addresses; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.referral_company_addresses (id, company_id, address_type, street_address, unit, city, province, country, postal_code) FROM stdin;
1	1	physical	123 Water Street	\N	Toronto	Ontario	Canada	M1M 1M1
2	2	physical	456 Gum Street	Suite 1500	Toronto	Ontario	Canada	M2M 3M4
\.


--
-- Name: assessment_base_types_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.assessment_base_types_id_seq', 5, true);


--
-- Name: document_templates_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.document_templates_id_seq', 8, true);


--
-- Name: images_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.images_id_seq', 2, true);


--
-- Name: referral_companies_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.referral_companies_id_seq', 2, true);


--
-- Name: referral_company_addresses_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.referral_company_addresses_id_seq', 2, true);


--
-- Name: assessment_base_types assessment_base_types_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.assessment_base_types
    ADD CONSTRAINT assessment_base_types_pkey PRIMARY KEY (id);


--
-- Name: assessors assessors_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.assessors
    ADD CONSTRAINT assessors_pkey PRIMARY KEY (registration_id);


--
-- Name: document_templates document_templates_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.document_templates
    ADD CONSTRAINT document_templates_pkey PRIMARY KEY (id);


--
-- Name: document_type_members document_type_members_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.document_type_members
    ADD CONSTRAINT document_type_members_pkey PRIMARY KEY (combination_id, type_id);


--
-- Name: images images_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.images
    ADD CONSTRAINT images_pkey PRIMARY KEY (id);


--
-- Name: referral_companies referral_companies_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.referral_companies
    ADD CONSTRAINT referral_companies_pkey PRIMARY KEY (id);


--
-- Name: referral_company_addresses referral_company_addresses_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.referral_company_addresses
    ADD CONSTRAINT referral_company_addresses_pkey PRIMARY KEY (id);


--
-- Name: document_type_members document_type_members_combination_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.document_type_members
    ADD CONSTRAINT document_type_members_combination_id_fkey FOREIGN KEY (combination_id) REFERENCES public.document_templates(id);


--
-- Name: document_type_members document_type_members_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.document_type_members
    ADD CONSTRAINT document_type_members_type_id_fkey FOREIGN KEY (type_id) REFERENCES public.assessment_base_types(id);


--
-- Name: images images_assessor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.images
    ADD CONSTRAINT images_assessor_id_fkey FOREIGN KEY (assessor_id) REFERENCES public.assessors(registration_id);


--
-- Name: referral_company_addresses referral_company_addresses_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.referral_company_addresses
    ADD CONSTRAINT referral_company_addresses_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.referral_companies(id);


--
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE USAGE ON SCHEMA public FROM PUBLIC;
GRANT ALL ON SCHEMA public TO jstg;


--
-- Name: TABLE assessment_base_types; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.assessment_base_types TO jstg;


--
-- Name: SEQUENCE assessment_base_types_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,USAGE ON SEQUENCE public.assessment_base_types_id_seq TO jstg;


--
-- Name: TABLE assessors; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.assessors TO jstg;


--
-- Name: TABLE document_templates; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.document_templates TO jstg;


--
-- Name: SEQUENCE document_templates_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,USAGE ON SEQUENCE public.document_templates_id_seq TO jstg;


--
-- Name: TABLE document_type_members; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.document_type_members TO jstg;


--
-- Name: TABLE images; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.images TO jstg;


--
-- Name: SEQUENCE images_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,USAGE ON SEQUENCE public.images_id_seq TO jstg;


--
-- Name: TABLE referral_companies; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.referral_companies TO jstg;


--
-- Name: SEQUENCE referral_companies_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,USAGE ON SEQUENCE public.referral_companies_id_seq TO jstg;


--
-- Name: TABLE referral_company_addresses; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.referral_company_addresses TO jstg;


--
-- Name: SEQUENCE referral_company_addresses_id_seq; Type: ACL; Schema: public; Owner: postgres
--

GRANT SELECT,USAGE ON SEQUENCE public.referral_company_addresses_id_seq TO jstg;


--
-- PostgreSQL database dump complete
--

