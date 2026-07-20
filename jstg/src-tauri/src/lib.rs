// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use std::path::Path;

use log::{info, error, LevelFilter};
use log4rs::append::console::ConsoleAppender;
use log4rs::append::file::FileAppender;
use log4rs::config::{Appender, Logger, Root};
use log4rs::encode::pattern::PatternEncoder;
use log4rs::Config;

use tauri::Manager;

use sqlx::postgres::PgPool;

use std::env;

use config_manager::initialize_config;

use crate::config_manager::recover_config_file;

mod db;
mod document_request;
mod fs;
mod storage;
mod util;
mod config_manager;

extern crate dirs;

const DB_CONN_STR: &str = "JSTG_DB_POSTGRESQL";

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_shell::init())
        .setup(setup_handler)
        .invoke_handler(tauri::generate_handler![
            db::get_assessor_options,
            db::get_template_options,
            db::get_referral_company_options,
            document_request::request_document,
            storage::get_config,
            storage::update_config,
            util::verify_directory,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

fn setup_handler(app: &mut tauri::App) -> Result<(), Box<dyn std::error::Error + 'static>> {


    let mut app_logs: String = (&app.package_info().name).into();
    app_logs.push_str("/logs.log");

    // let log_dir_path = Path::new(&tauri::api::path::config_dir().unwrap()).join(app_logs);

    // Config Dir
    // Linux: $HOME/.config
    // Windows: RoamingAddData
    let log_dir_path = Path::new(&dirs::config_dir().unwrap()).join(app_logs);

    let stdout = ConsoleAppender::builder().build();

    let requests = FileAppender::builder()
        .encoder(Box::new(PatternEncoder::new(
            "{d(%Y-%m-%d %H:%M:%S)} | {({l}):5.5} | {f}:{L} - {m}{n}",
        )))
        .build(log_dir_path)
        .unwrap();

    // setup loggers
    let config = Config::builder()
        .appender(Appender::builder().build("stdout", Box::new(stdout)))
        .appender(Appender::builder().build("requests", Box::new(requests)))
        .logger(
            Logger::builder()
                .appender("requests")
                .additive(false)
                .build("app", LevelFilter::Debug),
        )
        .build(Root::builder().appender("stdout").build(LevelFilter::Warn))
        .unwrap();

    let _ = log4rs::init_config(config).unwrap();

    info!(target: "app", "JSTG is starting.");

    let config: config_manager::Config = match initialize_config(app.handle()) {
        Ok(config) => config,
        Err(e) => {
            error!("Config not initialized: {}", e);
            recover_config_file(app.handle())
        }
    };

    app.manage(config);

    let pool = tauri::async_runtime::block_on(
        PgPool::connect(&get_connection_string())
    ).unwrap();

    app.manage(pool);

    Ok(())
}

// A custom error type that represents all command errors
#[derive(Debug, thiserror::Error)]
pub enum Error {
    #[error("Failed to read file: {0}")]
    Io(#[from] std::io::Error),
    #[error("File is not valid utf8: {0}")]
    Utf8(#[from] std::string::FromUtf8Error),
    #[error("Error retrieving values from the database: {0}")]
    Sqlx(#[from] sqlx::Error),
    #[error("Error converting data to struct.")]
    Serde(#[from] serde_json::Error),
    #[error("Error making HTTP request.")]
    Reqwest(#[from] reqwest::Error),
    #[error("Error validating document API response.")]
    DocErr,
    #[error("Error saving file to disk.")]
    WriteErr,
    #[error("Error deserializing TOML string")]
    TomlDeserializeErr(#[from] toml::de::Error),
    #[error("Error serializing TOML string")]
    TomlSerializeErr(#[from] toml::ser::Error),
    #[error("Tauri Error")]
    TauriErr(#[from] tauri::Error)
}

impl serde::Serialize for Error {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: serde::ser::Serializer,
    {
        serializer.serialize_str(self.to_string().as_ref())
    }
}

fn get_connection_string() -> String {
    // dev environment
    if cfg!(dev) {
        "postgres://jstg:password@localhost:5432/jsot".to_string()
    } else {
        env::var(DB_CONN_STR).unwrap()
    }
}

