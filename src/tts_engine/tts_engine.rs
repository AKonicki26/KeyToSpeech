use std::collections::HashSet;
use std::sync::mpsc;
use std::sync::mpsc::{Receiver, Sender};
use std::thread::JoinHandle;
use std::time::Duration;
use rdev::{Event, EventType, Key};
use tts::Tts;
use crate::tts_engine::key_listener::KeyListener;
use crate::tts_engine::tts_key::TtsKey;

pub struct TtsEngine {
    hotkeys: HashSet<TtsKey>,
    tts: Tts,
    keybind_sender: Option<Sender<(Key, String)>>,
    thread_handle: Option<JoinHandle<()>>,
}

impl TtsEngine {
    pub fn start(&mut self) {
        let (key_tx, key_rx) = mpsc::channel::<Event>();
        let (bind_tx, bind_rx) = mpsc::channel::<(Key, String)>();

        self.keybind_sender = Some(bind_tx);

        std::thread::spawn(move || {
            KeyListener.listen(move |event| {
                let _ = key_tx.send(event);
            });
        });

        let hotkeys = std::mem::take(&mut self.hotkeys);
        let tts = std::mem::replace(
            &mut self.tts,
            Tts::default().expect("Error when creating TTS engine"),
        );

        self.thread_handle = Some(std::thread::spawn(move || {
            Self::run_event_loop(key_rx, bind_rx, hotkeys, tts);
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
            None => { panic!("Cannot modify keybindings before starting the engine");
            }
        }
    }

    pub fn remove_keybinding(&mut self, key: Key) {
        match &self.keybind_sender {
            Some(sender) => { let _ = sender.send((key, "".to_string())); }
            None => { panic!("Cannot remove keybindings before starting the engine"); }
        }
    }

    fn run_event_loop(
        key_rx: Receiver<Event>,
        bind_rx: Receiver<(Key, String)>,
        mut hotkeys: HashSet<TtsKey>,
        mut tts: Tts,
    ) {
        println!("Event loop started");
        loop {
            // add keybinds to the system if any are requested
            Self::process_bind_queue(&bind_rx, &mut hotkeys);

            // check for key presses
            match key_rx.recv_timeout(Duration::from_millis(10)) {
                // if a key press is detected, handle it
                Ok(event) => {
                    if let EventType::KeyPress(key) = event.event_type {
                        Self::handle_keypress(key, &hotkeys, &mut tts);
                    }
                }
                // if we errored for some reason, continue or exit
                Err(mpsc::RecvTimeoutError::Timeout) => {}
                Err(mpsc::RecvTimeoutError::Disconnected) => break,
            }
        }
    }

    fn process_bind_queue(bind_rx: &Receiver<(Key, String)>, hotkeys: &mut HashSet<TtsKey>) {
        while let Ok((key, output)) = bind_rx.try_recv() {
            // clear any existing keybinds for this key
            hotkeys.retain(|hotkey| !hotkey.is_same_key(key));

            // if the output is empty, return early since we don't need to add anything
            if output.is_empty() {
                continue;
            }

            // add the new keybind
            hotkeys.insert(TtsKey::from_string(key, output));
            println!("Added keybind: {:?}", key);
        }
    }

    fn handle_keypress(key: Key, hotkeys: &HashSet<TtsKey>, tts: &mut Tts) {
        let message = hotkeys.iter()
            .find(|hotkey| hotkey.is_same_key(key))
            .map(|hotkey| hotkey.get_output());

        if let Some(msg) = message {
            println!("Hotkey Pressed: {:?}", key);
            tts.speak(msg, true).ok();
        }
    }
}

impl Default for TtsEngine {
    fn default() -> Self {
        Self {
            hotkeys: HashSet::new(),
            tts: Tts::default().expect("Error when creating TTS engine"),
            keybind_sender: None,
            thread_handle: None,
        }
    }
}
