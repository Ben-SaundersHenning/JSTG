use dotenvy;

fn main() {

    // Setup the DOCUMENT_API_URL at compile time

    let is_dev = std::env::var("PROFILE").unwrap_or_default() == "debug";

    let env_file = if is_dev { ".env.dev" } else { ".env.prod" };

    dotenvy::from_filename(env_file).ok();

    let api_url = std::env::var("DOCUMENT_API_URL").unwrap_or_else(|_| "http://localhost:5250".to_string());

    println!("cargo::rustc-env=DOCUMENT_API_URL={}", api_url);

    tauri_build::build()

}
