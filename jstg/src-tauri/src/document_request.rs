mod ac;
mod cat;
mod mrb;

use crate::db;
use crate::config_manager::Config;
use crate::fs::save_file_to_disk;
use crate::Error;
use ac::Ac;
use bytes::Bytes;
use cat::Cat;
use chrono::NaiveDate;
use log::info;
use mrb::Mrb;
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

const DOCUMENT_API_URL: &str = env!("DOCUMENT_API_URL");

#[derive(Deserialize, Debug)]
#[serde(rename_all(serialize = "snake_case", deserialize = "camelCase"))]
struct FormRequest {
    assessor_registration_id: String,
    adjuster: Option<String>,
    insurance_company: String,
    claim_number: String,
    referral_company_id: i32,
    date_of_assessment: NaiveDate,
    claimant: db::Claimant,
    document_id: i32,
    ac: Option<Ac>,
    cat: Option<Cat>,
    mrb: Option<Mrb>,
}

impl FormRequest {
    fn from_json(data: String) -> Result<Self, Error> {
        let request: FormRequest = serde_json::from_str(&data)?;
        Ok(request)
    }

    // Builds a DocumentRequest object by consuming self.
    // TODO: Add some checks to the data returning from the DB.
    // Since the IDs are retrieved from the DB, they should theoretically
    // never be incorrect, but the DB itself could be down.
    async fn build_document_request(self, pool: &PgPool) -> Result<DocumentRequest, Error> {
        // 1. Retrieive assessor
        let assessor: db::Assessor = db::get_assessor(&self.assessor_registration_id, pool)
            .await?;

        // 2. Retrieive referral company
        let referral_company: db::ReferralCompany =
            db::get_referral_company(self.referral_company_id, pool)
                .await?;

        // 3. Retrieive document file name
        let document: db::Template = db::get_template(self.document_id, pool).await?;

        // 4. Retrieive image (signature file name)
        // let image_data: db::ImageData =
        //     db::get_assessor_signature_file_name(&self.assessor_registration_id)
        //         .await?
        //         .unwrap();

        // 5. Build AC portion
        let ac: Option<Ac> = match &self.ac {
            Some(val) => {
                if val.first_assessment {
                    Some(Ac {
                        first_assessment: val.first_assessment,
                        date_of_last_assessment: None,
                        monthly_allowance: None,
                        hourly_rates: None,
                    })
                } else {
                    Some(Ac {
                        first_assessment: val.first_assessment,
                        date_of_last_assessment: val.date_of_last_assessment,
                        monthly_allowance: val.monthly_allowance.clone(),
                        hourly_rates: Ac::determine_hourly_rates(val.date_of_last_assessment),
                    })
                }
            }
            None => None,
        };

        // 6. Return a Document Request
        Ok(DocumentRequest::from_form_request(
            self,
            assessor,
            referral_company,
            document,
            ac,
        ))
    }
}

#[derive(Serialize, Debug)]
#[serde(rename_all(serialize = "snake_case", deserialize = "camelCase"))]
struct DocumentRequest {
    assessor: db::Assessor,
    adjuster: Option<String>,
    insurance_company: String,
    claim_number: String,
    referral_company: db::ReferralCompany,
    date_of_assessment: NaiveDate,
    claimant: db::Claimant,
    document: db::Template,
    ac: Option<Ac>,
    cat: Option<Cat>,
    mrb: Option<Mrb>,
}

impl DocumentRequest {
    fn from_form_request(
        form_request: FormRequest,
        assessor: db::Assessor,
        referral_company: db::ReferralCompany,
        document: db::Template,
        ac: Option<Ac>,
    ) -> Self {

        // Calculate age in years
        let doa: i32 = form_request
            .date_of_assessment
            .format("%Y%m%d")
            .to_string()
            .parse()
            .unwrap();
        let dob: i32 = form_request
            .claimant
            .date_of_birth
            .format("%Y%m%d")
            .to_string()
            .parse()
            .unwrap();
        let age = (doa - dob) / 10000;

        let youth: bool = age < 18;

        DocumentRequest {
            assessor,
            adjuster: form_request.adjuster,
            insurance_company: form_request.insurance_company,
            claim_number: form_request.claim_number,
            referral_company,
            date_of_assessment: form_request.date_of_assessment,
            claimant: db::Claimant {
                first_name: form_request.claimant.first_name,
                last_name: form_request.claimant.last_name,
                gender: form_request.claimant.gender,
                age: Some(age),
                youth: Some(youth),
                date_of_birth: form_request.claimant.date_of_birth,
                date_of_loss: form_request.claimant.date_of_loss,
                address: form_request.claimant.address,
            },
            document,
            ac,
            cat: form_request.cat,
            mrb: form_request.mrb,
        }
    }

    async fn send_request(self) -> Result<Bytes, Error> {
        let request = serde_json::to_string(&self).unwrap();

        // let r = request.clone();
        // print the request to stdout
        // println!("{r}");

        let endpoint = format!("{DOCUMENT_API_URL}/DocRequest");

        info!("Pointing to {0}", endpoint.clone());

        let client = reqwest::Client::new();
        let res = client
            .post(endpoint)
            .json(&(&self))
            .header("responseType", "blob")
            .header("content-type", "application/json")
            .send()
            .await?;

        match res.status() {
            reqwest::StatusCode::OK => {
                // File
                let body = res.bytes().await?;

                return Ok(body);
            }
            _status => {}
        }

        Err(Error::DocErr)
    }

    fn build_file_name(&self) -> String {
        format!(
            "{}_{}_{} {}_{}{}.docx",
            self.referral_company.common_name,
            self.document.label,
            self.claimant.first_name,
            self.claimant.last_name,
            self.assessor.first_name.chars().next().unwrap(),
            self.assessor.last_name.chars().next().unwrap()
        )
    }
}

#[tauri::command]
pub async fn request_document(data: String, pool: tauri::State<'_, PgPool>, config: tauri::State<'_, Config>) -> Result<String, Error> {

    info!("Processing new request.");

    let request = FormRequest::from_json(data).unwrap();
    let document_request = request.build_document_request(pool.inner()).await?;
    let file_name = document_request.build_file_name();

    let response = document_request.send_request().await;

    match response {
        Ok(file) => {
            let _ = save_file_to_disk(file, file_name, config);
            return Ok("Successfully saved file to disk".to_owned());
        }
        Err(e) => {
            info!("Error: {e}");
        }
    }

    Err(Error::WriteErr)
}
