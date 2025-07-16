use chrono::NaiveDate;
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, sqlx::FromRow, Debug, Clone)]
#[serde(rename_all(serialize = "snake_case", deserialize = "camelCase"))]
pub struct Ac {
    pub first_assessment: bool,
    pub date_of_last_assessment: Option<NaiveDate>,
    pub monthly_allowance: Option<String>,
    pub hourly_rates: Option<Vec<String>>,
}

const DATE0: &str = "2008-03-31";
const DATE1: &str = "2010-09-01";
const DATE2: &str = "2014-06-01";
const DATE3: &str = "2015-10-01";
const DATE4: &str = "2016-10-01";
const DATE5: &str = "2017-10-01";
const DATE6: &str = "2018-01-01";
// const DATE7: &str = "2018-04-14";

impl Ac {
    pub fn determine_hourly_rates(
        date_of_last_assessment: Option<NaiveDate>,
    ) -> Option<Vec<String>> {
        match date_of_last_assessment {
            Some(val) => {
                let rates: Option<Vec<String>> = if val >= parse_date(DATE0).unwrap()
                    && val < parse_date(DATE1).unwrap()
                {
                    Some(vec![
                        "11.23".to_string(),
                        "8.75".to_string(),
                        "17.98".to_string(),
                    ])
                } else if val >= parse_date(DATE1).unwrap() && val < parse_date(DATE2).unwrap() {
                    Some(vec![
                        "13.19".to_string(),
                        "10.25".to_string(),
                        "19.35".to_string(),
                    ])
                } else if val >= parse_date(DATE2).unwrap() && val < parse_date(DATE3).unwrap() {
                    Some(vec![
                        "13.19".to_string(),
                        "11.00".to_string(),
                        "19.35".to_string(),
                    ])
                } else if val >= parse_date(DATE3).unwrap() && val < parse_date(DATE4).unwrap() {
                    Some(vec![
                        "13.19".to_string(),
                        "11.25".to_string(),
                        "19.35".to_string(),
                    ])
                } else if val >= parse_date(DATE4).unwrap() && val < parse_date(DATE5).unwrap() {
                    Some(vec![
                        "14.90".to_string(),
                        "11.40".to_string(),
                        "21.11".to_string(),
                    ])
                } else if val >= parse_date(DATE5).unwrap() && val < parse_date(DATE6).unwrap() {
                    Some(vec![
                        "14.90".to_string(),
                        "11.60".to_string(),
                        "21.11".to_string(),
                    ])
                } else {
                    Some(vec![
                        "14.90".to_string(),
                        "14.00".to_string(),
                        "21.11".to_string(),
                    ])
                };

                rates
            }
            None => None,
        }
    }
}

fn parse_date(input: &str) -> Option<NaiveDate> {
    //try to parse from a date like "2023-11-01"
    let date = NaiveDate::parse_from_str(input, "%Y-%m-%d");

    if let Ok(d) = date {
        return Some(d);
    }

    //try to parse from a date like November 1, 2023
    let date = NaiveDate::parse_from_str(input, "%B %d, %Y");

    date.ok()
}
