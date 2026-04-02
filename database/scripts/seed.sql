INSERT INTO assessment_base_types (name) VALUES
    ('AC'), -- id 1
    ('MRB'), -- id 2
    ('NEB'), -- id 3
    ('CAT'), -- id 4
    ('CAT_GOSE'); -- id 5

INSERT INTO document_templates (label, file_path) VALUES
    ('AC', 'templates/AC.dotx'), -- id 1
    ('MRB', 'templates/MRB.dotx'), -- id 2
    ('NEB', 'templates/NEB.dotx'), -- id 3
    ('CAT', 'templates/CAT.dotx'), -- id 4
    ('CAT GOSE', 'templates/CATGOSE.dotx'), -- id 5
    ('AC/NEB', 'templates/AC_NEB.dotx'), -- id 6
    ('MRB/NEB', 'templates/MRB_NEB.dotx'), -- id 7
    ('CAT/CAT GOSE', 'templates/CAT_CATGOSE.dotx'); -- id 8

INSERT INTO document_type_members (combination_id, type_id) values
    (1, 1), -- AC -> AC
    (2, 2), -- MRB -> MRB
    (3, 3), -- NEB -> NEB
    (4, 4), -- CAT -> CAT
    (5, 5), -- CAT GOSE -> CAT GOSE
    (6, 1), -- AC / NEB -> AC
    (6, 3), -- AC / NEB -> NEB
    (7, 2), -- MRB / NEB -> MRB
    (7, 3), -- MRB / NEB -> NEB
    (8, 4), -- CAT / CAT GOSE -> CAT
    (8, 5); -- CAT / CAT GOSE -> CAT GOSE

INSERT INTO assessors (registration_id, is_active, first_name, last_name, gender, email, qualifications_paragraph) VALUES
    (
        'G1234567',
        TRUE,
        'Frodo',
        'Baggins',
        'male',
        'frodo@lotr.com',
        'Ring Bearer'
    ),
    (
        'G7654321',
        FALSE,
        'Samwise',
        'Gamgee',
        'male',
        'sam@lotr.com',
        'Gardener'
    );

INSERT INTO images (assessor_id, image_type, file_path) VALUES
    (
        'G1234567',
        'signature',
        'images/G1234567.png'
    ),
    (
        'G7654321',
        'signature',
        'images/G7654321.png'
    );

INSERT INTO referral_companies (is_active, name, common_name, phone, fax, email) VALUES
    (
        TRUE,
     'Viewpoint Medical Assessments',
     'Viewpoint',
     '999-999-9999',
     '888-888-8888',
     'info@viewpoint.com'
    ),
    (
        FALSE,
        'HVE Medical Assessments',
        'HVE',
        '777-777-7777',
        '666-666-6666',
        'info@hve.com'
    );

INSERT INTO referral_company_addresses (company_id, address_type, street_address, unit, city, postal_code) VALUES
    (
        1,
     'physical',
     '123 Water Street',
     NULL,
     'Toronto',
     'M1M 1M1'
    ),
    (
        2,
        'physical',
        '456 Gum Street',
        'Suite 1500',
        'Toronto',
        'M2M 3M4'
    );

