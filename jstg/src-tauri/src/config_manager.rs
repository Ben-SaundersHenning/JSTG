use toml::Table;
use std::fs;

const CONFIG_FILE: &str = "user_config.toml";

pub fn initialize_config(app: &tauri::AppHandle) {

    let config_path: PathBuf = get_config_path(app);

    if !config_path.is_file() {

        // create a default config there

    }

    // verify that the mandatory values are there,
    // and then populate them with defaults if not

}

pub fn get_config_path(app: &tauri::AppHandle) -> PathBuf {

    let config_dir = app.path().app_config_dir().unwrap();
    fs::create_dir_all(&config_dir).unwrap();
    config_dir.join(CONFIG_FILE);

    config_dir

}
// TODO: finsih this

/*
 * 1. Create a default TOML config. It will be copied with the application, so it should exist.
 * 2. Create an init method. If the file happens to not exist, it should create it with the default
 *    config.
 * 3. Create a method that opens the TOML file for read/write (general use)
 * 3. Create a get method. Retrieves a value of a particular key.
 * 4. Create a set method.
 * 5. Create a delete method.
 */
