// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use log::{info, error};
use tauri::Manager;
use sqlx::postgres::PgPool;
use tauri::path::BaseDirectory;

use std::env;

use config_manager::initialize_config;

use crate::config_manager::recover_config_file;

mod db;
mod document_request;
mod fs;
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
            util::verify_directory,
            config_manager::get_config,
            config_manager::update_config
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

fn setup_handler(app: &mut tauri::App) -> Result<(), Box<dyn std::error::Error + 'static>> {

    // Config Dir
    // Linux: $HOME/.config
    // Windows: RoamingAddData
    let base_config_dir = app.app_handle().path().config_dir()?;
    let log_dir = base_config_dir.join("jstg");

    std::fs::create_dir_all(&log_dir)?;

    std::fs::create_dir_all(log_dir.join("archive"))?;

    env::set_var("JSTG_LOG_DIR", log_dir.to_string_lossy().to_string());

    let log_config_path = app.path()
                .resolve("log4rs.yml", BaseDirectory::Resource)
                .expect("Failed to locate log4rs.yml in bundled resources");

    log4rs::init_file(log_config_path, Default::default())?;

    info!("JSTG is starting.");

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
    TauriErr(#[from] tauri::Error),
    #[error("Error in configuration file: missing document_api_url")]
    DocumentApiUrlMissingErr,
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

