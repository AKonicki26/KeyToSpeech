use std::collections::{HashMap, HashSet};
use std::sync::mpsc;
use std::sync::mpsc::Receiver;
use evdev::Key;
use tts::Tts;
use crate::tts_engine::key_listener::KeyListener;
use crate::tts_engine::tts_key::TtsKey;

pub mod tts_key;
pub mod key_listener;
pub mod tts_engine;

