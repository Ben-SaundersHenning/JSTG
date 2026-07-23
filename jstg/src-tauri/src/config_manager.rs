use serde::{Serialize, Deserialize};
use tauri::Manager;
use toml::Table;
use std::str::FromStr;
use std::path::PathBuf;
use std::fs;
use crate::Error;

const CONFIG_FILE: &str = "user_config.toml";

#[derive(Serialize, Deserialize)]
pub struct Config {
    pub document_save_path: String
}

#[derive(Serialize, Deserialize)]
pub struct Advanced {
    pub document_api_url: String
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

    // add the necessary keys
    let toml = generate_default_table(&contents, &config_path.display().to_string(), app)?;

    let config = toml::to_string(&toml)?;

    // overwrite and save the file
    fs::write(&config_path, &config)?;

    let conf: Config = toml::from_str(&config)?;

    Ok(conf)

}

fn generate_default_table(file_contents: &str, config_path: &str, app_handle: &tauri::AppHandle) -> Result<Table, Error> {

    // entire toml file
    let mut toml: Table = Table::from_str(file_contents)?;

    // user-config subsection
    let mut user_conf: Table = Table::new();

    if !user_conf.contains_key("document_save_path") {
        user_conf.insert("document_save_path".to_string(), toml::Value::String(app_handle.path().document_dir()?.display().to_string()));
    }

    // advanced config subsection
    let advanced_conf: Table = Table::new();

    // TODO: fix this, it's not logically correct
    if !toml.contains_key("document_api_url") {
        // TODO: propagate this error, make a diaglog so the user sees that there is an issue. Exit.
        log::error!("Configuration file is missing required value: document_api_url. Please check the config file at {0}", config_path);
        return Err(Error::DocumentApiUrlMissingErr);
    }

    // insert both subsections

    if !toml.contains_key("user-config") {
        toml.insert("user-config".to_string(), toml::Value::Table(user_conf));
    }

    if !toml.contains_key("advanced") {
        toml.insert("advanced".to_string(), toml::Value::Table(advanced_conf));
    }

    Ok(toml)

}

pub fn recover_config_file(app: &tauri::AppHandle) -> Config {

    // TODO: write this super safely
    Config {
        document_save_path: "".to_owned()
    }
}

fn get_config_path(app: &tauri::AppHandle) -> Result<PathBuf, Error> {
    let config_dir = app.path().config_dir()?;
    let jstg_config_dir = config_dir.join("jstg");
    fs::create_dir_all(&jstg_config_dir)?;
    Ok(config_dir.join(CONFIG_FILE))
}
