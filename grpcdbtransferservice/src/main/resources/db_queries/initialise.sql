-- Table: public.macrofamilies

-- DROP TABLE public.macrofamilies;

CREATE TABLE IF NOT EXISTS public.macrofamilies
(
    id integer NOT NULL,
    name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT macrofamilies_pkey PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

-- Table: public.families

-- DROP TABLE public.families;

CREATE TABLE IF NOT EXISTS public.families
(
    id integer,
    name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    macrofamily_id integer,
    CONSTRAINT families_pkey PRIMARY KEY (id),
    CONSTRAINT families_macrofamily_id_fkey FOREIGN KEY (macrofamily_id)
        REFERENCES public.macrofamilies (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

-- Table: public.languages

-- DROP TABLE public.languages;

CREATE TABLE IF NOT EXISTS public.languages
(
    id integer NOT NULL,
    name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    family_id integer NOT NULL,
    CONSTRAINT languages_pkey PRIMARY KEY (id),
    CONSTRAINT languages_family_id_fkey FOREIGN KEY (family_id)
        REFERENCES public.families (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

-- Table: public.alphabets

-- DROP TABLE public.alphabets;

CREATE TABLE IF NOT EXISTS public.alphabets
(
    id integer,
    name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    count_of_letters integer NOT NULL,
    CONSTRAINT alphabets_pkey PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;
	
-- Table: public.writing_types

-- DROP TABLE public.writing_types;

CREATE TABLE IF NOT EXISTS public.writing_types
(
    id integer NOT NULL,
    name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT writing_types_pkey PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

-- Table: public.writings

-- DROP TABLE public.writings;

CREATE TABLE IF NOT EXISTS public.writings
(
    id integer NOT NULL,
    name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    symbols_count integer NOT NULL,
    type_id integer NOT NULL,
    CONSTRAINT writings_pkey PRIMARY KEY (id),
    CONSTRAINT writings_type_id_fkey FOREIGN KEY (type_id)
        REFERENCES public.writing_types (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

-- Table: public.language_info

-- DROP TABLE public.language_info;

CREATE TABLE IF NOT EXISTS public.language_info
(
    record_id integer NOT NULL,
    language_id integer NOT NULL,
    writing_id integer,
    alphabet_id integer,
    CONSTRAINT language_info_pkey PRIMARY KEY (record_id),
    CONSTRAINT language_info_alphabet_id_fkey FOREIGN KEY (alphabet_id)
        REFERENCES public.alphabets (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT language_info_language_id_fkey FOREIGN KEY (language_id)
        REFERENCES public.languages (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT language_info_writing_id_fkey FOREIGN KEY (writing_id)
        REFERENCES public.writings (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

-- Table: public.countries

-- DROP TABLE public.countries;

CREATE TABLE IF NOT EXISTS public.countries
(
    id integer,
    name character varying(120) COLLATE pg_catalog."default" NOT NULL,
    capital character varying(160) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT countries_pkey PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

-- Table: public.languages_by_countries_info

-- DROP TABLE public.languages_by_countries_info;

CREATE TABLE IF NOT EXISTS public.languages_by_countries_info
(
    rec_id integer NOT NULL,
    lang_info_rec_id integer NOT NULL,
    country_id integer NOT NULL,
    is_state_language boolean NOT NULL,
    CONSTRAINT languages_by_countries_info_pkey PRIMARY KEY (rec_id),
    CONSTRAINT languages_by_countries_info_country_id_fkey FOREIGN KEY (country_id)
        REFERENCES public.countries (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT languages_by_countries_info_lang_info_rec_id_fkey FOREIGN KEY (lang_info_rec_id)
        REFERENCES public.language_info (record_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;