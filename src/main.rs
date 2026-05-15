pub mod tts_engine;

use crate::tts_engine::tts_engine::TtsEngine;

fn main() {
    let mut tts_engine = TtsEngine::default();


    tts_engine.start();

    tts_engine.add_keybinding(rdev::Key::KeyA, "Hello, world!".to_string());

    println!("Hello, world!");

    tts_engine.wait();


}
