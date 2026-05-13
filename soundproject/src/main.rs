pub mod tts_engine;

use crate::tts_engine::tts_engine::TtsEngine;

fn main() {
    let mut tts_engine = TtsEngine::default();
    tts_engine.start();
}
