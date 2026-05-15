use evdev::{Device, EventType, Key};

#[derive(Default)]
pub struct KeyListener;

impl KeyListener {
    pub fn listen<F>(&self, callback: F)
    where
        F: Fn(Key) + Send + Clone + 'static,
    {
        let keyboards = Self::get_keyboards();

        if keyboards.is_empty() {
            eprintln!("No keyboards found");
            return;
        }

        let handles: Vec<_> = keyboards
            .into_iter()
            .map(|mut device| {
                let cb = callback.clone();
                std::thread::spawn(move || loop {
                    match device.fetch_events() {
                        Ok(events) => {
                            for event in events {
                                if event.event_type() == EventType::KEY && event.value() == 1 {
                                    cb(Key::new(event.code()));
                                }
                            }
                        }
                        Err(e) => {
                            eprintln!("Key listener error: {e}");
                            break;
                        }
                    }
                })
            })
            .collect();

        for handle in handles {
            let _ = handle.join();
        }
    }

    fn get_keyboards() -> Vec<Device> {
        let keyboards: Vec<_> = evdev::enumerate()
            .filter_map(|(_, device)| {
                if device
                    .supported_keys()
                    .map_or(false, |keys| keys.contains(Key::KEY_A))
                {
                    Some(device)
                } else {
                    None
                }
            })
            .collect();

        keyboards
    }
}
