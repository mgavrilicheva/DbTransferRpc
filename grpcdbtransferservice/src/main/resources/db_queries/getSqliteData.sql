SELECT *
FROM languages
ORDER BY
    macrofamily_id, family_id, language_id,
    writing_type_id, writing_id, alphabet_id,
    country_id, lang_info_rec_id, language_by_countries_rec_id;