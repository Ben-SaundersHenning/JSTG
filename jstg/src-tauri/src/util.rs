use std::path::Path;

#[tauri::command]
pub fn verify_directory(directory: String) -> bool {
    let path = Path::new(&directory);

    path.is_dir()
}
