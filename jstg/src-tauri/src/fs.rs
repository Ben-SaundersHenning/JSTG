use std::fs::create_dir_all;

use bytes::Bytes;
use log::info;

use crate::config_manager::Config;
use crate::Error;
use std::fs::File;
use std::io::Write;

pub fn save_file_to_disk(file: Bytes, file_name: String, conf: tauri::State<Config>) -> Result<(), Error> {

    let mut path = conf.user_config.document_save_path.to_owned();

    if create_dir_all(path.clone()).is_ok() {
        path.push_str(format!("/{file_name}").as_str());
    }

    info!("Saving file as: {0}", path);

    let mut f: File = File::create(path).unwrap();

    let _ = f.write_all(&file);

    Ok(())

}
