use crate::Error;
use serde::{Deserialize, Serialize};
use sqlx::{postgres::PgConnection, Connection};
use chrono::NaiveDate;
use std::env;

const DB_CONN_STR: &str = "JSTG_DB_POSTGRESQL";

#[derive(Serialize, Deserialize, sqlx::Type, Debug)]
#[sqlx(rename_all = "lowercase")]
pub enum Gender {
    Male,
    Female,
    Other,
}

#[derive(Serialize, sqlx::FromRow, Debug)]
pub struct JsonListing {
    pub listing_details: sqlx::types::JsonValue,
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
pub struct Assessor {
    pub registration_id: String,
    pub first_name: String,
    pub last_name: String,
    pub gender: Gender,
    pub email: String,
    pub qualifications_paragraph: String,
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
    pub postal_code: String,
    pub country: String,
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
#[serde(rename_all(serialize = "snake_case", deserialize = "camelCase"))]
pub struct Document {
    pub id: i32,
    pub user_friendly_name: String,
    pub file_name: String,
}

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug)]
#[serde(rename_all(serialize = "snake_case", deserialize = "camelCase"))]
pub struct ImageData {
    pub file_name: String,
}


// Retrieves the set of documents
// (name)
#[tauri::command]
pub async fn get_document_options() -> Result<JsonListing, Error> {
    let mut conn_str: String = String::new();

    // dev environment
    if cfg!(dev) {
        conn_str.push_str("postgres://jstg:password@localhost:5432/jsot");
    } else {
        conn_str.push_str(&env::var(DB_CONN_STR).unwrap());
    }

    let mut conn = PgConnection::connect(&conn_str).await?;

    // Fetches a JSON array of JSON objects,
    // where each object represents one company.
    let query = "SELECT json_agg(json_build_object(
                    'document', d.common_name,
                    'id', d.id
                    )) as listing_details
                 FROM \"documents\" d;";

    let documents = sqlx::query_as::<_, JsonListing>(query)
        .fetch_one(&mut conn)
        .await?;

    conn.close().await?;

    Ok(documents)
}

// Retrieives a document file name from the database based on
// a given unique ID.
pub async fn get_document(document_id: i32) -> Result<Option<Document>, Error> {
    let mut conn_str: String = String::new();

    // dev environment
    if cfg!(dev) {
        conn_str.push_str("postgres://jstg:password@localhost:5432/jsot");
    } else {
        conn_str.push_str(&env::var(DB_CONN_STR).unwrap());
    }

    let mut conn = PgConnection::connect(&conn_str).await?;

    let query = "SELECT id,
                        user_friendly_name,
                        file_name
                 FROM \"documents\"
                 WHERE id = $1";

    let document = sqlx::query_as::<_, Document>(query)
        .bind(document_id)
        .fetch_optional(&mut conn)
        .await?;

    conn.close().await?;

    Ok(document)
}

// Retrieves the set of assessors
// (name, id)
#[tauri::command]
pub async fn get_assessor_options() -> Result<JsonListing, Error> {
    let mut conn_str: String = String::new();

    // dev environment
    if cfg!(dev) {
        conn_str.push_str("postgres://jstg:password@localhost:5432/jsot");
    } else {
        conn_str.push_str(&env::var(DB_CONN_STR).unwrap());
    }

    let mut conn = PgConnection::connect(&conn_str).await?;

    // Fetches a JSON array of JSON objects,
    // where each object represents one company.
    let query = "SELECT json_agg(json_build_object(
                    'name', a.first_name || ' ' || a.last_name,
                    'id', trim(a.registration_id)
                    )) as listing_details
                 FROM \"assessors\" a;";

    let assessors = sqlx::query_as::<_, JsonListing>(query)
        .fetch_one(&mut conn)
        .await?;

    conn.close().await?;

    Ok(assessors)
}

// Retrieives an assessor from the database based on
// a given unique ID.
pub async fn get_assessor(registration_id: &str) -> Result<Option<Assessor>, Error> {
    let mut conn_str: String = String::new();

    // dev environment
    if cfg!(dev) {
        conn_str.push_str("postgres://jstg:password@localhost:5432/jsot");
    } else {
        conn_str.push_str(&env::var(DB_CONN_STR).unwrap());
    }

    let mut conn = PgConnection::connect(&conn_str).await?;

    let query = "SELECT registration_id,
                        first_name,
                        last_name,
                        gender,
                        email,
                        qualifications_paragraph
                 FROM \"assessors\"
                 WHERE registration_id = $1";

    let assessor = sqlx::query_as::<_, Assessor>(query)
        .bind(registration_id)
        .fetch_optional(&mut conn)
        .await?;

    conn.close().await?;

    Ok(assessor)
}

// Retrieves the set of companies
// (name, id)
#[tauri::command]
pub async fn get_referral_company_options() -> Result<JsonListing, Error> {
    let mut conn_str: String = String::new();

    // dev environment
    if cfg!(dev) {
        conn_str.push_str("postgres://jstg:password@localhost:5432/jsot");
    } else {
        conn_str.push_str(&env::var(DB_CONN_STR).unwrap());
    }

    let mut conn = PgConnection::connect(&conn_str).await?;

    // Fetches a JSON array of JSON objects,
    // where each object represents one company.
    let query = "SELECT json_agg(json_build_object(
                    'name', rc.common_name,
                    'id', rc.id
                    )) as listing_details
                 FROM \"referral_companies\" rc;";

    let companies = sqlx::query_as::<_, JsonListing>(query)
        .fetch_one(&mut conn)
        .await?;

    conn.close().await?;

    Ok(companies)
}

// Retrieives a company from the database based on
// a given unique ID.
pub async fn get_referral_company(
    referral_company_id: i32,
) -> Result<Option<ReferralCompany>, Error> {
    let mut conn_str: String = String::new();

    // dev environment
    if cfg!(dev) {
        conn_str.push_str("postgres://jstg:password@localhost:5432/jsot");
    } else {
        conn_str.push_str(&env::var(DB_CONN_STR).unwrap());
    }

    let mut conn = PgConnection::connect(&conn_str).await?;

    let query = "SELECT id,
                        name,
                        common_name,
                        phone,
                        fax,
                        email,
                        street_address,
                        unit,
                        postal_code,
                        city,
                        province,
                        country
                 FROM \"referral_companies\"
                 WHERE \"id\" = $1;";

    let company = sqlx::query_as::<_, ReferralCompany>(query)
        .bind(referral_company_id)
        .fetch_optional(&mut conn)
        .await?;

    conn.close().await?;

    Ok(company)
}

pub async fn get_assessor_signature_file_name(registration_id: &str) -> Result<Option<ImageData>, Error> {

    let mut conn_str: String = String::new();

    // dev environment
    if cfg!(dev) {
        conn_str.push_str("postgres://jstg:password@localhost:5432/jsot");
    } else {
        conn_str.push_str(&env::var(DB_CONN_STR).unwrap());
    }

    let mut conn = PgConnection::connect(&conn_str).await?;

    let query = "SELECT file_name
                 FROM \"images\"
                 WHERE assessor_id = $1
                 AND image_type = 'signature'";

    let signature = sqlx::query_as::<_, ImageData>(query)
        .bind(registration_id)
        .fetch_optional(&mut conn)
        .await?;

    conn.close().await?;

    Ok(signature)

}
