use rdev::Key;

#[derive(Eq, Hash, PartialEq, Clone)]
pub struct TtsKey {
    keycode: Key,
    output: String,
}

impl TtsKey {
    pub fn from_str(keycode: Key, output: &str) -> Self {
        Self { keycode, output: output.to_string() }
    }

    pub fn from_string(keycode: Key, output: String) -> Self {
        Self { keycode, output }
    }

    pub fn get_keycode(&self) -> Key {
        self.keycode
    }

    pub fn change_key(&mut self, key: Key) {
        self.keycode = key;
    }
    pub fn get_output(&self) -> String { self.output.clone() }

    pub fn change_output(&mut self, output: Box<str>) {
        self.output = output.to_string();
    }

    pub fn is_same_key(&self, key: Key) -> bool {
        self.keycode == key
    }


}