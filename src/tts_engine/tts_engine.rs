use std::collections::HashSet;
use std::sync::mpsc;
use std::sync::mpsc::{Receiver, Sender};
use std::thread::JoinHandle;
use std::time::Duration;
use rdev::{Event, EventType, Key};
use tts::Tts;
use crate::tts_engine::key_listener::KeyListener;
use crate::tts_engine::tts_key::TtsKey;

struct TtsEngineWorker {
    hotkeys: HashSet<TtsKey>,
    tts: Tts,
}

impl TtsEngineWorker {
    fn run(mut self, key_rx: Receiver<Event>, bind_rx: Receiver<(Key, String)>) {
        println!("Event loop started");
        loop {
            self.process_bind_queue(&bind_rx);

            match key_rx.recv_timeout(Duration::from_millis(10)) {
                Ok(event) => {
                    if let EventType::KeyPress(key) = event.event_type {
                        self.handle_keypress(key);
                    }
                }
                Err(mpsc::RecvTimeoutError::Timeout) => {}
                Err(mpsc::RecvTimeoutError::Disconnected) => break,
            }
        }
    }

    fn process_bind_queue(&mut self, bind_rx: &Receiver<(Key, String)>) {
        while let Ok((key, output)) = bind_rx.try_recv() {
            self.hotkeys.retain(|hotkey| !hotkey.is_same_key(key));
            if !output.is_empty() {
                self.hotkeys.insert(TtsKey::from_string(key, output));
                println!("Added keybind: {:?}", key);
            }
        }
    }

    fn handle_keypress(&mut self, key: Key) {
        let message = self.hotkeys.iter()
            .find(|hotkey| hotkey.is_same_key(key))
            .map(|hotkey| hotkey.get_output());

        if let Some(msg) = message {
            println!("Hotkey Pressed: {:?}", key);
            self.tts.speak(&msg, true).ok();
        }
    }
}

pub struct TtsEngine {
    worker: Option<TtsEngineWorker>,
    keybind_sender: Option<Sender<(Key, String)>>,
    thread_handle: Option<JoinHandle<()>>,
    speaking_sender: Option<Sender<bool>>,
    can_speak: bool,
}

impl TtsEngine {
    pub fn start(&mut self) {
        let worker = self.worker.take().expect("Engine already started");

        let (key_tx, key_rx) = mpsc::channel::<Event>();
        let (bind_tx, bind_rx) = mpsc::channel::<(Key, String)>();

        self.keybind_sender = Some(bind_tx);

        std::thread::spawn(move || {
            KeyListener.listen(move |event| {
                let _ = key_tx.send(event);
            });
        });

        self.thread_handle = Some(std::thread::spawn(move || {
            worker.run(key_rx, bind_rx);
        }));
    }

    pub fn wait(self) {
        if let Some(handle) = self.thread_handle {
            let _ = handle.join();
        }
    }

    pub fn add_keybinding(&mut self, key: Key, output: String) {
        match &self.keybind_sender {
            Some(sender) => { let _ = sender.send((key, output)); }
            None => { panic!("Cannot add keybindings before starting the engine"); }
        }
    }

    pub fn modify_keybinding(&mut self, key: Key, output: String) {
        match &self.keybind_sender {
            Some(sender) => { let _ = sender.send((key, output)); }
            None => { panic!("Cannot modify keybindings before starting the engine"); }
        }
    }

    pub fn remove_keybinding(&mut self, key: Key) {
        match &self.keybind_sender {
            Some(sender) => { let _ = sender.send((key, "".to_string())); }
            None => { panic!("Cannot remove keybindings before starting the engine"); }
        }
    }

    pub fn allow_speaking(&mut self, allow: bool) {
        match &self.speaking_sender {
            Some(sender) => { let _ = sender.send(allow); }
            None => { panic!("Cannot allow speaking before starting the engine"); }
        }
    }

    pub fn block_speaking(&mut self) {
        self.can_speak = false;
    }
}

impl Default for TtsEngine {
    fn default() -> Self {
        Self {
            worker: Some(TtsEngineWorker {
                hotkeys: HashSet::new(),
                tts: Tts::default().expect("Error when creating TTS engine"),
            }),
            keybind_sender: None,
            thread_handle: None,
            speaking_sender: None,
            can_speak: true,
        }
    }
}
