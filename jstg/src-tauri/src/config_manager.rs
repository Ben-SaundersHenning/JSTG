fn get_config_path(app: &tauri::AppHandle) -> PathBuf {
    let config_dir = app.path().app_config_dir().unwrap();
    fs::create_dir_all(&config_dir).unwrap();
    config_dir.join("config.toml")
}
// TODO: finsih this
