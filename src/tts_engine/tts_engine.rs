use std::collections::HashSet;
use std::sync::mpsc;
use std::sync::mpsc::Receiver;
use rdev::{Event, EventType, Key};
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
        let (tx, rx) = mpsc::channel::<Event>();

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


    fn handle_keypress(&mut self, key: Key) {
        match key {
            Key::F1 => { self.say_message("Hello, world!"); }
            Key::F2 => { self.say_message("This is message two."); }
            _ => {}
        }
    }

    fn handle_responses(&mut self, responses: &Receiver<Event>) {


        for event in responses {
            match event.event_type {
                EventType::KeyPress(key) => self.handle_keypress(key),
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