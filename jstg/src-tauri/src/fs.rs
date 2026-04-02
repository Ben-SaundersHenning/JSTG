use std::fs::create_dir_all;

use bytes::Bytes;
use log::info;

use crate::storage::Settings;
use crate::Error;
use std::fs::File;
use std::io::Write;

pub fn save_file_to_disk(app: tauri::AppHandle, file: Bytes, file_name: String) -> Result<(), Error> {
    let settings = Settings::open();

    let mut path = settings.get("documentSavePath").unwrap().to_owned();

    if create_dir_all(path.clone()).is_ok() {
        path.push_str(format!("/{file_name}").as_str());
    }

    info!(target: "app", "Saving file as: {0}", path);

    let mut f: File = File::create(path).unwrap();

    let _ = f.write_all(&file);

    Ok(())
}
