use serde::{Serialize, Deserialize};
use tauri::Manager;
use toml::Table;
use std::str::FromStr;
use std::path::PathBuf;
use std::fs;
use crate::Error;

const CONFIG_FILE: &str = "user_config.toml";

#[derive(Serialize, Deserialize, Clone)]
pub struct Config {
    pub user_config: UserConfig,
    pub advanced: Advanced
}

#[derive(Serialize, Deserialize, Clone)]
pub struct UserConfig {
    pub document_save_path: String
}

#[derive(Serialize, Deserialize, Clone)]
pub struct Advanced {
    pub document_api_url: String
}

enum ConfigValidation {
    Valid,
    Fixable,
    Invalid
}


// inits a base config if a valid configuration does not already exist
pub fn initialize_config(app: &tauri::AppHandle) -> Result<Config, Error> {

    // get the path to the configuration file
    let config_path = get_config_path(app)?;

    // create the config file if it doesn't already exist
    if !config_path.is_file() {

        // generate the base default config
        let default = Config {
            user_config: UserConfig {
                document_save_path: app.path().document_dir()?.display().to_string()
            },
            advanced: Advanced {
                document_api_url: "http://localhost:5250".to_string()
            }
        };

        let toml: String = toml::to_string_pretty(&default)?;


        // write the config to a new file
        fs::write(&config_path, &toml)?;

        return Ok(default)

    }

    /* the config file already exists! */

    // grab the file contents
    let contents: String = fs::read_to_string(&config_path)?;
    let mut toml = Table::from_str(&contents)?;

    match validate_config(&toml) {
        ConfigValidation::Valid => return Ok(toml::from_str(&contents)?),
        ConfigValidation::Fixable => fill_in_config_with_defaults(&mut toml, app)?,
        ConfigValidation::Invalid => {
            log::error!("Calling panic!(), invalid, unfixable config detected");
            panic!();
        }
    }

    /* config has been fixed, write to it */

    let config = toml::to_string(&toml)?;

    // overwrite and save the file
    fs::write(&config_path, &config)?;

    let conf: Config = toml::from_str(&config)?;

    Ok(conf)

}

// fills in missing config keys with default values, where possible
fn fill_in_config_with_defaults(config: &mut Table, app_handle: &tauri::AppHandle) -> Result<(), Error> {

    if let Some(user_config) = config.get_mut("user_config") {

        // user config IS a table, validate config checked that.
        if user_config.get("document_save_path").is_none() {
            // insert document_save_path into user_config
            user_config.as_table_mut().unwrap().insert(
                "document_save_path".to_string(),
                toml::Value::String(app_handle.path().document_dir()?.display().to_string())
                );
        }

    } else {

        // user config section is missing entirely, fill in its defaults and insert it into the table
        let mut user_conf = Table::new();
        user_conf.insert("document_save_path".to_string(), toml::Value::String(app_handle.path().document_dir()?.display().to_string()));
        config.insert("user_config".to_string(), toml::Value::Table(user_conf));

    }

    Ok(())

}

// validates the given config, logs error/warnings if found
fn validate_config(config: &Table) -> ConfigValidation {

    // check user_config subsection (NOT REQUIRED)
    if let Some(user_config) = config.get("user_config") {

        // verify user_config is actually a table
        if !user_config.is_table() {
            log::error!("Invalid configuration: user_config must be a table. Please check the configuration file.");
            // TODO: have to make this apparent from the GUI, force stop usage
            return ConfigValidation::Invalid;
        }

        // check user_config -> document_save_path
        if user_config.get("document_save_path").is_none() {
            log::warn!("Missing configuration value: document_save_path. Please check the configuration file.");
            return ConfigValidation::Fixable;
        }
    } else {
        log::warn!("Missing configuration subsection: user_config. Please check the configuration file.");
        return ConfigValidation::Fixable;
    }


    // check advanced subsection (REQUIRED)
    if let Some(advanced_config) = config.get("advanced") {
        // check advanced -> document_api_url
        if advanced_config.get("document_api_url").is_none() {
            // TODO: have to make this apparent from the GUI, force stop usage
            log::error!("Missing configuration value: document_api_url. Please check the configuration file.");
            return ConfigValidation::Invalid;
        }
    } else {

        // TODO: have to make this apparent from the GUI, force stop usage
        log::error!("Missing configuration subsection: advanced. Please check the configuration file.");
        return ConfigValidation::Invalid;
    }

    ConfigValidation::Valid

}

// TODO: write this super safely
pub fn recover_config_file(app: &tauri::AppHandle) -> Config {

    // Config {
    //     document_save_path: "".to_owned()
    // }
    Config {
        user_config: UserConfig { document_save_path: "".to_owned() },
        advanced: Advanced { document_api_url: "".to_owned() }
    }
}

fn get_config_path(app: &tauri::AppHandle) -> Result<PathBuf, Error> {
    let config_dir = app.path().config_dir()?;
    let jstg_config_dir = config_dir.join("jstg");
    fs::create_dir_all(&jstg_config_dir)?;
    Ok(jstg_config_dir.join(CONFIG_FILE))

}

#[tauri::command]
pub fn get_config(config: tauri::State<Config>) -> Result<Config, Error> {
    Ok(config.inner().clone())
}
