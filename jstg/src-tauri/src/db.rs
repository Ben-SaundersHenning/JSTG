use crate::Error;
use chrono::NaiveDate;
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

#[derive(Serialize, Deserialize, sqlx::Type, Debug)]
#[sqlx(rename_all = "lowercase")]
pub enum Gender {
    Male,
    Female,
    Other,
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
pub struct Assessor {
    pub registration_id: String,
    pub first_name: String,
    pub last_name: String,
    pub gender: Gender,
    pub email: String,
    pub qualifications_paragraph: String,
    pub signature_path: String
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
pub struct AssessorListing {
    pub registration_id: String,
    pub name: String
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
pub struct ReferralCompany {
    pub id: i32,
    pub name: String,
    pub common_name: String,
    pub phone: String,
    pub fax: String,
    pub email: String,
    #[sqlx(flatten)]
    pub address: Address,
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
#[serde(rename_all = "camelCase")]
pub struct ReferralCompanyListing {
    pub id: i32,
    pub common_name: String,
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
pub struct TemplateListing {
    pub id: i32,
    pub label: String
}

// template base types
#[derive(Serialize, Deserialize, Debug)]
pub struct BaseType {
    pub id: i32,
    pub name: String
}

#[derive(Serialize, Deserialize, Debug)]
pub struct Template {
    pub id: i32,
    pub label: String,
    pub file_path: String,
    pub base_types: Vec<BaseType>
}

#[derive(sqlx::FromRow, Debug)]
pub struct TemplateRow {
    pub id: i32,
    pub label: String,
    pub file_path: String,
    pub base_type_id: i32,
    pub base_type_name: String,
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
#[serde(rename_all(serialize = "snake_case", deserialize = "camelCase"))]
pub struct Claimant {
    pub first_name: String,
    pub last_name: String,
    pub gender: Gender,
    pub age: Option<i32>,
    pub youth: Option<bool>,
    pub date_of_birth: NaiveDate,
    pub date_of_loss: NaiveDate,
    #[sqlx(flatten)]
    pub address: Address,
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
#[serde(rename_all(serialize = "snake_case", deserialize = "camelCase"))]
pub struct Address {
    pub street_address: String,
    pub unit: Option<String>,
    pub city: String,
    pub province: String,
    pub country: String,
    pub postal_code: String,
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
#[serde(rename_all(serialize = "snake_case", deserialize = "camelCase"))]
pub struct DocumentTemplates {
    pub id: i32,
    pub label: String
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
#[serde(rename_all(serialize = "snake_case", deserialize = "camelCase"))]
pub struct ImageData {
    pub file_path: String,
}

#[tauri::command]
pub async fn get_assessor_options(pool: tauri::State<'_, PgPool>) -> Result<Vec<AssessorListing>, Error> {

    let query = "SELECT registration_id,
                        first_name || ' ' || last_name as name
                 FROM assessors
                 WHERE is_active";

    let assessor_opts = sqlx::query_as::<_, AssessorListing>(query)
        .fetch_all(pool.inner())
        .await?;

    Ok(assessor_opts)

}

pub async fn get_assessor(registration_id: &str, pool: &PgPool) -> Result<Assessor, Error> {

    let query = "SELECT a.registration_id,
                        a.first_name,
                        a.last_name,
                        a.gender,
                        a.email,
                        a.qualifications_paragraph,
                        i.file_path as signature_path
                 FROM assessors a
                 INNER JOIN images i
                 ON a.registration_id = i.assessor_id
                 WHERE registration_id = $1";

    let assessor = sqlx::query_as::<_, Assessor>(query)
        .bind(registration_id)
        .fetch_one(pool)
        .await?;

    Ok(assessor)

}

#[tauri::command]
pub async fn get_referral_company_options(pool: tauri::State<'_, PgPool>) -> Result<Vec<ReferralCompanyListing>, Error> {

    let query = "SELECT id,
                        common_name
                 FROM referral_companies
                 WHERE is_active";

    let referral_company_opts = sqlx::query_as::<_, ReferralCompanyListing>(query)
        .fetch_all(pool.inner())
        .await?;

    Ok(referral_company_opts)

}

pub async fn get_referral_company(id: i32, pool: &PgPool) -> Result<ReferralCompany, Error> {

    let query = "SELECT rc.id,
                        rc.name,
                        rc.common_name,
                        rc.phone,
                        rc.fax,
                        rc.email,
                        rca.street_address,
                        rca.unit,
                        rca.postal_code,
                        rca.city,
                        rca.province,
                        rca.country
                 FROM referral_companies rc
                 INNER JOIN referral_company_addresses rca
                 ON rc.id = rca.company_id
                 WHERE rc.id = $1
                 AND rca.address_type = 'physical';";

    let company = sqlx::query_as::<_, ReferralCompany>(query)
        .bind(id)
        .fetch_one(pool)
        .await?;

    Ok(company)

}

#[tauri::command]
pub async fn get_template_options(pool: tauri::State<'_, PgPool>) -> Result<Vec<TemplateListing>, Error> {

    let query = "SELECT id,
                        label
                 FROM document_templates
                 WHERE is_active";

    let template_opts = sqlx::query_as::<_, TemplateListing>(query)
        .fetch_all(pool.inner())
        .await?;

    Ok(template_opts)

}

pub async fn get_template(id: i32, pool: &PgPool) -> Result<Template, Error> {

    let query = "SELECT dt.id,
                        dt.label,
                        dt.file_path,
                        abt.id as base_type_id,
                        abt.name as base_type_name
                FROM document_templates dt
                INNER JOIN document_type_members dtm
                ON dt.id = dtm.combination_id
                INNER JOIN assessment_base_types abt
                ON dtm.type_id = abt.id
                WHERE dt.id = $1";

    let opts = sqlx::query_as::<_, TemplateRow>(query)
        .bind(id)
        .fetch_all(pool)
        .await?;

    let template = Template {
        id: opts[0].id,
        label: opts[0].label.clone(),
        file_path: opts[0].file_path.clone(),
        base_types: opts.iter().map(|r| BaseType {
            id: r.base_type_id,
            name: r.base_type_name.clone(),
        }).collect(),

    };

    Ok(template)

}
