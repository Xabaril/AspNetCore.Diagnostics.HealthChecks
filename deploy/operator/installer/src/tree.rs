#[derive(Deserialize, Debug, Clone)]
pub struct Tree {
    pub tree: Vec<TreeDefinition>,
    pub url: String,
}

#[derive(Deserialize, Debug, Clone)]
pub struct TreeDefinition {
    pub path: String,
    pub url: String,
    #[serde(rename(deserialize = "type"))]
    pub file_type: String,
}
