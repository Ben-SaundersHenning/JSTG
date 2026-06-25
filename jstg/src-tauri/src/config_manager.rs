use serde::{Serialize};
use tauri::Manager;
use toml::Table;
use std::str::FromStr;
use std::path::PathBuf;
use std::fs;

use crate::Error;

const CONFIG_FILE: &str = "user_config.toml";

#[derive(Serialize)]
pub struct Config {
    document_save_path: String
}


// inits a base config if a valid configuration does not already exist
pub fn initialize_config(app: &tauri::AppHandle) -> Result<Config, Error> {

    // get the path to the configuration file
    let config_path = get_config_path(app)?;

    // create the config file if it doesn't already exist
    if !config_path.is_file() {

        let default = Config {
            document_save_path: config_path.display().to_string()
        };

        // write default config to new file
        let toml: String = toml::to_string(&default)?;

        fs::write(&config_path, &toml)?;

        return Ok(default)

    }

    // the config file already exists!

    let contents: String = fs::read_to_string(&config_path)?;

    let mut tom: Table = Table::from_str(&contents)?;

    // add the necessary keys

    if !tom.contains_key("document_save_path") {
        tom.insert("document_save_path".to_string(), toml::Value::String(config_path.display().to_string()));
    }

    let config = toml::to_string(&tom)?;

    // overwrite and save the file
    fs::write(&config_path, &config)?;

    Ok(config)

}

pub fn recover_config_file(app: &tauri::AppHandle) -> String {

    // TODO: write this super safely
    "sdlfkj".to_string()
}

fn get_config_path(app: &tauri::AppHandle) -> Result<PathBuf, Error> {
    let config_dir = app.path().app_config_dir()?;
    fs::create_dir_all(&config_dir)?;
    Ok(config_dir.join(CONFIG_FILE))
}
