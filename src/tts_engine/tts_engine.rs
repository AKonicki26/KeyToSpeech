use std::collections::HashSet;
use std::sync::mpsc;
use std::sync::mpsc::Receiver;
use evdev::Key;
use tts::Tts;
use crate::tts_engine::key_listener::KeyListener;
use crate::tts_engine::tts_key::TtsKey;

pub struct TtsEngine {
    keyboard: KeyListener,
    hotkeys: HashSet<TtsKey>,
    tts: Tts,
}

impl TtsEngine {
    pub fn start(&mut self) {
        let (tx, rx) = mpsc::channel::<Key>();

        std::thread::spawn(move || {
            KeyListener.listen(move |key| {
                let _ = tx.send(key);
            });
        });

        self.handle_responses(&rx);
    }

    fn say_message(&mut self, message: &str) {
        self.tts.speak(message, true).ok();
    }

    fn handle_responses(&mut self, responses: &Receiver<Key>) {
        for key in responses {
            match key {
                Key::KEY_F1 => { self.say_message("Hello, world!"); }
                Key::KEY_F2 => { self.say_message("This is message two."); }
                _ => {}
            }
        }
    }

    fn add_keybinding(&mut self, key: Key, output: String) {
        self.hotkeys.insert(TtsKey::from_string(key, output));
    }
}

impl Default for TtsEngine {
    fn default() -> Self {
        Self {
            keyboard: KeyListener::default(),
            hotkeys: HashSet::new(),
            tts: Tts::default().expect("Error when creating TTS engine")
        }
    }
}